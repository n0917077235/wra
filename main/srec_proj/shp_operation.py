import pandas as pd
import geopandas as gpd

# import numpy as np
from shapely.geometry import (
    Point,
    LineString,
    Polygon,
    MultiLineString,
)
import shapely
import sys
import os
import math
import matplotlib.pyplot as plt
from typing import List, Tuple, Union, Optional
import contextily as ctx
import requests
import rasterio
import numpy as np
from multiprocessing import Pool
from cpuinfo import get_cpu_info

# import inspect
from pyproj import CRS  # pylint: disable=no-name-in-module

import jutility as jut
import group_ts as gts
import plt_parameters  # noqa
import file_utility as fut
import netcdf_analysis as NA


def from_polygon2line(polygon: Polygon) -> LineString:
    """
    將 Polygon 轉為 LineString
    """
    linestrings = LineString(
        [
            polygon.exterior.coords[i]
            for i in range(len(polygon.exterior.coords))
        ]
    )
    return linestrings


def clip_line_in_extent(
    line: LineString, extent_polygon: Polygon
) -> List[List]:
    """
    保留 LineString 在 extent_polygon 的節點
    因為可能被邊框切割成多段
    --> [
            [],
            [],
        ]
    """
    multi_line_point = []
    line_point = []
    for i, p in enumerate(line.coords):
        if extent_polygon.contains(Point(p[:2])):
            line_point.append(list(p[:2]))
        else:
            # 不在邊框內
            # 如果前一點在裡面, 以連線計算
            try:
                if jut.check_list_in_list(
                    line_point, line.coords[i - 1][:2]
                ):
                    # 表示前一點, 已在裡面
                    line_edge = LineString(
                        [
                            list(line.coords[i - 1][:2]),
                            list(line.coords[i][:2]),
                        ]
                    )
                    # 計算交點
                    p_intersection = extent_polygon.intersection(
                        line_edge
                    )
                    line_point.append(list(p_intersection[:2]))
            except IndexError:
                pass
            if len(line_point) >= 2:
                # 至少兩點以上, 兩點構成線
                multi_line_point.append(line_point)
                line_point = []  # 清空
    if len(line_point) >= 2:
        # 處理尚未紀錄的部分
        multi_line_point.append(line_point)
    return multi_line_point


def clip_gpd_in_extent(
    gpd_shp: gpd.GeoDataFrame, **kwargs
) -> gpd.GeoDataFrame:
    """
    kwargs 輸入 extent
    從 geometry 擷取所有的點, 保留 extent 內的點
    # 原本格式 -->        新的格式
    # Point              Point
    # LineString         MultiLineString
    # MultiLineString    MultiLineString
    # Polygon            MultiLineString
    # MultiPolygon       MultiLineString    (尚未實作)
    """
    assert "extent" in kwargs
    assert isinstance(kwargs["extent"], list)
    assert len(kwargs["extent"]) == 4
    for i in range(4):
        assert isinstance(kwargs["extent"][i], (int, float))
    gpd_shp2 = gpd_shp.copy()

    extent_polygon = Polygon(
        [
            [kwargs["extent"][0], kwargs["extent"][2]],
            [kwargs["extent"][1], kwargs["extent"][2]],
            [kwargs["extent"][1], kwargs["extent"][3]],
            [kwargs["extent"][0], kwargs["extent"][3]],
            [kwargs["extent"][0], kwargs["extent"][2]],
        ]
    )
    for index in gpd_shp2.index:
        if gpd_shp2.loc[index, "geometry"].geom_type == "Point":
            if not extent_polygon.contains(
                gpd_shp2.loc[index, "geometry"]
            ):
                # 不包含該點
                gpd_shp2 = gpd_shp2.drop(index=index)
        elif gpd_shp2.loc[index, "geometry"].geom_type in [
            "LineString",
            "Polygon",
        ]:
            line = gpd_shp2.loc[index, "geometry"]
            if (
                gpd_shp2.loc[index, "geometry"].geom_type
                == "Polygon"
            ):
                line = from_polygon2line(
                    gpd_shp2.loc[index, "geometry"]
                )
            multi_line_point: List = clip_line_in_extent(
                line, extent_polygon
            )
            gpd_shp2.loc[index, "geometry"] = MultiLineString(
                multi_line_point
            )
        elif (
            gpd_shp2.loc[index, "geometry"].geom_type
            == "MultiLineString"
        ):
            multi_line_point: List = []
            for line in gpd_shp2.loc[index, "geometry"].geoms:
                multi_line_point += clip_line_in_extent(
                    line, extent_polygon
                )
            gpd_shp2.loc[index, "geometry"] = MultiLineString(
                multi_line_point
            )
        elif (
            gpd_shp2.loc[index, "geometry"].geom_type
            == "MultiPolygon"
        ):
            Polygons = list(
                gpd_shp2.loc[index, "geometry"].geoms
            )
            multi_line_point: List = []
            for poly in Polygons:
                assert poly.geom_type == "Polygon"
                line = from_polygon2line(poly)
                multi_line_point += clip_line_in_extent(
                    line, extent_polygon
                )
            gpd_shp2.loc[index, "geometry"] = MultiLineString(
                multi_line_point
            )

    return gpd_shp2


