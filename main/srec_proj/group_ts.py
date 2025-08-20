# -*- coding: utf-8 -*-
# import numpy as np
import pandas as pd
import geopandas as gpd
from numba import jit
import numpy as np
import matplotlib.pyplot as plt
from typing import List, Union, Tuple, Optional
from sklearn.linear_model import LinearRegression
from alive_progress import alive_bar
import datetime
from geopy.distance import distance
import twd97
import operator

# from typing import List
import jutility as jut
import file_utility as fut
import sys
import warnings
import ts_process_base as tsb
import parallel_framework

warnings.filterwarnings("ignore")


# Jacky's module
# 建立群體時間序列計算
# =======================================================================
# =======================================================================
# =======================================================================


def wrapper(argv):
    func = argv[0]
    return func(*argv[1:])


def filter_phrase(filter_code: str) -> List:
    """
    輸入 filter_code, 為長串的字串
    a. 以 & 來區隔不同的條件
    b. 以 = 號來區隔欄位與條件, = 左號為欄位
    c. 以 :: 來定義檔案與欄位
    [檔案]::[欄位]=

    # 可以一次下多個篩選條件
    # 例如: f1=s1&f2=s2,s3
    # f1=s1 & f2=s2 or s3
    # f1 與 f2 兩個條件採取 交集
    # f2=s2,s3 則採聯合集

    -->
        [
            ["檔案", "欄位", [條件1, 條件2]],   <---- [檔案]::[欄位]= 的設定方式
            ["", "欄位", [條件1, 條件2]]
        ]

    備註: 如果沒有限定特定檔案, 可用 None:column 或是直接設定 column
    """
    assert isinstance(filter_code, str)
    # pylint: disable=no-else-return
    if filter_code == "":
        return []

    sepline = filter_code.split("&")
    mat = []
    for i in range(len(sepline)):
        if sepline[i] != "":
            sepline2 = []
            operator = ""
            for flag in [">=", "<=", "==", "!=", ">", "<"]:
                if sepline[i].find(flag) >= 0:
                    sepline2 = sepline[i].split(flag)
                    operator = flag
                    break

            sub_mat = []
            if sepline2[0].find("::") >= 0:
                sub_mat = sepline2[0].split("::")
            else:
                sub_mat = ["", sepline2[0]]
            if sub_mat[0].lower() == "none":
                sub_mat[0] = ""

            try:
                sub_mat.append(sepline2[1].split(","))  # 條件
            except IndexError as e:
                raise IndexError(
                    "{} / {}".format(sepline[i], sepline2)
                ) from e
            mat.append(sub_mat + [operator])
    return mat


@jit
def determine_index(
    column_name1, column_list
) -> Union[int, None]:
    """
    計算資料排名
    """
    for i, column_name2 in enumerate(column_list):
        if column_name1 == column_name2:
            return i
    return None


# pylint: disable=inconsistent-return-statements
def df_filter_condition(
    df_stat: Union[pd.DataFrame, gpd.GeoDataFrame],
    filter_code: str,
    layer_name: str = "",
) -> Union[pd.DataFrame, gpd.GeoDataFrame]:
    """
    依據 filter code 進行篩選動作
    如若篩選無符合篩選條件, 回傳 Empty DataFrame

    備註: 原本回傳 None, 目前改為 Empty DataFrame (2024/2/21)
    """
    assert isinstance(filter_code, str)
    assert isinstance(df_stat, (pd.DataFrame, gpd.GeoDataFrame))
    df_and_merge = df_stat
    mat_filter = filter_phrase(filter_code)  # 解析條件 code str
    if filter_code != "":
        # 不應該是空字串
        print("'{}' --> {}".format(filter_code, mat_filter))

    operator_params = {
        "==": operator.eq,
        "!=": operator.ne,
        ">=": operator.ge,
        "<=": operator.le,
        ">": operator.gt,
        "<": operator.lt,
    }
    if len(mat_filter) > 0:
        for elem in mat_filter:  # 外層條件, 採用交集
            filter_lname = elem[0]  # 圖層名稱
            filter_col = elem[1]
            filter_str = elem[2]
            filter_operator = elem[3]  # 運算子

            log_file_matched = True
            if (layer_name != "") and (filter_lname != ""):
                # 限定圖層名稱
                if layer_name != filter_lname:
                    log_file_matched = False

            if isinstance(filter_str, str):
                # 如果只有單個條件, 轉為 List
                filter_str = filter_str.split(",")

            if log_file_matched:
                df_or_merge = pd.DataFrame([])
                for j in range(len(filter_str)):
                    # 相同的欄位, 但有多個內涵
                    # 聯集
                    # 改成依序篩選, 再將篩選結果以 concat 合併
                    if filter_operator in [">=", "<=", ">", "<"]:
                        # 數值
                        assert jut.check_isfloat(
                            filter_str[j]
                        )  # 確認是否可轉換為 float
                        filter_str[j] = float(filter_str[j])
                    mask = [
                        operator_params[filter_operator](
                            df_and_merge.loc[index, filter_col],
                            filter_str[j],
                        )
                        for index in df_and_merge.index
                    ]
                    df_inner = df_and_merge[mask]
                    # OR 方式組合
                    df_or_merge = pd.concat(
                        [df_or_merge, df_inner], axis=0
                    )
                    # 刪除重複 index 資訊
                    df_or_merge = df_or_merge[
                        ~df_or_merge.index.duplicated(
                            keep="first"
                        )
                    ]

                # 將本次篩選, 備份到 df_and_merge
                df_and_merge = df_or_merge
        return df_and_merge
    else:
        return df_stat


