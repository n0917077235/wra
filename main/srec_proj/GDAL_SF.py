"""
GDAL superfile & utility
"""

# -*- coding: utf-8 -*-
import os
import sys
from typing import List, Tuple, Dict, Optional
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import json
import re
import math
import shapely

sys.path.append(os.path.join("..", "..", "jlib", "srcs"))
import gdal_utility as gdut
import file_utility as fut
import jutility as jut
import encode_utility as eut
import shp_operation as shp_op
import tif_operation as tif_op
import jlib_logging
import parallel_framework as pf
import cell_inform as CI
import group_ts
import polar_coord

# pylint: disable=unused-import
import plt_parameters  # noqa=C901


def tif_extra_pre_plot(
    ax, tif_extra_pre: Optional[Dict] = None, **kwargs
):
    """
    如果有設定額外的 tif 底圖資訊, 進行繪製
    底圖, 所以在最底層, 一開始就要處理
    """
    root_logger = kwargs.get("root_logger", None)

    if tif_extra_pre is not None:
        # load multiple TIFFs
        # merge
        my_tms = tif_op.tif_meta_system(
            tif_extra_pre["tif_flist"],
            trim_extent=tif_extra_pre["extent"],
            root_logger=root_logger,
        )
        # Plot
        my_tms.tif_multi_plot(
            ax,
            alpha=tif_extra_pre["alpha"],
            **{
                flag: elem
                for flag, elem in kwargs.items()
                if flag in ["grid", "fig_title"]
            },
        )


def determine_annotate_param(
    line: shapely.LineString, label: str
) -> List:
    """
    輸出 annotate_param
        [座標, label, 角度]
    """
    annotate_param = [[], [], []]
    # 標在LineString 的節點
    index_center = int(len(line.coords) / 2)
    annotate_param[0] = tuple(line.coords[index_center][:2])
    annotate_param[1] = label

    # 計算極座標角度
    my_polor = polar_coord.Polar_coord(annotate_param[0][:2])
    try:
        annotate_param[2] = (
            my_polor.cart_to_polar(
                tuple(line.coords[index_center + 5])[:2]
            )[1]
            / math.pi
            * 180
        )
    except IndexError:
        annotate_param[2] = (
            my_polor.cart_to_polar(
                tuple(line.coords[index_center + 1])[:2]
            )[1]
            / math.pi
            * 180
        )

    if (annotate_param[2] > 90) and (annotate_param[2] <= 270):
        annotate_param[2] -= 180
    return annotate_param


def random_select(rval, data_list: List) -> List:
    """
    隨機挑選
    """
    data_size = len(data_list)
    rval2 = np.floor(rval * data_size)  # 無條件捨去
    rval2 = np.where(
        rval2 == len(data_list),
        data_size - 1,
        rval2,
    ).astype(int)
    data_list2 = [data_list[r] for r in rval2]
    return data_list2


