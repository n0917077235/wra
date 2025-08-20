# -*- coding: utf-8 -*-
"""
時間序列基礎計算
"""
import datetime

from typing import Union, List, Optional
import warnings
import matplotlib.pyplot as plt
from numba import jit
import numpy as np
import pandas as pd
import jutility as jut
import time_phrase
import except_stdout as exso

warnings.filterwarnings("ignore")

# Jacky's FFT module
# 建立時間序列基礎資料格式
# =======================================================================
# =======================================================================
# =======================================================================


class NotMatchedColumnsError(Exception):
    """
    如果輸入一些欄位名稱, 但全部都不符合, 則回傳此錯誤
    """

    pass


def calc_tendays_index(dt: datetime.datetime) -> int:
    """
    依據 月-日 資訊, 判斷位於那一旬
    """

    month = dt.month
    day = dt.day

    m = month - 1
    td = None
    if day <= 10:
        td = 0
    elif day <= 20:
        td = 1
    else:
        td = 2
    index = 3 * m + td
    return index


def dtstr2datetime(
    dtstr: Union[str, datetime.datetime]
) -> datetime.datetime:
    """
    統一轉為 datetime.datetime 格式
    """
    # pylint: disable=no-else-return
    if isinstance(dtstr, str):
        return time_phrase.time_phrase_multi(dtstr)
    elif isinstance(dtstr, datetime.datetime):
        return dtstr
    else:
        raise TypeError(
            "!!! Wrong Type of argument: '{}'".format(dtstr)
        )


def operator_func(df, dt, operator: str):
    assert isinstance(operator, str)
    assert operator in [">=", ">", "<", "<=", "=="]

    mask_sub = np.full(df.shape[0], True, dtype=bool)
    if operator == ">":
        mask_sub = df > dtstr2datetime(dt)
    elif operator == ">=":
        mask_sub = df >= dtstr2datetime(dt)
    elif operator == "<":
        mask_sub = df < dtstr2datetime(dt)
    elif operator == "<=":
        mask_sub = df <= dtstr2datetime(dt)
    elif operator == "==":
        mask_sub = df == dtstr2datetime(dt)
    return mask_sub


def operation_mask(df, dt, mask, operator: str):
    """
    建立切割的 mask
    """
    if dt is not None:
        mask_sub = np.full(df.shape[0], True, dtype=bool)
        try:
            mask_sub = operator_func(df, dt, operator)
        except TypeError as e:
            # TypeError: Invalid comparison between dtype=datetime64[ns, Asia/Taipei] and Timestamp
            raise TypeError("{} / {}".format(df, dt)) from e
            df.index = pd.to_datetime(df.index)
            mask_sub = operator_func(df, dt, operator)

        mask = np.logical_and(
            mask,
            np.array(
                mask_sub,
                dtype=bool,
            ),
        )
    return mask


def Slice_TSData_mask(
    df: Union[pd.DataFrame, pd.Series],
    dt_str: Union[str, None, datetime.datetime] = None,
    dt_end: Union[str, None, datetime.datetime] = None,
):
    """
    切割資料
    用來避免 df[dt_str: dt_end] 在 typehint 時所產生的錯誤碼
    error: Slice index must be an integer or None
    """
    # 格式必須要是 pd.DataFrame 或 pd.Series 之一
    # assert isinstance(df, pd.DataFrame) or isinstance(df, pd.Series)
    assert isinstance(df, (pd.DataFrame, pd.Series))
    for dt in [dt_str, dt_end]:
        assert isinstance(dt, (str, datetime.datetime)) or (
            dt is None
        )

    mask = np.full(df.shape[0], True, dtype=bool)

    mask = operation_mask(df.index, dt_str, mask, ">=")
    mask = operation_mask(df.index, dt_end, mask, "<")
    return mask


