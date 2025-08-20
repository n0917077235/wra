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

warnings.filterwarnings("ignore")


# Jacky's module
# 建立群體時間序列計算
# =======================================================================
# =======================================================================
# =======================================================================

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


class Found(Exception):
    """
    專用於雙層迴圈的搜尋 & break
    """

    # pylint: disable=unnecessary-pass
    pass