def gis_extra_post_plot(
    ax, gis_extra_post: Optional[List] = None, **kwargs
):
    """
    如果有設定額外的 gis 資訊, 進行繪製
    """
    cell_inform = kwargs["cell_inform"]

    if gis_extra_post is not None:
        assert isinstance(gis_extra_post, list)
        for gis_extra_content in gis_extra_post:
            try:
                assert len(gis_extra_content) == 5
            except AssertionError as e:
                raise AssertionError(gis_extra_content) from e
            my_shp = shp_op.shp_operation(
                gis_extra_content[0],
                set_crs="EPSG:3826",
                **kwargs,
            )
            my_shp.plot(
                ax=ax,
                edgecolor=gis_extra_content[1],
                facecolor="none",
                linestyle=gis_extra_content[2],
                linewidth=gis_extra_content[3],
                alpha=gis_extra_content[4],
                extent=cell_inform.extent,
                **kwargs,
            )
            """ # 備註, debug, 直接以 cell_inform 輸入
            tics_limit_layout=[
                [
                    np.min(cell_inform.xedges),
                    np.min(cell_inform.yedges),
                ],
                [
                    np.max(cell_inform.xedges),
                    np.max(cell_inform.yedges),
                ],
            ],
            """

            if kwargs.get("log_label", False):
                # 如為 True, 標註 shp 中欄位名稱
                # 篩選欄位, 只輸出篩選後的結果
                gdf_shp = group_ts.df_filter_condition(
                    my_shp.gdf_shp,
                    kwargs.get("label_filter_code", ""),
                    layer_name=gis_extra_content[0],
                )

                # 解析 label 的描述設定
                label_columns = kwargs.get("label_columns", None)

                label_columns2 = []
                if label_columns is not None:
                    label_columns2 = [
                        lc.split("::")
                        for lc in label_columns.split("&")
                    ]
                    for i, lc in enumerate(label_columns2):
                        if len(lc) == 0:
                            label_columns2[i] = [""] + lc

                for lc in label_columns2:
                    log_matched = True
                    if lc[0] != "":
                        # 檔案是否吻合
                        log_matched = (
                            lc[0] == gis_extra_content[0]
                        )
                    if log_matched:
                        # 檔案與輸出設定吻合
                        for index in gdf_shp.index:
                            label = gdf_shp.loc[index, lc[1]]
                            annotate_params = []
                            # 幾何形狀
                            geometry = gdf_shp.loc[
                                index, "geometry"
                            ]
                            if geometry.geom_type == "Point":
                                annotate_params.append(
                                    [
                                        tuple(geometry.coords),
                                        label,
                                        0,
                                    ]
                                )
                            elif (
                                geometry.geom_type
                                == "LineString"
                            ):
                                annotate_params.append(
                                    determine_annotate_param(
                                        geometry, label
                                    )
                                )
                            elif (
                                geometry.geom_type
                                == "MultiLineString"
                            ):
                                locs = []
                                for line in geometry.geoms:
                                    annotate_params.append(
                                        determine_annotate_param(
                                            line, label
                                        )
                                    )

                                # 挑選對應的數值
                                rval = np.random.uniform(0, 1, 1)
                                annotate_params = random_select(
                                    rval, annotate_params
                                )
                            elif geometry.geom_type == "Polygon":
                                annotate_params.append(
                                    [
                                        tuple(
                                            geometry.centroid.coords
                                        ),
                                        label,
                                        0,
                                    ]
                                )

                            for (
                                annotate_param
                            ) in annotate_params:
                                try:
                                    ax.annotate(
                                        "{}".format(
                                            annotate_param[1]
                                        ),
                                        xy=annotate_param[0][:2],
                                        xytext=annotate_param[0][
                                            :2
                                        ],
                                        xycoords="data",
                                        alpha=0.7,
                                        fontsize=10,
                                        color=gis_extra_content[
                                            1
                                        ],
                                        rotation=annotate_param[
                                            2
                                        ],
                                    )
                                except ValueError as e:
                                    raise ValueError(
                                        "{} / {} / {}".format(
                                            gdf_shp.loc[
                                                index,
                                                [
                                                    "name",
                                                    "length",
                                                ],
                                            ],
                                            annotate_param[0],
                                            geometry.geom_type,
                                        )
                                    ) from e

    return ax


# pylint: disable=dangerous-default-value
def annotate_extra_post_plot(
    ax, annotate_extra_posts: List = [], **_kwargs
):
    """
    如果有設定額外的 gis 資訊, 進行繪製
    """

    for annotate_extra_post in annotate_extra_posts:
        ax.annotate(
            annotate_extra_post["text"],
            **{
                flag: elem
                for flag, elem in annotate_extra_post.items()
                if flag not in ["text"]
            },
        )
    return ax


def plot_content(
    soil_content,
    fig_fname: str,
    fig_title: str,
    plot_params: pd.DataFrame,
    **kwargs,
):
    """
    土壤圖繪圖
    """
    plt_parameters.assign_fig_detail(None, "fig_style", **kwargs)
    _fig, ax = plt.subplots(
        1, figsize=kwargs.get("figsize", (9, 8))
    )
    # tif_extra_pre_plot(ax, **kwargs)
    ax = soil_content.plot_GIS_data(
        plot_params,
        ax,
        fig_title=fig_title,
        ticks=kwargs["ticks"],
        ticklabels=kwargs["ticklabels"],
        log_inverse_yaxis=True,
    )
    # 如有額外定義圖匡, 進行繪製
    gis_extra_post_plot(ax, **kwargs)

    if "background_shp_list" in kwargs:
        for i in range(len(kwargs["background_shp_list"])):
            kwargs["background_shp_list"][i].plot(
                ax=ax,
                edgecolor=get_list_value(kwargs["edgecolor"], i),
                facecolor=get_list_value(kwargs["facecolor"], i),
                linewidth=get_list_value(kwargs["linewidth"], i),
                alpha=get_list_value(kwargs["alpha"], i),
            )
    ax.grid()
    plt.tight_layout()
    jut.save_fig(fig_fname, dpi=300)


