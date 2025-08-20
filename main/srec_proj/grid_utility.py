"""
Grid Utility
"""

import os
import pandas as pd
import geopandas as gpd
import numpy as np
from numba import jit
from typing import Union, List, Dict, Tuple, Optional
import twd97
import matplotlib.pyplot as plt
import gdal_utility
import netcdf_analysis as NCA
import file_utility as fut
import cell_inform as CI

cell_taiwan_params = {
    "WGS84": [
        [119.75, 122.15, 0.01],  # 東經, Lon
        [21.70, 25.50, 0.01],  # 北緯, Lat
    ],
    "TWD97_121": [
        [120000, 360000, 1000],  # X
        [2392000, 2819000, 1000],  # Y
    ],
}
projection_params = {
    "EPSG::4326": "WGS84",
    "EPSG::3826": "TWD97_121",
    "EPSG::3825": "TWD97_119",
}


##############################################################
# @jit
def define_id(
    df: Union[pd.DataFrame, gpd.geodataframe.GeoDataFrame],
    flag_in: Union[str, List],
    flag_id: str,
) -> pd.DataFrame:
    """
    從對應欄位, 定義 id 配對
    """
    assert isinstance(
        df, (pd.DataFrame, gpd.geodataframe.GeoDataFrame)
    )
    flag_set = []
    if isinstance(flag_in, str):
        try:
            flag_set = list(set(df.loc[:, flag_in].values))
        except KeyError as e:
            raise KeyError(
                "{} / {}".format(flag_in, df.columns)
            ) from e
        except TypeError as e:
            raise TypeError(
                "{}: {} / {}".format(
                    flag_in,
                    df.loc[:, flag_in].values,
                    df.columns,
                )
            ) from e
    elif isinstance(flag_in, list):
        flag_merge = "{}".format(flag_in[0])
        for i in range(1, len(flag_in)):
            flag_merge += "_{}".format(flag_in[i])
        df.loc[:, flag_merge] = [
            str(df.loc[index, flag_in].values.tolist())
            for index in df.index
        ]
        flag_in = flag_merge
        flag_set = list(set(df.loc[:, flag_merge].values))

    for i, fs in enumerate(flag_set):
        mask = df[flag_in] == fs
        df.loc[mask, flag_id] = i

    if isinstance(df, pd.DataFrame):
        df = df.astype({flag_id: int})
    else:
        # gpd.geodataframe.GeoDataFrame
        df[flag_id] = df[flag_id].astype("Int64")
    return df


def remove_duplicated_col(gdf, col_name: str):
    """
    刪除重複欄位
    """
    col_count = 0
    for col in gdf.columns:
        if col == col_name:
            col_count += 1

    if col_count > 1:
        log_duplicate = True
        for i, col in enumerate(gdf.columns):
            if col == col_name:
                if log_duplicate:
                    col_index = list(np.arange(0, i)) + list(
                        np.arange(i + 1, gdf.shape[1])
                    )
                    gdf = gdf.iloc[:, col_index]
                    log_duplicate = False
    return gdf


