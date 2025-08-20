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


# @jit, 不可使用 jit
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


def grep_mf_coordinates(
    dis,
    xu: float = 0,
    yu: float = 0,
) -> Dict:
    """
    MF dis 為獨立座標系統
    (xu, yu) 為整體網格座標系統之最左上角 tics (not central brock)
    """
    xedges = xu + np.array(  # 網格邊
        [0] + list(np.cumsum(dis.delr.array))
    )
    yedges = yu - np.array([0] + list(np.cumsum(dis.delc.array)))

    xlist = (xedges[:-1] + xedges[1:]) / 2  # 中心點座標
    ylist = (yedges[:-1] + yedges[1:]) / 2
    mf_cell = {
        "xedges": xedges,
        "yedges": yedges,
        "xlist": xlist,
        "ylist": ylist,
    }
    return mf_cell


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

    def cell_refine(self, refine_ratio: int):
        """
        網格加密
        refine_ratio 為加密倍數
        """
        assert isinstance(refine_ratio, int)
        assert refine_ratio > 0

        # 定義 refine 後的 cell_fname, 以 Dict 定義
        cell_fname_refined = {}

        def refine_process(edge_list, refine_ratio):
            """
            等比例切割
            """
            refine_edge_list = [edge_list[0]]
            for i in range(1, edge_list.shape[0]):
                length = (
                    edge_list[i] - edge_list[i - 1]
                ) / refine_ratio
                for _ in range(refine_ratio):
                    refine_edge_list.append(
                        refine_edge_list[-1] + length
                    )
            return np.array(refine_edge_list)

        cell_fname_refined["xedges"] = refine_process(
            self.xedges, refine_ratio
        )
        cell_fname_refined["yedges"] = refine_process(
            self.yedges, refine_ratio
        )
        self.cell_setup(cell_fname_refined)
        return self

    def trim_cell_inform(self, extent_trim: List) -> List:
        """
        輸入 extent_trim = [xmin, xmax, ymin, ymax]
            其範圍小於原有的 cell_utility
        return 範圍內的網格編號
        """
        argx = np.argwhere(
            np.logical_and(
                self.xlist >= extent_trim[0],
                self.xlist < extent_trim[1],
            ),
        )
        argy = np.argwhere(
            np.logical_and(
                self.xlist >= extent_trim[0],
                self.xlist < extent_trim[1],
            ),
        )
        print(argx)
        print(argy)
        # sys.exit()
        return [argx, argy]

    def select_cell_ticks(self, ticks: List) -> Tuple:
        """
        輸入 ticks, 從中取得網格資訊

        ticks = [xmin, xmax, ymin, ymax]
            其範圍小於 cell
        return (xindex, yindex)
            xlist[xindex] & ylist[yindex] 位於 ticks 內
            另外, xindex & yindex 格式為 np.ndarray
        """
        assert isinstance(ticks, list)
        assert len(ticks) == 4

        xindex = np.argwhere(
            np.logical_and(
                self.xlist >= ticks[0],
                self.xlist < ticks[1],
            )
        ).reshape(-1)
        yindex = np.argwhere(
            np.logical_and(
                self.ylist >= ticks[2],
                self.ylist < ticks[3],
            )
        ).reshape(-1)
        return xindex, yindex

    def index2list(
        self, xindex: np.ndarray, yindex: np.ndarray
    ) -> Tuple:
        """
        輸入 Xindex & Yindex
        回傳選上的 xlist & ylist
        """
        selected_xlist = np.array(
            [self.xlist[i] for i in xindex]
        )
        selected_ylist = np.array(
            [self.ylist[j] for j in yindex]
        )
        return selected_xlist, selected_ylist

    def index2edge(
        self, xindex: np.ndarray, yindex: np.ndarray
    ) -> Tuple:
        """
        輸入 Xindex & Yindex
        回傳選上的 xlist & ylist 的兩側 edge
        備注: 由於 edge 為兩側, 因此回傳數量會是 len(list) + 1
        """
        selected_xedge = np.array(
            [self.xedges[i] for i in xindex]
            + [self.xedges[np.max(xindex) + 1]]
        )
        selected_yedge = np.array(
            [self.yedges[j] for j in yindex]
            + [self.yedges[np.max(yindex) + 1]]
        )
        return selected_xedge, selected_yedge

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

        # cell_inform 參數
        if isinstance(cell_fname, (str, list, tuple)):
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

        elif isinstance(cell_fname, dict):
            for flag in ["xedges", "yedges"]:
                assert flag in cell_fname
            if root_logger is not None:
                root_logger.debug(
                    "  --> Cell Inform Type (dict): {}".format(
                        type(cell_fname)
                    )
                )
            # 直接定義好 xedges, yedges, xlist, ylist
            self.xedges = cell_fname["xedges"]
            self.yedges = cell_fname["yedges"]
            self.xlist = np.array(
                [
                    (self.xedges[i] + self.xedges[i + 1]) / 2
                    for i in range(self.xedges.shape[0] - 1)
                ]
            )
            self.ylist = np.array(
                [
                    (self.yedges[j] + self.yedges[j + 1]) / 2
                    for j in range(self.yedges.shape[0] - 1)
                ]
            )

            self.minx, self.maxx, self.resx = (
                np.min(self.xedges),
                np.max(self.xedges),
                None,
            )
            self.miny, self.maxy, self.resy = (
                np.min(self.yedges),
                np.max(self.yedges),
                None,
            )

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

    def plot_grid(self, ax, **kwargs):
        """
        繪製 grid edges
        """
        ax.scatter(
            self.grid_x.reshape(-1),
            self.grid_y.reshape(-1),
            **kwargs,
        )

    def create_grid_frame(self):
        """
        建立grid外框的Polygon (shaply)
        """
        gdf_grid_frame = gpd.GeoDataFrame(
            [
                Polygon(
                    [
                        [self.extent[0], self.extent[2]],
                        [self.extent[1], self.extent[2]],
                        [self.extent[1], self.extent[3]],
                        [self.extent[0], self.extent[3]],
                        [self.extent[0], self.extent[2]],
                    ]
                )
            ],
            columns=["geometry"],
        )
        return gdf_grid_frame

    def plot_grid_frame(self, ax, **kwargs):
        """
        繪製 grid edges
        """
        gdf_grid_frame = self.create_grid_frame()
        gdf_grid_frame.plot(ax, **kwargs)

    def export_xyz(self, band: np.ndarray) -> np.ndarray:
        """
        輸出 X, Y, Z 格式, 三個 columns
        刪除 np.NaN 數值
        """
        assert isinstance(band, np.ndarray)
        assert band.shape == self.grid_x.shape

        df = pd.DataFrame(
            np.concatenate(
                [
                    self.grid_x.reshape((-1, 1)),
                    self.grid_y.reshape((-1, 1)),
                    band.reshape((-1, 1)),
                ],
                axis=1,
            ),
            columns=["X", "Y", "Z"],
        )
        # 刪除 np.NaN
        df = df[~df["Z"].isnull()]
        xyz_result = df.to_numpy()
        return xyz_result

    # @jit
    def interpolate_grid(
        self,
        points: np.ndarray,
        vals: np.ndarray,
        mask: Optional[np.ndarray] = None,
        mask_val: float = -999.0,
        log_merge: bool = True,
    ):
        """
        內插網格
        points & vals 為觀測點
        1. 應用 linear & nearest 來組合
        2. mask 為 bool 之 np.ndarray, True 部份保留; False 部份設定為 mask_val

        if mask is None, 則不以 mask_val 代換
        """
        grid_z_nearest = griddata(
            points,
            vals,
            (self.grid_x, self.grid_y),
            method="nearest",
        )
        grid_z_linear = griddata(
            points,
            vals,
            (self.grid_x, self.grid_y),
            method="linear",
        )
        if log_merge:
            # 整合 linear & nearest
            grid_z = np.where(
                np.isnan(grid_z_linear),
                grid_z_nearest,
                grid_z_linear,
            )
        else:
            # 單純以 linear
            grid_z = grid_z_linear
        if isinstance(mask, np.ndarray):
            assert mask.shape == grid_z.shape
            grid_z = np.where(
                mask,
                grid_z,
                mask_val,
            )

        return grid_z

    def trim_gridz_by_rect(
        self, grid_z: np.ndarray, data_limit: List
    ) -> np.ndarray:
        """
        設定  data_limit [Xmin, Xmax, Ymin, Ymax]
        挑選區域內的數據
        """
        assert grid_z.shape == self.grid_x.shape
        assert isinstance(data_limit, list)
        assert len(data_limit) == 4

        def check_in_range(
            coor_min: Union[int, float],
            coor_max: Union[int, float],
            coor_val: Union[int, float],
        ) -> bool:
            """
            判斷座標是否位於區域內
            """
            for var in [coor_min, coor_max, coor_val]:
                assert isinstance(var, (int, float))

            return (coor_min <= coor_val) and (
                coor_max >= coor_val
            )

        # data_range 必須位於 grid 區域內
        try:
            assert check_in_range(
                self.extent[0], self.extent[1], data_limit[0]
            )
            assert check_in_range(
                self.extent[0], self.extent[1], data_limit[1]
            )
            assert check_in_range(
                self.extent[2], self.extent[3], data_limit[2]
            )
            assert check_in_range(
                self.extent[2], self.extent[3], data_limit[3]
            )
        except AssertionError as e:
            raise TypeError(
                "!!! data_limit 在網格範圍外: {} / {}".format(
                    self.extent, data_limit
                )
            ) from e

        # 確認 data_limit_index
        data_limit_index = [
            find_nearest_arg(self.xlist, data_limit[0]),
            find_nearest_arg(self.xlist, data_limit[1]),
            find_nearest_arg(self.ylist, data_limit[2]),
            find_nearest_arg(self.ylist, data_limit[3]),
        ]
        if data_limit_index[1] < data_limit_index[0]:
            temp_val = data_limit_index[1]
            data_limit_index[1] = data_limit_index[0]
            data_limit_index[0] = temp_val
        if data_limit_index[3] < data_limit_index[2]:
            # 避免南北互換的問題
            temp_val = data_limit_index[3]
            data_limit_index[3] = data_limit_index[2]
            data_limit_index[2] = temp_val

        return grid_z[
            data_limit_index[2] : min(
                data_limit_index[3] + 1, grid_z.shape[0]
            ),
            data_limit_index[0] : min(
                data_limit_index[1] + 1, grid_z.shape[1]
            ),
        ]


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