# @jit
def sorted_value_stat(
    vector_value: np.ndarray,
    sid_list: List,
    sname_list: List,
    ascending: bool = True,
) -> List[List]:
    """
    輸入變數向量 (例如距離或相關性等), 排序,
    並輸出排序後的 sid & sname
    """

    mat = list(zip(sid_list, sname_list, list(vector_value)))

    df_stat = pd.DataFrame(mat, columns=["sid", "sname", "val"])
    df_stat = df_stat.sort_values(
        by=["val"], ascending=ascending
    )
    return df_stat.loc[
        :, ["sid", "sname", "val"]
    ].values.tolist()


@jit
def calc_distance(
    loc1: np.ndarray, loc2: np.ndarray
) -> np.ndarray:
    """
    輸入兩個座標點的序列，回傳一個距離陣列 NXM
    N 為 loc1 之尺寸
    M 為 loc2 之尺寸
    """
    mat_distance = np.zeros(
        (loc1.shape[0], loc2.shape[0]), dtype=np.float64
    )
    for m in range(loc1.shape[0]):
        for n in range(2):
            # 二維度
            mat_distance[m, :] += np.power(
                loc1[m, n] * np.ones(loc2.shape[0]) - loc2[:, n],
                2,
            )
        # 開根號
        mat_distance[m, :] = np.power(mat_distance[m, :], 0.5)
    return mat_distance


def determine_distance_multi(flag: List) -> List:
    """
    計算距離
    loc1
    log2_list
    log_WGS84
    """
    loc1 = flag[0]
    loc2_list = flag[1]
    log_WGS84 = flag[2]

    mat = []
    try:
        if log_WGS84:
            mat = [
                distance(
                    (loc1[1], loc1[0]),  #
                    (loc2[1], loc2[0]),
                ).km
                for loc2 in loc2_list
            ]
        else:
            # TWD97
            # m --> km
            mat = list(calc_distance(loc1, loc2_list) / 1000)
    except ValueError as e:
        if bool(np.any(np.isnan(loc1))):
            return [np.NaN for _ in loc2_list]
        raise ValueError(
            "{} / {}".format(loc1, loc2_list)
        ) from e
    return mat


class Found(Exception):
    """
    專用於雙層迴圈的搜尋 & break
    """

    # pylint: disable=unnecessary-pass
    pass