def Slice_TSData(
    df: Union[pd.DataFrame, pd.Series, np.ndarray],
    dt_str: Optional[Union[str, datetime.datetime]] = None,
    dt_end: Optional[Union[str, datetime.datetime]] = None,
) -> pd.DataFrame:
    """
    切割資料
    用來避免 df[dt_str: dt_end] 在 typehint 時所產生的錯誤碼
    error: Slice index must be an integer or None
    """
    assert isinstance(
        df, (pd.DataFrame, pd.Series, np.ndarray)
    ), "!!! Wrong Type of argument: '{}'".format(type(df))
    for dt in [dt_str, dt_end]:
        assert isinstance(dt, (str, datetime.datetime)) or (
            dt is None
        ), "{} / {}".format(dt, type(dt))

    df2 = df.copy()
    if isinstance(df, (pd.DataFrame, pd.Series)):
        if df.shape[0] > 0:
            mask = Slice_TSData_mask(df, dt_str, dt_end)
            df2 = df[mask]
    elif isinstance(df, np.ndarray):
        # 新增功能: for ndarray
        mask = np.full(df.shape[0], True, dtype=bool)
        mask = np.logical_and(
            mask,
            np.array(
                operator_func(df2, dt_str, ">="),
                dtype=bool,
            ),
        )
        mask = np.logical_and(
            mask,
            np.array(
                operator_func(df2, dt_end, "<"),
                dtype=bool,
            ),
        )
        df2 = np.array(
            [df2[i] for i in range(df2.shape[0]) if mask[i]]
        )

    return df2


def create_time_fmt(dt_string: str) -> str:
    time_fmt_final: str = "%Y-%m-%d"

    sep = "-"
    if dt_string.find("/") >= 0:
        sep = "/"
        time_fmt_final = time_fmt_final.replace("-", sep)
    return time_fmt_final


class DF_operation:
    """
    DataFrame Operation
    concat 為常用的技法, 但錯誤的使用, 可能衍生錯誤
    例如: 相同欄位, 但並未串接在後, 而是產生相同欄位名稱之數據
    1. 檢查是否存在相同欄位名稱
    2. 校正
        a. 將不同欄位但名稱相同的數據, 合併並加在後方
        b. 刪除重複的欄位
    """

    def __init__(self, df: pd.DataFrame):
        self.df = df

    # def get_df(self) -> pd.DataFrame:
    #    return self.df

    def check_duplicated_columns(self) -> bool:
        """
        檢查是否存在重複欄位名稱
        """
        columns = []
        count = 0
        for column in self.df.columns:
            if column not in columns:
                columns.append(column)
            else:
                count += 1
        return count > 0  # >0, 代表存在重複欄位

    def check_duplicated_column_single(
        self, column: str
    ) -> bool:
        """
        檢查特定欄位名稱是否存在多次
        """
        assert column in self.df.columns
        count = 0
        for i in range(self.df.shape[1]):
            if column == self.df.columns[i]:
                count += 1
        return count > 1  # >1, 代表存在重複欄位

    def find_matched_columns(
        self, columns: List[str]
    ) -> List[str]:
        """
        輸入 columns 資訊, 回傳符合的欄位名稱
        """
        assert isinstance(columns, list)

        matched_columns = [
            column
            for column in columns
            if column in self.df.columns
        ]
        return matched_columns

    def duplicated_columns_df_revise(self) -> pd.DataFrame:
        """
        修正重複欄位名稱的 DataFrame
        1. 重複的欄位, 串接在後
        2. 再重新整並
        """
        columns_unique = list(set(self.df.columns))
        df_dict = {}
        for column in columns_unique:
            if self.check_duplicated_column_single(column):
                df_dict[column] = pd.DataFrame([])
                for i in range(self.df.shape[1]):
                    if column == self.df.columns[i]:
                        df_dict[column] = pd.concat(
                            [
                                df_dict[column],
                                self.df.iloc[:, [i]],
                            ],
                            axis=0,
                        )
            else:
                df_dict[column] = self.df.loc[:, [column]]

        df_merge = pd.DataFrame([])
        for _column, df in df_dict.items():
            df2 = df[~df[_column].isnull()]
            df2 = df2[~df2.index.duplicated(keep="last")]
            df_merge = pd.concat(
                [df_merge, df2], axis=1, join="outer"
            )
        return df_merge


def select_matched_df_columns(df, columns: List) -> pd.DataFrame:
    """
    輸入多個欄位, 依據符合欄位, 回傳 DataFrame
    """
    DFO = DF_operation(df)
    matched_columns = DFO.find_matched_columns(columns)
    if len(matched_columns) == 0:
        raise NotMatchedColumnsError(
            "'{}' not in '{}'".format(
                columns,
                df.columns,
            )
        )
    return df.loc[:, matched_columns]