'''
2024/02/03 已改為 parallel_framework.parallel_process_framework 處理
def parallel_process_framework(
    func,
    flags: List,
    log_parallel_process: bool,
    process_size: int,
) -> List:
    """
    平行或循序計算
    """
    return_list = []
    if log_parallel_process:
        with Pool(processes=process_size) as pool:
            return_list = pool.map(func, flags)
            pool.close()  # Close the pool to not create new process
            pool.join()  # Make main process to wait for the pool
    else:
        return_list = []
        for flag in flags:
            return_list.append(func(flag))
    return return_list
'''


def determine_pair(
    string_in: str, condition1: List, condition2: List
):
    """
    從 string_in內, 找出 condition1 & condition2 內符合的子字串
    """
    assert isinstance(string_in, str)
    assert isinstance(condition1, list)
    assert isinstance(condition2, list)

    def search_matched_pattern(
        string_in: str, conditions: List
    ) -> List:
        for elem in conditions:
            if string_in.find(elem) >= 0:
                return [elem]
        return []

    pair_match = search_matched_pattern(
        string_in, condition1
    ) + search_matched_pattern(string_in, condition2)
    return pair_match


def get_list_value(list_vals: List, index: int):
    """
    傳入一個 List 與 index
    回傳對應 index 位置之數值
    if index > len(List):
        return None
    """
    return_val = None
    try:
        return_val = list_vals[index]
    except IndexError:
        return None
    return return_val


def get_fname(data_path: str, flags: List) -> pd.DataFrame:
    """
    找出 data_path 路徑下, 符合 flags 搜尋條件的所有檔案
    1. 符合 flags 條件
    2. 副檔名為 .shp

    flags 存在多個條件, 回傳結果是多個條件下的聯集

    2023/04/29 將回傳值從 List, 改為 pd.DataFrame
    內容為 two columns
        flag & fname
    """
    assert isinstance(data_path, str)
    assert isinstance(flags, list)
    assert len(flags) > 0

    mat: List = []
    for flag in flags:
        flist = fut.filter_search_result(
            fut.search_files_in_dir(
                data_path, log_recursive=True, regular_flags=flag
            ),
            regular_flags=".shp$",
        )
        for fname in flist:
            mat.append([flag, fname])
    return pd.DataFrame(mat, columns=["flag", "fname"])


def define_vlim(
    sepline: List,
) -> Dict:
    vlim_params = {
        0: "vmin",
        1: "vmax",
    }
    vlim_result: Dict[str, Optional[float]] = {}
    for (
        key,
        elem,
    ) in vlim_params.items():
        try:
            vlim_result[elem] = float(sepline[key])
        except ValueError:
            vlim_result[elem] = None
    return vlim_result


def annotate_extra_post_analysis(var: str) -> Dict:
    """
    annotate_extra_post 設定
    輸入為 str
    輸出為 Dict
    """

    try:
        my_dict = json.loads(var)
    except json.decoder.JSONDecodeError as e:
        raise TypeError(var) from e
    for key in ["", "text"]:
        my_dict["xy{}".format(key)] = (
            my_dict["x{}".format(key)],
            my_dict["y{}".format(key)],
        )
        del my_dict["x{}".format(key)]
        del my_dict["y{}".format(key)]
    if "arrowprops" in my_dict:
        # Expecting property name enclosed in double quotes: line 1 column 2 (char 1)
        # 將 ' 符號改為 "
        my_dict["arrowprops"] = my_dict["arrowprops"].replace(
            "'", '"'
        )
        my_dict["arrowprops"] = json.loads(my_dict["arrowprops"])
    return my_dict