def split_by_point_distance(
    gpd_data: gpd.GeoDataFrame, dist_criteria
) -> gpd.GeoDataFrame:
    """
    只處理 geom_type 為 LineString
    如果 dist_criteria 大於門檻者, 作為切割點
    改為 多筆資料的 LineString
    """
    mat2 = []
    geometry2 = []
    for index in gpd_data.index:
        if (
            gpd_data.loc[index, "geometry"].geom_type
            == "LineString"
        ):
            # 計算距離
            mat = []
            for i, p in enumerate(
                gpd_data.loc[index, "geometry"].coords
            ):
                try:
                    mat.append(
                        [
                            p[0],
                            p[1],
                            shapely.distance(
                                Point(p),
                                Point(
                                    gpd_data.loc[
                                        index, "geometry"
                                    ].coords[i - 1]
                                ),
                            ),
                        ]
                    )
                except IndexError:
                    mat.append(
                        [
                            p[0],
                            p[1],
                            0,
                        ]
                    )

            df_distance = pd.DataFrame(
                mat, columns=["X", "Y", "distance"]
            )
            df_distance.loc[:, "criteria"] = (
                df_distance.loc[:, "distance"] < dist_criteria
            )

            multi_line_point = []
            line_point = []
            for index2 in df_distance.index:
                if df_distance.loc[index2, "criteria"]:
                    line_point.append(
                        list(
                            df_distance.loc[
                                index2, ["X", "Y"]
                            ].values
                        )
                    )
                else:
                    if len(line_point) >= 2:
                        multi_line_point.append(line_point)
                        line_point = []
            if len(line_point) >= 2:
                multi_line_point.append(line_point)
            # gpd_data.loc[index, "geometry"] = MultiLineString(multi_line_point)

            mls = MultiLineString(multi_line_point)
            assert isinstance(mls, MultiLineString)
            for line in mls.geoms:
                assert isinstance(line, LineString)
                geometry2.append(line)
                mat2.append(
                    list(
                        gpd_data.loc[
                            index,
                            [
                                col
                                for col in gpd_data.columns
                                if col != "geometry"
                            ],
                        ].values
                    )
                )
    gpd_data = gpd.GeoDataFrame(
        mat2,
        index=np.arange(0, len(mat2), 1),
        columns=[
            col for col in gpd_data.columns if col != "geometry"
        ],
        geometry=geometry2,
    )
    return gpd_data


def check_point_in_polygon_shell(flag: Tuple) -> bool:
    (obj, x, y) = flag
    _index, log = obj.check_XY_in_polygon((x, y))
    return log


def tics2polygon(tics_limit_layout: List) -> Polygon:
    """
    Layout 用的兩個端點
    [
        [loc of ll],
        [loc of ur],
    ]
    """
    coords = [
        tuple(tics_limit_layout[0]),
        (tics_limit_layout[1][0], tics_limit_layout[0][1]),
        tuple(tics_limit_layout[1]),
        (tics_limit_layout[0][0], tics_limit_layout[1][1]),
        tuple(tics_limit_layout[0]),
    ]
    return Polygon(coords)


def check_point_in_region(
    point_loc: Tuple, tics_limit_layout: List
) -> bool:
    """
    Check Point in Region
    """
    point: Point = Point(point_loc)
    polygon: Polygon = tics2polygon(tics_limit_layout)

    # 確認 point 是否位於 poly 內
    return point.within(polygon)


def check_line_in_region(
    polygon: Polygon, line: LineString, **kwargs
) -> bool:
    """
    檢查 line 是否在 Polygon 中
    kwargs["gis_extra_post_filter_type"]
        all --> np.all
        any --> np.any
        most --> 總數需達到一半以上
    """
    check_results = [
        Point(*tuple(point)).within(polygon)
        for point in shapely.get_coordinates(line).tolist()
    ]
    assert kwargs.get("gis_extra_post_filter_type", "all") in [
        "all",
        "any",
        "most",
    ]

    if kwargs.get("gis_extra_post_filter_type", "all") == "all":
        return np.all(check_results)
    elif (
        kwargs.get("gis_extra_post_filter_type", "all") == "any"
    ):
        return np.any(check_results)
    # most
    return (
        float(np.sum(check_results)) / len(check_results) >= 0.5
    )


def add_tuple(a: Union[List, Tuple], b: Tuple) -> List:
    try:
        assert len(a) == len(b)
        c = []
        for i in range(len(a)):
            c.append(a[i] + b[i])
        return c
    except AssertionError:
        # 備註: inspect.currentframe().f_code.co_name
        #      為 Method Name
        # frame = inspect.currentframe()
        # try:
        #    print(
        #        "-- Method ``{}'' in {}".format(
        #            frame.f_code.co_name, __name__
        #        )
        #    )
        # finally:
        #    del frame
        print(
            "a & b 輸入陣列尺寸不一致: {} {}".format(
                len(a), len(b)
            )
        )
        sys.exit()


def sid_addzero(sid: str, sid_length: int) -> str:
    if not isinstance(sid, str):
        return sid

    sid_length_in = len(sid)
    sid_output = None
    if sid_length_in < sid_length:
        sid_output = ""
        for _ in range(sid_length_in, sid_length):
            sid_output += "0"
        sid_output = sid_output + sid
    else:
        sid_output = sid
    return sid_output


def determine_sid2_consider_distance(
    myGTSM,
    sid1: str,
    vname1: str,
    vname2: str,
    columns2: List,
    tics_limit_layout,
    stat_info2: List,
    matrix_distance,
    distance_criteria: float,
):
    index1 = myGTSM.get_GTSP(vname1).query_index_from_sid(sid1)
    sid2 = None
    sname2 = None
    r2_value = None

    for n in range(len(stat_info2)):
        sid2 = stat_info2[n][0]
        sname2 = stat_info2[n][1]
        r2_value = stat_info2[n][2]

        index2 = myGTSM.get_GTSP(vname2).query_index_from_sid(
            sid2
        )
        distance = matrix_distance[index1, index2]

        loc2 = myGTSM.get_GTSP(vname2).query_value_from_sid(
            sid2, columns2
        )
        if distance < distance_criteria:
            if check_point_in_region(loc2, tics_limit_layout):
                break
    return sid2, sname2, r2_value, loc2