def interpolate_multiple_interface(
    points,
    vals,
    grid_x,
    grid_y,
    method="",
    **kwargs,
):
    if method in ["linear", "nearest", "cubic"]:
        return interpolate_scipy(
            points, vals, grid_x, grid_y, method=method
        )
    elif method in ["OrdinaryKriging", "UniversalKriging"]:
        return interpolate_kriging(
            points, vals, grid_x, grid_y, method=method, **kwargs
        )[0]
    else:
        raise TypeError("!!! method is wrong! {}".format(method))


class cross_cell_projection:
    """
    兩組粗細網格定義, 並擷取粗網格資訊, 內插到新網格中
    """

    def __init__(
        self,
        cell_inform_coarse: cell_utility,
        cell_inform_fine: cell_utility,
    ):
        self.cell_inform = {
            "coarse": cell_inform_coarse,
            "fine": cell_inform_fine,
        }

    def grid_project(
        self, grid_coarse_index: List[Tuple]
    ) -> List[Tuple]:
        """
        粗網格的網格座標
            (j, i) --> 找出包在內部的細網格坐標
        """
        assert isinstance(grid_coarse_index, list)
        grid_fine_index = []

        for loc_index in grid_coarse_index:
            grid_selected = np.ones(
                self.cell_inform["fine"].grid_x.shape
            )
            grid_selected = np.where(
                (
                    self.cell_inform["fine"].grid_x
                    - self.cell_inform["coarse"].xedges[
                        loc_index[0]
                    ]
                )
                * (
                    self.cell_inform["fine"].grid_x
                    - self.cell_inform["coarse"].xedges[
                        loc_index[0] + 1
                    ]
                )
                <= 0,
                grid_selected,
                0,
            )
            grid_selected = np.where(
                (
                    self.cell_inform["fine"].grid_y
                    - self.cell_inform["coarse"].yedges[
                        loc_index[1]
                    ]
                )
                * (
                    self.cell_inform["fine"].grid_y
                    - self.cell_inform["coarse"].yedges[
                        loc_index[1] + 1
                    ]
                )
                <= 0,
                grid_selected,
                0,
            )
            grid_fine_index += [
                (arg[1], arg[0])
                for arg in np.argwhere(grid_selected == 1)
            ]
        return grid_fine_index

    def data_projection(
        self, data_mat_coarse: np.ndarray, **kwargs
    ):
        """
        擷取細網格範圍內的資料,
        內插,
        投射到細網格上

        **kwargs 為相關的內插參數
        """

        # 檢查資料型態
        assert isinstance(data_mat_coarse, np.ndarray)
        assert (
            data_mat_coarse.shape
            == self.cell_inform["coarse"].grid_x.shape
        )

        # 挑出 extent_fine 內部的數據
        extent_fine = self.cell_inform["fine"].extent
        data_mat_coarse = np.where(
            self.cell_inform["coarse"].grid_x
            >= np.min(extent_fine[:2]),
            data_mat_coarse,
            np.NaN,
        )
        data_mat_coarse = np.where(
            self.cell_inform["coarse"].grid_x
            <= np.max(extent_fine[:2]),
            data_mat_coarse,
            np.NaN,
        )
        data_mat_coarse = np.where(
            self.cell_inform["coarse"].grid_y
            >= np.min(extent_fine[2:]),
            data_mat_coarse,
            np.NaN,
        )
        data_mat_coarse = np.where(
            self.cell_inform["coarse"].grid_y
            <= np.max(extent_fine[2:]),
            data_mat_coarse,
            np.NaN,
        )

        df_data_points = pd.DataFrame(
            np.concatenate(
                (
                    self.cell_inform["coarse"].grid_x.reshape(
                        (-1, 1)
                    ),
                    self.cell_inform["coarse"].grid_y.reshape(
                        (-1, 1)
                    ),
                    data_mat_coarse.reshape((-1, 1)),
                ),
                axis=1,
            ),
            columns=["X", "Y", "data"],
        )
        df_data_points = df_data_points[
            ~df_data_points["data"].isnull()
        ]  # 排除 NaN
        data_points = np.array(
            df_data_points.loc[:, ["X", "Y", "data"]].values
        )

        data_mat_fine = interpolate_scipy(
            data_points[:, :2],  # points
            data_points[:, 2],  # vals
            self.cell_inform["fine"].grid_x,
            self.cell_inform["fine"].grid_y,
            **kwargs,
        )
        return data_mat_fine