def town_raster_data(
    town_shp: str,
    project_name: str = "default_proj",
    export_path: str = "",
    log_refresh: bool = False,
    cell_inform=None,
    # log_show: bool = False,
    # log_debug: bool = False,
    log_add_rightend: bool = False,
    **kwargs,
):
    """
    鄉鎮 raster map create & load
    """
    root_logger = kwargs.get("root_logger", None)
    if root_logger is not None:
        root_logger.debug(
            "  --> Town data (vector data to raster data)"
        )

    ##############################################################
    town_nc = os.path.join(
        export_path,
        os.path.basename(town_shp.replace(".shp", ".nc")),
    )
    
    if log_refresh:
        # 清除舊的檔案
        flist = fut.search_files_in_dir(
            export_path, regular_flags=town_nc
        )

        for target_file in flist + [argv_pickle]:
            assert isinstance(target_file, str)
            try:
                fut.remove_file(target_file)
            except FileNotFoundError:
                # 如果缺少 file, 表示不存在
                # pass
                pass

    if export_path != "":
        # 表示 local, 無須再建立目錄
        os.makedirs(export_path, exist_ok=True)
    town_data = {}
    gf = None
    
    if root_logger is not None:
        root_logger.debug(
            "--> pickle does not exist and load from '{}'".format(
                town_shp
            )
        )
    ##############################################################
    # read shp with geopandas
    # 加入 town_id, export dbf
    # 2022/5/17 加入 town_mapping 部分除了原有的 id vs. town name 外,
    #   額外加入各鄉鎮面積
    gf = gpd.read_file(town_shp, **kwargs)
    rename_params = {
        "縣市名稱": "county_name",
        "county": "county_name",
        "county_n_1": "county_name",  # 太長的欄位名稱, 會被截掉
        "鄉鎮名稱": "town_name",
        "town": "town_name",
        "AREA": "area",
    }
    gf = gf.rename(columns=rename_params)
    # 刪除重複 column
    for col in [
        "county_name",
        "county_id",
        "town_name",
        "town_id",
    ]:
        gf = remove_duplicated_col(gf, col)

    # Add county & town id
    gf = define_id(gf, "county_name", "county_id")
    gf = define_id(
        gf, ["county_name", "town_name"], "town_id"
    )

    df_town_mapping = (
        gf.loc[
            :,
            [
                "county_name",
                "county_id",
                "town_name",
                "town_id",
                "area",
            ],
        ]
        .set_index("town_id")
        .sort_index()
    )

    if not os.path.exists(town_nc):
        ##############################################################
        # re-write to shp
        town_shp2 = os.path.join(
            export_path,
            os.path.basename(town_shp),
        )
        if root_logger is not None:
            root_logger.debug(
                "--> {}".format(
                    "Re-export to shapefile: {}".format(
                        town_shp2
                    )
                )
            )
        try:
            gf.to_file(town_shp2)
        except AttributeError as e:
            raise AttributeError(
                "{} / {} / {} / {}".format(
                    gf,
                    gf.dtypes,
                    gpd.__version__,
                    pd.__version__,
                )
            ) from e
        except ValueError as e:
            raise ValueError(gf.columns) from e

        # shp file --> nc file
        if root_logger is not None:
            root_logger.debug(
                "--> shp to nc: {} / {}".format(
                    town_shp2, town_nc
                )
            )

        if cell_inform is None:
            cell_inform = cell_taiwan_params["TWD97_121"]
        mygdal = gdal_utility.gdal_utility(
            town_nc,
            cell_inform,
            log_debug=True,
            log_add_rightend=log_add_rightend,
        )
        mygdal.gdal_rasterizing(
            town_shp2,
            attribute="town_id",
            log_quiet=kwargs.get("log_quiet", False),
            root_logger=root_logger,
        )
            
    message = "Read NC data and post analysis"
    if root_logger is not None:
        root_logger.debug("--> {}".format(message))
    # 建立鄉鎮 id 的 raster map
    (ylist, xlist, town_band) = NCA.read_ncband(town_nc)
    # 除去 < 0 者, 改為 np.nan
    town_band = town_band.astype("float")
    town_band = np.where(
        town_band < 0,
        np.nan,
        town_band,
    )
    town_data = {
        "xlist": xlist,
        "ylist": ylist,
        "band": town_band,
        "town_mapping": df_town_mapping,
        "gpd": gf,
    }
    
    return town_data


def project_translate(proj_name: str) -> str:
    """
    投影轉換, 以EPSG::4326呈現
    """
    proj_name2 = None
    if proj_name.find("EPSG") >= 0:
        proj_name2 = proj_name
    else:
        for key in projection_params.keys():
            if projection_params[key] == proj_name:
                proj_name2 = key
    assert proj_name2 is not None  # 不應該還是 None
    return proj_name2


