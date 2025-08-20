"""
計算 RCH, 應用 super file
"""

# pylint: disable=too-many-lines
# -*- coding: utf-8 -*-
import sys
import os
from typing import List, Tuple, Optional, Union, Dict
from multiprocessing import Pool
import datetime
from numba import jit
from scipy.interpolate import griddata
import numpy as np
from cpuinfo import get_cpu_info
from mpl_toolkits.axes_grid1.inset_locator import inset_axes
import matplotlib.pyplot as plt
import pandas as pd
import geopandas as gpd
import GDAL_SF

sys.path.append(os.path.join("..", "srcs", "rch_calc"))
import rch_coef

sys.path.append(os.path.join("..", "..", "jlib", "srcs"))
import cell_inform as CI
import file_utility as fut
import tendays_analysis as TA
import ts_process_base as TSB
import netcdf_analysis as NA
import jutility as jut
import TimeConsume as tcs
import plt_parameters
import jlib_logging
import gif_utility
import time_phrase
import cmap_define

# load super file
# 以 gdal 進行處理
# 降雨入滲計算


program = "gdal_process"
root_logger = jlib_logging.logger_setup(
    program,
    filename=os.path.join(
        os.path.join("logging", program),
    ),
    log_append=True,  # 接續 GDAL 運算
)


sf_fname = sys.argv[1]


def determine_year(argv: str) -> int:
    """
    定義起始與結束年
    """
    try:
        year = int(argv)
    except ValueError:
        if argv.lower() == "now":
            year = datetime.datetime.now().year
    return year


str_year = determine_year(sys.argv[2])  # 起始年
end_year = determine_year(sys.argv[3])  # 結束年


log_debug = False
log_filter = True
log_interpolate = True
log_parallel_process = False
log_seasonal = True

plt.rcParams["font.sans-serif"] = ["simhei"]  # 中文字體
plt.rcParams["axes.unicode_minus"] = False

# 全域變數
filter_code: List = []
df_sat: pd.DataFrame = pd.DataFrame([])
df_ri: pd.DataFrame = pd.DataFrame([])
df_unsat: pd.DataFrame = pd.DataFrame([])
peddy_decline_ratio = 0.5  # 牛踏層, 補注量衰退係數
# 一期, 二期稻作對應月份
aggr_mapping_mat = [
    [1, 2],
    [1, 3],
    [1, 4],
    [2, 7],
    [2, 8],
    [2, 9],
]
aggr_mapping: pd.DataFrame = pd.DataFrame(
    aggr_mapping_mat, columns=["aggr_index", "month"]
)
# 全域變數
df_GridInfo_regular = None
grid_x_refine = None
grid_y_refine = None
xlist_refine = None
ylist_refine = None
band_is_land: np.ndarray = np.array([])
band_luse = None
band_soil = None
xlist_luse = None
ylist_luse = None
xlist_soil = None
ylist_soil = None

# 鄉鎮圖
town_shp_fname_multi = [
    "../../srec_air3/input/gis/Taiwan_town_twd97.shp",
    "../../../../GIS/Taiwan_town_twd97.shp",
]


# pylint: disable=inconsistent-return-statements
def multi_read_file(multi_flist: List):
    """
    輸入多個可能的結果，載入正確可行的檔案
    """
    for fname in multi_flist:
        return gpd.read_file(fname)


town_shp = multi_read_file(town_shp_fname_multi)
county_shp = multi_read_file(town_shp_fname_multi)


# pylint: disable=dangerous-default-value
def plot_grid(
    band: np.ndarray,
    ax,
    extent=None,
    title=None,
    vmin=None,
    vmax=None,
    xlabel=None,
    ylabel=None,
    cbar_label=None,
    background: List = [],
    log_cbar: bool = False,
):
    """
    繪製網格資訊
    1. 背景圖 (鄉鎮圖, 縣市圖)
    2. 網格數據
    """
    ###############################################################################
    # 背景
    try:
        assert isinstance(background, list)
        for bg in background:
            gis_data = bg["gis_data"]
            keys = bg.keys()

            kwargs = {
                key: bg[key] for key in keys if key != "gis_data"
            }
            gis_data.plot(ax=ax, **kwargs)
    except AssertionError as e:
        raise TypeError("輸入之背景檔案格式錯誤") from e

    ###############################################################################
    # 前景
    vlimit = cmap_define.data_limits["precipitation_tendays"]
    im = ax.imshow(
        band,
        vmin=vlimit[0],
        vmax=vlimit[1],
        cmap=cmap_define.determine_cmap_continue(
            cmap_define.data_params["precipitation_tendays"],
            cmap_define.data_limits["precipitation_tendays"],
        ),
        # norm=norm,
        extent=extent,
    )

    ax.set_xlabel(xlabel, fontsize=9)
    ax.set_ylabel(ylabel, fontsize=9)
    if log_cbar:
        axins = inset_axes(
            ax,
            width="5%",  # width = 5% of parent_bbox width
            height="95%",  # height : 50%
            loc="center left",
            bbox_to_anchor=(1.0, 0.0, 1, 1),
            bbox_transform=ax.transAxes,
            borderpad=0,
        )
        cbar = plt.colorbar(im, ax=ax, cax=axins)
        if cbar_label is not None:
            cbar.set_label(cbar_label, rotation=270, fontsize=9)

    if title is not None:
        ax.set_title(title, fontsize=9)
    ax.invert_yaxis()
    ax.tick_params(axis="x", rotation=20)
    ax.tick_params(axis="y")
    ax.set_xlim(extent[:2])
    ax.set_ylim([extent[3], extent[2]])
    ax.grid(linestyle="-.", lw=0.5, color="dimgray", alpha=0.6)

    ax.spines["bottom"].set_color("0.5")
    ax.spines["top"].set_color("0.5")
    ax.spines["right"].set_color("0.5")
    ax.spines["left"].set_color("0.5")
    return ax