class CI_mapping_type(TypedDict):
    """
    mapping = {
        "X": ...,
        "Y": ...,
    }
    """

    X: npt.NDArray[np.int32]
    Y: npt.NDArray[np.int32]


class CI_type(TypedDict):
    """
    定義 Region Params 的 TypedDict
    """

    fine: cell_utility
    coarse: cell_utility
    mapping: CI_mapping_type


class cross_cell_mapping:
    """
    兩組粗細網格定義, 定義對照關係
    """

    def __init__(
        self,
        cell_inform_coarse: cell_utility,
        cell_inform_fine: cell_utility,
    ):
        """
        初始化
        輸入粗網格與細網格定義
        """
        # 定義網格大小
        self.CI: CI_type = {
            "coarse": cell_inform_coarse,
            "fine": cell_inform_fine,
            "mapping": {
                "X": np.array([]),
                "Y": np.array([]),
            },
        }
        self.CI["mapping"] = {
            "X": -np.ones(
                self.CI["fine"].xlist.shape, dtype=int
            ),
            "Y": -np.ones(
                self.CI["fine"].ylist.shape, dtype=int
            ),
        }

        # 建立細網格 --> 粗網格對照
        for i in range(self.CI["coarse"].xlist.shape[0]):
            self.CI["mapping"]["X"] = np.where(
                (
                    self.CI["fine"].xlist
                    - self.CI["coarse"].xedges[i + 1]
                )
                * (
                    self.CI["fine"].xlist
                    - self.CI["coarse"].xedges[i]
                )
                <= 0,
                i,
                self.CI["mapping"]["X"],
            )
        for j in range(self.CI["coarse"].ylist.shape[0]):
            self.CI["mapping"]["Y"] = np.where(
                (
                    self.CI["fine"].ylist
                    - self.CI["coarse"].yedges[j + 1]
                )
                * (
                    self.CI["fine"].ylist
                    - self.CI["coarse"].yedges[j]
                )
                <= 0,
                j,
                self.CI["mapping"]["Y"],
            )