def plot_gw_rain_infil(
    ax,
    matrix_r2,
    myGTSM,
    matrix_distance,
    shp_stat1,
    shp_stat2,
    vname1: str,
    vname2: str,
    arraw_color,
    tics_limit_layout,
    log_debug: bool = False,
    **kwargs,
):
    root_logger = kwargs.get("root_logger", None)
    loc_list1 = shp_stat1.gdf_shp.loc[
        :, shp_stat1.columns
    ].values
    # loc_list2 = shp_stat2.gdf_shp.loc[:, shp_stat2.columns].values

    arrowstyle = "-|>"
    connectionstyle = "arc3,rad=0."
    # arraw_color = "indigo"
    # arraw_color = "purple"
    alpha = 0.9
    lw = 0.9
    distance_criteria = 15000

    count = 0
    for m in range(len(loc_list1)):
        sid_query1 = loc_list1[m][0]
        sname_query1 = myGTSM.get_GTSP(
            vname1
        ).query_sname_from_sid(sid_query1)

        if sname_query1 is not None:  # 表示不在群裡
            loc1 = myGTSM.get_GTSP(vname1).query_value_from_sid(
                sid_query1, shp_stat1.columns[1:]
            )
            if check_point_in_region(loc1, tics_limit_layout):
                # 找出最相關的雨量站
                stat_info = myGTSM.determine_sorted_stat(
                    matrix_r2,
                    vname1,
                    vname2,
                    sid_query1,
                    ascending=False,
                    log_debug=False,
                )

                (
                    sid_query2,
                    sname_query2,
                    r2_value,
                    loc2,
                ) = determine_sid2_consider_distance(
                    myGTSM,
                    sid_query1,
                    vname1,
                    vname2,
                    shp_stat2.columns[1:],
                    tics_limit_layout,
                    stat_info,
                    matrix_distance,
                    distance_criteria,
                )
                log_check = r2_value is not None
                if not log_check:
                    print(
                        r2_value, r2_value is not None, log_check
                    )
                    log_check = log_check & (
                        not math.isnan(r2_value)
                    )
                if log_debug:
                    if count < 20:
                        if log_check:
                            if r2_value > 0.2:
                                root_logger.debug(
                                    " -- {}::{}/{} --> {}::{}/{} | {}".format(
                                        vname1,
                                        sid_query1,
                                        sname_query1,
                                        vname2,
                                        sid_query2,
                                        sname_query2,
                                        r2_value,
                                    )
                                )
                                count += 1

                if log_check:
                    if r2_value > 0.2:
                        # message = "{}".format(round(r2_value, 2))
                        message = ""
                        plt.annotate(
                            message,
                            xy=loc1,
                            xytext=loc2,
                            xycoords="data",
                            textcoords="data",
                            arrowprops=dict(
                                arrowstyle=arrowstyle,
                                connectionstyle=connectionstyle,
                                facecolor=arraw_color,
                                edgecolor=arraw_color,
                                lw=lw,
                            ),
                            ha="center",
                            va="center",
                            fontsize=9,
                            alpha=alpha,
                        )
    return ax


def plot_flow_network(
    ax,
    my_GTS,
    corr_criteria: float,
    distance_criteria: float,
    df_describe,
    tics_limit_layout,
    log_debug: bool = False,
    **kwargs,
):
    root_logger = kwargs.get("root_logger", None)
    matrix_distance = my_GTS.get_matrix("distance")
    matrix_corr = my_GTS.get_matrix("correlation")
    sid_list = my_GTS.get_GTS_sid()
    sname_list = my_GTS.get_GTS_sname()
    loc_columns = ["TWD97X_121", "TWD97Y_121"]
    my_GTS.df_stat.loc[:, "mean"] = df_describe.loc[:, "mean"]
    arrowstyle = "-|>"
    # arrowstyle = "simple"
    # connectionstyle = "arc3,rad=0.6"
    # connectionstyle = "angle,angleA=0,angleB=-90,rad=10"
    connectionstyle = "arc3,rad=0."
    for i in range(matrix_distance.shape[0]):
        for j in range(i + 1, matrix_distance.shape[1]):
            log1 = True
            if distance_criteria is not None:
                # 距離近
                log1 = matrix_distance[i, j] < distance_criteria

            # 相關性高
            log2 = matrix_corr[i, j] > corr_criteria

            if log1 and log2:
                sid1 = sid_list[i]
                sid2 = sid_list[j]
                sname1 = sname_list[i]
                sname2 = sname_list[j]
                loc1 = my_GTS.query_value_from_sid(
                    sid1, loc_columns
                )
                loc2 = my_GTS.query_value_from_sid(
                    sid2, loc_columns
                )

                if check_point_in_region(
                    loc1, tics_limit_layout
                ) and check_point_in_region(
                    loc2, tics_limit_layout
                ):
                    [mean1] = my_GTS.query_value_from_sid(
                        sid1, ["mean"]
                    )
                    [mean2] = my_GTS.query_value_from_sid(
                        sid2, ["mean"]
                    )

                    loc_upper = loc1
                    loc_lower = loc2
                    if mean2 > mean1:
                        loc_upper = loc2
                        loc_lower = loc1
                    arraw_color = "blue"
                    related_val = (
                        matrix_corr[i, j] - corr_criteria
                    ) / (1.0 - corr_criteria)
                    alpha = min(0.6 * related_val + 0.4, 1.0)
                    lw = 1.0
                    lw = 0.6 * math.pow(related_val, 0.5) + 0.4
                    message = ""
                    # message = "{}".format(round(matrix_corr[i, j], 2))
                    plt.annotate(
                        message,
                        xy=loc_lower,
                        xytext=loc_upper,
                        xycoords="data",
                        textcoords="data",
                        arrowprops=dict(
                            arrowstyle=arrowstyle,
                            connectionstyle=connectionstyle,
                            facecolor=arraw_color,
                            edgecolor=arraw_color,
                            lw=lw,
                        ),
                        ha="center",
                        va="center",
                        fontsize=9,
                        alpha=alpha,
                    )

                    if log_debug:
                        dirc = "-->"
                        if mean1 >= mean2:
                            dirc = "-->"
                        else:
                            dirc = "<--"

                        root_logger.debug(
                            "  -- {} {} {} {} {}".format(
                                sid1, sname1, dirc, sid2, sname2
                            )
                        )
                        root_logger.debug(
                            "     {} {} {} {} {}".format(
                                matrix_distance[i, j],
                                matrix_corr[i, j],
                                alpha,
                                mean1,
                                mean2,
                            )
                        )
    return ax


