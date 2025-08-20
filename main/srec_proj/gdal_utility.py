# -*- coding: utf-8 -*-
import os
from numba import jit
import numpy as np
import pandas as pd
import dbfread
from typing import List, Union, Optional
import matplotlib.pyplot as plt
from matplotlib.colors import (
    BoundaryNorm,
    LinearSegmentedColormap,
)

# import superfile as sf
import netcdf_analysis as NCA
import cell_inform as CI
import jutility as jut
import file_utility as fut


class WrongUsageError(Exception):
    """
    專用於錯誤使用
    """

    # pylint: disable=unnecessary-pass
    pass


def check_attr_exist(shp_fname: str, attr: str) -> bool:
    """
    檢查 shape file 是否有該欄位
    """
    assert isinstance(shp_fname, str)
    assert os.path.exists(shp_fname)
    dbf_fname = shp_fname.replace(".shp", ".dbf")
    check_result = False
    with dbfread.DBF(dbf_fname) as table:
        for record in table:
            check_result = attr in record.keys()
            return check_result
    return check_result


def read_gdal_plot_param(
    plot_param, A_color: Union[int, float] = 255 * 0.8
):
    # 採取 RGBA 系統
    # A_color 0 - 255
    mat = []
    # pylint: disable=bad-option-value
    # pylint: disable=unspecified-encoding
    with open(plot_param, "r") as f:
        lines = f.readlines()

        for line in lines:
            if line[0] != "#":  # 排除 # 註解符號者
                sepline = line.rstrip().split()

                if len(sepline) >= 4:
                    sub_mat = (
                        np.array(
                            [
                                float(sepline[1]),
                                float(sepline[2]),
                                float(sepline[3]),
                                float(A_color),
                            ]
                        )
                        / 255
                    )
                    if sepline[0] == "nv":
                        mat.append([np.NaN] + sub_mat.tolist())
                    else:
                        mat.append(
                            [float(sepline[0])]
                            + sub_mat.tolist()
                        )
    df_gdal_plot_param = pd.DataFrame(
        mat, columns=["burn_index", "R", "G", "B", "A"]
    ).sort_values(by=["burn_index"])

    # 排除 np.NaN 者, 即 nv
    df = df_gdal_plot_param[
        ~df_gdal_plot_param["burn_index"].isnull()
    ]
    cmaplist = df.iloc[:, 1:].values.tolist()

    cmap = LinearSegmentedColormap.from_list(
        "Custom cmap", cmaplist, len(cmaplist)
    )
    # define the bins and normalize
    bounds = np.linspace(0, len(cmaplist), len(cmaplist) + 1)
    norm = BoundaryNorm(bounds, len(cmaplist))
    return cmap, norm, df_gdal_plot_param


def determine_extent(xlist, ylist):
    return [
        np.min(xlist),
        np.max(xlist),
        np.min(ylist),
        np.max(ylist),
    ]


def pharse_specific_item(sepline: str):
    # 去除等號前面的資訊
    sepline2 = sepline.split("=")
    result = sepline2[1]
    return result


# pylint: disable=too-many-branches
def pharse_plot_param(plot_param: str):
    """
    phrase plot argv
    """
    sepline = plot_param.split("&")
    argv_params = {}
    flag_list = [
        "cmap",
        "color",
        "edgecolor",
        "facecolor",
        "linewidth",
        "markersize",
        "alpha",
        "filter",
    ]
    for argv in sepline:
        for flag in flag_list:
            if argv.find(flag) >= 0:
                argv_params[flag] = pharse_specific_item(argv)

            if flag == "filter":
                sepline = argv_params[flag].split(":")
                sepline2 = sepline[1].split(",")
                filter_code_result = [sepline[0], sepline2]

                if "filter_code" not in argv_params.keys():
                    argv_params["filter_code"] = []
                argv_params["filter_code"].append(
                    filter_code_result
                )

    return (
        argv_params[flag]
        for flag in [
            "cmap",
            "color",
            "edgecolor",
            "facecolor",
            "linewidth",
            "markersize",
            "alpha",
            "filter_code",
        ]
    )


def filter_process(df, filter_code):
    for i in range(len(filter_code)):
        print(i, filter_code[i])
        # 外部條件, 採用 and
        mask = None
        for j in range(len(filter_code[i][1])):
            # 內部條件, 採用 or
            mask_sub = (
                df[filter_code[i][0]] == filter_code[i][1][j]
            )

            if mask is None:
                mask = mask_sub.copy()
            else:
                mask = np.logical_or(mask, mask_sub)
        df = df.loc[mask, :]
    return df