@jit
def filter_DataFrame(df, extent, flags):
    """
    擷取 extent 座標範圍內的數據
    """
    minx = min(extent[:2])
    maxx = max(extent[:2])
    miny = min(extent[2:])
    maxy = max(extent[2:])

    mask = df[flags[0]] >= minx
    mask = np.logical_and(mask, df[flags[0]] < maxx)
    mask = np.logical_and(mask, df[flags[1]] >= miny)
    mask = np.logical_and(mask, df[flags[1]] < maxy)

    return df.loc[mask, :]


# @jit
def calc_rainsum_tendays(
    df_flist: pd.DataFrame, log_filter: bool, **kwargs
) -> Tuple[np.ndarray, np.ndarray, np.ndarray]:
    """
    # 計算該旬累積降雨量
    """
    if df_flist.shape[0] == 0:
        return np.array([]), np.array([]), np.array([])

    band_sum: np.ndarray = np.array([])
    xlist: np.ndarray = np.array([])
    ylist: np.ndarray = np.array([])
    for d in range(df_flist.shape[0]):
        nc_fname = df_flist.iloc[d, 0]

        # 包含時間, 3D
        _tlist, ylist, xlist, band1 = NA.read_ncband(nc_fname)
        if log_filter:
            # 篩選研究區域
            band1 = CI.filter_band(band1, filter_code)
        band1 = np.where(
            band1 < -1e5, np.NaN, band1
        )  # 刪除 極小負值

        # 加總
        band_temp = np.nansum(band1, axis=0)
        if band_sum.shape[0] == 0:
            band_sum = band_temp
        else:
            if not np.isnan(np.nansum(band_temp)):
                band_sum += band_temp
    return band_sum, xlist, ylist


@jit
def determine_ax_index(
    m: int, nrow: int, ncol: int, log_debug: bool = False
):
    """
    依據 圖的編號, ncol & nrow, 回傳 ax_index
    """
    ax_index = (int((m - m % ncol) / ncol), m % ncol)
    if log_debug:
        print(" -- NROW: {} / NCOL: {}".format(nrow, ncol))
        print("    {}".format(ax_index))
    return ax_index


def interpolate_refine(
    band1: np.ndarray,
    df,
    grid_x,
    grid_y,
    col_flags,
    method="linear",
):
    """
    網格內插, refinement
    """
    ### # : disable=unused-variable

    @jit
    def grep_tics(df: pd.DataFrame, flags: List):
        """
        找出 tics 值
        """
        assert isinstance(flags, list)
        assert len(flags) == 2

        return (
            df[flags[0]].min(),
            df[flags[1]].min(),
            df[flags[0]].max(),
            df[flags[1]].max(),
        )

    (
        _row_min,
        col_min,
        row_max,
        _col_max,
    ) = grep_tics(df, ["ROW", "COL"])

    # points = []
    # gr = []

    # 改用新的寫法, 不要在迴圈中存在 if-then
    mat = []
    for i, index in enumerate(df.index):
        loc = tuple(df.loc[df.index[i], ["ROW", "COL"]].values)
        rindex = int(row_max - loc[0])
        cindex = int(loc[1] - col_min)
        (x, y) = tuple(df.loc[index, col_flags].values)
        val = band1[rindex, cindex]
        mat.append([x, y, val])
    df = pd.DataFrame(mat, columns=["x", "y", "val"])
    df = df[~df["val"].isnull()]  # 刪除 NaN or None
    points = np.array(df.loc[:, ["x", "y"]].values)
    gr = np.array(df.loc[:, "val"].values)

    # 內插
    if len(gr) > 5:
        band_refine = griddata(
            points, gr, (grid_x, grid_y), method=method
        )
    else:
        return None
    return band_refine