class shp_operation:
    """
    專門用於 shape file 的操作
    """

    def __init__(
        self,
        shp_fname: str,
        set_crs: str = "EPSG:3826",  # TWD97 121
        **kwargs,
    ):
        """
        Loading shape file
        """
        root_logger = kwargs.get("root_logger", None)
        # pylint: disable=broad-except
        self.gdf_shp: gpd.GeoDataFrame = gpd.GeoDataFrame([])
        try:
            encoding = "utf-8"
            self.gdf_shp = gpd.read_file(
                shp_fname, encoding=encoding
            )
        except UnicodeDecodeError:
            # 嘗試不同的編碼
            encoding = "big5"
            self.gdf_shp = gpd.read_file(
                shp_fname, encoding=encoding
            )
        except FileNotFoundError as e:
            if root_logger is not None:
                root_logger.error(
                    "!!! {} doest not exists!".format(shp_fname),
                    exc_info=True,
                )
            raise FileNotFoundError(
                "!!! {} doest not exists!".format(shp_fname)
            ) from e

        try:
            self.gdf_shp.crs = set_crs
            if root_logger is not None:
                root_logger.debug("CRS: {}".format(set_crs))
            if set_crs == "EPSG:3828":
                """
                重新設定 TWD67 121 參數
                """
                TWD67_121_str = "+proj=tmerc +lat_0=0 +lon_0=121 +k=0.9999 +x_0=250000 "
                TWD67_121_str += "+y_0=0 +ellps=GRS67 +towgs84=-752,-358,-179,-0.0000011698,"
                TWD67_121_str += "0.0000018398,0.0000009822,0.00002329 +units=m +no_defs"
                # set_crs = pyproj.Proj(TWD67_121_str, preserve_units=False)

                # 重新設定 TWD67 121 參數
                crs_twd67_121 = CRS.from_string(TWD67_121_str)
                self.gdf_shp.crs = crs_twd67_121
                if root_logger is not None:
                    root_logger.debug(
                        "  --> Re-setting TWD67 121"
                    )
        except AttributeError:
            # 如果檔案不存在
            pass
        if root_logger is not None:
            root_logger.debug(
                "           CRS before assignment: {}".format(
                    self.gdf_shp.crs
                )
            )

    def add_backtrace_line(self):
        """
        僅適用於 LineString 型別, 原本如果為 pa -> pb -> pc 順序的線段
        變成 pa -> pb -> pc -> pb -> pc
        由於 LineString 在繪圖時， Geopandas 似乎會轉換為 Polygon 來呈現
        因此會額外加入 pc -> pa 的封閉線段
        """
        if self.gdf_shp.shape[0] > 0:
            # 只適用於 LineString
            assert (
                self.gdf_shp.loc[
                    self.gdf_shp.index[0], "geometry"
                ].geom_type
                == "LineString"
            )

        def add_reverse_points(geometry: LineString) -> List:
            assert geometry.geom_type == "LineString"
            coords = [list(p) for p in geometry.coords]
            coords2 = coords.copy()

            if not shapely.is_closed(geometry):
                # 非封閉, 反向回溯
                coords.reverse()
                coords2 += coords  # 反向回溯點
            return coords2

        for j in range(self.gdf_shp.shape[0]):
            index = self.gdf_shp.index[j]
            geometry = self.gdf_shp.loc[index, "geometry"]

            if geometry.geom_type in ["LineString"]:
                self.gdf_shp.loc[index, "geometry"] = LineString(
                    add_reverse_points(
                        self.gdf_shp.loc[index, "geometry"]
                    )
                )
            elif geometry.geom_type in ["MultiLineString"]:
                # 分段加入
                coords = []
                for line in geometry.geoms:
                    coords.append(add_reverse_points(line))
                self.gdf_shp.loc[index, "geometry"] = (
                    MultiLineString(coords)
                )
            else:
                raise TypeError(
                    "尚未建立 '{}' 之處理流程 / {}: {}".format(
                        geometry.geom_type,
                        index,
                        self.gdf_shp.loc[index, "name"],
                    )
                )

    def plot(
        self,
        ax=None,
        tics_limit_layout=None,
        **kwargs,
    ):
        # 檢查過時用法
        jut.check_outdated_IO(
            "tics_limit_layout",
            "請改用 extent 設定繪製範圍",
            **kwargs,
        )
        root_logger = kwargs.get("root_logger", None)
        if (
            self.gdf_shp.loc[
                self.gdf_shp.index[0], "geometry"
            ].geom_type
            == "LineString"
        ):
            """
            僅適用於 LineString 型別, 原本如果為 pa -> pb -> pc 順序的線段
            變成 pa -> pb -> pc -> pb -> pc
            由於 LineString 在繪圖時， Geopandas 似乎會轉換為 Polygon 來呈現
            因此會額外加入 pc -> pa 的封閉線段
            """
            self.add_backtrace_line()

        if root_logger is not None:
            root_logger.debug(kwargs)
        try:
            plot_kwargs = {
                "ax": ax,
                **{
                    flag: elem
                    for flag, elem in kwargs.items()
                    if flag
                    in [
                        "edgecolor",
                        "facecolor",
                        "linestyle",
                        "linewidth",
                        "alpha",
                        "marker",
                    ]  # 排除 root_logger
                },
            }
            if "extent" not in kwargs:
                # 無需做任何處理
                self.gdf_shp.plot(**plot_kwargs)
            else:
                # 設定邊界
                """
                # 原本做法, 是 select 符合框內的 polygon or LineString 等
                # self.tics_limit_layout = [list(ll_loc), list(ur_loc)]
                tics_polygon = tics2polygon(tics_limit_layout)
                mask = [
                    check_line_in_region(
                        tics_polygon,
                        self.gdf_shp.loc[index, "geometry"],
                        **kwargs,
                    )
                    for index in self.gdf_shp.index
                ]
                gdf_shp2 = self.gdf_shp.loc[mask, :]
                """
                # 以 extent 作為外框, clip 原有的 geometry
                # 原本格式 -->        新的格式
                # Point              Point
                # LineString         MultiLineString
                # MultiLineString    MultiLineString
                # Polygon            MultiLineString
                gdf_shp2 = clip_gpd_in_extent(
                    self.gdf_shp, **kwargs
                )
                gdf_shp2.plot(**plot_kwargs)

        except IndexError as e:
            message = "{}".format(kwargs.keys())
            if root_logger is not None:
                root_logger.error(message, exc_info=True)
            raise IndexError(message) from e

        return ax

    def check_shapely_point_in_polygon(
        self, point: Point, **kwargs
    ) -> Tuple:
        """
        point 為 shapely.geometry.point.Point 格式
        X & Y 為 float 格式
        兩者需選一
        確認 point 是否位於 polygon 中間
        回傳對應的 polygon index, 是否完成查詢
        """
        root_logger = kwargs.get("root_logger", None)
        if root_logger is not None:
            root_logger.debug("")
        for index in self.gdf_shp.index:
            # 逐一檢查 polygon 是否包含 point
            geometry = self.gdf_shp.loc[index, "geometry"]
            if geometry.geom_type.find("Polygon") >= 0:
                if root_logger is not None:
                    root_logger.debug(
                        "{}. point={}, polygon={}".format(
                            index, point, geometry
                        )
                    )
                    root_logger.debug(
                        "  --> {}".format(
                            geometry.contains(point)
                        )
                    )
                if geometry.contains(point):
                    return index, True
        return None, False

    def check_XY_in_polygon(self, XY: Tuple, **kwargs) -> Tuple:
        """
        point 為 shapely.geometry.point.Point 格式
        X & Y 為 float 格式
        兩者需選一
        確認 point 是否位於 polygon 中間
        回傳對應的 polygon index, 是否完成查詢
        """
        assert isinstance(XY, tuple)
        assert len(XY) == 2
        for val in XY:
            try:
                assert isinstance(val, (int, float, np.float32))
            except AssertionError as e:
                message = "{} / {}".format(val, type(val))
                if kwargs.get("root_logger", None) is not None:
                    kwargs["root_logger"].debug(
                        message, exc_info=True
                    )
                raise AssertionError(message) from e
        point = Point(*XY)
        return self.check_shapely_point_in_polygon(
            point, **kwargs
        )

    def check_point_in_polygon(
        self,
        X=None,
        Y=None,
        point: Optional[Point] = None,
        **kwargs,
    ) -> Tuple:
        """
        point 為 shapely.geometry.point.Point 格式
        X & Y 為 float 格式
        兩者需選一
        確認 point 是否位於 polygon 中間
        回傳對應的 polygon index, 是否完成查詢
        """

        log_xy = (X is not None) and (Y is not None)
        log_point = point is not None

        # 兩者擇一
        try:
            assert not (log_xy or log_point)
        except AssertionError:
            return None, False

        if log_xy:
            return self.check_XY_in_polygon((X, Y), **kwargs)
        else:
            # log_point
            return self.check_shapely_point_in_polygon(
                point, **kwargs
            )

    def clip_nc_result(
        self, nc_fname: str, **kwargs
    ) -> np.ndarray:
        """
        輸入 nc file name
            --> Y, X, band
        產生 polygon 內的 mask
        """
        root_logger = kwargs.get("root_logger", None)
        ylist, xlist, band1 = NA.read_ncband(nc_fname)

        if not kwargs.get("log_parallel", False):
            # sequence
            mask = np.full(band1.shape, True)
            for j, y in enumerate(ylist):
                for i, x in enumerate(xlist):
                    index, log = self.check_point_in_polygon(
                        X=x, Y=y
                    )
                    mask[j, i] = log
        else:
            # parallel
            mask = []
            cpu_item = get_cpu_info()
            # 計算可用的 CPU 數量
            process_size = max(
                int(
                    float(cpu_item["count"])
                    * kwargs.get("processor_ratio", 0.4)
                ),
                1,
            )
            for _j, y in enumerate(ylist):
                flags = [
                    (
                        self,
                        x,
                        y,
                    )
                    for _, x in enumerate(xlist)
                ]
                if _j % 100 == 0:
                    if root_logger is not None:
                        root_logger.debug(
                            "  --> Processing {}/{}=({}%)".format(
                                _j,
                                len(ylist),
                                round(
                                    float(_j) / len(ylist) * 100,
                                    2,
                                ),
                            )
                        )
                with Pool(processes=process_size) as pool:
                    result = pool.map(
                        check_point_in_polygon_shell, flags
                    )
                    pool.close()  # Close the pool to not create new process
                    pool.join()  # Make main process to wait for the pool
                mask.append(result)
            mask = np.array(mask)
        return mask

    def associated_db_from_point(
        self, flagname: str, X=None, Y=None, point=None
    ):
        """
        point 為 shapely.geometry.point.Point 格式
        X & Y 為 float 格式
        兩者需選一
        確認 point 是否位於 polygon 中間
        回傳對應的 polygon 的資料內容, 是否完成查詢
        """
        log_xy = (X is not None) and (Y is not None)
        log_point = point is not None
        # 兩者擇一
        if not (log_xy or log_point):
            return None, False

        if log_xy:
            point = Point(X, Y)

        index, log = self.check_point_in_polygon(point=point)
        if log:
            # pylint: disable=no-else-return
            if flagname == "all":
                return self.gdf_shp.loc[
                    index, self.gdf_shp.columns[:-1]
                ]
            else:
                return self.gdf_shp.loc[index, flagname]
        return None


