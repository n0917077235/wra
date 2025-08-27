"""
首潤, 地震資料內差
    回傳 geojson 形式的震度等直線
    採用 geojsoncontour package

"""

import sys
import os
from typing import Dict, List, Optional

import pandas as pd
import numpy as np
import geopandas as gpd
from shapely import Polygon
import shapely
import shapely.errors
from matplotlib.ticker import FormatStrFormatter
import twd97
import matplotlib.pyplot as plt
import geojsoncontour
import grid_utility
import cell_inform
import jutility as jut
import plt_parameters
import shp_operation

import GDAL_SF

class InnerLoop(Exception):
    '''
    break inner loop
    '''
    pass


def load_config(config_file: str, input_fname: str) -> Dict:
    """
    load config.txt
    """
    argv_params = {}
    with open(config_file, "r", encoding="utf-8") as f:
        for line in f:
            sepline = line.strip().split("=")
            if len(sepline) >= 2:
                if sepline[0][0] != "#":
                    flag = sepline[0].lower()
                    if flag not in ["annotate_extra_post"]:
                        val = (
                            sepline[1]
                            .replace('"', "")
                            .replace("'", "")
                        )
                        argv_params[flag] = val
                    else:
                        val = line.rstrip().replace(
                            "{}=".format(flag), ""
                        )
                        if flag not in argv_params:
                            argv_params[flag] = []
                        argv_params[flag].append(
                            GDAL_SF.annotate_extra_post_analysis(
                                val
                            )
                        )

    input_folder = argv_params["input"]
    argv_params["input"] = os.path.join(
        input_folder, input_fname
    )
    argv_params["coordinate"] = os.path.join(
        input_folder, "coor_taipei.txt"
    )
    argv_params["levels"] = [
        float(level)
        for level in argv_params["levels"].split(",")
    ]
    if "gis_extra_post" in argv_params:
        sepline2 = (
            argv_params["gis_extra_post"]
            .split(" ")[0]
            .split(",")
        )
        if "#" in sepline2:
            # 如果中間有 # 符號, 刪除後方的資訊
            for i in range(len(sepline2)):
                if sepline2[i] == "#":
                    sepline2 = sepline[:i]
                    break

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
                "{} / {}".format(sepline2, len(sepline2))
            ) from e

        sepline3 = [
            sepline2[j : j + 5]
            for j in range(0, len(sepline2), 5)
        ]
        for j in range(len(sepline3)):
            # 定義 line width & alpha
            for k in [3, 4]:
                assert jut.check_isfloat(
                    sepline3[j][k]
                )  # 確認可轉換為 float
                sepline3[j][k] = float(  # type: ignore
                    sepline3[j][k]
                )
            for k in [0, 1, 2]:
                # 字串
                assert isinstance(sepline3[j][k], str)
        argv_params["gis_extra_post"] = sepline3
    return argv_params


def load_point_data(point_fname: str) -> pd.DataFrame:
    """
    load point data
    """
    assert os.path.exists(point_fname)
    df_point = pd.read_csv(point_fname)
    return df_point


def plot_grid(
    _fig,
    ax,
    points,
    vals,
    grid_z,
    my_cell,
    levels,
    **kwargs,
):
    if kwargs.get("log_grouping", False):
        eq_level = [
            0.5,
            1.5,
            2.5,
            3.5,
            4.5,
            5.5,
            6.0,
            6.5,
            7.0,
            8.0,
        ]
        eq_label = [
            "1",
            "2",
            "3",
            "4",
            "5-",
            "5+",
            "6-",
            "6+",
            "7",
        ]
        for i in range(len(eq_level) - 1):
            grid_z = np.where(
                np.logical_and(
                    grid_z < eq_level[i + 1],
                    grid_z >= eq_level[i],
                ),
                (eq_level[i] + eq_level[i + 1]) / 2,
                grid_z,
            )

    pos = ax.imshow(
        grid_z,
        extent=my_cell.extent,
        origin="lower",
        alpha=0.4,
        cmap="gist_ncar",
        **{
            key: elem
            for key, elem in kwargs.items()
            if key in ["vmin", "vmax"]
        },
    )
    cbar = _fig.colorbar(pos, ax=ax, fraction=0.03, pad=0.04)
    cbar.set_label(
        "震度",
        labelpad=20,
        rotation=270,
        fontsize=16,
    )

    if kwargs.get("log_plot_points", True):
        loc_shift = np.array(kwargs.get("loc_shift", [50, 200]))
        for l in range(len(points)):
            if my_cell.check_incell(tuple(points[l, :])):
                try:
                    for i in range(len(eq_level) - 1):
                        if (vals[l] >= eq_level[i]) and (vals[l] < eq_level[i + 1]):
                            message = eq_label[i]
                            ax.annotate(
                                message,
                                xy=points[l, :] + loc_shift,
                                xytext=points[l, :] + loc_shift,
                                xycoords="data",
                                fontsize=16,
                            )
                            raise InnerLoop
                except InnerLoop:
                    pass

    contour = ax.contour(
        my_cell.grid_x,
        my_cell.grid_y,
        grid_z,
        levels=levels,
    )
    
    return ax, contour