class grid_utility:
    """
    建立 Grid Utility
    1. 建立全台 Grid, 可考慮 TWD97 or WGS84
    2. 可釐清海上與陸地
    3. TWD97 Vs. WGS84 的自由轉換
    """

    def __init__(
        self,
        ci_file: Union[str, List, CI.cell_utility],
        proj_name: str,
        town_data: Optional[Dict] = None,
        log_add_rightend: bool = False,
        **kwargs,
    ):
        """
        初始化
        ci_file: 可以是設定檔, 也可以是 list
            # cell_inform = [
            #   [min_x, max_x, delta_x],
            #   [min_y, max_y, delta_y],
            # ]
        proj_name 為 projection 投影系統, 可以設定 WGS84 或 EPSG::4326, 內部以 EPSG::4326 體系儲存
        TWD97 121, EPSG:3826

        town_data: 鄉鎮資料

        南北互換

        """
        self.proj_name = project_translate(proj_name)

        mycell: Optional[CI.cell_utility] = None
        if isinstance(ci_file, (str, list)):
            # str or list, 轉換建立 cell_utility
            mycell = CI.cell_utility(
                ci_file,
                log_add_rightend=log_add_rightend,
                **kwargs,
            )  # 改用 cell_utility
        elif isinstance(ci_file, CI.cell_utility):
            mycell = ci_file

        self.grid_param = {
            "cell": mycell,
        }
        self.town_data = town_data

        self.root_logger = kwargs.get("root_logger", None)

    def ocean_mask_trim(
        self,
        grid_z: np.ndarray,
        log_reverse: bool = False,
        **kwargs,
    ):
        """
        刪除 mask 外的數值
        if mask is None, 選擇 self.town_data
        log_reverse: 是否要反向選擇
        """
        if kwargs.get("root_logger", None) is not None:
            kwargs.get("root_logger", None).debug(
                "      Ocean mask trimming"
            )

        mask = kwargs.get("town_data_band", None)
        if mask is None:
            # 如無設置, 則改用內部變數
            try:
                mask = self.town_data["band"]
            except TypeError:
                pass
        if mask is not None:
            if not log_reverse:
                try:
                    grid_z = np.where(
                        mask >= 0,
                        grid_z,
                        np.nan,
                    )
                except TypeError as e:
                    raise TypeError(
                        "!!! {} / {}".format(type(mask), mask)
                    ) from e
            else:
                # 反向選擇
                grid_z = np.where(
                    mask < 0,
                    grid_z,
                    np.nan,
                )
        return grid_z

    def grid_interpolate(
        self,
        points: List,
        vals: List,
        grid_x=None,
        grid_y=None,
        method="linear",
        **kwargs,
    ):
        """
        內插
        """
        if grid_x is None:
            grid_x = self.grid_param["cell"].grid_x
        if grid_y is None:
            grid_y = self.grid_param["cell"].grid_y

        # 呼叫內插
        grid_z = CI.interpolate_scipy(
            points, vals, grid_x, grid_y, method=method
        )

        if kwargs.get("log_ocean_remove", False):
            # 刪除 海上數據
            grid_z = self.ocean_mask_trim(grid_z, **kwargs)
        return grid_z

    def interpolate_combine(self, points, vals, **kwargs):
        """
        聯合內插
        1. nearest & linear (外差部份為 np.nan)
        2. 整併
        """
        method = kwargs.get("method", "linear")

        def revise_kwargs(kwargs, flag: str, default_content):
            """
            若 flag 不存在於 kwargs.keys(), 則設定預設值
            """
            if flag not in kwargs.keys():
                kwargs[flag] = default_content
            return kwargs

        # 若無 grid_x, grid_y, town_data_band
        # 則改為系統預設值
        revise_kwargs(
            kwargs, "grid_x", self.grid_param["cell"].grid_x
        )
        kwargs = revise_kwargs(
            kwargs, "grid_y", self.grid_param["cell"].grid_y
        )
        try:
            kwargs = revise_kwargs(
                kwargs, "town_data_band", self.town_data["band"]
            )
        except TypeError:
            # TypeError: 'NoneType' object is not subscriptable
            kwargs = revise_kwargs(
                kwargs, "town_data_band", None
            )

        grid_z_interpolate = np.array([])  # empty ndarray
        if kwargs.get("root_logger", None) is not None:
            kwargs.get("root_logger", None).debug(
                "      Interpolate step 1 (core): {}".format(
                    {
                        flag: kwargs[flag]
                        for flag in ["method", "variogram_model"]
                        if flag in kwargs
                    }
                )
            )
            if kwargs.get("log_debug", False):
                kwargs.get("root_logger", None).debug(
                    "Points: {}".format(points)
                )
                kwargs.get("root_logger", None).debug(
                    "Values: {}".format(vals)
                )
        if method in ["linear", "cubic"]:
            grid_z_interpolate = self.grid_interpolate(
                points,
                vals,
                **kwargs,  # kwargs 中已包含 variogram_model
            )
        elif method in ["OrdinaryKriging", "UniversalKriging"]:
            (
                grid_z_interpolate,
                _kriging_model,
            ) = CI.interpolate_kriging(
                points,
                vals,
                kwargs["grid_x"][0, :],
                kwargs["grid_y"][:, 0],
                **kwargs,  # kwargs 中已包含 variogram_model
            )

        # 合併
        if kwargs.get("root_logger", None) is not None:
            kwargs.get("root_logger", None).debug(
                "      Interpolate step 2 (merged with nearest)"
            )
        if kwargs.get("log_nearest_merge", True):
            grid_z = np.where(
                np.isnan(grid_z_interpolate),
                self.grid_interpolate(  # 以最鄰近者, 補充數據
                    points,
                    vals,
                    method="nearest",
                    **{
                        key: elem
                        for key, elem in kwargs.items()
                        if key not in ["method"]
                    },
                ),
                grid_z_interpolate,
            )
        else:
            grid_z = grid_z_interpolate

        if kwargs.get("log_ocean_remove", False):
            # 刪除 海上數據
            grid_z_interpolate = self.ocean_mask_trim(
                grid_z_interpolate, **kwargs
            )

        if kwargs.get("log_debug", False):
            if kwargs.get("root_logger", None) is not None:
                kwargs.get("root_logger", None).debug(
                    "{} / {}".format(vals.shape, vals)
                )
                kwargs.get("root_logger", None).debug(
                    "{} / {}".format(
                        np.mean(vals), np.nanmean(grid_z)
                    )
                )
                kwargs.get("root_logger", None).debug(
                    "{} / {}".format(
                        grid_z.shape,
                        len(np.argwhere(np.isnan(grid_z))),
                    )
                )

        return grid_z