# pylint: disable=too-few-public-methods
class stat_gpd:
    """
    專門處理站井資訊
    """

    # pylint: disable=dangerous-default-value
    def __init__(
        self,
        stat_fname2: str,
        columns: List,
        set_crs=None,
        tic_lim=[[None, None], [None, None]],
        filter_code: str = "",
        layer_name=None,
        # log_debug: bool = False,
    ):

        # print (pd.__version__, np.__version__, gpd.__version__)
        try:
            df_stat = pd.read_hdf(
                stat_fname2.replace(".csv", ".h5")
            )
        except FileNotFoundError as e:
            raise FileNotFoundError(
                "'{}'".format(stat_fname2)
            ) from e
        # except:
        #    df_stat = pd.read_csv(
        #        stat_fname2.replace(".h5", ".csv")
        #    )

        self.columns = columns
        try:
            # 型別改變
            df_stat = df_stat.sort_values(
                by=[columns[1], columns[2]]
            )
            df_stat = df_stat[df_stat[columns[1]] > 0]
            df_stat = df_stat[df_stat[columns[2]] > 0]
            df_stat = df_stat.astype({columns[1]: "float"})
            df_stat = df_stat.astype({columns[2]: "float"})

            # 針對 filter code 進行篩選
            try:
                df_stat2 = gts.df_filter_condition(
                    df_stat,
                    filter_code,
                    layer_name=layer_name,
                    # log_debug=log_debug,
                )
                assert df_stat2.shape[0] > 0
                df_stat = df_stat
            except AssertionError as e:
                # 篩選不到內容, 回傳 None
                raise AssertionError(
                    "Filter code: {} from {} / {}".format(
                        filter_code,
                        df_stat.head(),
                        df_stat.columns,
                    )
                ) from e

            # 針對座標篩選
            if tic_lim[0][0] is not None:
                try:
                    df_stat = df_stat[
                        df_stat[columns[1]] >= tic_lim[0][0]
                    ]
                except TypeError as e:
                    raise TypeError(
                        "column: {} / {} / {}".format(
                            columns[1],
                            type(columns[1]),
                            df_stat.columns,
                        )
                    ) from e

            if tic_lim[1][0] is not None:
                df_stat = df_stat[
                    df_stat[columns[1]] < tic_lim[1][0]
                ]
            if tic_lim[0][1] is not None:
                df_stat = df_stat[
                    df_stat[columns[2]] >= tic_lim[0][1]
                ]
            if tic_lim[1][1] is not None:
                df_stat = df_stat[
                    df_stat[columns[2]] < tic_lim[1][1]
                ]

            self.gdf_shp = gpd.GeoDataFrame(
                df_stat,
                geometry=[
                    Point(xy)
                    for xy in zip(
                        df_stat[columns[1]].values,
                        df_stat[columns[2]].values,
                    )
                ],
            )

        except KeyError as e:
            print(" -- Columns: {}".format(df_stat.columns))
            print(
                "    缺乏欄位: {} or {}".format(
                    columns[1], columns[2]
                )
            )
            print("    請檢討欄位")
            jut.warning_message2(e)
            sys.exit()

        if set_crs is not None:
            self.gdf_shp.crs = set_crs

    # pylint: disable=too-many-branches
    def plot_stat(
        self,
        ax=None,
        # cmap=None,
        # color=None,
        # edgecolor=None,
        # facecolor=None,
        # linewidth=None,
        # markersize=None,
        # marker=None,
        # fontsize=None,
        label_col=None,
        offset=(20, 20),
        log_label_forced_stop: bool = False,
        layout_crs: str = "epsg:3826",
        log_debug: bool = False,
        **kwargs,
    ):
        if layout_crs.lower() != str(self.gdf_shp.crs).lower():
            if log_debug:
                print(
                    "           to_crs for layout: {}".format(
                        layout_crs
                    )
                )
            self.gdf_shp.to_crs(
                epsg=int(layout_crs.replace("epsg:", ""))
            )

        self.gdf_shp.plot(
            ax=ax,
            # cmap=cmap,
            # color=color,
            # edgecolor=edgecolor,
            # facecolor=facecolor,
            # linewidth=linewidth,
            # markersize=markersize,
            # marker=marker,
            **{
                key: elem
                for key, elem in kwargs.items()
                if key
                not in [
                    "fontsize",
                    "set_crs",
                    "filter_code",
                    "layer_name",
                ]
            },
        )
        if (not log_label_forced_stop) and (
            label_col is not None
        ):
            # 輸出 label
            for i in range(self.gdf_shp.shape[0]):
                index = self.gdf_shp.index[i]
                message = "{}".format(
                    self.gdf_shp.loc[index, label_col]
                )
                ###########################################################
                # debug
                xy = tuple(
                    add_tuple(
                        list(
                            self.gdf_shp.loc[
                                index, self.columns[1:3]
                            ].values
                        ),
                        offset,
                    )
                )

                if log_debug:
                    if i < 5:
                        print(
                            "      Annotate: {} locate at {}".format(
                                message, xy
                            )
                        )
                ax.annotate(
                    message,
                    xy=xy,
                    xytext=xy,
                    xycoords="data",
                    textcoords="data",
                    va="top",
                    fontsize=kwargs.get("fontsize", None),
                    color=kwargs.get("color", None),
                )
        return ax