# pylint: disable=too-many-branches
def calc_recharge_infil(  # noqa: C901
    band_rain: pd.DataFrame,
    xlist_rain,
    ylist_rain,
    tlength: float,
    luse_fname: str,
    soil_fname: str,
    cell_area: float,
    log_peddy: bool,
    peddy_decline_ratio: float,
    vege_fname: str,
    # log_debug: bool = False,
    recursive_count: int = 0,
    **kwargs,
):
    """
    計算降雨入滲
    """
    # pylint: disable=global-statement
    global band_luse
    global band_soil
    global xlist_luse
    global ylist_luse
    global xlist_soil
    global ylist_soil

    # 簡化土地利用圖與土壤圖的載入, 避免重複載入
    def load_ncfile_reprocess(
        ylist_luse,
        xlist_luse,
        band_data,
        nc_fname,
        # log_debug: bool = False,
        **kwargs,
    ):
        root_logger = kwargs.get("root_logger", None)
        if band_data is None:
            # loading date
            try:
                (
                    ylist_luse,
                    xlist_luse,
                    band_data,
                ) = NA.read_ncband(luse_fname)
            except FileNotFoundError:
                if root_logger is not None:
                    root_logger.debug(
                        "nc file: {} 不存在, re-created from {}".format(
                            nc_fname, sf_fname
                        )
                    )
                # 重新輸出 土地利用與表層土壤數值
                code_path = os.path.join(
                    "..",
                    "srcs",
                    "gdal_analysis",
                    "super_process_gdal.py",
                )
                command = "python {} {}".format(
                    code_path, sf_fname
                )
                os.system(command)

                if root_logger is not None:
                    root_logger.debug(
                        "re-call '{}'".format(
                            load_ncfile_reprocess
                        )
                    )
                # 再次載入
                return load_ncfile_reprocess(
                    ylist_luse,
                    xlist_luse,
                    band_data,
                    nc_fname,
                    # log_debug=log_debug,
                    **kwargs,
                )

        if root_logger is not None:
            root_logger.debug(
                "    |--> Loading netCDF4 data (DEBUG): {}".format(
                    nc_fname
                )
            )
            root_logger.debug(
                "         Data band: {} / {}".format(
                    np.nanmin(band_data), np.nanmax(band_data)
                )
            )
        return ylist_luse, xlist_luse, band_data

    # 土地利用
    ylist_luse, xlist_luse, band_luse = load_ncfile_reprocess(
        ylist_luse, xlist_luse, band_luse, luse_fname, **kwargs
    )
    # 表層土壤
    ylist_soil, xlist_soil, band_soil = load_ncfile_reprocess(
        ylist_soil, xlist_soil, band_soil, soil_fname, **kwargs
    )

    try:
        # 確認陣列維度
        assert band_luse.shape == band_rain.shape
        assert band_soil.shape == band_rain.shape
    except AssertionError as e:
        # 重新輸出 土地利用與表層土壤數值
        recursive_count += 1
        # pylint: disable=no-else-return
        if recursive_count < 3:
            code_path = os.path.join(
                "..",
                "srcs",
                "gdal_analysis",
                "super_process_gdal.py",
            )
            command = "python {} {}".format(code_path, sf_fname)
            os.system(command)
            # raise ValueError("輸入之土地利用或土壤圖尺寸不符") from e
            # raise FileNotFoundError("缺乏土地利用或土壤圖資訊") from e
            return calc_recharge_infil(
                band_rain,
                xlist_rain,
                ylist_rain,
                tlength,
                luse_fname,
                soil_fname,
                cell_area,
                log_peddy,
                peddy_decline_ratio,
                vege_fname,
                # log_debug=log_debug,
                recursive_count=recursive_count,
                **kwargs,
            )
        else:
            message = (
                "!!! band shape different: {} / {} / {}".format(
                    band_luse.shape,
                    band_soil.shape,
                    band_rain.shape,
                )
            )
            message += "\n    rain: ({} ~ {})".format(
                xlist_rain[0], xlist_rain[-1]
            )
            message += "\n    luse: ({} ~ {})".format(
                xlist_luse[0], xlist_luse[-1]
            )
            message += "\n    soil: ({} ~ {})".format(
                xlist_soil[0], xlist_soil[-1]
            )
            raise AssertionError(message) from e

    else:
        band_soil = band_soil.astype(int)
        band_luse = band_luse.astype(int)

        # 飽和入滲
        # 飽和入滲係數 * 天數 * 面積 / 1000
        # 1000 是 mm -> m
        band_sat = np.zeros(band_soil.shape)
        band_unsat = np.zeros(band_soil.shape)
        for i in range(df_sat.shape[0]):
            # 飽和入滲
            soil_type: int = int(df_sat.iloc[i, 0])
            sat_coef: float = float(df_sat.iloc[i, 1])
            # 產出單位: L^3 (飽和入滲量)
            val = sat_coef * tlength * cell_area / 1000
            # print(i, soil_type, sat_coef, tlength, cell_area, val)
            mask1 = band_soil == soil_type
            band_sat = np.where(mask1, val, band_sat)

            # 未飽和部份
            for j in range(df_ri.shape[0]):
                # 選擇雨量分級
                mask2 = band_rain >= df_ri.iloc[j, 0]
                mask2 = np.logical_and(
                    mask2, band_rain < df_ri.iloc[j, 1]
                )
                ri_index = df_ri.iloc[j, 2]
                index = (ri_index, soil_type - 1)
                if index in df_unsat.index:
                    unsat_coef = df_unsat.loc[index, "ratio"]

                    # 結合土壤
                    mask3 = np.logical_and(mask1, mask2)

                    # 降雨入滲量 = 降雨入滲係數 * 降雨量 * 面積 / 1000
                    # 1000, mm -> m
                    # 產出單位: L^3 (累積降雨入滲量)
                    band_unsat = np.where(
                        mask3,
                        unsat_coef
                        * band_rain
                        * cell_area
                        / 1000,
                        band_unsat,
                    )

        # _xlist = None
        # _ylist = None
        band_vege = None
        if vege_fname != "":
            # 載入衛星影像辨識的作物資料
            try:
                _ylist, _xlist, band_vege = NA.read_ncband(
                    vege_fname
                )
            except AssertionError:
                # raise FileNotFoundError("衛星影響辨識作物資料: '{}' not found!!! {}".format(vege_fname, os.path.exists(vege_fname))) from e
                # 對於缺少該年度的作物辨識資料, 則回歸原本的 GIS 土地利用的成果
                pass
            except FileNotFoundError:
                # band_vege 回歸 None
                pass

        # 結合
        band_merge = np.zeros(band_luse.shape)
        # 1. 不透水不用處理
        # 2. 旱田
        # 3. 水稻田
        # 4. 靜止水體
        # 5. 河川, 不用處理, 等 SFR 再處理
        band_merge = np.where(
            band_luse == 2, band_unsat, band_merge
        )
        band_merge = np.where(
            band_luse == 4, band_sat, band_merge
        )

        if log_peddy:  # 是否為稻作期間
            # 水田補注量減少, 牛踏層
            # peddy_decline_ratio
            band_temp = band_sat * peddy_decline_ratio

            if band_vege is None:
                # 直接使用內政部定義的水田資訊
                band_merge = np.where(
                    band_luse == 3, band_temp, band_merge
                )
            else:
                # 以衛星影像辨識成果, 歷年動態變化
                band_merge = np.where(
                    band_vege == 1.0, band_temp, band_merge
                )
        else:
            band_merge = np.where(
                band_luse == 3, band_unsat, band_merge
            )

        root_logger = kwargs.get("root_logger", None)
        if root_logger is not None:
            root_logger.debug(
                "    |--> 飽和入滲量 per cell: {} / {} (最大值/平均值)".format(
                    round(np.nanmax(band_sat), 4),
                    round(np.nanmean(band_sat), 4),
                )
            )
            root_logger.debug(
                "    |--> 未飽和入滲量 per cell: {} / {} (最大值/平均值)".format(
                    round(np.nanmax(band_unsat), 4),
                    round(np.nanmean(band_unsat), 4),
                )
            )
            root_logger.debug(
                "    |--> 總補注量: {}, 立方公尺/旬".format(
                    round(np.sum(band_merge), 0)
                )
            )
        return band_merge