###########################################################################
def define_edges(cell_inform: List) -> Tuple:
    """
    由 cell_inform 定義 edges
    """
    mycell = cell_utility(cell_inform)
    return mycell.xedges, mycell.yedges


# 網格 tics 點
def define_tics(cell_inform):
    """
    定義 cell tics
    """
    """
    @jit
    def determine_coor_list(edges: np.ndarray) -> np.ndarray:
        '''
        依據 edge, 計算 cell 中心
        '''
        coor_list = (edges[:-1] + edges[1:]) / 2
        return coor_list

    (minx, maxx, resx) = tuple(cell_inform[0])
    (miny, maxy, resy) = tuple(cell_inform[1])

    xedge, yedge = define_edges(cell_inform)
    xlist2 = determine_coor_list(xedge)
    ylist2 = determine_coor_list(yedge)
    numx = xlist2.shape[0]
    numy = ylist2.shape[0]
    return minx, maxx, miny, maxy, resx, resy, numx, numy
    """
    mycell = cell_utility(cell_inform)
    return (
        mycell.minx,
        mycell.maxx,
        mycell.miny,
        mycell.maxy,
        mycell.resx,
        mycell.resy,
        mycell.numx,
        mycell.numy,
    )


def regular_grid(
    cell_inform, log_yreverse: bool = False
) -> Tuple:
    """
    取出 grid 資訊: grid_x, grid_y, xlist, ylist
    """
    mycell = cell_utility(cell_inform)
    return (
        mycell.grid_x,
        mycell.grid_y,
        mycell.xlist,
        mycell.ylist,
    )