class shp_project:
    """
    shp project
    用以紀錄不同圖層與對應的處置
    """

    def __init__(self, proj_name=None):
        self.extent = None  # Layout 的範圍
        self.layer_infom = None
        self.proj_name = proj_name

    def add_layer(
        self,
        shp_fname: str,
        shp_type: str,
        **kwargs,
    ):
        if self.layer_infom is None:
            self.layer_infom = []

        self.layer_infom.append(
            [
                shp_fname,
                shp_type,
                kwargs,
            ]
        )

    def add_gw_rain_infil(
        self,
        matrix_r2,
        myGTSM,
        matrix_distance,
        vname1,
        vname2,
        arraw_color=None,
        log_debug: bool = False,
    ):
        log_gw_rain_infil = False
        mat_gw_rain = [-99999, -99999]
        # flow_network_index = None
        for i in range(len(self.layer_infom)):
            if self.layer_infom[i][0][1].find("cross") >= 0:
                log_gw_rain_infil = True

                print(type(self.layer_infom))
                if self.layer_infom[i][0][1] == "cross_gw":
                    # 地下水
                    mat_gw_rain[0] = i
                elif self.layer_infom[i][0][1] == "cross_rain":
                    # 降雨量
                    mat_gw_rain[1] = i

        mat = [
            matrix_r2,
            "gw_rain_infil",
            myGTSM,
            matrix_distance,
            vname1,
            vname2,
            mat_gw_rain,
            arraw_color,
            log_debug,
        ]
        if log_gw_rain_infil:
            self.layer_infom.append(mat)

    def add_flow_network(
        self,
        my_GTS,
        corr_criteria: float,
        distance_criteria: float,
        df_describe,
        log_debug: bool = False,
    ):
        log_flow_network = False
        flow_network_index = None
        for i in range(len(self.layer_infom)):
            if self.layer_infom[i][1] == "flow_network":
                log_flow_network = True
                flow_network_index = i
                break

        mat = [
            my_GTS,
            "flow_network",
            corr_criteria,
            distance_criteria,
            df_describe,
            log_debug,
        ]
        if not log_flow_network:
            self.layer_infom.append(mat)
        else:
            self.layer_infom[flow_network_index] = mat

    # pylint: disable=too-many-branches
    def plot_multi(
        self,
        figsize=(9, 6),
        style: str = "seaborn-dark",
        fig_fname=None,
        fig_title=None,
        log_label_forced_stop: bool = False,
        offset=(20, 20),
        shp_filter: str = "",
        layout_crs: str = "epsg:3826",
        log_chl: bool = False,
        log_debug: bool = False,
        **kwargs,
    ):
        root_logger = kwargs.get("root_logger", None)
        layer_list = []
        # if log_chl:
        #    # 中文圖型, 設定
        #    plt.rcParams["font.sans-serif"] = ["simhei"]
        #    plt.rcParams["axes.unicode_minus"] = False
        plt.style.use(style)
        _fig, ax = plt.subplots(1, figsize=figsize)
        for i in range(len(self.layer_infom)):
            if root_logger is not None:
                root_logger.debug(
                    "{}. File name: {}".format(
                        i,
                        self.layer_infom[i][0],
                    )
                )
                root_logger.debug(
                    "   -----> type: {}".format(
                        self.layer_infom[i][1],
                    )
                )

            # if log_debug:
            if self.layer_infom[i][1] in [
                "shp line",
                "XY point",
            ]:
                if root_logger is not None:
                    root_logger.debug(
                        "   -----> CRS: {}".format(
                            self.layer_infom[i][2]["set_crs"],
                        )
                    )
            if self.layer_infom[i][1].find("shp") >= 0:
                # loading & plot
                layer_list.append(
                    shp_operation(
                        self.layer_infom[i][0],
                        log_debug=log_debug,
                        **{
                            key: elem
                            for key, elem in self.layer_infom[i][
                                2
                            ].items()
                            if key in ["set_crs"]
                        },
                    )
                )
                try:
                    ax = layer_list[-1].plot(
                        ax=ax,
                        log_debug=log_debug,
                        tics_limit_layout=self.tics_limit_layout,
                        **self.layer_infom[i][2],
                        **kwargs,
                    )
                except AttributeError:
                    # 因為 gdf_shp 不存在
                    pass

            elif self.layer_infom[i][1] == "XY point":
                # label_col = self.layer_infom[i][2]["label_col"]
                # if log_label_forced_stop:  # 強制關閉 label 輸出
                #    label_col = None

                stat_fname = None
                if isinstance(self.layer_infom[i][0], str):
                    stat_fname = self.layer_infom[i][0]
                elif isinstance(
                    self.layer_infom[i][0], (list, tuple)
                ):
                    stat_fname = self.layer_infom[i][0][0]

                # 加入刪除條件
                filter_code = self.layer_infom[i][2].get(
                    "filter_code", ""
                )
                if filter_code != "":
                    filter_code += "&"
                filter_code += shp_filter
                my_stat_gpd = stat_gpd(
                    str(stat_fname),
                    self.layer_infom[i][2]["columns"],
                    filter_code=filter_code,
                    tic_lim=self.tics_limit_layout,
                    **{
                        key: elem
                        for key, elem in self.layer_infom[i][
                            2
                        ].items()
                        if key
                        in ["set_crs", "layer_name", "set_crs"]
                    },
                )
                ax = my_stat_gpd.plot_stat(
                    ax=ax,
                    offset=offset,
                    # shp_filter=shp_filter,
                    layout_crs=layout_crs,
                    log_debug=log_debug,
                    **{
                        key: elem
                        for key, elem in self.layer_infom[i][
                            2
                        ].items()
                        if key not in ["alpha", "columns"]
                    },
                )
            elif self.layer_infom[i][1] == "flow_network":
                arg = (
                    self.layer_infom[i][0],
                    self.layer_infom[i][2],
                    self.layer_infom[i][3],
                    self.layer_infom[i][4],
                    self.tics_limit_layout,
                    self.layer_infom[i][5],
                )
                root_logger.debug("   -----> {}".format(arg))
                ax = plot_flow_network(
                    ax,
                    *arg,
                    **kwargs,
                )
            elif self.layer_infom[i][1] == "gw_rain_infil":
                arg = (
                    self.layer_infom[i][0],
                    self.layer_infom[i][2],
                    self.layer_infom[i][3],
                    layer_list[self.layer_infom[i][6][0]],
                    layer_list[self.layer_infom[i][6][1]],
                    self.layer_infom[i][4],
                    self.layer_infom[i][5],
                    self.layer_infom[i][7],
                    self.tics_limit_layout,
                    self.layer_infom[i][8],
                )
                root_logger.debug("   -----> {}".format(arg))
                ax = plot_gw_rain_infil(
                    ax,
                    *arg,
                    **kwargs,
                )

        assert isinstance(self.tics_limit_layout, list) or (
            self.tics_limit_layout is None
        )
        if isinstance(self.tics_limit_layout, list):

            def get_loc_lim(
                loc: Union[List, None], axis_index: int
            ) -> Union[int, float, None]:
                """
                傳入座標點 (X, Y) 或 None
                axis_index 為 座標編號
                    0: X
                    1: Y

                取得座標值
                """
                assert isinstance(loc, list) or (loc is None)
                assert axis_index in [0, 1]

                if loc is None:
                    return None
                else:
                    assert len(loc) == 2
                    return loc[axis_index]

            def axis_lim(tics_limit_layout):
                """
                tics_limit_layout = [
                    [X_ll, Y_ll],   # loc of ll
                    [X_ur, Y_ur],   # loc of ur
                ]
                loc of ur & loc of ll 可以是 None
                則不設定
                """
                if isinstance(tics_limit_layout, list):
                    assert (
                        len(tics_limit_layout) == 2
                    )  # locs of ll and ur
                    ax.set_xlim(
                        [
                            get_loc_lim(
                                self.tics_limit_layout[
                                    loc_index
                                ],  # for ll and ur
                                0,
                            )
                            for loc_index in range(2)
                        ]
                    )
                    ax.set_ylim(
                        [
                            get_loc_lim(
                                self.tics_limit_layout[
                                    loc_index
                                ],  # for ll and ur
                                1,
                            )
                            for loc_index in range(2)
                        ]
                    )

        if fig_title is not None:
            ax.set_title(fig_title)
        elif self.proj_name is not None:
            ax.set_title(self.proj_name)
        ax.set_xlabel(r"X")
        ax.set_ylabel(r"Y")

        if root_logger is not None:
            root_logger.debug("# Adding basement")
            root_logger.debug(os.environ["PROJ_LIB"])
            root_logger.debug(
                fut.search_files_in_dir(
                    os.environ["PROJ_LIB"], regular_flag="proj"
                )
            )
        if layer_list[0].gdf_shp.crs.to_string() not in [
            "EPSG:3826"
        ]:
            try:
                source = ctx.providers.Stamen.Terrain
                # source = ctx.providers.Stamen.TonerLite
                # source = ctx.providers.OpenStreetMap.Mapnik
                try:
                    ctx.add_basemap(
                        ax,
                        crs=layer_list[
                            0
                        ].gdf_shp.crs.to_string(),
                        source=source,  # Background,
                    )
                except rasterio.errors.CRSError as e:
                    raise rasterio.errors.CRSError(
                        "{} / {}".format(
                            layer_list[
                                0
                            ].gdf_shp.crs.to_string(),
                            source,
                        )
                    ) from e
            except requests.HTTPError as e:
                # 如果是這個錯誤, 不繪製底圖
                jut.warning_message2(e)

        # plt.tight_layout()
        if fig_fname is None:
            plt.show()
        else:
            jut.save_fig(fig_fname)
            if root_logger is not None:
                root_logger.debug(
                    "   |--  Save figure to {}:".format(
                        fig_fname + ".png"
                    )
                )