# pylint: disable=too-many-instance-attributes
class GDAL_SF:
    """
    用來進行土地利用與表層土壤處置
    1. 以 superfile 定義相關資訊
    2. 循序處理, 包含繪圖
    """

    def __init__(self, sf_fname: str, **kwargs):  # noqa: C901
        """
        change: 2022/5/2
            取消 log_debug 功能
        """
        jut.check_outdated_IO(
            "log_debug",  # 不再使用 log_debug,
            "debug輸出改採用 root_logger",
            **kwargs,
        )
        if kwargs.get("root_logger", None) is not None:
            kwargs.get("root_logger", None).debug(
                " GDAL_SF: Loading superfile '{}'".format(
                    sf_fname,
                )
            )

        sepline: List = []
        with open(
            sf_fname,
            "r",
            encoding=eut.check_file_encode(sf_fname),
        ) as f:
            lines = f.readlines()
            # proj. name, data path & coor_fname
            sepline = lines[0].rstrip().split()
            self.proj_name: str = sepline[0]
            self.data_path: str = sepline[1]
            self.coor_fname: str = sepline[2]
            try:
                assert os.path.exists(self.coor_fname)
            except AssertionError:
                # 修正路徑
                self.coor_fname = os.path.join(
                    os.path.dirname(sf_fname),
                    "..",
                    self.coor_fname,
                )
            self.cell_inform = CI.cell_utility(
                self.coor_fname, **kwargs
            )

            self.cname: str = sepline[0]
            if len(sepline) > 3:
                self.cname = sepline[3]

            # LAND USE
            sepline = lines[1].rstrip().split()
            # _luse_size: int = int(sepline[0])  # noqa, F841
            self.luse_path = sepline[1]
            self.luse_colname = sepline[2]
            self.luse_flist = lines[2].rstrip().split()
            self.luse_regroup = lines[3].rstrip()

            def load_gdal_cmap(
                fname: str, **kwargs
            ) -> pd.DataFrame:
                """
                讀取 gdal_cmap.csv
                columns: ["burn_index", "R", "G", "B"]
                """
                assert isinstance(fname, str)
                assert os.path.exists(fname), fname
                mat = []
                with open(
                    fname,
                    "r",
                    encoding=eut.check_file_encode(fname),
                ) as f:
                    lines = f.readlines()
                    for line in lines:
                        sepline = line.rstrip().split()
                        if len(sepline) > 0:
                            mat_sub: List = []
                            for elem in sepline:
                                try:
                                    mat_sub.append(int(elem))
                                except ValueError:
                                    mat_sub.append(elem)
                            mat.append(mat_sub)
                df = pd.DataFrame(
                    mat, columns=["burn_index", "R", "G", "B"]
                )
                if not kwargs.get("log_quiet", True):
                    print(
                        "GDAL CMAP: {} --> {}".format(fname, df)
                    )
                return df

            # Land Use GDAL cmap
            self.luse_gdal: pd.DataFrame = pd.DataFrame([])
            try:
                self.luse_gdal = load_gdal_cmap(
                    lines[4].rstrip(), **kwargs
                )
            except AssertionError:
                # 修正路徑
                fixed_fname = os.path.join(
                    os.path.dirname(sf_fname),
                    "..",
                    lines[4].rstrip(),
                )
                self.luse_gdal = load_gdal_cmap(
                    fixed_fname, **kwargs
                )

            # SOIL
            sepline = lines[5].rstrip().split()
            # _layer_size: int = int(sepline[1])  # noqa, F841, 層數
            self.soil_path = sepline[2]
            self.soil_colname = sepline[3]

            self.soil_flist = lines[6].rstrip().split()
            self.soil_layer = lines[7].rstrip().split()

            # Soil GDAL cmap
            self.soil_gdal: pd.DataFrame = pd.DataFrame([])
            try:
                self.soil_gdal = load_gdal_cmap(
                    lines[8].rstrip(), **kwargs
                )
            except AssertionError:
                # 修正路徑
                fixed_fname = os.path.join(
                    os.path.dirname(sf_fname),
                    "..",
                    lines[8].rstrip(),
                )
                self.soil_gdal = load_gdal_cmap(
                    fixed_fname, **kwargs
                )

            # 第10行, 之後為載入其他 shape file
            self.GRPATH: str = ""
            self.INFIL: Tuple[str, ...] = ("", "", "")
            self.VEGE: List = []
            self.VEGE_PATH: str = ""
            self.hillshade: Dict[str, Optional[float]] = {
                "vmin": None,
                "vmax": None,
            }  # 日照陰影圖的 dem 高程 vlimit
            self.slope_vlim: Dict[str, Optional[float]] = {
                "vmin": None,
                "vmax": None,
            }  # dem 坡度的繪圖 vlimit
            self.gis_cell: str = ""
            self.gis_extra_post: Optional[List] = None
            self.gis_extra_post_filter_type: str = "all"
            self.tif_extra_pre: Optional[Dict] = None
            self.annotate_extra_post: List = []
            self.loc_debugs = None
            self.proj_debugs = ["ALL"]
            self.contour_level: Optional[float] = None
            var: str = ""
            self.aquifer_vlim = {}
            self.aquifer_level: Optional[float] = None
            for i in range(9, len(lines)):
                if lines[i].find("#") >= 0:
                    lines[i] = lines[i][: lines[i].find("#")]

                for flag in [
                    "GRPATH",
                    "INFIL",
                    "VEGE",
                    "VEGE_SOURCE",
                    "VLIM",
                    "SLOPE_VLIM",
                    "AZIMUTH",
                    "ANGLE_ALTITUDE",
                    "HILLSHADE_ALPHA",
                    "PLOT_LABEL_KWARGS",
                    "CONTOUR_LEVEL",
                    "GIS_CELL",
                    "AQUIFER_VLIM",
                    "AQUIFER_LEVEL",
                    "GIS_EXTRA_POST_FILTER_TYPE",
                    "GIS_EXTRA_POST",
                    "ANNOTATE_EXTRA_POST",
                    "TIF_EXTRA_PRE",
                    "TIF_EXTRA_PRE_ALPHA",
                    "TIF_EXTRA_PRE_EXTENT",
                    "LOC_DEBUGS",
                    "PROJ_DEBUGS",
                ]:
                    if re.compile("^{}=".format(flag)).search(
                        lines[i]
                    ):
                        # if lines[i].find("{}=".format(flag)) >= 0:
                        # log_start = False
                        var = (
                            lines[i]
                            .rstrip()
                            .replace("{}=".format(flag), "")
                        )
                        sepline = var.split(",")
                        if flag == "GRPATH":
                            # 網格降雨路徑
                            self.GRPATH = var
                        elif flag == "INFIL":
                            self.INFIL = tuple(sepline)
                        elif flag == "VEGE":
                            self.VEGE = sepline
                        elif flag == "VEGE_SOURCE":
                            self.VEGE_PATH = var
                        elif flag == "CONTOUR_LEVEL":
                            self.contour_level = float(var)
                        elif flag == "AQUIFER_LEVEL":
                            self.aquifer_level = float(var)
                        elif flag == "GIS_CELL":
                            self.gis_cell = var
                        elif (
                            flag == "GIS_EXTRA_POST_FILTER_TYPE"
                        ):
                            self.gis_extra_post_filter_type = var
                        elif flag == "GIS_EXTRA_POST":
                            line_elem: str = var.split("#")[
                                0
                            ].replace(" ", "")
                            sepline2 = line_elem.split(",")

                            # len == 5 的倍數
                            # 0, shape file name
                            # 1, line color
                            # 2, line type
                            # 3, line width
                            # 4, alpha
                            # 5, marker
                            try:
                                assert len(sepline2) % 5 == 0
                            except AssertionError as e:
                                raise AssertionError(
                                    "{} / {}".format(
                                        sepline2, len(sepline2)
                                    )
                                ) from e

                            sepline3 = [
                                sepline2[j : j + 5]
                                for j in range(
                                    0, len(sepline2), 5
                                )
                            ]
                            try:
                                for j in range(len(sepline3)):
                                    # 定義 line width & alpha
                                    for k in [3, 4]:
                                        assert jut.check_isfloat(
                                            sepline3[j][k]
                                        )  # 確認可轉換為 float
                                        sepline3[j][k] = float(  # type: ignore
                                            sepline3[j][k]
                                        )
                                    # debug, 處理成為 5 的倍數
                                    for k in [0, 1, 2]:
                                        # 字串
                                        assert isinstance(
                                            sepline3[j][k], str
                                        )
                            except IndexError as e:
                                raise IndexError("IndexError: {}".format(sepline3)) from e
                            self.gis_extra_post = sepline3
                        elif flag == "ANNOTATE_EXTRA_POST":
                            # 在外部解析好, 回傳為 Dict
                            my_dict = (
                                annotate_extra_post_analysis(var)
                            )
                            self.annotate_extra_post.append(
                                my_dict
                            )

                        elif flag.find("TIF_EXTRA_PRE") >= 0:
                            if self.tif_extra_pre is None:
                                self.tif_extra_pre = {}

                            if flag == "TIF_EXTRA_PRE":
                                self.tif_extra_pre[
                                    "tif_flist"
                                ] = var.split(",")
                            elif flag == "TIF_EXTRA_PRE_ALPHA":
                                self.tif_extra_pre["alpha"] = (
                                    float(var)
                                )
                            elif flag == "TIF_EXTRA_PRE_EXTENT":
                                self.tif_extra_pre["extent"] = [
                                    float(elem)
                                    for elem in var.split(",")
                                ]

                        elif flag in [
                            "VLIM",
                            "SLOPE_VLIM",
                            "AQUIFER_VLIM",
                        ]:
                            if flag == "VLIM":
                                self.hillshade = define_vlim(
                                    sepline
                                )
                            elif flag == "SLOPE_VLIM":
                                self.slope_vlim = define_vlim(
                                    sepline
                                )
                            elif flag == "AQUIFER_VLIM":
                                self.aquifer_vlim = define_vlim(
                                    sepline
                                )

                        elif flag in [
                            "AZIMUTH",
                            "ANGLE_ALTITUDE",
                            "HILLSHADE_ALPHA",
                        ]:
                            self.hillshade[flag.lower()] = float(
                                var
                            )
                        elif flag in ["LOC_DEBUGS"]:
                            sepline = (
                                var.replace("(", "")
                                .replace(")", "")
                                .split(",")
                            )
                            self.loc_debugs = [
                                tuple(
                                    int(sepline[i + j])
                                    for j in range(3)
                                )
                                for i in range(
                                    0, len(sepline), 3
                                )
                            ]
                        elif flag in ["PROJ_DEBUGS"]:
                            self.proj_debugs = var.split(",")

        # 後續各專案細節寫入 proj_logger
        proj_name = "sf_{}".format(self.proj_name)
        self.proj_logger = jlib_logging.logger_setup(
            proj_name,
            filename=os.path.join(
                os.path.join("logging", proj_name),
            ),
            log_append=kwargs.get("proj_logger_append", True),
        )
        # 建立分隔線
        self.proj_logger.debug(jut.create_line_bar(60, "="))
        self.proj_logger.debug(jut.create_line_bar(60, "="))
        self.proj_logger.debug(jut.create_line_bar(60, "="))

        self.get_luse_fname()
        self.get_soil_fname()

        if self.proj_logger is not None:
            self.proj_logger.debug(
                " GDAL Project Name: {}".format(
                    self.proj_name,
                )
            )
            self.proj_logger.debug(
                "  網格降雨路徑: {}".format(
                    self.GRPATH,
                )
            )
            self.proj_logger.debug(
                "  Data Path: {}".format(
                    self.data_path,
                )
            )
            self.proj_logger.debug(
                "  Coordinate File Name: {}".format(
                    self.coor_fname,
                )
            )

    def __str__(self) -> str:
        """
        GDAL sf 輸出資訊
        """
        message = "GDAL Project Name: {}\n".format(
            self.proj_name
        )
        message += "網格降雨路徑: {}\n".format(self.GRPATH)
        message += "Data Path: {}\n".format(self.data_path)
        message += "Coordinate File Name: {}\n".format(
            self.coor_fname
        )

        params = {
            "INFIL": self.INFIL,
            "VEGE": self.VEGE,
            "VEGE_PATH": self.VEGE_PATH,
            "VLIM": self.hillshade,
            "SLOPE_VLIM": self.slope_vlim,
            "CONTOUR_LEVEL": self.contour_level,
            "AQUIFER_VLIM": self.aquifer_vlim,
            "AQUIFER_LEVEL": self.aquifer_level,
            "AZIMUTH": self.hillshade["azimuth"],
            "ANGLE_ALTITUDE": self.hillshade["angle_altitude"],
            "HILLSHADE_ALPHA": self.hillshade["hillshade_alpha"],
            "GIS_CELL": self.gis_cell,
        }
        for key, elem in params.items():
            message += "    ==> {} :{}\n".format(key, elem)
        return message

    def get_luse_fname(self):
        """
        土地利用部份
        """
        self.luse_shp: pd.DataFrame = get_fname(
            self.luse_path, self.luse_flist
        )
        if self.proj_logger is not None:
            self.proj_logger.debug(
                " 建立土地利用檔案 path={}".format(
                    self.luse_path,
                )
            )
            self.proj_logger.debug(
                "   --> {}".format(
                    self.luse_shp,
                )
            )

    def get_soil_fname(self):
        """
        土壤部份
        """
        self.soil_shp: pd.DataFrame = get_fname(
            self.soil_path, self.soil_flist
        )
        mat = []
        for index in self.soil_shp.index:
            mat.append(
                [
                    self.soil_shp.loc[index, "flag"],
                    int(
                        os.path.basename(
                            self.soil_shp.loc[index, "fname"]
                        )
                        .split("_")[0]
                        .replace("TEXTURE", "")
                    ),
                    self.soil_shp.loc[index, "fname"],
                ]
            )
        # 表層土壤列表
        self.df_soil_pair = pd.DataFrame(
            mat, columns=["city", "layer", "filename_shp"]
        )

        if self.proj_logger is not None:
            self.proj_logger.debug(
                " 建立表層土壤檔案 path={}".format(
                    self.soil_path,
                )
            )
            self.proj_logger.debug(
                "   --> {}".format(
                    self.df_soil_pair,
                )
            )

    def shp_rasterizing(
        self,
        log_parallel_process: bool = False,
        log_quiet=False,
        **kwargs,
    ):
        """
        cpu_ratio, default = 0.6    平行的 CPU 使用率
        calc_types, default = ["landuse", "soil"]    計算的類型
            landuse: 土地利用
            soil: 表層土壤

        # 土地利用 & 表層土壤
        # vector-based GIS file --> raster-based file
        # dt1 = datetime.datetime.now()
        """
        jut.check_outdated_IO(
            "root_logger",  # 不再使用 root_logger 輸入
            "通用部份使用 root_logger, 專案細節使用內部的 self.proj_logger",
            **kwargs,
        )
        calc_types = kwargs.get(
            "calc_types", ["landuse", "soil"]
        )

        if self.proj_logger is not None:
            self.proj_logger.debug(
                "log_quiet: {}".format(
                    log_quiet,
                )
            )

        # pylint: disable=attribute-defined-outside-init
        self.luse_nc: List = []
        self.soil_nc: List = []
        flags = []

        if "landuse" in calc_types:
            if self.proj_logger is not None:
                self.proj_logger.debug("GDAL for Land Use")

            for index in self.luse_shp.index:
                shp_fname = self.luse_shp.loc[index, "fname"]
                nc_fname = os.path.join(
                    self.data_path,
                    "landuse",
                    self.luse_shp.loc[index, "flag"],
                    os.path.basename(shp_fname).replace(
                        ".shp", ".nc"
                    ),
                )
                flags.append(
                    [
                        shp_fname,
                        self.luse_path,
                        nc_fname,
                        self.coor_fname,
                        self.luse_colname,  # attri
                        None,  # burn index
                        False,
                        # self.log_debug,
                        self.proj_logger,
                        True,  # log_add_rightend
                        log_quiet,
                        "landuse",
                    ]
                )

        if "soil" in calc_types:
            if self.proj_logger is not None:
                self.proj_logger.debug("GDAL for Surface Soil")
            # 表層土壤

            for index in self.soil_shp.index:
                shp_fname = self.soil_shp.loc[index, "fname"]
                nc_fname = os.path.join(
                    self.data_path,
                    "soil",
                    self.soil_shp.loc[index, "flag"],
                    os.path.basename(shp_fname).replace(
                        ".shp", ".nc"
                    ),
                )
                flags.append(
                    [
                        shp_fname,
                        self.soil_path,
                        nc_fname,
                        self.coor_fname,
                        self.soil_colname,
                        None,
                        False,
                        # self.log_debug,
                        self.proj_logger,
                        True,  # log_add_rightend
                        log_quiet,
                        "soil",
                    ]
                )
        if self.proj_logger is not None:
            self.proj_logger.debug(
                "Number of waiting processes (Landuse + Soil): {}".format(
                    len(flags)
                )
            )
            for i, flag in enumerate(flags):
                self.proj_logger.debug(
                    "  --> {}: {}".format(i, flag)
                )

        # 平行計算 landuse & soil
        # 將 shp --> nc
        # 2024/2/3 統一改為 parallel_framework.parallel_process_framework 處理
        mat = pf.parallel_process_framework(
            gdut.shp_rasterize_shell,  # rasterize
            flags,
            log_parallel=log_parallel_process,
            processor_ratio=kwargs.get("cpu_ratio", 0.6),
            root_logger=self.proj_logger,
            **{
                key: elem
                for key, elem in kwargs.items()
                if key not in ["cpu_ratio", "root_logger"]
            },
        )

        self.luse_nc = [
            mat[i]
            for i, flag in enumerate(flags)
            if flag[-1] == "landuse"
        ]
        self.soil_nc = [
            mat[i]
            for i, flag in enumerate(flags)
            if flag[-1] == "soil"
        ]
        self.df_soil_pair.loc[:, "filename_nc"] = self.soil_nc
        if self.proj_logger is not None:
            self.proj_logger.debug(
                "Landuse & Soil Rasterization Completed"
            )

    # pylint: disable=dangerous-default-value
    def luse_process(
        self,
        background_shp_list: List = [],
        edgecolor: List = [],
        facecolor: List = [],
        linewidth: List = [],
        alpha: List = [],
        **kwargs,
    ):
        """
        # 土地利用處理運算
        """
        nc_fname = os.path.join(
            self.data_path, "luse_{}.nc".format(self.proj_name)
        )
        if not os.path.exists(nc_fname):
            luse_merged: gdut.gdal_utility = gdut.nc_merge(
                self.luse_nc,
                self.coor_fname,
                log_add_rightend=True,
                log_remove=False,  # merge 後, 刪除原本檔案
                **kwargs,
            )  # 整合

            # 重歸類
            luse_merged.param_regroup(
                self.luse_regroup, **kwargs
            )  # 重歸類
            luse_merged.nc_fname = nc_fname
            luse_merged.export_band_data(
                luse_merged.nc_fname, log_debug=False
            )  # 輸出

            if self.proj_logger is not None:
                self.proj_logger.debug(
                    " --> Landuse Visualization"
                )
            # 出圖
            ticks = [1, 2, 3, 4, 5, 6]
            ticklabels = [
                "不透水用地",
                "旱田或裸露地",
                "水田",
                "靜止水體",
                "河川",
                "海洋",
            ]

            # 出圖
            plot_content(
                luse_merged,
                luse_merged.nc_fname.replace(".nc", ""),
                "{}({})".format(
                    self.cname, "土地利用"
                ),  # fig_title
                self.luse_gdal,
                ticks=ticks,
                ticklabels=ticklabels,
                # cell_inform=self.cell_inform,
                # gis_extra_post_filter_type = self.gis_extra_post_filter_type,
                **kwargs,
            )

    # pylint: disable=dangerous-default-value
    def soil_process(
        self,
        **kwargs,
    ):
        """
        # 表層土壤計算
        """
        jut.check_outdated_IO(
            "log_debug",
            "'log_debug' 為過時參數, 應改採 root_logger",
            **kwargs,
        )
        # groupby 找出土壤列表
        keys_soil_layer = list(
            self.df_soil_pair.groupby("layer").groups.keys()
        )

        soil_list_nc = []
        soil_list = []
        ticks = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]
        ticklabels = [
            "粗砂",
            "細砂",
            "壤質細砂",
            "極細砂",
            "坋土",
            "壤土",
            "砂質粘壤土",
            "粘質壤土",
            "坋質黏土",
            "粘土",
            "石礫",
        ]
        layer_mat = {
            1: r"$0-30\; cm$",
            2: r"$30-60\; cm$",
            3: r"$60-90\; cm$",
            4: r"$90-150\; cm$",
        }

        for l, layer in enumerate(keys_soil_layer):  # noqa, E741
            nc_fname = os.path.join(
                self.data_path,
                "soil_{}_{}.nc".format(self.proj_name, l),
            )
            if not os.path.exists(nc_fname):
                # 表層土壤,
                soil_merged: gdut.gdal_utility = gdut.nc_merge(
                    list(
                        self.df_soil_pair[
                            self.df_soil_pair["layer"] == layer
                        ]["filename_nc"].values
                    ),
                    self.coor_fname,
                    log_add_rightend=True,
                    log_remove=False,  # merge 後, 刪除原本的 nc file
                    **kwargs,
                )  # 整合

                soil_merged.nc_fname = nc_fname
                soil_merged.export_band_data(
                    soil_merged.nc_fname
                )  # 輸出

                soil_list.append(soil_merged)
                soil_list_nc.append(soil_merged.nc_fname)

                # 出圖, 各層土壤
                plot_content(
                    soil_merged,
                    soil_merged.nc_fname.replace(".nc", ""),
                    "{}({})".format(
                        self.cname, layer_mat[layer]
                    ),  # fig_title
                    self.soil_gdal,
                    ticks=ticks,
                    ticklabels=ticklabels,
                    **kwargs,
                )

        if self.proj_logger is not None:
            self.proj_logger.debug(" --> 代表性土壤功能")

        nc_fname = os.path.join(
            self.data_path,
            "soil_{}_significant.nc".format(self.proj_name),
        )
        if not os.path.exists(nc_fname):
            soil_list[0].significant_soil(
                soil_list
            )  # 計算代表性土壤

            soil_list[0].export_band_data(nc_fname)  # 輸出
            soil_list[0].nc_fname = nc_fname

            if self.proj_logger is not None:
                self.proj_logger.debug(" --> Visualization")
            # 出圖
            plot_content(
                soil_list[0],
                nc_fname.replace(".nc", ""),
                "{}({})".format(
                    self.cname, "Significant"
                ),  # fig_title
                self.soil_gdal,
                ticks=ticks,
                ticklabels=ticklabels,
                **kwargs,
            )