# @jit
def rch_process_core(
    rch_nc_fname: str,
    band_sum: np.ndarray,
    # df_flist: pd.DataFrame,
    # df_flist_previus: pd.DataFrame,
    dt: datetime.datetime,
    t: int,
    mat_size: int,
    tlength: float,
    col_flags: List,
    luse_nc_fname: str,
    soil_nc_fname: str,
    cell_area: float,
    peddy_decline_ratio: float,
    VEGE_info,
    log_filter: bool,
    # log_debug: bool = False,
    **kwargs,
):
    """
    Core of RCH Process
    """
    # 全域變數
    # pylint: disable=global-statement
    # pylint: disable=global-variable-not-assigned
    global band_is_land
    global log_interpolate
    global df_GridInfo_regular
    global grid_x_refine
    global grid_y_refine
    global xlist_refine
    global ylist_refine

    root_logger = kwargs.get("root_logger", None)
    root_logger.info(
        " -- {} ({}/{})".format(
            dt.strftime("%Y-%m-%d"), t, mat_size
        )
    )
    # 區分不同地下水區的補注量計算成果
    # 檔案不存在, 才執行下列程式碼

    """
    # 加總旬雨量資料
    (
        band_sum,
        _xlist_rain,
        _ylist_rain,
    ) = calc_rainsum_tendays(df_flist, log_filter, **kwargs)
    if band_sum.shape[0] == 0:
        # 表示缺乏本期雨量資料
        # 改以前期雨量資料
        # 加總旬雨量資料
        #(
        #    band_sum,
        #    _xlist_rain,
        #    _ylist_rain,
        #) = calc_rainsum_tendays(
        #    df_flist_previus, log_filter, **kwargs
        #)
    """
    band_sum = np.where(
        band_is_land, band_sum, np.NaN
    )  # 排除海上部份

    # 雨量內插
    if log_interpolate:
        band_sum = interpolate_refine(
            band_sum,
            df_GridInfo_regular,
            grid_x_refine,
            grid_y_refine,
            col_flags,
            method="nearest",
        )

        # 判斷是否為水稻田種植期
        log_peddy = dt.month in list(
            aggr_mapping["month"].values
        )
        vege_fname = ""
        if log_peddy:
            if len(VEGE_info[1]) > 0:
                # 水田種植期, 需要另外處理
                # 以衛星影像辨識之作物資料進行處理
                aggr_mapping_sub: pd.DataFrame = aggr_mapping[
                    aggr_mapping["month"] == dt.month
                ]

                vege_period = aggr_mapping_sub.loc[
                    aggr_mapping_sub.index[0], "aggr_index"
                ]
                # 這是考慮衛星影像辨識成果
                vege_fname = os.path.join(
                    VEGE_info[2],
                    "vege",
                    "vege_{}_{}{}.nc".format(
                        VEGE_info[0], dt.year, vege_period
                    ),
                )
                if root_logger is not None:
                    root_logger.debug(
                        "VEGE: {}".format(vege_fname)
                    )

        # 從降雨量計算降雨入滲量
        band_infil = calc_recharge_infil(
            band_sum,
            xlist_refine,
            ylist_refine,
            tlength,
            luse_nc_fname,
            soil_nc_fname,
            cell_area,
            log_peddy,
            peddy_decline_ratio,
            vege_fname,
            # log_debug=log_debug,
            **kwargs,
        )
        if root_logger is not None:
            root_logger.debug(
                "     |--> Export RCH NetCDF: {}".format(
                    rch_nc_fname
                )
            )

        NA.export_netcdf(
            [ylist_refine, xlist_refine],
            band_infil,
            ("Y", "X"),
            rch_nc_fname,
            dtype="f4",
        )