def define_extent(cell_inform):
    """
    定義 imshow 的 extent
    extent = [
        cell_inform[0][0],
        cell_inform[0][1],
        cell_inform[1][0],
        cell_inform[1][1],
    ]
    return extent
    """
    mycell = cell_utility(cell_inform)
    return mycell.extent


def define_cell(cell_inform, **kwargs):
    """
    # 建構網格內涵
    """
    # 網格 tics 點
    jut.check_outdated_IO(
        "log_debug",
        "'log_debug' 為過時用法",
        **kwargs,
    )

    minx, maxx, miny, maxy, resx, resy, numx, numy = define_tics(
        cell_inform
    )
    (
        grid_x_cell,
        grid_y_cell,
        xlist_cell,
        ylist_cell,
    ) = regular_grid(cell_inform)
    assert np.all(xlist_cell >= minx), "Wrong xlist definition"
    assert np.all(xlist_cell <= maxx), "Wrong xlist definition"
    assert np.all(ylist_cell >= miny), "Wrong ylist definition"
    assert np.all(ylist_cell <= maxy), "Wrong ylist definition"

    # grid_y_cell, grid_x_cell = np.mgrid[
    #    miny + resy / 2 : maxy - resy / 2 : numy * 1j,
    #    minx + resx / 2 : maxx - resx / 2 : numx * 1j,
    # ]
    cell_size = numx * numy
    cell_area = resx * resy
    # grid_x_cell2 = np.array(grid_x_cell).reshape(cell_size)
    # grid_y_cell2 = np.array(grid_y_cell).reshape(cell_size)
    # xlist_cell = np.linspace(minx + resx / 2, maxx - resx / 2, numx)
    # ylist_cell = np.linspace(maxy + resy / 2, miny - resy / 2, numy)
    extent = [
        cell_inform[0][0],
        cell_inform[0][1],
        cell_inform[1][0],
        cell_inform[1][1],
    ]
    return (
        minx,
        maxx,
        miny,
        maxy,
        numx,
        numy,
        grid_y_cell,
        grid_x_cell,
        cell_size,
        cell_area,
        xlist_cell,
        ylist_cell,
        resx,
        resy,
        grid_x_cell.reshape(cell_size),
        grid_y_cell.reshape(cell_size),
        extent,
    )


