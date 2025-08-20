# -*- coding: utf-8 -*-
import netCDF4 as nc4
import numpy as np
import pandas as pd
from numba import jit
import warnings
from typing import List, Dict, Tuple, Union
import os

warnings.filterwarnings("ignore")

# pylint: disable=no-member
log_show = False


# 展示 nc dimension info
def show_nc_dimensions(fr, log_debug: bool = False):
    assert isinstance(log_debug, bool)
    if log_debug:
        print(fr.dimensions)  # 欄位


def show_ncband_describe(
    nc_fname: str, **kwargs
) -> pd.DataFrame:
    fr = nc4.Dataset(nc_fname, "r")  # 讀取資料
    df_describe = get_dimension_size(fr.dimensions)  # keys
    fr.close()

    if kwargs.get("log_debug", False):
        print(
            "      |--> Process: {}".format(
                "NCA:show_ncband_describe"
            )
        )
        print("           檔案: {}".format(nc_fname))
        print("           欄位名稱: \n{}".format(df_describe))
        show_nc_dimensions(
            fr, **kwargs
        )  # 展示 nc dimension info
    return df_describe


# @jit
def get_dimension_name(fr) -> List:
    """
    回傳 ncband 的 dimemsions 欄位名稱
    """
    # dimensions = fr.dimensions
    keys_dim = list(fr.dimensions.keys())
    return keys_dim


# @jit
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


@jit
def get_dimension_size(dimensions: Dict) -> pd.DataFrame:
    """
    回傳 ncband 的欄位名稱
    """

    mat = []
    keys = get_dimension_name(dimensions)
    for i in range(len(keys)):
        mat.append([keys[i], dimensions[keys[i]].size])

    df = pd.DataFrame(mat, columns=["name", "size"])
    return df


# @jit
def read_ncband_vname(nc_fname) -> List:
    """
    讀取 nc band, 回傳 variable names
    """
    vname_list = []
    with nc4.Dataset(nc_fname, "r") as fr:  # 讀取資料
        vname_list = get_dimension_name(fr)
    vname_list.reverse()  # 反轉順序

    return vname_list


# @jit, 不可使用 jit
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


# 讀取 nc band (x, y, t, band1)
@jit
def read_ncband_withtime(nc_fname, log_debug: bool = False):
    fr = nc4.Dataset(nc_fname, "r")  # 讀取資料
    keys_dim = get_dimension_name(fr)
    xlist = np.array(fr.variables[keys_dim[0]])
    ylist = np.array(fr.variables[keys_dim[1]])
    tlist = np.array(fr.variables[keys_dim[2]])
    band1 = np.array(fr.variables["Band1"])
    if log_debug:
        print(
            " -- read_ncband_withtime:",
            xlist.shape,
            ylist.shape,
            tlist.shape,
            band1.shape,
        )
        show_nc_dimensions(
            fr, log_debug=log_debug
        )  # 展示 nc dimension info
        print("X: {}".format(xlist[:5]))
        print("Y: {}".format(ylist[:5]))
        print("T: {}".format(tlist[:5]))
    fr.close()
    return xlist, ylist, tlist, band1


@jit
def translate_nc4_pd(xlist, ylist, band1):
    xv, yv = np.meshgrid(xlist, ylist)
    mat_shape = band1.shape[0] * band1.shape[1]
    data = {
        "lon": xv.reshape(mat_shape),
        "lat": yv.reshape(mat_shape),
        "band1": band1.reshape(mat_shape),
    }
    df_band1 = pd.DataFrame.from_dict(data)
    return df_band1


def assert_netcdf_dimension_consist(
    dimension_list: Union[List, Tuple],
    grid_val: np.ndarray,
    vname_list: Union[List, Tuple],
):
    """
    dimension_list 為單一維度的向量清單 [Zvec, Yvec, Xvec]
        Zvec, Yvec, Xvec 為 np.ndarray
    grid_val 為陣列 (Zsize, Ysize, Xsize)
    vname_list 為變數名稱 [Zname, Yname, Xname]

    確認維度設定均一致
    """
    try:
        assert isinstance(dimension_list, (list, tuple))
        assert isinstance(grid_val, np.ndarray)
        assert isinstance(vname_list, (list, tuple))
    except AssertionError as e:
        raise TypeError(
            "{} / {} / {}".format(
                type(dimension_list),
                type(grid_val),
                type(vname_list),
            )
        ) from e

    try:
        # 確認陣列維度一致
        assert len(dimension_list) == len(grid_val.shape)
        for dim in range(len(dimension_list)):
            assert isinstance(dimension_list[dim], np.ndarray)
            assert (
                dimension_list[dim].shape[0]
                == grid_val.shape[dim]
            )
        assert len(dimension_list) == len(vname_list)
        assert len(dimension_list) < 5
    except AssertionError as e:
        raise ValueError(
            "The dimensions of arg. are not consist! {} / {} / {}".format(
                len(dimension_list),
                vname_list,
                grid_val.shape,
            )
        ) from e

    dimension_size = len(dimension_list)  # 維度尺寸
    # 確認陣列尺寸大小
    for i in range(dimension_size):
        try:
            assert (
                dimension_list[i].shape[0] == grid_val.shape[i]
            )
        except AssertionError as e:
            message = "The dimension doesn't consist! dimension_list: ("
            for j in range(dimension_size):
                message += "{}".format(
                    dimension_list[j].shape[0]
                )
                if j < dimension_size - 1:
                    message += ", "
                else:
                    message += "), "
            message += "shape of grid_val: {}".format(
                grid_val.shape
            )
            raise ValueError(message) from e