# pylint: disable=too-many-public-methods
# pylint: disable=too-many-instance-attributes
class Group_TSP:
    """
    # 專門用於, 資料量極多
    # 以 stat_fname 紀錄各站資訊
    # 採循序載入資料, 交叉計算跨站資訊
    """

    def __init__(self):
        """
        # 建構子
        """
        self.df_stat: pd.DataFrame = pd.DataFrame({"A": []})
        self.sid: Optional[str] = None
        self.sname: Optional[str] = None
        self.layer_name: Optional[str] = None
        self.matrix_corr: np.ndarray = np.empty(
            1
        )  # 相關係數陣列

    # @jit, try - except
    def group_station_assign(
        self,
        stat_fname: Union[str, pd.DataFrame],
        sid: str,
        sname: str,
        data_path: Union[str, List[str]],
        filter_code: Optional[str] = "",
        layer_name=None,
        # log_debug: bool = False,
        **kwargs,
    ):
        """
        # 群站建立
        stat_fname: 站井資訊
        sid: col name for sid
        sname: col name for sname
        data_path: 存放時間序列資料之目錄路徑
        filter_code: filter code
        篩選條件
        # 可以一次下多個篩選條件
        # 例如: f1=s1&f2=s2,s3
        # f1=s1 & f2=s2 or s3
        # f1 與 f2 兩個條件採取 交集
        # f2=s2,s3 則採聯合集

        log_wgs2twd 決定是否以 WGS84 轉換成 TWD97
        pkm 是否為澎湖、金門與馬祖
        wgs_flags 為 WGS84 的欄位名稱
        """
        root_logger = kwargs.get("root_logger", None)
        self.df_stat = pd.DataFrame({"A": []})

        assert isinstance(stat_fname, (str, pd.DataFrame))
        if isinstance(stat_fname, str):
            file_params = {
                "h5": pd.read_hdf,
                "hdf": pd.read_hdf,
                "csv": pd.read_csv,
                "txt": pd.read_csv,
            }
            for key, func in file_params.items():
                if stat_fname.find(key) >= 0:
                    if root_logger is not None:
                        root_logger.debug(
                            "{}: Loading {}".format(
                                key, stat_fname
                            )
                        )
                    self.df_stat = func(stat_fname)
        elif isinstance(stat_fname, pd.DataFrame):
            self.df_stat = stat_fname

        self.df_stat = self.df_stat.astype({sid: str})
        # pylint: disable=attribute-defined-outside-init
        if isinstance(data_path, str):
            self.data_path = [data_path]
        elif isinstance(data_path, list):
            self.data_path = data_path
        self.sid = sid
        self.sname = sname
        self.layer_name = layer_name
        if kwargs.get("log_wgs2twd", False):
            assert "wgs_flags" in kwargs
            assert isinstance(kwargs["wgs_flags"], list)
            for j in range(len(kwargs["wgs_flags"])):
                assert isinstance(kwargs["wgs_flags"][j], str)
                assert (
                    kwargs["wgs_flags"][j]
                    in self.df_stat.columns
                )  # 必須有此欄位
            twd_flags = [
                "TWD97_X",
                "TWD97_Y",
            ]
            for index in self.df_stat.index:
                loc_wgs84 = tuple(
                    self.df_stat.loc[
                        index, kwargs["wgs_flags"]
                    ].values
                )
                loc_twd97 = twd97.fromwgs84(
                    *loc_wgs84,
                    **{
                        key: elem
                        for key, elem in kwargs.items()
                        if key in ["pkm"]
                    },
                )
                for j in range(2):
                    self.df_stat.loc[index, twd_flags[j]] = (
                        loc_twd97[j]
                    )

        # 欄位篩選
        if filter_code != "":
            self.df_stat = self.stat_filter_condition(
                filter_code, **kwargs
            )

        # log_recursive=True
        # 往深層搜尋
        self.determine_data_file(**kwargs)  # 找出路徑中的檔案
        if root_logger is not None:
            root_logger.debug(
                self.df_stat.loc[
                    :,
                    [
                        self.sid,
                        self.sname,
                        "ftype",
                        "data_fname",
                    ],
                ]
            )
            root_logger.debug(self.df_stat.shape)
        self.matrix_corr = np.empty(1)  # 相關係數陣列

    def stat_filter_condition(
        self, filter_code: Optional[str], **kwargs
    ):
        """
        篩選條件
        # 可以一次下多個篩選條件
        # 例如: f1=s1&f2=s2,s3
        # f1=s1 & f2=s2 or s3
        # f1 與 f2 兩個條件採取 交集
        # f2=s2,s3 則採聯合集

        可重複給予多個條件
        """

        # pylint: disable=too-many-branches
        def phrase_filter_condition(
            df: pd.DataFrame,
            filter_code: Optional[str],
            layer_name: Optional[str] = None,
            # log_debug: bool = False,
            **kwargs,
        ):
            """
            filter code phraser
            # 可以一次下多個篩選條件
            # 例如: f1=s1&f2=s2,s3
            # f1=s1 & f2=s2 or s3
            # f1 與 f2 兩個條件採取 交集
            # f2=s2,s3 則採聯合集
            """
            root_logger = kwargs.get("root_logger", None)
            if root_logger is not None:
                root_logger.debug(
                    "filter code: {}".format(filter_code)
                )
            assert (
                isinstance(filter_code, str)
                or filter_code is None
            )
            if filter_code is None:
                # 用來針對舊的作法, filter_code = None
                filter_code = ""
            mat_filter = filter_phrase(filter_code)

            if mat_filter == []:
                # 不篩選, 跳出
                return df

            size1 = df.shape
            df_outer = df.copy()
            for i in range(
                len(mat_filter)
            ):  # 外層條件, 採用交集
                filter_lname = mat_filter[i][0]
                filter_col = mat_filter[i][1]
                filter_conditions: List = mat_filter[i][2]

                log_isnone = False
                if isinstance(filter_col, str):
                    log_isnone = log_isnone or (
                        filter_col.lower() == "none"
                    )
                if (layer_name is not None) and (
                    filter_lname != ""
                ):
                    # 確認正在分析的檔案，是否為 filter_lname
                    log_isnone = log_isnone or (
                        layer_name != filter_lname
                    )

                if not log_isnone:
                    df_merge = None
                    for filter_condition in filter_conditions:
                        # for j in range(len(filter_str)):
                        # 相同的欄位, 但有多個內涵
                        # 聯集
                        # 改成依序篩選, 再將篩選結果以 concat 合併

                        try:
                            try:
                                df_inner = df_outer[
                                    df_outer[filter_col]
                                    == filter_condition
                                ]
                            except KeyError as e:
                                message = (
                                    "'{}' not in {}".format(
                                        filter_col,
                                        df_outer.columns,
                                    )
                                )
                                raise KeyError(message) from e
                            size2 = df_outer.shape
                            size3 = df_inner.shape

                            if df_merge is None:
                                df_merge = df_inner
                            else:
                                # OR 方式組合
                                df_merge = pd.concat(
                                    [df_merge, df_inner]
                                )
                            size4 = df_merge.shape

                            if root_logger is not None:
                                root_logger.debug(
                                    "    欄位篩選: {} {}".format(
                                        filter_col,
                                        filter_condition,
                                    )
                                )
                                root_logger.debug(
                                    "    --> Size: {} --> {} / {} / {}".format(
                                        size1,
                                        size2,
                                        size3,
                                        size4,
                                    )
                                )
                        except KeyError as e:
                            check_bool = (
                                filter_col in df_outer.columns
                            )
                            raise KeyError(
                                "!!! 錯誤的篩選條件，欄位名稱不存在! filter_code='{}' / df columns={} / {}".format(
                                    filter_code,
                                    df_outer.columns,
                                    check_bool,
                                )
                            ) from e

                    if df_merge is not None:
                        df_outer = df_merge
            df = df_outer
            df = df.sort_index()
            return df

        ######################################################################
        # 重複加入多個篩選條件
        self.df_stat = phrase_filter_condition(
            self.df_stat,
            filter_code,
            layer_name=self.layer_name,
            **kwargs,
        )
        return self.df_stat

    def determine_data_file(self, **kwargs):
        """
        # 找出路徑中的檔案
        """
        root_logger = kwargs.get("root_logger", None)
        path_result = []

        for dpath in self.data_path:
            assert isinstance(dpath, str)  # 必須要是 str
            path_result += fut.search_files_in_dir(
                dpath, **kwargs
            )

        self.df_stat.loc[:, "data_fname"] = ""
        self.df_stat.loc[:, "ftype"] = ""

        for i in range(self.df_stat.shape[0]):
            index = self.df_stat.index[i]
            sid = self.df_stat.loc[index, self.sid]

            # 找出符合 sid 的檔案
            data_fname = fut.filter_search_result(
                path_result, regular_flags=sid, **kwargs
            )
            if root_logger is not None:
                root_logger.debug(data_fname)
            try:
                self.df_stat.loc[index, "data_fname"] = (
                    data_fname[0]
                )
                # 副檔名
                self.df_stat.loc[index, "ftype"] = data_fname[
                    0
                ].split(".")[-1]
            except IndexError:
                self.df_stat.loc[index, "data_fname"] = None
                self.df_stat.loc[index, "ftype"] = None

    # @jit
    def get_stat_size(self) -> int:
        """
        取得觀測站數量
        """
        return self.df_stat.shape[0]

    @jit
    def get_matrix(
        self, item: str = "correlation"
    ) -> np.ndarray:
        """
        取得相關係數矩陣 或 距離矩陣
        """
        matrix: np.ndarray = np.zeros(1)
        if item == "correlation":
            matrix = self.matrix_corr
        elif item == "distance":
            matrix = self.matrix_distance
        return matrix

    def load_ts(
        self, index: int, **kwargs
    ) -> Optional[pd.DataFrame]:
        """
        從物件中, 依據 index 編號, 找出對應的時序列數據
        """
        root_logger = kwargs.get("root_logger", None)
        export_size = 40
        if index < export_size:
            if root_logger is not None:
                root_logger.debug(
                    "Station Meta: {}".format(
                        self.df_stat.iloc[index, :].values
                    )
                )
        assert index < self.df_stat.shape[0]

        # sid = self.df_stat.loc[
        #    self.df_stat.index[index], self.sid
        # ]
        fname = self.df_stat.loc[
            self.df_stat.index[index], "data_fname"
        ]
        ftype = self.df_stat.loc[
            self.df_stat.index[index], "ftype"
        ]
        if index < export_size:
            if root_logger is not None:
                root_logger.debug("{}: {}".format(fname, ftype))

        df = None
        if ftype is None:
            return None
        elif ftype == "h5":
            df = pd.read_hdf(fname)
        elif ftype == "csv":
            df = pd.read_csv(fname)
            if df.columns[0].find("-") >= 0:
                # 表示 columns[0] 為日期
                # 重新讀取
                df = pd.read_csv(fname, header=None)
                try:
                    columns = list(df.columns)
                    columns[0] = "datetime"
                    df.columns = columns
                except TypeError as e:
                    raise TypeError(df) from e
            try:
                df = df.rename(columns={"time": "datetime"})
                df = df.set_index("datetime")
            except KeyError as e:
                raise KeyError(
                    "{} / {} / {}".format(df, df.columns, fname)
                ) from e
            df.index = pd.to_datetime(df.index)
        if index < export_size:
            if root_logger is not None:
                root_logger.debug("{}".format(df))

        return df

    # @jit
    def get_GTS_sid(self) -> List:
        """
        get sid list
        """
        return self.df_stat.loc[:, self.sid].tolist()

    # @jit
    def get_GTS_sname(self) -> List:
        """
        get sname list
        """
        return self.df_stat.loc[:, self.sname].tolist()

    # @jit
    def query_index_from_sid(self, sid_query: str) -> int:
        if isinstance(sid_query, int):
            sid_query = str(sid_query)
        df_stat2 = self.df_stat
        df_stat2.index = np.arange(
            df_stat2.shape[0]
        )  # 重設 index
        df_stat2 = df_stat2[df_stat2[self.sid] == sid_query]

        index = -1
        if df_stat2.shape[0] > 0:
            index = df_stat2.index[0]
        return index

    # @jit, don't use
    def query_sname_from_sid(self, sid_query: str) -> str:
        index = self.query_index_from_sid(sid_query)
        # print (sid_query, index)
        result: str = ""
        if index != -1:
            result = self.get_GTS_sname()[index]
        return result

    # @jit, don't use
    def query_value_from_sid(
        self, sid_query: str, columns: Union[List, str]
    ):
        index = self.query_index_from_sid(sid_query)
        results = None
        try:
            if isinstance(columns, type([])):
                results = self.df_stat.loc[
                    index, columns
                ].tolist()
            elif isinstance(columns, str):
                results = self.df_stat.loc[index, columns]
        except KeyError:
            # if index == -1, 表示找不到該 sid
            print("!!! Query SID: {}".format(sid_query))
            return None
        return results

    def get_GTS_list(
        self,
        downsample_freq: Optional[str] = None,
        trim_ts_data: Tuple[
            Optional[Union[str, datetime.datetime]]
        ] = (None, None),
        **kwargs,
    ) -> List:
        """
        將所有資料一次載入
            downsample_freq 用來降採樣率
            例如 downsample_freq = "1d"

            trim_ts_data 用來對齊數列前後
        """
        self.determine_data_file(**kwargs)  # 找出路徑中的檔案
        root_logger = kwargs.get("root_logger", None)
        assert isinstance(trim_ts_data, tuple)
        assert len(trim_ts_data) == 2

        mat_size = self.get_stat_size()  # 陣列尺寸
        # pylint: disable=no-else-return
        if root_logger is not None:
            root_logger.debug("Group data loading")
        mat = []
        for i in range(mat_size):
            if root_logger is not None:
                root_logger.debug("{}/{}".format(i, mat_size))
            mat.append(self.load_ts(i, **kwargs))
        # mat: List = [
        #    self.load_ts(i, **kwargs)
        #    for i in range(mat_size)
        # ]

        if root_logger is not None:
            root_logger.debug(
                "Downsampling: {}".format(downsample_freq)
            )
        if downsample_freq is not None:
            # 降採樣率
            assert isinstance(downsample_freq, str)
            for i in range(mat_size):
                # assert isinstance(mat[i], pd.DataFrame) or (mat[i] is None)
                if isinstance(mat[i], pd.DataFrame):
                    mat[i] = (
                        mat[i].resample(downsample_freq).mean()
                    )

        if root_logger is not None:
            root_logger.debug(
                "Export group data into LIST / {}".format(
                    trim_ts_data
                )
            )
        for i in range(mat_size):
            assert isinstance(mat[i], pd.DataFrame) or (
                mat[i] is None
            )
            if isinstance(mat[i], pd.DataFrame):
                mat[i] = tsb.Slice_TSData(mat[i], *trim_ts_data)
        return mat

    # @jit, 不可使用
    def group_ts_framework(
        self,
        func,
        log_parallel: bool = True,
        cpu_ratio: float = 0.5,
    ):
        """
        # 專門用來處理大量數據的框架
        """

        mat_size = self.get_stat_size()
        flags = [[i] for i in range(mat_size)]

        # 建立共用的平行框架
        return parallel_framework.parallel_process_framework(
            func,
            flags,
            log_parallel=True,
            processor_ratio=cpu_ratio,
        )

    # @jit
    def copy_L2U(self, matrix):
        """
        LU矩陣, 從L矩陣複製到U矩陣
        """
        mat_size = self.get_stat_size()
        matrix2 = np.zeros((mat_size, mat_size)) * np.NaN
        for i, mat_slice in enumerate(matrix):
            matrix2[i, i:] = mat_slice[i:]
            matrix2[i + 1 :, i] = matrix2[i, i + 1 :]
        return matrix2

    # @jit
    def calc_describe(self, cpu_ratio: float = 0.5):
        """
        計算各站的平均值與標準差
        sid, mean, std
        """
        mat_describe = np.array(
            self.group_ts_framework(
                self.calc_describe_sub, cpu_ratio=cpu_ratio
            )
        )

        # pylint: disable=attribute-defined-outside-init
        try:
            self.df_GTS_describe = self.df_stat.loc[
                :, [self.sid]
            ]
            self.df_GTS_describe.loc[:, "mean"] = mat_describe[
                :, 0
            ]
            self.df_GTS_describe.loc[:, "std"] = mat_describe[
                :, 1
            ]
        except KeyError as e:
            raise KeyError from e
        return (
            self.df_GTS_describe
        )  # .loc[:, self.sid + ["mean", "std"]]

    # @jit
    def calc_describe_sub(self, flag):
        i = flag[0]
        df1 = self.load_ts(i)
        mat = [None, None]
        if df1 is not None:
            mat = [
                np.nanmean(df1.iloc[:, 0]),
                np.nanstd(df1.iloc[:, 0]),
            ]
        return mat

    def calc_distance(
        self, loc_cols: Union[List, Tuple]
    ) -> np.ndarray:
        """
        計算交互距離
        回傳：一個二維陣列, 尺寸: NXN
            陣列內為站與站的距離
        """
        assert isinstance(loc_cols, (list, tuple))
        assert len(loc_cols) == 2
        for lcol in loc_cols:
            assert lcol in self.df_stat.columns

        # pylint: disable=attribute-defined-outside-init
        loc = np.array(self.df_stat.loc[:, loc_cols].values)
        self.matrix_distance = calc_distance(loc, loc)
        return self.matrix_distance

    # @jit
    def calc_correlation(
        self, cpu_ratio: float = 0.5
    ) -> np.ndarray:
        """
        # 建立相關性矩陣
        """
        # pylint: disable=attribute-defined-outside-init
        self.matrix_corr = self.copy_L2U(
            self.group_ts_framework(
                self.calc_correlation_sub, cpu_ratio=cpu_ratio
            )
        )
        return self.matrix_corr

    # @jit
    def calc_correlation_sub(self, flag):
        """
        計算相關係數矩陣

        一次處理一行
        for the i=th row
            from i+1 to the end
        """
        i = flag[0]

        mat_size = self.get_stat_size()
        mat_line = np.ones(mat_size) * np.NaN

        def data_load_multi(index):
            df = self.load_ts(index)
            if df is not None:
                df = df.resample("1d").mean()
            return df

        df1 = data_load_multi(i)
        if df1 is not None:
            mat_line[i] = 1.0
            for j in range(i + 1, mat_size):
                df2 = data_load_multi(j)
                if df2 is not None:
                    df_merge = pd.concat(
                        [df1, df2], axis=1, join="inner"
                    )
                    df_merge = df_merge[
                        ~df_merge.isnull()
                    ]  # 排除 np.NaN 者
                    mat_corr = df_merge.corr(
                        method="pearson"
                    )  # pearson correlation
                    mat_line[j] = mat_corr.iloc[0, 1]
        return mat_line

    def load_corr_matrix(self, corr_matrix: np.ndarray):
        self.matrix_corr = corr_matrix

    def sort_corrlated(self, num: int = 5) -> pd.DataFrame:
        df_stat = self.df_stat
        df_stat = df_stat.set_index(self.sid)

        columns = ["sid", "sname"]
        for i in range(num):
            columns.append("csid{}".format(i + 1))
            columns.append("csn{}".format(i + 1))
            columns.append("csv{}".format(i + 1))

        sid_list = self.df_stat.loc[:, self.sid].values
        sname_list = self.df_stat.loc[:, self.sname].values
        fname_list = self.df_stat.loc[:, "data_fname"].values
        mat = []
        for i in range(len(sid_list)):
            if fname_list[i] is not None:  # 有資料, 才處理
                sub_mat = [sid_list[i], sname_list[i]]

                # 複製
                df_copy = pd.DataFrame(
                    self.matrix_corr[:, i],
                    columns=[sid_list[i]],
                    index=sid_list,
                )
                df_copy = df_copy.sort_values(
                    by=[sid_list[i]], ascending=False
                )

                for j in range(num):
                    sid = df_copy.index[j + 1]
                    sub_mat.append(sid)
                    sub_mat.append(df_stat.loc[sid, self.sname])
                    sub_mat.append(df_copy.iloc[j + 1, 0])
                mat.append(sub_mat)

        df_corr_sort = pd.DataFrame(mat, columns=columns)
        return df_corr_sort

    # @jit
    def plot_matrix(
        self,
        matrix_corr,
        item: str = "correlation",
        vmax: float = 1.0,
        vmin: float = 0.0,
        cmap: str = "Blues",
        fig_fname=None,
    ):
        # fname = None
        """
        if item == "correlation":
            fname = "matrix_corr.npy"  # 輸出檔案

            if self.matrix_corr is None:
                if os.path.exists(fname):
                    self.matrix_corr = np.load(fname)
                else:
                    self.calc_correlation()
                    np.save(fname.split(".")[0], self.matrix_corr)
        """

        # 繪圖
        fig, ax = plt.subplots(1, figsize=(9, 9))
        if item == "correlation":
            cb = ax.imshow(
                matrix_corr, vmax=vmax, vmin=vmin, cmap=cmap
            )
            ax.set_title("Matrix of Correlation")
            fig.colorbar(cb, ax=ax, extend="both")

        ax.grid()

        plt.tight_layout()
        if fig_fname is None:
            plt.show()
        else:
            jut.save_fig(fig_fname)


