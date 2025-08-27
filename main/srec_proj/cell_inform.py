# -*- coding: utf-8 -*-
from numba import jit
import numpy as np
import numpy.typing as npt
import pandas as pd
import copy
import os
from typing import List, Tuple, Union, Dict, Optional, TypedDict
from scipy.interpolate import griddata
from pykrige.ok import OrdinaryKriging
from pykrige.uk import UniversalKriging
from shapely.geometry import Polygon
import geopandas as gpd
import jutility as jut
import warnings

warnings.filterwarnings("ignore", category=DeprecationWarning)


def determine_index_core(
    edges_array: np.ndarray, coor_val: Union[float, int]
) -> int:
    """
    回傳edges 兩端包覆的 index
    """
    assert isinstance(coor_val, (int, float))
    assert isinstance(edges_array, np.ndarray)
    multiply_result = (edges_array[:-1] - coor_val) * (
        edges_array[1:] - coor_val
    )
    arg_result = np.argwhere(
        multiply_result <= 0
    )  # 找出小於 0 者
    if arg_result.shape[0] > 0:
        return int(arg_result[0])
    return -999


def readcell_inform(cell_fname: str, **kwargs) -> List:
    """
    讀取 cell 定義資訊
    """
    try:
        assert isinstance(cell_fname, str)
        assert os.path.exists(cell_fname)
    except AssertionError as e:
        raise AssertionError(
            "{} / {} / {}".format(
                cell_fname,
                type(cell_fname),
                os.path.exists(cell_fname),
            )
        ) from e

    cell_inform: List = []
    # pylint: disable=unspecified-encoding
    with open(cell_fname, "r") as f:
        sepline = f.readline().split()

        def val_transform(val_str: str) -> Union[int, float]:
            myval: Union[int, float] = 0
            try:
                myval = int(val_str)
            except ValueError:
                myval = float(val_str)
            return myval

        # X
        min_x = val_transform(sepline[0]) + kwargs.get(
            "shink", 0.0
        )
        max_x = val_transform(sepline[1]) - kwargs.get(
            "shink", 0.0
        )
        delta_x = val_transform(sepline[2])
        # Y
        sepline = f.readline().split()
        min_y = val_transform(sepline[0]) + kwargs.get(
            "shink", 0.0
        )
        max_y = val_transform(sepline[1]) - kwargs.get(
            "shink", 0.0
        )
        delta_y = val_transform(sepline[2])

        cell_inform = [[min_x, max_x, delta_x]]
        cell_inform.append([min_y, max_y, delta_y])
    return cell_inform


def edge_create(
    coor_inform: List, log_add_rightend: bool = False, **_kwargs
) -> np.ndarray:
    """
    針對單一維度, 計算 edge

    log_add_rightend = True
    針對右端, 如若恰好數值一致, 額外增列右端節點
    """
    # 預設
    edge = np.arange(*tuple(coor_inform))
    if log_add_rightend:
        if (
            abs(coor_inform[1] - coor_inform[0]) % coor_inform[2]
        ) == 0.0:
            edge = np.arange(
                coor_inform[0],
                coor_inform[1] + coor_inform[2] * 0.1,
                coor_inform[2],
            )
    return edge


@jit
def determine_coor_list(edges: np.ndarray) -> np.ndarray:
    """
    依據 edge, 計算 cell 中心
    """
    coor_list = (edges[:-1] + edges[1:]) / 2
    return coor_list


def cell_inform_transform(
    cell_fname: Union[Tuple, str, List], **kwargs
) -> List:
    """
    多型別檔案名稱轉換

    shink: float = 0.002, 縮減範圍
    """
    assert isinstance(cell_fname, (tuple, str, list))

    if isinstance(cell_fname, tuple):
        cell_fname = list(cell_fname)[0]

    cell_inform: List = []
    if isinstance(cell_fname, str):
        assert os.path.exists(
            cell_fname
        ), "'{}' doesn't exists!".format(cell_fname)
        # 檔名
        cell_inform = readcell_inform(cell_fname, **kwargs)
    elif isinstance(cell_fname, list):
        assert len(cell_fname) == 2
        for i in range(len(cell_fname)):
            assert isinstance(cell_fname[i], list)
            assert len(cell_fname[i]) == 3
        cell_inform = cell_fname
    return cell_inform