# 依據 mf1 網格設定，取出並建立相對應之網格資訊
def define_mf_cell(
    dis, minx, _maxx, _miny, maxy
) -> Tuple:  # pylint: disable=unused-argument
    # pylint: disable=unsubscriptable-object
    # 取出 dis 資訊
    # nrow = dis.nrow
    # ncol = dis.ncol
    delc = dis.delc
    delr = dis.delr
    # top = dis.top
    # botm = dis.botm

    # delc for y
    # delr for x
    xlist = np.zeros(delr.array.shape[0])
    ylist = np.zeros(delc.array.shape[0])
    grid_x = np.zeros((delc.array.shape[0], delr.array.shape[0]))
    grid_y = np.zeros((delc.array.shape[0], delr.array.shape[0]))

    xlist[0] = minx + delr.array[0] / 2
    for i in range(1, xlist.shape[0]):
        xlist[i] = (
            xlist[i - 1]
            + (delr.array[i - 1] + delr.array[i]) / 2
        )
    ylist[0] = maxy - delc.array[0] / 2
    for i in range(1, ylist.shape[0]):
        ylist[i] = (
            ylist[i - 1]
            - (delc.array[i - 1] + delc.array[i]) / 2
        )

    for i in range(xlist.shape[0]):
        grid_x[:, i] = xlist[i]
    for i in range(ylist.shape[0]):
        grid_y[i, :] = ylist[i]
    return xlist, ylist, grid_x, grid_y


# 依據 mf1 網格設定，取出並建立相對應之網格資訊
def define_mf_bin2d(
    dis, xlist: np.ndarray, ylist: np.ndarray
):  # pylint: disable=unused-argument
    # pylint: disable=unsubscriptable-object
    delc = dis.delc
    delr = dis.delr
    bin_x = xlist - delr[0] / 2  # 中心點 - deltax / 2
    bin_x = np.append(bin_x, np.array(xlist[-1] + delr[-1] / 2))
    # print (xlist[:3], xlist[-3:])
    # print (bin_x[:3], bin_x[-3:])
    bin_y = ylist + delc[0] / 2  # 中心點 - deltay / 2
    bin_y = np.append(bin_y, np.array(ylist[-1] - delc[-1] / 2))

    # print (ylist[:3], ylist[-3:])
    # print (bin_y[:3], bin_y[-3:])
    return bin_x, bin_y


def grep_observation_points(observation_points, xlist, ylist):
    index = np.array(observation_points).astype(int)
    for i in range(observation_points.shape[0]):
        index[i, 0] = find_nearest_arg(
            xlist, observation_points[i, 0]
        )
        index[i, 1] = find_nearest_arg(
            ylist, observation_points[i, 1]
        )
    return index


def find_nearest(array: np.ndarray, value: float) -> float:
    idx = find_nearest_arg(array, value)
    return array[idx]


def find_nearest_arg(array: np.ndarray, value: float) -> int:
    """
    找到最接近 value 的 arg
    """
    array = np.asarray(array)
    idx: int = int((np.abs(array - value)).argmin())
    return idx


def get_mat_shape(ts_mat: Union[List, np.ndarray]):
    assert isinstance(ts_mat, (list, np.ndarray))

    numx = -1
    numy = -1
    if isinstance(ts_mat, list):
        assert len(ts_mat) == 2
        for i in range(2):
            assert isinstance(ts_mat[i], np.ndarray)

        numx = ts_mat[0].shape[1]
        numy = ts_mat[0].shape[0]
    else:
        numx = ts_mat.shape[1]
        numy = ts_mat.shape[0]
    return (numx, numy)


def filter_band(band: np.ndarray, filter_slice: List, **kwargs):
    """
    篩選 band, 標準化的處理函式
    """
    root_logger = kwargs.get("root_logger", None)
    jut.check_outdated_IO(  # 不再使用 log_debug
        "log_debug",
        "'log_debug' 已經過時不再使用, 改以 'root_logger' 取代",
        **kwargs,
    )

    # debug, Y軸部份
    # index 需要設定為反向
    band2 = np.array(None)
    band_shape = band.shape
    if len(filter_slice) == 2:
        band2 = band[
            band_shape[0]
            - filter_slice[0][1] : band_shape[0]
            - filter_slice[0][0],
            filter_slice[1][0] : filter_slice[1][1],
        ]
    elif len(filter_slice) == 3:
        band2 = band[
            filter_slice[0][0] : filter_slice[0][1],
            band_shape[1]
            - filter_slice[1][1] : band_shape[1]
            - filter_slice[1][0],
            filter_slice[2][0] : filter_slice[2][1],
        ]

    if root_logger is not None:
        root_logger.debug(
            "filter slice code: {} / from {} to {}".format(
                filter_slice,
                band.shape,
                band2.shape,
            )
        )
    return band2