def plot_grid_export(
    points,
    vals,
    grid_z,
    my_cell,
    levels,
    fig_fname: str,
    argv_params,
    **kwargs,
):
    plt.style.use("bmh")
    _fig, ax = plt.subplots(1, figsize=(16, 10.5))

    # 繪製背景
    GDAL_SF.gis_extra_post_plot(
        ax,
        gis_extra_post=argv_params.get("gis_extra_post", None),
        cell_inform=my_cell,
        log_label=True,
        label_columns="Taiwan_county_twd97::county",
    )

    ax, contour = plot_grid(
        _fig,
        ax,
        points,
        vals,
        grid_z,
        my_cell,
        levels,
        vmin=np.min(levels),
        vmax=np.max(levels),
        **kwargs,
    )

    # 額外加入 annotate
    GDAL_SF.annotate_extra_post_plot(
        ax, argv_params["annotate_extra_post"], **kwargs
    )

    for flag in ["fig_title", "fig_xlabel", "fig_ylabel"]:
        plt_parameters.assign_fig_detail(ax, flag, **kwargs)

    geojson = None
    geojson_fname = os.path.join(
        argv_params["output"],
        os.path.basename(argv_params["input"]).replace(
            ".txt", ".geojson"
        ),
    )
    if kwargs.get("log_geojson", False):
        # Convert matplotlib contour to geojson
        geojson = geojsoncontour.contour_to_geojson(
            contour=contour,
            ndigits=3,
            unit="m",
            geojson_filepath=geojson_fname,
        )

    ax.yaxis.set_major_formatter(FormatStrFormatter("%.0f"))
    plt.yticks(rotation=90)
    plt.tight_layout()
    if argv_params["log_plot"]:
        jut.save_fig(fig_fname)
    return geojson_fname, geojson