# pylint: disable=too-many-branches
def export_netcdf(
    dimension_list: Union[List, Tuple],
    grid_val: np.ndarray,
    vname_list: Union[List, Tuple],
    export_fname: str,  # 輸出檔名
    dtype: Union[str, List] = "f4",
):
    """
    以 netCDF4 輸出
    dimension_list 為單一維度的向量清單 [Zvec, Yvec, Xvec]
        Zvec, Yvec, Xvec 為 np.ndarray
    grid_val 為陣列 (Zsize, Ysize, Xsize)
    vname_list 為變數名稱 [Zname, Yname, Xname]

    三者順序一致
    dtype 資料型態, 預設為 f4, float; 如要 double, 則為 f8
    輸入過程為 X, Y, Z 來
    """
    # 處理維度尺寸確認
    assert_netcdf_dimension_consist(
        dimension_list,
        grid_val,
        vname_list,
    )
    assert isinstance(dtype, (str, list))
    if isinstance(dtype, list):
        assert len(dimension_list) == len(dtype) + 1

    if not os.path.exists(os.path.dirname(export_fname)):
        try:
            # 如果是目錄, 則建立目錄
            # 處理檔案路徑
            os.makedirs(os.path.dirname(export_fname))
        except FileNotFoundError:
            # FileNotFoundError: [Errno 2] No such file or directory: ''
            pass

    dimension_size = len(dimension_list)  # 維度尺寸
    with nc4.Dataset(export_fname, "w", format="NETCDF4") as fr:
        mat = []
        for i in range(dimension_size):
            # j = dimension_size - i - 1
            j = i
            # Create Dimension
            fr.createDimension(
                vname_list[j], dimension_list[j].shape[0]
            )
            dtype2 = dtype
            if isinstance(dtype, list):
                dtype2 = dtype[i]
            mat.append(
                fr.createVariable(
                    vname_list[j],
                    dtype2,
                    (vname_list[j],),
                    zlib=True,
                )
            )

            # Assign variable
            mat[i][:] = dimension_list[j]

        dtype2 = dtype
        if isinstance(dtype, list):
            dtype2 = dtype[-1]
        mat.append(
            fr.createVariable(
                "Band1",
                dtype2,
                tuple(vname_list),
                compression="zlib",
            )
        )
        for ds in range(dimension_size):
            assert (
                dimension_list[ds].shape[0] == grid_val.shape[ds]
            )

        # Assign variable
        if dimension_size == 2:
            mat[-1][:, :] = grid_val
        elif dimension_size == 3:
            mat[-1][:, :, :] = grid_val
        elif dimension_size == 4:
            mat[-1][:, :, :, :] = grid_val


# ===================================================================
# ===================================================================
# ===================================================================
# 二維陣列運算, 不同形式的 replace 指令
# replace 指令之基礎框架
@jit
def replace_mat2D(
    mat_old, sval_new, sval_location: np.ndarray
) -> np.ndarray:
    """
    sval_location 為 argwhere 之產物
    sval_new 為替代值
    """
    assert isinstance(sval_location, np.ndarray)
    assert sval_location.shape[1] == 2  # 二維陣列

    mat_new = np.array(mat_old)
    for i in range(sval_location.shape[0]):
        mat_new[sval_location[i, 0], sval_location[i, 1]] = (
            sval_new
        )
    return mat_new


# 輸入含NaN陣列，以特定數值取代，並回傳新陣列
@jit
def replace_NaN2D(mat_old, sval_new) -> np.ndarray:
    """
    將 mat 中的 np.NaN 替代成 sval_new
    """
    sval_location = np.argwhere(np.isnan(mat_old))
    return replace_mat2D(mat_old, sval_new, sval_location)


# 輸入含NaN陣列，以特定數值取代，並回傳新陣列
@jit
def replace_negative2D(mat_old, sval_new):
    sval_location = np.argwhere(mat_old < 0)
    return replace_mat2D(mat_old, sval_new, sval_location)


# 偵測不合理數值，以NaN取代
# 並回傳新陣列
@jit
def replace_sval2D(mat_old, sval, sval_new):
    sval_location = np.argwhere(mat_old == sval)
    return replace_mat2D(mat_old, sval_new, sval_location)


@jit
def replace_sval2D_min(mat_old, sval, sval_new):
    sval_location = np.argwhere(mat_old < sval)
    return replace_mat2D(mat_old, sval_new, sval_location)


# ===================================================================
# ===================================================================
# ===================================================================