class cell_utility:
    """
    Cell Utility

    log_add_rightend = True,
        針對右端, 如若恰好數值一致, 額外增列右端節點
    """

    def __init__(
        self,
        cell_fname: Union[str, List, Dict, Tuple],
        **kwargs,
    ):
        """
        讀取 cell_fname

        **kwargs
        log_add_rightend = True
        針對右端, 如若恰好數值一致, 額外增列右端節點

        log_yreverse = True
        南北互換

        """
        root_logger = kwargs.get("root_logger", None)
        if root_logger is not None:
            root_logger.debug(
                {
                    flag: elem
                    for flag, elem in kwargs.items()
                    # if flag not in ["root_logger"]
                }
            )
        try:
            assert isinstance(
                cell_fname, (str, list, dict, tuple)
            )
        except AssertionError as e:
            message = "!!! type is {} / {}".format(
                type(cell_fname), cell_fname
            )
            if root_logger is not None:
                root_logger.error(
                    message,
                    exc_info=True,
                )
            raise TypeError(message) from e

        self.xedges: npt.NDArray[np.float32] = np.array([])
        self.yedges: npt.NDArray[np.float32] = np.array([])
        self.xlist: npt.NDArray[np.float32] = np.array([])
        self.ylist: npt.NDArray[np.float32] = np.array([])
        self.xwidth: npt.NDArray[np.float32] = np.array([])
        self.ywidth: npt.NDArray[np.float32] = np.array([])
        self.grid_y: np.ndarray = np.array([])
        self.grid_x: np.ndarray = np.array([])
        self.cell_center = np.array([])
        self.cell_setup(cell_fname, **kwargs)


    def define_edges(self, **kwargs):
        """
        由 cell_inform 定義 edges
        """
        self.xedges = edge_create(self.cell_inform[0], **kwargs)
        self.yedges = edge_create(self.cell_inform[1], **kwargs)

    def __str__(self):
        """
        return 描述訊息
        """
        message = "Cell_inform: {}\n".format(self.cell_inform)
        message += "    --> xlist: {}\n".format(
            pd.DataFrame(self.xlist, columns=["X"])
        )
        message += "    --> ylist: {}\n".format(
            pd.DataFrame(self.ylist, columns=["Y"])
        )
        message += "    --> number: ({}, {})".format(
            self.numx, self.numy
        )
        return message

    def cell_setup(
        self, cell_fname: Union[str, List, Dict, Tuple], **kwargs
    ):
        """
        Load Cell Inform
        log_yreverse: bool = False,
        log_yreverse = True: 南北互換

        shink: float = 0.002, 縮減範圍
        """
        root_logger = kwargs.get("root_logger", None)
        if kwargs.get("root_logger", None) is not None:
            kwargs.get("root_logger", None).debug("Cell Setup")

        assert isinstance(cell_fname, (str, list, dict, tuple))
        log_yreverse = kwargs.get("log_yreverse", False)

        if root_logger is not None:
            root_logger.debug(
                "  --> Cell Inform Type (str, list, tuple): {}".format(
                    type(cell_fname)
                )
            )
        # 多種形式的 cell_inform 定義
        # str, 從檔案讀取
        # list 則直接設定,
        self.cell_inform = cell_inform_transform(
            cell_fname, **kwargs
        )

        # 相關參數
        (self.minx, self.maxx, self.resx) = tuple(
            self.cell_inform[0]
        )
        (self.miny, self.maxy, self.resy) = tuple(
            self.cell_inform[1]
        )

        # 定義 edge
        self.define_edges(**kwargs)
        # 定義 xlist & ylist
        self.xlist = determine_coor_list(self.xedges)
        self.ylist = determine_coor_list(self.yedges)

        self.extent = [
            self.minx,
            self.maxx,
            self.miny,
            self.maxy,
        ]
        self.cell_center = np.array(
            [
                (self.minx + self.maxx) / 2,
                (self.miny + self.maxy) / 2,
            ]
        )
        if root_logger is not None:
            root_logger.debug(
                "  --> extent: {}".format(self.extent)
            )
            root_logger.debug(
                "  --> log Y reverse: {}".format(log_yreverse)
            )

        if log_yreverse:
            self.extent = [
                self.minx,
                self.maxx,
                self.maxy,
                self.miny,
            ]
        self.numx = self.xlist.shape[0]
        self.numy = self.ylist.shape[0]

        if log_yreverse:
            # 南北互換, 由北往南編排, 從數量大者開始
            self.ylist = np.array(
                sorted(list(self.ylist), reverse=True)
            )
            self.yedges = np.array(
                sorted(list(self.yedges), reverse=True)
            )

        self.xwidth = np.array(
            [
                abs(self.xedges[i + 1] - self.xedges[i])
                for i in range(self.xedges.shape[0] - 1)
            ]
        )
        self.ywidth = np.array(
            [
                abs(self.yedges[j + 1] - self.yedges[j])
                for j in range(self.yedges.shape[0] - 1)
            ]
        )
        self.grid_area = np.zeros(
            (self.ywidth.shape[0], self.xwidth.shape[0])
        )
        for j, ywidth in enumerate(self.ywidth):
            self.grid_area[j, :] = ywidth * self.xwidth

        self.maxx = np.max(self.xedges)
        self.maxy = np.max(self.yedges)
        self.minx = np.min(self.xedges)
        self.miny = np.min(self.yedges)

        # 定義 grid
        self.determine_grid(**kwargs)  # grid_x & grid_y

    def check_incell(self, loc, **kwargs) -> bool:
        result = np.array(
            [
                (loc[0] - self.extent[0])
                * (loc[0] - self.extent[1])
                <= 0,
                (loc[1] - self.extent[2])
                * (loc[1] - self.extent[3])
                <= 0,
            ]
        )
        return bool(np.all(result))

    def determine_grid(self, **kwargs):
        """
        定義 grid_x & grid_y
        """
        if kwargs.get("root_logger", None) is not None:
            kwargs.get("root_logger", None).debug(
                "Determine Grid"
            )
        self.grid_y, self.grid_x = np.meshgrid(
            self.ylist, self.xlist, indexing="ij"
        )

    def determine_index(self, loc: Tuple, **kwargs):
        """
        loc 為坐標
        找到對應的 loc index
        """
        assert isinstance(loc, tuple), "{} / {}".format(
            loc, type(loc)
        )

        loc_index = (
            determine_index_core(self.xedges, loc[0]),
            determine_index_core(self.yedges, loc[1]),
        )
        return loc_index