df_empty = pd.DataFrame([[0]], columns=["empty"])


def revise_column_name(
    column_name: str, columns, sub_index: int = 0
) -> str:
    """
    加入修改的字串, 使其不重複
    """
    column_name2 = "{}_{}".format(column_name, sub_index + 1)
    if column_name2 not in columns:
        # 如果不重複, 則回傳
        return column_name2
    else:
        return revise_column_name(
            column_name, columns, sub_index=sub_index + 1
        )


def rename_columns(
    df2: pd.DataFrame, columns: List
) -> pd.DataFrame:
    """
    if df2.columns 的名稱與 columns 重複,
    則在 columns name 中加入 "_1", "_2" 等字串
    """
    assert isinstance(columns, list)

    sub_index = 0
    for column in df2.columns:
        if column in columns:
            column2 = revise_column_name(
                column, columns, sub_index=sub_index
            )
            df2 = df2.rename(columns={column: column2})
            columns.append(column2)
    return df2


class TSP_base:
    """
    # 以物件導向寫法，建立時間序列容器 for base
    # 只限制一個 column 的數據
    """

    # 建構子
    def __init__(
        self,
        df: Union[pd.DataFrame, pd.Series] = None,
        style: str = "ggplot",
    ):
        """
        建構子

        modification: 2022/2/12
                刪除欄位名稱的設定功能
        """
        self.__df_ts: pd.DataFrame = pd.DataFrame([])
        self.__var_name: List[str] = []
        self.fig_style: str = style

        if df is not None:
            self.assign_data(df)  # 設定數值

    # pylint: disable=too-many-branches
    # @jit, don't use it'
    def assign_data(
        self,
        df: Union[pd.DataFrame, pd.Series],
    ):
        """
        TSP 加入 df (型態僅限 pd.DataFrame or pd.Series)
        variable_name 設定變數名稱
        """
        try:
            assert isinstance(df, (pd.Series, pd.DataFrame))
        except AssertionError as e:
            message = "!!! df 型態錯誤: {} (應為 pd.Series or pd.DataFrame)\n兩者數量須一致".format(
                type(df)
            )
            raise TypeError(message) from e

        df2 = df.copy()
        if isinstance(df, pd.Series):
            df2 = df.to_frame()

        # 如果欄位名稱重複, 加入 "_{1, 2, 3, 4, 5, ...}"
        df2 = rename_columns(df2, self.__var_name)

        # 交集
        self.__df_ts = pd.concat(
            [self.__df_ts, df2], axis=1, join="outer"
        )
        self.__var_name = list(self.__df_ts.columns)
        self.__df_ts.index.names = ["time"]

        # 時間的起始與結束
        self.start_date: str = self.__df_ts.index[0]
        self.end_date: str = self.__df_ts.index[-1]

    def determine_vname(self, variable_name: str):
        # 更改 variable_name, 使其無重複
        if variable_name in self.__var_name:
            variable_name = self.determine_vname(
                variable_name + "_1"
            )
        return variable_name

    def date_limit(
        self,
        start: Optional[str] = None,
        end: Optional[str] = None,
    ):
        if start is not None:
            self.start_date = start
        else:
            self.start_date = self.__df_ts.index[0].strftime(
                "%Y-%m-%d %H:%M:%S"
            )

        if end is not None:
            self.end_date = end
        else:
            self.end_date = datetime.datetime.now().strftime(
                "%Y-%m-%d %H:%M:%S"
            )
        self.date_limit_update()

    def date_limit_update(self):
        # pylint: disable=attribute-defined-outside-init
        start_date: Optional[str] = self.start_date
        end_date: Optional[str] = self.end_date
        if self.start_date != "":
            start_date = None
        if self.end_date != "":
            end_date = None
        self.__df_ts = Slice_TSData(
            self.__df_ts, start_date, end_date
        )
        """
        if self.start_date != "":
            self.__df_ts = self.__df_ts[
                self.start_date :
            ]  # 設定日期前後段

        if self.end_date != "":
            self.__df_ts = self.__df_ts[
                : self.end_date
            ]  # 設定日期前後段
        """

    def update_data(self, df: pd.DataFrame):
        # pylint: disable=attribute-defined-outside-init
        self.__df_ts = df
        self.date_limit_update()

    def get_data(self) -> pd.DataFrame:
        return self.__df_ts

    def get_var_name(self) -> List[str]:
        return self.__var_name

    def get_data_size(self) -> int:
        return self.__df_ts.shape[0]

    def get_data_col_size(self) -> int:
        return self.__df_ts.shape[1]

    def export_data_csv(self, fname: str = "export.csv"):
        # fname = self.get_var_name() + ".csv"
        self.__df_ts.to_csv(fname)

    def import_data_csv(self, fname: str):
        # pylint: disable=broad-except
        try:
            # pylint: disable=attribute-defined-outside-init
            self.__df_ts = pd.read_csv(fname).rename(
                columns={"Unnamed: 0": "time"}
            )
            self.__df_ts = self.__df_ts.set_index("time")
            self.__df_ts.index = pd.to_datetime(
                self.__df_ts.index
            )
            self.__var_name = self.__df_ts.columns
        except FileNotFoundError as e:
            raise FileNotFoundError(
                "!!! import data: {}".format(fname)
            ) from e

    def export_data_hdf(self, fname: str):
        assert isinstance(fname, str)
        self.__df_ts.to_hdf(
            fname,
            key="df",
            mode="w",
            complevel=4,
            complib="blosc",
        )

    def import_data_hdf(self, fname: str):
        """
        data import
        """
        # pylint: disable=broad-except
        # pylint: disable=attribute-defined-outside-init
        self.__df_ts = pd.read_hdf(fname)
        self.__var_name = self.__df_ts.columns

    def determine_fig_fname(
        self, fname: Union[str, None] = None
    ):
        if isinstance(fname, type(None)):
            # 預設出圖檔名, 以欄位一 + "_raw" 繪圖
            fname = self.get_var_name()[0] + "_raw"
        return fname

    def assign_fig_style(self):
        plt.style.use(self.fig_style)  # style assignment

    # @jit
    def plot_method(self, ax=None, **kwargs):
        df = self.__df_ts
        if ax is None:
            df.plot(grid=True)
        else:
            df.plot(ax=ax, grid=True)

    # @jit
    def plot_TS(
        self,
        fig_fname=None,
        # fig_xlabel=None,
        # fig_ylabel=None,
        # fig_title=None,
        figsize: tuple = (9, 6),
        fig_style: str = "ggplot",
        **kwargs,
    ):
        self.assign_fig_style()

        # pylint: disable=unused-variable
        plt.style.use(fig_style)
        fig, ax = plt.subplots(1, figsize=figsize)
        self.plot_method(ax=ax, **kwargs)

        def check_fig_setup(func, label: str, **kwargs):
            """
            確認外部是否有輸入參數, 如有則執行設定
            """
            if label in kwargs.keys():
                func(kwargs[label])

        check_fig_setup(ax.set_xlabel, "fig_xlabel", **kwargs)
        check_fig_setup(ax.set_ylabel, "fig_ylabel", **kwargs)
        check_fig_setup(ax.set_title, "fig_title", **kwargs)
        check_fig_setup(ax.set_xlim, "fig_xlim", **kwargs)
        check_fig_setup(ax.set_ylim, "fig_ylim", **kwargs)

        ax.legend(shadow=True)

        plt.tight_layout()
        if fig_fname is not None:
            jut.save_fig(fig_fname)  # 存檔
        else:
            plt.show()

    @jit
    def show_TS(self):
        print(self.__df_ts)


def plot_TS_multi(
    list_TSP: List, fname: str, figsize: tuple = (9, 6)
):
    # pylint: disable=broad-except
    try:
        assert len(list_TSP) > 0

        list_TSP[0].assign_fig_style()  # fig style

        # pylint: disable=unused-variable
        fig, ax = plt.subplots(1, figsize=figsize)
        for i in range(len(list_TSP)):
            list_TSP[i].plot_method(ax=ax)
        # list_TSP[0].export_fig_close(fname, fig, ax)
        jut.save_fig(fname)  # 存檔

    except Exception as e:
        exso.except_stdout(e)