class gdal_utility:
    """
    專門用於 gdal file 的操作
    """

    def __init__(
        self,
        nc_fname: str,
        ci_file: Union[str, List, CI.cell_utility],
        log_add_rightend: bool = False,
        **kwargs,
    ):
        """
        初始化
        設定 nc_fname, 設定檔
        ci_file: 可以是設定檔, 也可以是 list
            # cell_inform = [
            #   [min_x, max_x, delta_x],
            #   [min_y, max_y, delta_y],
            # ]
        """
        self.root_logger = kwargs.get("root_logger", None)
        if self.root_logger is not None:
            self.root_logger.debug(
                "Initialization of GDAL Operation"
            )
        self.nc_fname = nc_fname
        # self.sfo = sf.sf_object(sf_file, log_debug=log_debug)

        try:
            assert isinstance(
                ci_file, (str, list, CI.cell_utility)
            )
            if isinstance(ci_file, CI.cell_utility):
                ci_file = ci_file.cell_inform
        except AssertionError as e:
            raise TypeError(
                "{} / type={}".format(ci_file, type(ci_file))
            ) from e

        if isinstance(ci_file, (str, list)):
            self.cell_inform = CI.cell_utility(
                ci_file, log_add_rightend=log_add_rightend
            )

        else:
            # CI.cell_utility
            # 直接設定 cell_inform
            self.cell_inform = ci_file
        if self.root_logger is not None:
            self.root_logger.debug(
                "網格設定:{}".format(self.cell_inform)
            )

        self.band_data: np.ndarray = np.zeros(1)
        self.xlist: np.ndarray = np.zeros(1)
        self.ylist: np.ndarray = np.zeros(1)

    @jit
    def get_band_size(self) -> int:
        return self.xlist.shape[0] * self.ylist.shape[0]

    def load_band_data(
        self,
        nc_fname_in: str,
        log_keep_first: bool = False,
    ):
        # 讀取資料
        try:
            assert isinstance(nc_fname_in, str)
            assert os.path.exists(nc_fname_in)
            ncband = NCA.read_ncband(nc_fname_in)
        except (OSError, AssertionError) as e:
            raise OSError(nc_fname_in) from e
        # 限定二維陣列
        assert len(ncband) == 3
        band1 = ncband[-1]
        self.xlist = ncband[-2]
        self.ylist = ncband[-3]

        # 攔住 -1e9 以下的部份, 視為 np.NaN
        band1 = np.where(band1 < -1e9, np.NaN, band1)
        """
        if self.root_logger is not None:
            # pylint: disable=unsubscriptable-object
            message = "           |--> Number of Non-nan Cells: {} / {}".format(
                np.count_nonzero(~np.isnan(band1)),
                band1.shape[0] * band1.shape[1],
            )
            self.root_logger.debug(message)
        """

        if self.band_data.shape[0] == 1:
            self.band_data = band1
        else:
            if log_keep_first:  # 保留 first
                self.band_data = np.where(
                    ~np.isnan(self.band_data),
                    self.band_data,
                    band1,
                )
            else:
                self.band_data = np.where(
                    ~np.isnan(band1), band1, self.band_data
                )

            if self.root_logger is not None:
                self.root_logger.debug(
                    "           |--> log_keep_first: {}".format(
                        log_keep_first
                    )
                )
                # self.root_logger.debug(
                #    "           |--> Number of Non-nan Cells (彙整後): {} / {}".format(
                #        np.count_nonzero(~np.isnan(band2)),
                #        band2.shape[0] * band2.shape[1],
                #    )
                # )

    def export_band_data(
        self,
        nc_fname_export: str,
        log_debug: bool = False,
        dtype: str = "f4",
    ):
        """
        輸出成為 nc file
        2D array
        """

        if log_debug:
            print(self.xlist.shape, self.ylist.shape)
            print(self.xlist[:5])
            print(self.ylist[:5])
            print(self.band_data.shape)

        if not os.path.exists(os.path.dirname(nc_fname_export)):
            # 建立目錄
            os.makedirs(os.path.dirname(nc_fname_export))
        NCA.export_netcdf(
            (self.ylist, self.xlist),
            self.band_data,
            ("Y", "X"),
            nc_fname_export,
            dtype=dtype,
        )

    def clip_band_data(self, nc_fname_bnd: str):
        # pylint: disable=unused-variable
        ncband = NCA.read_ncband(
            nc_fname_bnd
        )  # debug, read_ncband 新格式, 以 tuple 拋出
        self.band_data = np.where(
            ~np.isnan(ncband[-1]), self.band_data, np.NaN
        )

    def plot_gdaldem(
        self,
        plot_param: str,
        plot_fname=None,
        log_exec: bool = True,
    ):
        """
        # gdaldem 繪圖
        plot_param: 代表繪圖的塗色檔案
        log_exec: 代表要不要執行, 還是只是純粹輸出指令
        """
        if self.root_logger is not None:
            self.root_logger.debug(
                "      |--> Process: {}".format("plot_gdaldem")
            )
            self.root_logger.debug(
                "           Source NC fname: {}".format(
                    self.nc_fname
                )
            )
            self.root_logger.debug(
                "           plot fname: {}".format(plot_fname)
            )
            self.root_logger.debug(
                "           plot parameter file: {}".format(
                    plot_param
                )
            )
            self.root_logger.debug(
                os.system("cat {}".format(plot_param))
            )

        if plot_fname is None:
            plot_fname = self.nc_fname.split(".")[
                0
            ]  # 去除副檔名
            plot_fname += "_test"
            plot_fname += ".png"
            if self.root_logger is not None:
                self.root_logger.debug(
                    "           plot fname (處理後): {}".format(
                        plot_fname
                    )
                )

        command = "gdaldem color-relief {} {} {}".format(
            self.nc_fname, plot_param, plot_fname
        )
        if self.root_logger is not None:
            self.root_logger.debug(
                "           command: {}".format(command)
            )
            self.root_logger.debug(
                "           log_exec: {}".format(log_exec)
            )
            self.root_logger.debug(
                "           Export file: {}".format(plot_fname)
            )
        if log_exec:
            os.system(command)
        return command

    def plot_GIS_data(
        self,
        plot_params: pd.DataFrame,
        ax,
        fig_title=None,
        # log_debug: bool = False,
        ticks=None,
        ticklabels=None,
        # _shp_proj=None,
        alpha: Union[float, int] = 0.8,
        log_inverse_yaxis: bool = False,
        **kwargs,
    ):
        """
        # 繪圖, 以 mapplotlib 執行
        # 匯出 self.band_data 的數值
        plot_param: 代表繪圖的塗色檔案
        log_exec: 代表要不要執行, 還是只是純粹輸出指令
        """
        # 過時引數
        # log_debug & _shp_proj
        jut.check_outdated_IO(
            "log_debug", "無使用 log_debug", **kwargs
        )
        jut.check_outdated_IO(
            "_shp_proj", "無使用 _shp_proj", **kwargs
        )
        assert isinstance(
            plot_params, pd.DataFrame
        ), "Wrong type: {}".format(type(plot_params))
        if self.root_logger is not None:
            self.root_logger.debug(
                "      |--> Process: {}".format("plot_GIS_data")
            )

        # 讀取 plot_param, 建立 cmap & norm
        plot_params.loc[:, "A"] = alpha * 255
        plot_params2 = plot_params.copy()
        for column in ["R", "G", "B", "A"]:
            if plot_params2.loc[:, column].max() > 1:
                plot_params2.loc[:, column] /= 255
        cmaplist = [
            tuple(
                plot_params2.loc[
                    index, ["R", "G", "B", "A"]
                ].values
            )
            for index in plot_params.index
            if plot_params2.loc[index, "burn_index"]
            != "nv"  # 排除 nv
        ]
        cmap = LinearSegmentedColormap.from_list(
            "Custom cmap", cmaplist, len(cmaplist)
        )
        # define the bins and normalize
        # bounds = np.linspace(0, len(cmaplist), len(cmaplist) + 1)
        bounds = [
            plot_params.loc[index, "burn_index"]
            for index in plot_params.index
            if plot_params.loc[index, "burn_index"]
            != "nv"  # 排除 nv
        ]
        bounds += [np.max(bounds) + 1]
        norm = BoundaryNorm(np.array(bounds), len(cmaplist))
        extent = determine_extent(self.xlist, self.ylist)

        # 校正超出範圍的數值
        try:
            burn_index = plot_params.loc[
                :, "burn_index"
            ].tolist()
            burn_index = [
                elem
                for elem in burn_index
                if isinstance(elem, (int, float))
            ]
            bi_max = np.max(burn_index)
            bi_min = np.min(burn_index)
        except KeyError as e:
            raise KeyError(plot_params.head()) from e
        band_data2 = np.where(
            self.band_data > bi_max, np.NaN, self.band_data
        )
        band_data2 = np.where(
            band_data2 < bi_min, np.NaN, band_data2
        )

        # fig, ax = plt.subplots(1, figsize=figsize)
        # 南方TWD97Y < 北方TWD97_Y
        # band 的上方實質上為南方, 反之為北方
        # 以 invert_yaxis() 反轉
        # 因此 extent 的 bottom 應為 maxy; top 則為 miny
        extent2 = extent.copy()
        if log_inverse_yaxis:
            extent2[2:] = [extent[3], extent[2]]
        im = ax.imshow(
            band_data2,
            extent=extent2,
            cmap=cmap,
            norm=norm,
            interpolation="nearest",
        )
        if log_inverse_yaxis:
            ax.invert_yaxis()  # 繪圖, 南北歸回正常

        cbar = plt.colorbar(
            im, ax=ax, ticks=ticks, cmap=cmap, norm=norm
        )
        if ticklabels is not None:
            cbar.ax.set_yticklabels(
                ticklabels, fontsize=9
            )  # vertically oriented colorbar

        ax.set_xlabel("X")
        ax.set_ylabel("Y")
        ax.grid()
        if fig_title is None:
            fig_title = self.nc_fname
        ax.set_title(fig_title, fontsize=12)
        return ax

    def gdal_rasterizing(  # noqa: C901
        self,
        shp_fname: str,
        burn_index: Optional[int] = None,
        attribute: Optional[str] = None,
        log_exec: bool = True,
        log_quiet: bool = False,
        **kwargs,
    ) -> str:
        """
        將 shape file 轉換成為 raster file
        # 第一種使用方式
        設定 burn_index, 使得 polygon 所在位置, 寫入 burn_index 數值, 其餘則為 np.NaN

        # 第二種使用方式
        設定 attribute
        以該欄位的內容, 作為 raster file 的數值

        # 不可同時設定兩種, 或是兩種都不設定
        """

        # pylint: disable=broad-except
        root_logger = kwargs.get("root_logger", None)
        try:
            # pylint: disable=no-else-raise
            # ^ 代表 xor, 兩者僅能設定一個
            log1 = burn_index is None
            log2 = attribute is None
            assert (
                log1 ^ log2
            )  # 不可同時設定兩種, 或是兩種都不設定
        except AssertionError as e:
            message = "!!! Process {}: '{}' and '{}' 僅能擇一輸入".format(
                "gdal_rasterizing", burn_index, attribute
            )
            if root_logger is not None:
                root_logger.error(message)
            raise WrongUsageError(message) from e

        try:
            assert os.path.exists(shp_fname)
        except AssertionError as e:
            message = (
                "!!! shapefile '{}' does not exist!".format(
                    shp_fname
                )
            )
            if root_logger is not None:
                root_logger.error(message, exc_info=True)
            raise FileNotFoundError(message) from e

        # command: str = "LC_ALL=C.UTF-8 ;"
        # pylint: disable=no-else-raise
        command = "gdal_rasterize -of netCDF -ot Int32"
        if burn_index is not None:
            command += " -burn {} ".format(burn_index)
        elif attribute is not None:
            # 檢查 attribute 是否存在
            assert check_attr_exist(
                shp_fname, attribute
            ), "attribute '{}' doesn't exist in '{}'".format(
                attribute,
                shp_fname,
            )
            command += " -a {} ".format(attribute)

        # 座標
        # pylint: disable=unused-variable
        command += "-te {} {} {} {} ".format(
            self.cell_inform.minx,
            self.cell_inform.miny,
            self.cell_inform.maxx,
            self.cell_inform.maxy,
        )
        command += "-a_srs EPSG:3826 -tr {} {} {} {}".format(
            self.cell_inform.resx,
            self.cell_inform.resy,
            shp_fname,
            self.nc_fname,
        )
        # > /dev/null, 如正常執行, 關閉螢幕輸出
        if log_quiet:
            command += " > /dev/null"

        if root_logger is not None:
            message = "Command: {}".format(command)
            root_logger.debug(message)
        if log_exec:
            log_success = jut.system_call_running(
                command, root_logger=root_logger
            )
            if log_success:
                """
                事後確認是否有產出 nc file
                """
                try:
                    assert os.path.exists(self.nc_fname)
                except AssertionError as e:
                    raise FileNotFoundError(
                        "'{}' is not successfully generated (from command='{}')".format(
                            self.nc_fname,
                            command,
                        )
                    ) from e
            else:
                # 失敗
                raise jut.SystemCallFail(
                    "'gdal_rasterizing' fail for parameters: {} / {} / {}".format(
                        shp_fname,
                        burn_index,
                        attribute,
                    )
                )
        return command

    # 讀取土地利用重新歸類資料
    # @jit, 無法使用
    def param_regroup(
        self,
        regroup_fname: str,
        nc_fname_export: Optional[str] = None,
        # log_debug: bool = False,
        **kwargs,
    ):
        jut.check_outdated_IO(
            "log_debug",
            "'log_debug'已過時，改用root_logger",
            **kwargs,
        )
        # index 重新歸類
        # regroup_fname: 定義重歸類的定義檔
        # nc_fname_export: 輸出檔名
        if kwargs.get("root_logger", None) is not None:
            kwargs["root_logger"].debug(
                "GIS 重歸類: {} --> {}".format(
                    regroup_fname, nc_fname_export
                )
            )

        if nc_fname_export is None:
            sepline = self.nc_fname.split(".")
            nc_fname_export = (
                sepline[0] + "_0." + sepline[1]
            )  # 定義輸出檔名

        mat = []
        # pylint: disable=bad-option-value
        # pylint: disable=unspecified-encoding
        with open(regroup_fname, "r") as f:
            lines = f.readlines()
            for line in lines:
                sepline = (
                    line.rstrip().split()
                )  # 去除 \n 符號 & split
                if len(sepline) > 0:
                    mat.append(
                        [int(sepline[0]), int(sepline[1])]
                    )

        # 儲存成 pandas DataFrame
        columns = ["luse_index", "regroup_index"]
        df_luse_regroup = pd.DataFrame(mat, columns=columns)
        if kwargs.get("root_logger", None) is not None:
            kwargs["root_logger"].debug(
                "重歸類資訊: {}".format(df_luse_regroup.head())
            )

        # 重新載入資訊
        band_data_regroup = -np.ones(self.band_data.shape)
        for i in range(df_luse_regroup.shape[0]):
            index1 = df_luse_regroup.iloc[i, 0]
            index2 = df_luse_regroup.iloc[i, 1]
            if kwargs.get("root_logger", None) is not None:
                kwargs["root_logger"].debug(
                    "重新歸類: {} --> {}".format(index1, index2)
                )
            band_data_regroup = np.where(
                self.band_data == index1,
                index2,
                band_data_regroup,
            )
        self.band_data = band_data_regroup

    # 代表性土壤計算
    # @jit
    def significant_soil(self, soil_list: List):
        """
        計算代表性土壤,
        土壤入滲率排序: 11, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        # 以最小入滲率土壤，作為代表性土壤
        """

        soil_significant = -np.ones(
            list(soil_list[0].band_data.shape)
        )
        soil_list2 = []
        for m in range(len(soil_list)):
            # 將 11 改為 0
            soil_list2.append(
                np.where(
                    soil_list[m].band_data == 0,
                    -1,
                    soil_list[m].band_data,
                )
            )
            soil_list2[-1] = np.where(
                soil_list2[-1] == 11, 0, soil_list2[-1]
            )
            # print (m, stats.describe(soil_list[m].band_data.reshape((-1, )), nan_policy='omit'))

        for m in range(len(soil_list)):
            soil_significant = np.where(
                soil_significant < soil_list2[m],
                soil_list2[m],
                soil_significant,
            )

        # 0 改回 11
        soil_significant = np.where(
            soil_significant == 0, 11, soil_significant
        )
        # -1 改為 0
        soil_significant = np.where(
            soil_significant == -1, 0, soil_significant
        )
        self.band_data = soil_significant