class Group_TSP_multi:  # pylint: disable=too-few-public-methods
    """
    用來記錄多種物理量之跨站分析
    """

    # 建構子
    def __init__(self):
        self.df_GTSP: List = []
        self.vname_list: List = []

    # @jit
    def add_GTSP(
        self,
        vname: str,  # 物理量名稱
        stat_fname: Union[str, pd.DataFrame],
        sid: str,
        sname: str,
        data_path: Union[str, List[str]],
        # filter_code: str = "",
        # log_debug: bool = False,
        **kwargs,
    ):
        """
        逐一添加物理量, 對照站資料
        """
        self.vname_list.append(vname)
        my_TSP = Group_TSP()
        my_TSP.group_station_assign(
            stat_fname,
            sid,
            sname,
            data_path,
            layer_name=vname,
            # filter_code=filter_code,
            # log_debug=log_debug,
            **kwargs,
        )
        self.df_GTSP.append(my_TSP)

    def get_GTSP(self, vname):
        myid = self.get_variable_id(vname)
        try:
            # 輸入之 vname, 已透過 add_GTSP 輸入則回傳 int 型態之 id
            # 否則為 None
            assert myid is not None
        except AssertionError:
            print("!!! vname: {}".format(vname))
            print("    請利用 get_variable_id 加入 vname")
            sys.exit()
        return self.df_GTSP[myid]

    def get_variable_id(self, vname: str) -> Union[None, int]:
        """
        輸入 vname, 回傳 id
        """
        myid = None
        for i in range(len(self.vname_list)):
            if self.vname_list[i] == vname:
                myid = i
                break
        return myid

    def determine_sorted_stat(
        self,
        matrix,
        vname1: str,
        vname2: str,
        sid_query: str,
        ascending: bool = True,
        # log_debug: bool = False,
        **kwargs,
    ):
        root_logger = kwargs.get("root_logger", None)
        if isinstance(sid_query, int):
            sid_query = str(sid_query)
        # sid_query 是屬於 vname1 者
        GTSP1 = self.get_GTSP(vname1)
        GTSP2 = self.get_GTSP(vname2)
        stat_info = None
        if root_logger is not None:
            root_logger.debug(
                " -- Find SID:{} in stat system {} or {}".format(
                    sid_query, vname1, vname2
                )
            )
            root_logger.debug("    {}".format(type(sid_query)))
            root_logger.debug("    {}".format(len(sid_query)))
            root_logger.debug(
                "    {}: {}".format(
                    vname1, GTSP1.df_stat[GTSP1.sid].values
                )
            )
            root_logger.debug(
                "    {}: {}".format(
                    vname2, GTSP2.df_stat[GTSP2.sid].values
                )
            )
        if sid_query in GTSP1.df_stat[GTSP1.sid].values:
            # from vname1 --> vname2
            index = GTSP1.query_index_from_sid(sid_query)
            stat_info = sorted_value_stat(
                matrix[index, :],
                GTSP2.get_GTS_sid(),  # vname2
                GTSP2.get_GTS_sname(),  # vname2
                ascending=ascending,
            )
        elif sid_query in GTSP2.df_stat[GTSP2.sid].values:
            # from vname2 --> vname1
            index = GTSP2.query_index_from_sid(sid_query)
            stat_info = sorted_value_stat(
                matrix[:, index],
                GTSP1.get_GTS_sid(),  # vname1
                GTSP1.get_GTS_sname(),  # vname1
                ascending=True,
            )
        else:
            root_logger.debug(
                "!!! sid_query: {} doesn't exist in either {} or {}".format(
                    sid_query, vname1, vname2
                )
            )
            sys.exit()
        return stat_info

    def calc_cross_process_framework(
        self,
        vname1: str,
        vname2: str,
        func: str,
        param: List,
        downsample_freq=None,
        cpu_ratio: float = 0.5,
        log_debug: bool = False,
    ) -> np.ndarray:
        """
        計算交互過程關係
        回傳：一個二維陣列, 尺寸: NXM
            N 為 vname1 之站數
            M 為 vname2 之站數
            陣列內為站與站的距離
        """
        # GTSP1 = self.get_GTSP(vname1)
        # GTSP2 = self.get_GTSP(vname2)

        df_list1 = self.get_GTSP(vname1).get_GTS_list(
            downsample_freq=downsample_freq
        )  # 水位
        df_list2 = self.get_GTSP(vname2).get_GTS_list(
            downsample_freq=downsample_freq
        )  # 雨量
        sid_list1 = self.get_GTSP(vname1).get_GTS_sid()
        sid_list2 = self.get_GTSP(vname2).get_GTS_sid()
        sname_list1 = self.get_GTSP(vname1).get_GTS_sname()
        sname_list2 = self.get_GTSP(vname2).get_GTS_sname()

        matrix = []
        for m in range(len(df_list1)):
            print(
                "{}. ({}) / ({})".format(
                    m, len(df_list1), len(df_list2)
                )
            )
            flags = []
            sub_mat = []
            for n in range(len(df_list2)):
                flag = [
                    func,
                    df_list1[m],
                    sid_list1[m],
                    sname_list1[m],
                    df_list2[n],
                    sid_list2[n],
                    sname_list2[n],
                ] + param

                flags.append(flag)
            sub_mat = (
                parallel_framework.parallel_process_framework(
                    wrapper,
                    flags,
                    log_parallel=True,
                    processor_ratio=cpu_ratio,
                )
            )
            matrix.append(sub_mat)
        return np.array(matrix)

    def calc_cross_distance(
        self,
        vname1: str,
        loc_cols1: List,
        vname2: str,
        loc_cols2: List,
        # log_debug: bool = False,
        **kwargs,
    ) -> np.ndarray:
        """
        計算交互距離
        回傳：一個二維陣列, 尺寸: NXM
            N 為 vname1 之站數
            M 為 vname2 之站數
            陣列內為站與站的距離
        """
        GTSP1 = self.get_GTSP(vname1)
        GTSP2 = self.get_GTSP(vname2)
        root_logger = kwargs.get("root_logger", None)
        if root_logger is not None:
            root_logger.debug(
                "  -- 觀測系統暱稱: {}".format(vname1)
            )
            root_logger.debug("     欄位: {}".format(loc_cols1))
            root_logger.debug(
                "  -- 觀測系統暱稱: {}".format(vname2)
            )
            root_logger.debug("     欄位: {}".format(loc_cols2))

        # 確認輸入之 X & Y 欄位是否存在
        try:
            for col in loc_cols1:
                assert col in GTSP1.df_stat.columns
            for col in loc_cols2:
                assert col in GTSP2.df_stat.columns
        except AssertionError as e:
            message = "!!! 輸入之欄位，不存在於 df.columns 中\n    '{}' not in {}\n    '{}' not in {}".format(
                loc_cols1,
                GTSP1.df_stat.columns,
                loc_cols2,
                GTSP2.df_stat.columns,
            )
            if root_logger is not None:
                root_logger.debug(message, exc_info=True)
            raise AssertionError(message) from e

        loc1 = np.array(GTSP1.df_stat.loc[:, loc_cols1].values)
        loc2 = np.array(GTSP2.df_stat.loc[:, loc_cols2].values)
        matrix_distance = calc_distance(loc1, loc2)
        return matrix_distance


