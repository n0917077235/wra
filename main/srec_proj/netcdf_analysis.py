# -*- coding: utf-8 -*-
import netCDF4 as nc4
import numpy as np
from numba import jit
import warnings
from typing import List, Dict, Tuple, Union
import os

warnings.filterwarnings("ignore")

# pylint: disable=no-member

# 展示 nc dimension info
def show_nc_dimensions(fr, log_debug: bool = False):
    assert isinstance(log_debug, bool)
    if log_debug:
        print(fr.dimensions)  # 欄位


def get_dimension_name(fr) -> List:
    """
    回傳 ncband 的 dimemsions 欄位名稱
    """
    # dimensions = fr.dimensions
    keys_dim = list(fr.dimensions.keys())
    return keys_dim


def get_band_name(fr) -> List:
    """
    回傳 band 變數名稱

    取出所有變量, 扣除 get_dimension_name(fr) 部分
    """
    band_names = set(fr.variables.keys())
    # print (band_names)
    band_names -= set(get_dimension_name(fr))
    band_names -= set(["transverse_mercator"])
    return list(band_names)


def read_ncband(nc_fname: str, **kwargs) -> Tuple:
    """
    讀取 nc band (x, y, t, band1)
    回傳:
        長度為 N 的 list
        0 - N-2: 為各維度的網格值, N-2 為維度大小
        N-1 or -1: 為 grid 陣列值
    """
    try:
        assert isinstance(nc_fname, str)
        assert os.path.exists(nc_fname)  # 檢查檔案是否存在
    except AssertionError as e:
        raise FileNotFoundError(
            "Does '{}' exist? {} / {}".format(
                nc_fname,
                os.path.exists(nc_fname),
                type(nc_fname),
            )
        ) from e

    nc_content = []
    with nc4.Dataset(nc_fname, "r") as fr:  # 讀取資料
        show_nc_dimensions(fr, **kwargs)

        keys_dim = get_dimension_name(fr)
        band_names = get_band_name(fr)
        for key in keys_dim:
            nc_content.append(np.array(fr.variables[key]))
        nc_content.reverse()

        for key in band_names:
            nc_content.append(np.array(fr.variables[key]))
    return tuple(nc_content)