def shp_rasterize_shell(flag: List):
    shp_fname = flag[0]
    # import_path = flag[1]
    nc_fname = flag[2]
    coor_fname = flag[3]
    attri = flag[4]
    burn_index = flag[5]
    log_check_exists = True
    _log_debug = True  # noqa: F841
    root_logger = None
    log_quiet = False
    log_add_rightend = False
    if len(flag) >= 8:
        log_check_exists = flag[6]
        # log_debug = flag[7]
        root_logger = flag[7]
        log_add_rightend = flag[8]
        try:
            log_quiet = flag[9]
        except IndexError:
            pass

    if not os.path.exists(nc_fname):
        # 檔案不存在才要處理
        return shp_rasterize(
            shp_fname,
            # import_path,
            nc_fname,
            coor_fname,
            attri,
            burn_index,
            log_check_exists=log_check_exists,
            root_logger=root_logger,
            log_quiet=log_quiet,
            log_add_rightend=log_add_rightend,
        )
    return nc_fname


def shp_rasterize(
    shp_fname: str,
    # import_path: str,
    nc_fname: str,
    coor_fname: str,
    attri: str = "",
    burn_index: int = -99999,
    log_check_exists: bool = False,
    log_add_rightend: bool = False,
    **kwargs,
) -> str:
    """
    Vector-based GIS file rasterization
    shp_fname: 為檔案源頭
    export_path: 輸出路徑
    attri: 欄位名稱
    coor_fname: 網格切割設定檔案

    # shp_fname: source shape file
    # nc_fname: export nc file

    回傳: 建立好的 nc_fname
    """
    root_logger = kwargs.get("root_logger", None)

    log_run = True
    if log_check_exists:  # 啟用確認檔案存在
        # 檔案是否存在
        # 不存在才計算
        log_run = not os.path.exists(nc_fname)
    message = "Rasterization: from {} to {}".format(
        shp_fname,
        nc_fname,
    )
    if not kwargs.get("log_quiet", True):
        print(message)
    if root_logger is not None:
        root_logger.debug(message)
        root_logger.debug(
            "  --> log_check={}, log_exists={}, log_run={}, log_quiet={}".format(
                log_check_exists,
                os.path.exists(nc_fname),
                log_run,
                kwargs.get("log_quiet", True),
            )
        )

    if log_run:
        os.makedirs(os.path.dirname(nc_fname), exist_ok=True)
        gdal_nc = gdal_utility(
            nc_fname,
            coor_fname,
            root_logger=root_logger,
            log_add_rightend=log_add_rightend,
        )
        if attri != "":
            _command: str = gdal_nc.gdal_rasterizing(
                shp_fname, attribute=attri, **kwargs
            )
        elif burn_index != -99999:
            _command = gdal_nc.gdal_rasterizing(
                shp_fname, burn_index=burn_index, **kwargs
            )
        else:
            if root_logger is not None:
                message = "attri: {} / burn_index: {}".format(
                    attri, burn_index
                )
                root_logger.error(
                    "TypeError: {}".format(message)
                )
            raise TypeError(message)

        if root_logger is not None:
            for flag in ["GDAL_DATA", "PROJ_LIB"]:
                root_logger.debug(
                    "OS environment parameters '{}': {}".format(
                        flag,
                        os.environ[flag],
                    )
                )
            root_logger.debug("Command: {}".format(_command))

    return nc_fname


def nc_merge(nc_flist: List, cell_define: str, **kwargs):
    """
    透過 GDAL 功能, merge 多個 raster file
    """
    try:
        assert isinstance(nc_flist, list)
        assert len(nc_flist) > 0  # 避免是空 list
    except AssertionError as e:
        print(type(nc_flist))
        raise TypeError("nc_flist: {}".format(nc_flist)) from e
    gdal_nc = gdal_utility(nc_flist[0], cell_define, **kwargs)
    for _, nc_fname in enumerate(nc_flist):
        gdal_nc.load_band_data(nc_fname, log_keep_first=False)

    message = "Merge NC files: from {}".format(
        nc_flist,
    )
    if not kwargs.get("log_quiet", True):
        print(message)
    if kwargs.get("root_logger", None) is not None:
        kwargs["root_logger"].debug(message)
        kwargs["root_logger"].debug(
            "cell define: {}".format(cell_define)
        )
    if kwargs.get("log_remove", False):
        # merge之後, 刪除原本檔案
        fut.remove_file(nc_flist)
        for nc_fname in nc_flist:
            fut.remove_empty_folder(os.path.dirname(nc_fname))

    return gdal_nc