class ts_matrix_cell:
    # pylint: disable=too-many-instance-attributes
    """
    用來倉儲 時間序列的 time series matrix data
    """

    def __init__(self, extent, ts_mat):
        """
        # 2019/12/24
        # 延伸支援 dem 等非時間類資料擷取
        """
        if isinstance(ts_mat, list):  # 含時間
            self.tsize = len(ts_mat)
            self.log_time_series = True
        elif isinstance(ts_mat, np.ndarray):
            self.tsize = 1
            self.log_time_series = False
        (self.numx, self.numy) = get_mat_shape(ts_mat)

        self.deltax = (extent[1] - extent[0]) / self.numx
        self.deltay = (extent[3] - extent[2]) / self.numy
        self.xlist = np.linspace(
            extent[0] + self.deltax / 2,
            extent[1] - self.deltax / 2,
            self.numx,
        )
        self.ylist = np.linspace(
            extent[3] - self.deltay / 2,
            extent[2] + self.deltay / 2,
            self.numy,
        )

        self.mat_data = np.zeros(
            (self.tsize, self.numy, self.numx)
        )
        if isinstance(ts_mat, type([])):  # 含時間
            for t in range(self.tsize):
                self.mat_data[t, :, :] = ts_mat[t]
        elif isinstance(ts_mat, type(np.zeros(1))):
            self.mat_data[0, :, :] = ts_mat

    def point2nearest_grid(
        self, observation_points: np.ndarray
    ) -> Tuple:
        """
        計算觀測點最鄰近的grid
        """
        index = grep_observation_points(
            observation_points, self.xlist, self.ylist
        )
        # pylint: disable=unsubscriptable-object
        observation_grid_point = np.array(index)
        for i in range(index.shape[0]):
            observation_grid_point[i, 0] = self.xlist[
                index[i, 0]
            ]
            observation_grid_point[i, 1] = self.ylist[
                index[i, 1]
            ]
        return observation_grid_point, index

    def grep2pandas(
        self, observation_points: np.ndarray
    ) -> pd.DataFrame:
        # pylint: disable=unused-variable
        observation_grid_point, index = self.point2nearest_grid(
            observation_points
        )
        flags = []

        mat = np.zeros((self.tsize, observation_points.shape[0]))
        for i in range(observation_points.shape[0]):
            flags.append("P" + str(i + 1))
            mat[:, i] = self.mat_data[
                :, index[i, 1], index[i, 0]
            ]

        df: pd.DataFrame = pd.DataFrame(mat, columns=flags)
        return df


class cellindex_type(TypedDict):
    """
    定義 Region Params 的 TypedDict
    """

    i: npt.NDArray[np.int32]
    j: npt.NDArray[np.int32]
    X: npt.NDArray[np.float32]
    Y: npt.NDArray[np.float32]