class Group_TSP_wto_stat:
    """
    # 專門用於, 資料量極多
    # 以 stat_fname 紀錄各站資訊
    # 採循序載入資料, 交叉計算跨站資訊
    """

    def __init__(self, df_GTSP: pd.DataFrame):
        """
        # 建構子
        """
        self.df_GTSP = df_GTSP
        self.matrix_corr = None  # 相關性矩陣

    # @jit, 不可使用
    def group_ts_framework(
        self,
        func,
        cpu_ratio: float = 0.5,
        log_parallel: bool = True,
    ):
        """
        # 專門用來處理大量數據的框架
        """
        flags = list(self.df_GTSP.columns)
        matrix = parallel_framework.parallel_process_framework(
            func,
            flags,
            log_parallel=log_parallel,
            processor_ratio=cpu_ratio,
        )
        return matrix

    @jit
    def copy_L2U(self, matrix):
        """
        LU矩陣, 從L矩陣複製到U矩陣
        """
        mat_size = self.df_GTSP.shape[1]
        matrix2 = np.zeros((mat_size, mat_size)) * np.NaN
        for i, mat_slice in enumerate(matrix):
            matrix2[i, i:] = mat_slice[i:]
            matrix2[i + 1 :, i] = matrix2[i, i + 1 :]
        return matrix2

    #######################################################################################
    #######################################################################################
    #######################################################################################
    # @jit, 不可用 jit
    def calc_describe(self, **kwargs):
        """
        計算各站的平均值與標準差
        sid, mean, std
        """
        return np.array(
            self.group_ts_framework(
                self.calc_describe_core, **kwargs
            )
        )

    @jit
    def calc_describe_core(self, column_name):
        """
        計算平均值與標準差
        """
        assert isinstance(column_name, str)
        return [
            np.nanmean(self.df_GTSP.loc[:, column_name].values),
            np.nanstd(self.df_GTSP.loc[:, column_name].values),
        ]

    #######################################################################################
    #######################################################################################
    #######################################################################################
    # @jit
    def calc_correlation(
        self,
        **kwargs,
    ) -> np.ndarray:
        """
        # 建立相關性矩陣
        """
        # pylint: disable=attribute-defined-outside-init
        self.matrix_corr = self.copy_L2U(
            self.group_ts_framework(
                self.calc_correlation_sub,
                **kwargs,
            )
        )
        return self.matrix_corr

    # @jit
    def calc_correlation_sub(
        self, column_name1: str
    ) -> np.ndarray:
        """
        計算相關係數矩陣
        """
        index1 = determine_index(
            column_name1, list(self.df_GTSP.columns)
        )
        mat_line = np.zeros(self.df_GTSP.shape[1])
        mat_line[index1] = 1
        for index2 in range(index1 + 1, self.df_GTSP.shape[1]):
            column_name2 = self.df_GTSP.columns[index2]
            df_pair = self.df_GTSP.loc[
                :, [column_name1, column_name2]
            ]
            print(index1, index2)
            print(df_pair.head())

            # 排除 NaN
            for cn in [column_name1, column_name2]:
                df_pair = df_pair[
                    ~df_pair[cn].isnull()
                ]  # 排除 np.NaN 者
            mat_corr = df_pair.corr(
                method="pearson"
            )  # pearson correlation
            mat_line[index2] = mat_corr.iloc[0, 1]
        return mat_line

    #######################################################################################
    #######################################################################################
    #######################################################################################
    @jit
    def query_correlated(self, column_name) -> pd.DataFrame:
        """
        給予 column_name, 依序從最相關至不相關依序呈現
        回傳 DataFrame
        index 為相關係數, 已排序
        column_name 為唯一的欄位, 內含對應的column
        """
        assert column_name in self.df_GTSP.columns
        df_corr = pd.DataFrame(
            self.matrix_corr, columns=self.df_GTSP.columns
        ).loc[:, [column_name]]
        df_corr.loc[:, "column_name"] = list(
            self.df_GTSP.columns
        )
        df_corr = df_corr.set_index(column_name).sort_index(
            ascending=False
        )
        return df_corr

    def data_fixed_group(self, log_debug: bool = False):
        """
        群體資料校正
        # 從最相關的逐一建立線性回歸, 依序至不相關
        """
        if self.matrix_corr is None:
            self.calc_correlation()  # 缺少相關性矩陣, 呼叫計算相關性矩陣

        def data_fixed_core(
            column_in: Union[str, List], column_out: str
        ) -> Tuple:
            """
            column_in: 為自變數之欄位名稱, 可以為 str, 也可以是 List
                        str 型態, 代表單一自變數
                        List 型態則可能為多自變數
            column_out: 為應變數之欄位名稱, 必須為單一字串
            """
            assert isinstance(column_in, (str, list))
            assert isinstance(column_out, str)
            if isinstance(column_in, str):
                # str -> list, 統一型態為 list
                column_in = [column_in]
            column_list = [column_out] + column_in

            @jit
            def remove_NaN(
                df, column_list: List
            ) -> pd.DataFrame:
                """
                依據 column_list, 逐攔檢查, 排除 NaN 數據
                """
                for column in column_list:
                    df = df[~df[column].isnull()]
                return df

            # 排除 X || Y 為 np.NaN 者
            df_data_prepare = remove_NaN(
                self.df_GTSP.loc[:, column_list], column_list
            )

            X = np.array(df_data_prepare.loc[:, column_in])
            Y = np.array(
                df_data_prepare.loc[:, column_out]
            ).reshape((-1))
            reg = LinearRegression().fit(X, Y)

            # 排除 X 為 np.NaN 者
            df_data_prepare = remove_NaN(
                self.df_GTSP.loc[:, column_list], column_in
            )
            X2 = np.array(df_data_prepare.loc[:, column_in])
            Y2 = reg.predict(X2)
            return (
                df_data_prepare.index,
                Y2,
            )  # mask & prediction result

        with alive_bar(self.df_GTSP.shape[1]) as abar:
            for column in self.df_GTSP.columns:
                # 查詢係數矩陣
                df_corr = self.query_correlated(column)

                ###################################################################################
                # 螢幕輸出最相關之係數矩陣
                if np.any(
                    self.df_GTSP[column].isnull()
                ):  # 存在 NaN 數據
                    if log_debug:
                        print("# {}:".format(column))
                for i in range(df_corr.shape[0] - 1):
                    nan_count = np.sum(
                        self.df_GTSP[column].isnull()
                    )
                    if np.any(
                        self.df_GTSP[column].isnull()
                    ):  # 存在 NaN 數據
                        if log_debug:
                            print(
                                "    --> {} / {}".format(
                                    df_corr.loc[
                                        df_corr.index[i + 1],
                                        "column_name",
                                    ],
                                    df_corr.index[i + 1],
                                )
                            )
                        # 單變量線性回歸
                        column_in = [
                            df_corr.loc[
                                df_corr.index[i + 1],
                                "column_name",
                            ]
                        ]
                        mask1, y1 = data_fixed_core(
                            column_in, column
                        )
                        self.df_GTSP.loc[
                            mask1, column + "_fixed"
                        ] = y1
                        # replace
                        self.df_GTSP.loc[:, column] = np.where(
                            self.df_GTSP[column].isnull(),
                            self.df_GTSP.loc[
                                :, column + "_fixed"
                            ].values,  # 輸入補遺值
                            self.df_GTSP.loc[
                                :, column
                            ].values,  # 原數值
                        )
                        if log_debug:
                            print(
                                "        NaN 數量: from {} to {}".format(
                                    nan_count,
                                    np.sum(
                                        self.df_GTSP[
                                            column
                                        ].isnull()
                                    ),
                                )
                            )

                        # 刪除補遺值
                        self.df_GTSP = self.df_GTSP.drop(
                            columns=[column + "_fixed"]
                        )
                    else:
                        break
                # pylint: disable=not-callable
                abar()  # 進度 + 1
                ###################################################################################