# @jit
def rch_process_shell(flag: List):
    """
    Shell of RCH process
    """
    rch_nc_fname = flag[0]
    band_sum = flag[1]
    dt = flag[2]
    tlength = flag[3]
    col_flags = flag[4]
    luse_nc_fname = flag[5]
    soil_nc_fname = flag[6]
    cell_area = flag[7]
    peddy_decline_ratio = flag[8]
    VEGE_info = flag[9]
    log_filter = flag[10]
    # log_debug = flag[11]
    root_logger = flag[11]
    t = flag[12]
    mat_size = flag[13]

    # 逐旬處理

    rch_process_core(
        rch_nc_fname,
        band_sum,
        # df_flist,
        # df_flist_previus,
        dt,
        t,
        mat_size,
        tlength,
        col_flags,
        luse_nc_fname,
        soil_nc_fname,
        cell_area,
        peddy_decline_ratio,
        VEGE_info,
        log_filter,
        # log_debug=log_debug,
        root_logger=root_logger,
    )


@jit
def determine_time_code(yy: int, mm: int) -> str:
    """
    輸入年與月, 回傳 yyyymm 的字串
    """
    assert isinstance(yy, int)
    assert isinstance(mm, int)
    assert 1000 <= yy <= 9999
    assert 1 <= mm <= 12

    time_code = "{}{:0>2}".format(yy, mm)
    return time_code


def determine_specific_season_flist(
    y, mindex, season_month, log_seasonal=False, **kwargs
) -> List:
    """
    找尋對應季節的資料檔案
    """
    time_filter_code = []
    if log_seasonal:
        # 四季
        # 一季共有9旬
        for n in range(len(season_month[mindex][0])):
            time_filter_code.append(
                determine_time_code(
                    y, season_month[mindex][0][n]
                )
            )

    else:
        time_filter_code.append(determine_time_code(y, mindex))

    # 搜尋符合 time_code 的檔案清單
    # debug, 20210902 08:20 AM
    # 加入功能排除粗網格的檔案, 末尾加入 _c.nc 等字樣作為區隔
    flist2 = [
        fut.filter_search_result(
            fut.search_files_in_dir(
                os.path.join(
                    gdal_sf.data_path,
                    "rch",
                    gdal_sf.proj_name,
                ),
                regular_flags="{}[012*]1".format(
                    TFC
                ),  # 鎖定特定年/月
                log_recursive=True,
                log_debug=False,
            ),
            regular_flags="_c.nc",  # 末尾為 _c.nc
            log_reverse=True,  # 排除符合條件者, 即粗網格
            log_debug=False,
        )
        for TFC in time_filter_code
    ]

    flist = []
    for fl in flist2:
        flist += fl

    root_logger = kwargs.get("root_logger", None)
    if root_logger is not None:
        root_logger.debug(flist)
    return flist


def plot_year(
    flag: List, fig_style="ggplot", figsize=(9, 8)
):  # noqa: C901
    """
    繪製補注量四季累積圖
    """
    y = flag[0]
    nrow = flag[1]
    ncol = flag[2]
    gdal_sf = flag[3]
    season_month = flag[4]
    extent2 = flag[5]
    log_seasonal = flag[6]
    root_logger = flag[7]

    if root_logger is not None:
        root_logger.debug("Calculating Y: {}".format(y))
        root_logger.debug(
            "Create subplots size: ({}, {})".format(
                nrow,
                ncol,
            )
        )
    try:
        plt.style.use(fig_style)
        _fig, axs = plt.subplots(
            nrow, ncol, figsize=figsize, sharex=True, sharey=True
        )

        recharge_annual: Union[int, float] = 0
        ax_count = 0
        background = [
            {
                "gis_data": town_shp,
                "linestyle": "--",
                "edgecolor": "dimgrey",
                "facecolor": "none",
                "linewidth": 0.5,
                "alpha": 0.6,
            },
            {
                "gis_data": county_shp,
                "edgecolor": "k",
                "facecolor": "none",
                "linewidth": 1,
                "alpha": 0.6,
            },
        ]
        fig_fname = os.path.join(
            gdal_sf.data_path,
            "rch",
            gdal_sf.proj_name,
            "plot",
            "rch{}".format(y),
        )
        for m in range(nrow * ncol):
            # 找出對應季節的所屬檔案
            flist = determine_specific_season_flist(
                y,
                m,
                season_month,
                log_seasonal,
                root_logger=root_logger,
            )

            # loading grid data
            # accumulating 補注量
            band: Optional[np.ndarray] = None
            for n in range(len(flist)):
                # loading nc file for RCH
                _ylist, _xlist, band_temp = NA.read_ncband(
                    flist[n]
                )
                if band is None:
                    band = band_temp
                else:
                    band += band_temp  # 加總
            recharge_annual += np.nansum(band)  # 年補注量

            if band is not None:
                # 排除無資料的年度, 進行繪圖
                ax2 = plt_parameters.get_subax(axs, m)
                ax_index = determine_ax_index(
                    m, nrow, ncol, log_debug=False
                )  # (int((m - m % ncol) / ncol), m % ncol)
                xlabel = None
                ylabel = None
                if ax_index[1] == 0:
                    ylabel = "Y座標"
                if ax_index[0] == nrow - 1:
                    xlabel = "X座標"
                log_cbar = ax_index[1] == ncol - 1
                log_cbar = False

                # title = "第 {} 旬".format(m + 1)
                dt = datetime.datetime(y, m + 1, 1, 1, 1, 1, 1)
                title: str = ""
                if log_seasonal:
                    title = "{}-{}".format(y, season_month[m][1])
                else:
                    title = dt.strftime("%Y-%m")

                # 排除海洋
                band = np.where(
                    band_is_land_refine, band, np.NaN
                )

                # 定義背景檔案, 與繪圖方式
                vlimit = cmap_define.data_limits[
                    "precipitation_tendays"
                ]

                ax2 = plot_grid(
                    band / 4,
                    ax2,
                    extent=extent2,
                    title=title,
                    vmin=vlimit[0],
                    vmax=vlimit[1],
                    xlabel=xlabel,
                    ylabel=ylabel,
                    cbar_label=r"累積補注量 $cm$",
                    log_cbar=log_cbar,
                    background=background,
                )
                ax_count += 1

        print(y, np.round(recharge_annual, 2))
        if ax_count > 0:
            plt.tight_layout()
            jut.save_fig(fig_fname)
            if root_logger is not None:
                root_logger.debug(
                    "Export fig: {}".format(
                        fig_fname,
                    )
                )
    except AttributeError as e:
        # AttributeError: 'AnchoredSizeLocator' object has no attribute 'get_subplotspec'
        # pass
        raise AttributeError() from e
    return recharge_annual, fig_fname


