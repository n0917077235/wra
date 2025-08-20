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

