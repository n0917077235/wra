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
import shp_operation as shp_op
import group_ts
import polar_coord

# pylint: disable=unused-import
import plt_parameters  # noqa=C901


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