def figure_animator(fig_mat, flags: List):
    """
    建立 gif 動畫
    """
    gdal_sf = flags[0][3]
    gif_fname = os.path.join(
        gdal_sf.data_path,
        "rch",
        gdal_sf.proj_name,
        "plot",
        "rch_plot.gif",
    )
    gif_utility.gif_create(
        fig_mat, gif_fname, resize=0.8, duration=1
    )


def rainfall_data_loading(
    date_range: Tuple, rainfall_params: Dict, **kwargs
) -> np.ndarray:
    """
    loading rainfall data
    date_range: (start_date, end_date)
    rainfall_params = {
        "ver1": {
            "data_path": ,
            "df_flist": ,
        }
        "ver2": {}
    }

    優先使用 ver2, 其次為 ver1
    1. ver2: load data, & return
    2. ver1: load data
        加總一旬數據, 並回傳
    """
    # ver 2

    df_current = TSB.Slice_TSData(
        rainfall_params["ver2"]["df_flist"],
        date_range[0],
        date_range[1],
    )
    band_sum = np.ndarray([])
    if df_current.shape[0] > 0:
        # 有資料, 採用 ver2
        assert df_current.shape[0] == 1
        nc_content = NA.read_ncband(
            df_current.loc[df_current.index[0], "fname"]
        )
        band_sum = nc_content[-1]
        if log_filter:
            global filter_code
            band_sum = CI.filter_band(band_sum, filter_code[1:])
        band_sum = np.where(
            band_sum < -1e5, np.NaN, band_sum
        )  # 刪除 極小負值
    else:
        df_current = TSB.Slice_TSData(  # 篩選一旬資料
            rainfall_params["ver1"]["df_flist"],
            rng_tendays[t].strftime("%Y-%m-%d"),
            rng_tendays[t + 1].strftime("%Y-%m-%d"),
        )
        (
            band_sum,
            _xlist_rain,
            _ylist_rain,
        ) = calc_rainsum_tendays(
            df_current, log_filter, **kwargs
        )
    return band_sum