class cross_station_radius:
    """
    跨觀測站網的 radius 搜尋查詢
    """

    def __init__(
        self,
        df_stat1: pd.DataFrame,
        df_stat2: pd.DataFrame,
        log_WGS84: bool = True,
    ):
        """
        df_stat1 為主體, df_stat2 為對比體
        必須含有 sname, (X, Y)
        index 為 sid
        log_WGS84 是否為 WGS84 or TWD97 系統, 兩系統須一致
            如果為 WGS84, 則 X 為經度 longtitude , Y 為緯度 tatitude
            慣用輸入 (lat, lon)
        """
        for df in [df_stat1, df_stat2]:
            try:
                assert isinstance(df, pd.DataFrame)
                assert "sname" in df.columns
                assert "X" in df.columns
                assert "Y" in df.columns
            except AssertionError:
                raise ValueError(df.head())
        self.df_stat1 = df_stat1
        self.df_stat2 = df_stat2
        self.log_WGS84 = log_WGS84

    def determine_distance_matrix(
        self, cpu_ratio: float = 0.4, log_parallel: bool = True
    ) -> np.ndarray:
        """
        計算距離矩陣
        """
        stat1_locs = np.array(
            self.df_stat1.loc[:, ["X", "Y"]].values
        )
        stat2_locs = np.array(
            self.df_stat2.loc[:, ["X", "Y"]].values
        )
        flags = []
        for i in range(stat1_locs.shape[0]):
            flags.append(
                [stat1_locs[i, :], stat2_locs, self.log_WGS84]
            )
        matrix = parallel_framework.parallel_process_framework(
            determine_distance_multi,
            flags,
            log_parallel=log_parallel,
            processor_ratio=cpu_ratio,
        )
        self.matrix_distance = np.array(matrix)

        return self.matrix_distance

    def query_radius(self, sid1: str, radius: float) -> list:
        """
        查詢 sid1 半境內的測站
        """
        df_stat2_distance = self.df_stat2.copy()
        df_stat2_distance["distance"] = self.matrix_distance[
            self.df_stat1.index.get_loc(sid1), :
        ]
        df_stat2_distance = df_stat2_distance[
            df_stat2_distance["distance"] <= radius
        ]
        df_stat2_distance = df_stat2_distance.sort_values(
            by=["distance"]
        )
        return list(df_stat2_distance.index)