def interpolate_scipy(
    points, vals, grid_x, grid_y, method="nearest"
) -> np.ndarray:
    """
    Scipy griddata 的 Interface
    """
    assert isinstance(points, (list, np.ndarray))
    assert isinstance(vals, (list, np.ndarray))

    if isinstance(vals, list):
        assert len(vals) > 5, "{}: {} / {}".format(
            len(vals),
            points[: min(5, len(vals))],
            vals[: min(5, len(vals))],
        )
    elif isinstance(vals, np.ndarray):
        assert vals.shape[0] > 5
    assert grid_x.shape == grid_y.shape

    grid_z = griddata(
        points, vals, (grid_x, grid_y), method=method
    )
    return grid_z


def interpolate_kriging(
    points: np.ndarray,
    vals: np.ndarray,
    xlist,
    ylist,
    method="OrdinaryKriging",
    variogram_model="linear",
    **kwargs,
):
    """
    Interpolate using Kriging

    linear, power, gaussian, spherical, exponential, hole-effect.
    """
    # kriging 系列 model construction
    try:
        assert isinstance(points, np.ndarray)
        assert isinstance(vals, np.ndarray)
    except AssertionError as e:
        vector_size = 0
        try:
            vector_size = points.shape[0]
        except TypeError:
            vector_size = len(points)

        raise TypeError(
            "{} ({}) | {} ({})".format(
                points[: min(10, vector_size)],
                type(points),
                vals[: min(10, vector_size)],
                type(vals),
            )
        ) from e

    root_logger = kwargs.get("root_logger", None)
    variogram_list = [
        "linear",
        "power",
        "gaussian",
        "spherical",
        "exponential",
        "hole-effect",
    ]
    try:
        assert variogram_model in variogram_list
    except AssertionError as e:
        raise AssertionError(
            "!!! '{}' not in  {}".format(
                variogram_model,
                variogram_list,
            )
        ) from e

    # func = None
    func = OrdinaryKriging
    if method == "OrdinaryKriging":
        func = OrdinaryKriging
    elif method == "UniversalKriging":
        func = UniversalKriging

    kriging_model = None
    if "kriging_model" in kwargs.keys():
        # 設定前期計算成果
        kriging_model = kwargs["kriging_model"]
    if kriging_model is None:
        # 若無才建立 varigram model
        try:
            kriging_model = func(
                points[:, 0],
                points[:, 1],
                vals,
                variogram_model=variogram_model,
                verbose=False,
                enable_plotting=False,
            )
        except ValueError as e:
            # ValueError: Each lower bound must be strictly less than each upper bound.

            # 因為輸入的 vals 全部為相同數值, 導致指數型模型在 varigram 的上下限均為 0
            # 回傳 jut.KrigingFail 的錯誤, 跳過這個部份
            if root_logger is not None:
                root_logger.error(
                    "Kriging model creation fail", exc_info=True
                )
            raise jut.KrigingFail(
                "Kriging model creation fail"
            ) from e

    grid_z_interpolate, _ss = kriging_model.execute(
        "grid",
        xlist,
        ylist,
    )
    return grid_z_interpolate, kriging_model