if __name__ == "__main__":
    # input file
    input_fname = sys.argv[1]
    # load config.txt
    argv_params = load_config("config.txt", input_fname)
    for flag in ["log_plot", "log_debug", "log_krig_compare"]:
        argv_params[flag] = (
            np.sum(
                np.array(
                    [  # 是否要繪圖 or debug
                        argv.lower().find(flag) >= 0
                        for argv in sys.argv[1:]
                    ]
                )
            )
            > 0
        )

    log_append = False
    program_name = "srec_interpolate"
    root_logger = None
    
    # load 震度資料
    assert os.path.exists(argv_params["input"]), argv_params[
        "input"
    ]
    argv_params["point_data"] = load_point_data(
        argv_params["input"]
    )
    # WGS84 --> TWD97
    for index in argv_params["point_data"].index:
        (
            argv_params["point_data"].loc[index, "x"],
            argv_params["point_data"].loc[index, "y"],
        ) = twd97.fromwgs84(
            argv_params["point_data"].loc[index, "N"],
            argv_params["point_data"].loc[index, "E"],
        )

    # 處理縣市資料
    my_cell = cell_inform.cell_utility(argv_params["coordinate"])
    town_data = grid_utility.town_raster_data(
        "Taiwan_town_twd97.shp",
        cell_inform=argv_params["coordinate"],
        encoding="big5",
    )
    
    assert os.path.exists("Taiwan_town_twd97.nc")
    my_grid = grid_utility.grid_utility(
        my_cell,
        proj_name="EPSG:3826",
        town_data=town_data,
    )

    # 進行內差
    points = np.array(
        list(
            [
                list(
                    argv_params["point_data"]
                    .loc[index, ["x", "y"]]
                    .values
                )
                for index in argv_params["point_data"].index
            ]
        )
    )
    vals = np.array(
        list(argv_params["point_data"].loc[:, "震度"].values)
    )
    
    levels = argv_params["levels"]
    try:
        if np.all([val == np.mean(vals) for val in vals]):
            raise jut.KrigingFail("數據均一致")
        kwargs_linear = {"method": "linear"}
        kwargs_krig = {
            "method": "OrdinaryKriging",
            "variogram_model": "linear",
        }
        
        kwargs_krig["variogram_model"] = "spherical"

        grid_z = my_grid.interpolate_combine(
            points,
            vals,
            grid_x=my_cell.grid_x,
            grid_y=my_cell.grid_y,
            log_ocean_remove=True,
            log_nearest_merge=True,  # 外圍數據不呈現
            log_debug=argv_params["log_debug"],
            root_logger=root_logger,
            **kwargs_krig,
        )

        # 繪圖
        geojson_fname, _geojson = plot_grid_export(
            points,
            vals,
            grid_z,
            my_cell,
            levels,
            os.path.join(  # fig_fname
                argv_params["output"],
                os.path.basename(argv_params["input"])
                .replace(".csv", "")
                .replace(".txt", "_twd97"),
            ),
            argv_params,
            log_geojson=False,
            log_grouping=True,
        )

        #############################################################################
        # --> WGS 84
        points2 = []
        vals2 = []
        
        points2 = np.array(
            list(
                [
                    list(
                        argv_params["point_data"]
                        .loc[index, ["E", "N"]]
                        .values
                    )
                    for index in argv_params["point_data"].index
                ]
            )
        )
        vals2 = np.array(
            list(argv_params["point_data"].loc[:, "震度"].values)
        )

        points2 = np.array(points2)
        vals2 = np.array(vals2)

        my_cell_wgs84 = cell_inform.cell_utility(
            argv_params["coordinate"].replace(
                ".txt", "_wgs84.txt"
            )
        )
        my_grid_wgs84 = grid_utility.grid_utility(
            my_cell,
            proj_name="EPSG:3826",
            # town_data=town_data_wgs84,
        )
        grid_z2 = my_grid_wgs84.interpolate_combine(
            points2,
            vals2,
            grid_x=my_cell_wgs84.grid_x,
            grid_y=my_cell_wgs84.grid_y,
            log_ocean_remove=True,
            log_nearest_merge=True,  # 外圍數據不呈現
            log_debug=argv_params["log_debug"],
            root_logger=root_logger,
            **kwargs_krig,
        )

        # 繪圖
        # 轉換為整數階層
        grid_z3 = grid_z2.copy()
        for i in range(1, 9):
            grid_z3 = np.where(
                np.logical_and(
                    grid_z3 < i,
                    grid_z3 >= i - 1,
                ),
                i - 1,
                grid_z3,
            )

        geojson_fname, geojson = plot_grid_export(
            points2,
            vals2,
            grid_z2,
            my_cell_wgs84,
            levels,
            os.path.join(
                argv_params["output"],
                os.path.basename(argv_params["input"]).replace(
                    ".txt", ""
                ),
            ),
            argv_params,
            log_geojson=True,
            log_plot_points=False,
        )
        gpd_contour = gpd.read_file(geojson_fname)

        # 將 LineString 所有的節點, 計算彼此距離
        # 如距離高於門檻, 切割為多段線段
        # LineString --> MultiLineString
        gpd_contour = shp_operation.split_by_point_distance(
            gpd_contour, 0.05
        )
        # 重新輸出
        gpd_contour.to_file(
            os.path.join(
                argv_params["output"],
                os.path.basename(argv_params["input"]).replace(
                    ".txt", ".geojson"
                ),
            )
        )
    except jut.KrigingFail:
        if np.all(
            [val == np.mean(vals) for val in vals]
        ):  # 所有數據都相同
            # 無等值線, 以 cell 產生封閉曲線
            assigned_level = np.nan
            for i in range(len(levels)):
                try:
                    if (levels[i] <= np.nanmean(vals)) and (
                        levels[i + 1] > np.nanmean(vals)
                    ):
                        assigned_level = levels[i]
                        break
                except IndexError:
                    pass

            gpd_contour = gpd.GeoDataFrame([], geometry=[])
            gpd_contour.to_file(
                os.path.join(
                    argv_params["output"],
                    os.path.basename(
                        argv_params["input"]
                    ).replace(".txt", ".geojson"),
                )
            )