# ==================================================================
# ==================================================================
# ==================================================================
class MFCell2D:
    """
    # 以物件導向寫法，處理 mf 網格
    """

    # 建構子
    # mf.dis: flopy 中的 dis 物件
    # cell_inform: 網格定義資訊
    def __init__(
        self, dis, cell_inform: Union[str, List, Dict, Tuple]
    ):
        # 定義角點
        # pylint: disable=unused-variable
        (
            minx,
            maxx,
            miny,
            maxy,
            resx,
            resy,
            numx,
            numy,
        ) = define_tics(cell_inform)
        # 取出 dis 資訊
        delr = np.array(dis.delr)
        delc = np.array(dis.delc)
        accu_delr = delr.cumsum() - delr / 2
        accu_delc = delc.cumsum() - delc / 2

        # delc for y
        # delr for
        self.nrow = dis.nrow
        self.ncol = dis.ncol
        self.xlist = np.array(minx + accu_delr)
        self.ylist = np.array(maxy - accu_delc)
        self.xedges = np.array(
            [minx] + list(minx + delr.cumsum())
        )
        self.yedges = np.array(
            [maxy] + list(maxy - delc.cumsum())
        )

        # 建立 j, i, X, Y
        d: cellindex_type = {
            "j": np.zeros(dis.nrow * dis.ncol, dtype=np.int32),
            "i": np.zeros(dis.nrow * dis.ncol, dtype=np.int32),
            "Y": np.zeros(dis.nrow * dis.ncol, dtype=np.float64),
            "X": np.zeros(dis.nrow * dis.ncol, dtype=np.float64),
        }

        m = 0
        for j in range(dis.nrow):
            for i in range(dis.ncol):
                d["j"][m] = j
                d["i"][m] = i
                d["Y"][m] = self.ylist[j]
                d["X"][m] = self.xlist[i]
                m += 1
        self.df_cell = pd.DataFrame(
            data=d
        )  # .set_index(['j', 'i'])

        # 網格面積
        self.cell_area = np.zeros((dis.nrow, dis.ncol))
        for j in range(dis.nrow):
            self.cell_area[j, :] = delc[j] * delr

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

    def assign_data(self, array_data, flags):
        """
        self.df_cell 為 index 是 (j, i) 的 DataFrame
        array_data 為二維陣列, flags 為欄位名稱,
        將 array_data 加入 DataFrame
        """
        [nj, ni] = array_data.shape
        jj = np.linspace(0, nj - 1, nj).astype(int)
        ii = np.linspace(0, ni - 1, ni).astype(int)
        vec_i, vec_j = np.meshgrid(ii, jj)

        # 建立 array_data 的 pandas 資料
        vec1d_size = array_data.shape[0] * array_data.shape[1]
        d = {
            "j": vec_j.reshape(vec1d_size),
            "i": vec_i.reshape(vec1d_size),
            flags: array_data.reshape(vec1d_size),
        }
        df = pd.DataFrame(data=d)  # .set_index(['j', 'i'])
        if flags in self.df_cell.columns:
            self.df_cell[flags] = df[flags].values
        else:
            # 整合進入 self.df_cell
            self.df_cell = pd.concat(
                [self.df_cell, df.loc[:, [flags]]],
                axis=1,
            )

    # 螢幕輸出
    def print_info(self):
        print("id: ", id(self))
        print(self.df_cell.head())

    def get_nrow(self):
        return self.nrow

    def get_ncol(self):
        return self.ncol

    def get_xlist(self):
        mask = self.df_cell["j"] == 0
        df = self.df_cell[mask].sort_values(
            by="i"
        )  # 挑選 j == 0, 變成一個 row
        return np.array(df["X"].values)

    def get_ylist(self):
        mask = self.df_cell["i"] == 0
        df = self.df_cell[mask].sort_values(
            by="j"
        )  # 挑選 j == 0, 變成一個 row
        return np.array(df["Y"].values)

    def transform_to_array(self, flags):
        matrix = np.array(self.df_cell[flags].values).reshape(
            (self.nrow, self.ncol)
        )
        return matrix


# ==================================================================
# ==================================================================
# ==================================================================
# @jit
class MFCell_3D:
    """
    # 定義 MF 網格, 各層資訊
    # 建構子
    # mf.dis: flopy 中的 dis 物件
    # cell_inform: 網格定義資訊
    """

    def __init__(self, dis, cell_inform):
        self.Cell3D = [MFCell2D(dis, cell_inform)]
        # pylint: disable=unused-variable
        for _ in range(1, dis.nlay):
            self.Cell3D.append(
                copy.deepcopy(self.Cell3D[0])
            )  # 複製各層
        self.assign_elev(dis)  # 從中擷取 Top & Bottom 高程

    def assign_elev(self, dis):
        """
        # 設定 botm and top
        """
        self.Cell3D[0].assign_data(dis.top.array, "top")
        for i in range(dis.nlay):
            self.Cell3D[i].assign_data(
                dis.botm.array[i, :, :], "botm"
            )

    def assign_bas(self, bas):
        for i in range(len(self.Cell3D)):
            self.Cell3D[i].assign_data(
                bas.ibound.array[i, :, :], "ibound"
            )
            self.Cell3D[i].assign_data(
                bas.strt.array[i, :, :], "strt"
            )

    # 螢幕輸出
    def print_info(self):
        print(
            "Size: (",
            self.get_nlay(),
            ", ",
            self.get_nrow(),
            ", ",
            self.get_ncol(),
            ")",
        )
        for i in range(len(self.Cell3D)):
            print("Layer: ", i)
            print(self.Cell3D[i].print_info())

    # 取得 modflow 尺寸
    def get_nlay(self):
        return len(self.Cell3D)

    def get_nrow(self):
        return self.Cell3D[0].get_nrow()

    def get_ncol(self):
        return self.Cell3D[0].get_ncol()
