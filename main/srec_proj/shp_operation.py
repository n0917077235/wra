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
import numpy as np

# import inspect
from pyproj import CRS  # pylint: disable=no-name-in-module

import jutility as jut


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