if __name__ == "__main__":  # noqa=C901
    root_logger.info(
        "RCH. Calculation with superfile: {}".format(
            sf_fname,
        )
    )
    mytcs = tcs.TimeConsume()

    gdal_sf = GDAL_SF.GDAL_SF(
        sf_fname,
        root_logger=root_logger,
        proj_logger_append=True,
    )

    luse_nc_fname = os.path.join(
        gdal_sf.data_path, "luse_{}.nc".format(gdal_sf.proj_name)
    )
    soil_nc_fname = os.path.join(
        gdal_sf.data_path,
        "soil_{}_significant.nc".format(gdal_sf.proj_name),
    )
    gr_path = gdal_sf.GRPATH

    gdal_sf.proj_logger.debug(
        "    |--> 土地利用={}, 表層土壤={}".format(
            luse_nc_fname,
            soil_nc_fname,
        )
    )

    # 定義網格資訊
    (
        minx,
        maxx,
        miny,
        maxy,
        resx,
        resy,
        numx,
        numy,
    ) = CI.define_tics(CI.readcell_inform(gdal_sf.coor_fname))
    extent = [minx, maxx, miny, maxy]
    gdal_sf.proj_logger.debug(
        "    |--> cell extent: {}".format(extent)
    )

    # 網格降雨相關資訊
    flist = fut.search_files_in_dir(
        gr_path, regular_flags="GridInfo.h5"
    )
    grinfo_fname = flist[0]
    # 存成全域變數
    df_GridInfo_regular = pd.read_hdf(grinfo_fname)

    # 篩選研究區域內的格點
    if log_filter:
        df_GridInfo_regular = filter_DataFrame(
            df_GridInfo_regular, extent, ["TWD97_X", "TWD97_Y"]
        )
    filter_code = [
        [None, None],
        [
            df_GridInfo_regular["ROW"].min(),
            df_GridInfo_regular["ROW"].max() + 1,
        ],
        [
            df_GridInfo_regular["COL"].min(),
            df_GridInfo_regular["COL"].max() + 1,
        ],
    ]
    extent2 = [
        df_GridInfo_regular["lon"].min(),
        df_GridInfo_regular["lon"].max(),
        df_GridInfo_regular["lat"].max(),
        df_GridInfo_regular["lat"].min(),
    ]
    gdal_sf.proj_logger.debug(
        "    |--> 網格降雨路徑={}, 網格降雨網格資訊={}".format(
            gr_path,
            df_GridInfo_regular.head(),
        )
    )

    # 內插網格之相關資訊
    # 細網格資訊
    # 存成全域變數
    mycell = CI.cell_utility(
        gdal_sf.coor_fname, log_add_rightend=True
    )
    grid_x_refine = mycell.grid_x
    grid_y_refine = mycell.grid_y
    xlist_refine = mycell.xlist
    ylist_refine = mycell.ylist
    gdal_sf.proj_logger.debug(
        "    |--> 細網格資訊: {}".format(
            grid_x_refine.shape,
        )
    )

    # 確認是否為陸地
    flist_is_land = fut.search_files_in_dir(
        gr_path, regular_flags="is_land"
    )
    if len(flist_is_land) > 0:
        # 讀取陸地與海洋的分隔
        gdal_sf.proj_logger.debug(
            "{}: 讀取海陸網格資訊".format(
                jut.get_code_location()
            )
        )
        _ylist, _xlist, band_is_land = NA.read_ncband(
            flist_is_land[0]
        )
        print(band_is_land.shape)

        if log_filter:
            band_is_land = CI.filter_band(
                band_is_land,
                filter_code[1:],
                root_logger=gdal_sf.proj_logger,
            )
    # 內插
    band_is_land_refine = interpolate_refine(
        band_is_land,
        df_GridInfo_regular,
        grid_x_refine,
        grid_y_refine,
        ["TWD97_X", "TWD97_Y"],
        method="nearest",
    )
    band_is_land_refine = np.where(
        band_is_land_refine >= 0.5, True, False
    )

    # ================================================================
    # ================================================================
    # ================================================================
    # 讀取土壤參數
    # 單位 mm/day
    df_sat, df_ri, df_unsat = rch_coef.read_coef(
        *gdal_sf.INFIL  # , log_debug=log_debug
    )
    df_unsat = df_unsat.set_index(["RI_index", "Soil_index"])

    rng = pd.date_range(
        start="{}-1-1".format(str_year),
        end="{}-1-1".format(end_year + 1),
        freq="1y",
    )
    rng_tendays = TA.calc_period36(str_year, end_year).index

    # 把所有的網格降雨資料
    # 解析檔案時間, 排序
    flist = fut.search_files_in_dir(gr_path, log_recursive=True)
    mat = []

    #######################################################################
    def fname_phrase(
        whole_fname: str,
    ) -> Optional[datetime.datetime]:
        """
        分析檔案名稱, 並回傳對應的時間

        檔名 whole_fname
        """
        try:
            assert isinstance(whole_fname, str)
        except AssertionError as e:
            raise TypeError(
                "whole_name 型別錯誤! 應為 str, 實際為 {}".format(
                    type(whole_fname)
                )
            ) from e

        # 從檔案名稱, 解析對應的時間
        if whole_fname.find("grd_") >= 0:
            # 排除 grd_xxxxxxxx.nc 已完成的日資料
            return None
        sepline = os.path.basename(whole_fname).split(".")

        for length in [
            12,  # "%Y%m%d%H%M",
            10,  # "%Y%m%d%H",
            8,  # "%Y%m%d",
        ]:
            return time_phrase.time_phrase_multi(
                sepline[0][-length:]
            )
        # 完全不符合格式
        return None

    #######################################################################
    df_flist = pd.DataFrame(flist, columns=["fname"])
    df_flist.loc[:, "time"] = [
        fname_phrase(
            df_flist.loc[index, "fname"]
        )  # 解析對應的時間
        for index in df_flist.index
        # if df_flist.loc[index, "fname"].find(".nc") >= 0
    ]
    df_flist = df_flist.set_index("time")
    # 時間排序
    df_flist = df_flist.sort_index()
    df_flist = df_flist[~df_flist.index.isnull()]

    # 另外建立搜尋路徑
    flist = fut.search_files_in_dir(
        "../../hydro_process/workspace/ciot_grid/",
        regular_flags="nc",
        log_recursive=True,
    )
    df_flist2 = pd.DataFrame(flist, columns=["fname"])
    df_flist2.loc[:, "time"] = [
        datetime.datetime.strptime(
            os.path.basename(df_flist2.loc[index, "fname"])
            .split(".")[0]
            .split("_")[1],
            "%Y%m%d",
        )  # 解析對應的時間
        for index in df_flist2.index
    ]
    df_flist2 = df_flist2.set_index("time").sort_index()
    # df_flist 為舊版的降雨資料
    # df_flist2 為新版的降雨資料 (旬雨量)
    rainfall_params = {
        "ver1": {
            "data_path": gr_path,
            "df_flist": df_flist,
        },
        "ver2": {
            "data_path": "../../hydro_process/workspace/ciot_grid/",
            "df_flist": df_flist2,
        },
    }
    ########################################################

    flags = []
    df_flist_previus = None
    df_flist_current = None
    for t in range(rng_tendays.shape[0] - 1):
        rch_nc_fname = os.path.join(
            gdal_sf.data_path,
            "rch",
            gdal_sf.proj_name,
            "rch{}_{}.nc".format(
                rng_tendays[t].strftime("%Y%m%d"),
                gdal_sf.proj_name,
            ),
        )

        if not os.path.exists(rch_nc_fname):
            # 判斷 rch_nc_fname 是否存在, 如存在, 跳過
            # data load for 旬雨量
            # 優先從 ver2, 如無再從 ver1 處理
            log_continuous = True
            shift = 0
            while log_continuous:
                band_sum = rainfall_data_loading(
                    (
                        rng_tendays[t - shift],
                        rng_tendays[t + 1 - shift],
                    ),
                    rainfall_params,
                    root_logger=gdal_sf.proj_logger,
                )
                if band_sum.shape[0] == band_is_land.shape[0]:
                    if (
                        band_sum.shape[1]
                        == band_is_land.shape[1]
                    ):
                        log_continuous = False
                        # 如果不符合, 則往前回溯
                shift += 1

            flag = [
                rch_nc_fname,
                band_sum,
                rng_tendays[t],  # dt
                (
                    rng_tendays[t + 1] - rng_tendays[t]
                ).days,  # 天數, tlength
                ["TWD97_X", "TWD97_Y"],
                luse_nc_fname,
                soil_nc_fname,
                400.0,  # cell_area
                peddy_decline_ratio,
                [
                    gdal_sf.proj_name,
                    gdal_sf.VEGE,
                    gdal_sf.data_path,
                ],
                log_filter,
                # log_debug,
                gdal_sf.proj_logger,
                t,
                rng_tendays.shape[0] - 1,
            ]

            if t == 0:
                # 排除第一次, 以非平行方式計算
                rch_process_shell(flag)
            else:
                if log_parallel_process:
                    flags.append(flag)
                else:
                    # 循序計算
                    rch_process_shell(flag)

        if log_parallel_process:
            cpu_item = get_cpu_info()
            # 計算可用的 CPU 數量
            process_size = max(
                int(float(cpu_item["count"]) * 0.4), 1
            )
            with Pool(processes=process_size) as pool:
                pool.map(rch_process_shell, flags)
                pool.close()  # Close the pool to not create new process
                pool.join()  # Make main process to wait for the pool

    ##################################################################
    ##################################################################
    # 繪圖
    ##################################################################
    ##################################################################
    min_year = rng_tendays[0].year
    max_year = rng_tendays[-1].year
    fig = None
    ax = None
    nrow = 4
    ncol = 3
    season_month: List = [
        [[1, 2, 3], "S1"],
        [[4, 5, 6], "S2"],
        [[7, 8, 9], "S3"],
        [[10, 11, 12], "S4"],
    ]
    if log_seasonal:
        nrow = 2
        ncol = 2
    extent2 = [
        df_GridInfo_regular["TWD97_X"].min(),
        df_GridInfo_regular["TWD97_X"].max(),
        df_GridInfo_regular["TWD97_Y"].max(),
        df_GridInfo_regular["TWD97_Y"].min(),
    ]

    flags = []
    for y in range(min_year, max_year):
        # 繪製補注量四季累計圖
        flags.append(
            [
                y,
                nrow,
                ncol,
                gdal_sf,
                season_month,
                extent2,
                log_seasonal,
                gdal_sf.proj_logger,
            ]
        )

    if log_parallel_process:
        # if False:
        cpu_item = get_cpu_info()
        # 計算可用的 CPU 數量
        process_size = max(
            int(float(cpu_item["count"]) * 0.25), 1
        )
        with Pool(processes=process_size) as pool:
            mat = pool.map(plot_year, flags)
            pool.close()  # Close the pool to not create new process
            pool.join()  # Make main process to wait for the pool
    else:
        for flag in flags:
            mat.append(plot_year(flag))  # 循序計算

    mat_recharge = [elem[0] for elem in mat]  # 補注量
    mat_fig = [
        "{}.png".format(elem[1]) for elem in mat
    ]  # 補注量
    figure_animator(mat_fig, flags)

    df_recharge = pd.DataFrame(
        mat_recharge,
        columns=["recharge"],
        index=range(min_year, max_year),
    )
    csv_fname = os.path.join(
        gdal_sf.data_path,
        "rch",
        "{}_recharge_annual.csv".format(gdal_sf.proj_name),
    )
    df_recharge.to_csv(csv_fname)
    print(df_recharge)
    mytcs.TimeConsume_Calc("RCH 計算 (網格降雨 + NC file)")
    mytcs.export(ind_level=1, ind_width=2, prefix_label="--> ")
