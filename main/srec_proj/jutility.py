"""
Utility function and class
"""

# -*- coding: utf-8 -*-
import os
import math
from typing import List, Union, Tuple, Optional
import warnings

# pylint: disable=ungrouped-imports
import matplotlib.pyplot as plt
import matplotlib.font_manager
import pandas as pd
import numpy as np

warnings.filterwarnings("ignore")


class KrigingFail(Exception):
    """
    用來處理 Kriging 建模失敗的問題
    """

    pass


def save_fig(fig_fname: Union[str, None]):
    """
    繪圖寫入檔案
    if fig_fname is None, 螢幕輸出
    if type of fig_fname is str, export to file
    """
    assert isinstance(fig_fname, str) or (fig_fname is None)
    plt.tight_layout()
    png_name = fig_fname + ".png"
    os.makedirs(os.path.dirname(fig_fname), exist_ok=True)
    plt.savefig(png_name, dpi=150)
    plt.cla()
    plt.clf()
    plt.close()


def check_isfloat(elem: str) -> bool:
    """
    檢查字串, 是否可轉換乘實數?
    """
    try:
        assert isinstance(elem, str)

        float(elem)
        return True
    except ValueError:
        # print "Not a float"
        return False
    except AssertionError as e:
        raise TypeError(
            "Wrong type: {}".format(type(elem))
        ) from e


def check_same_list(list1: List, list2: List) -> bool:
    """
    確認兩個 list 是否相同
    裡面包涵 str & List
    """
    assert isinstance(
        list1, (list, str, int, float)
    ), "Type of list1 is {}".format(type(list1))
    log_result = False
    if isinstance(list1, type(list2)):
        # 型別需要一致
        if len(list1) == len(list2):
            # elem 內容需要一致
            log_result = True
            try:
                for i, elem in enumerate(list1):
                    assert isinstance(
                        elem, (list, str, int, float)
                    )
                    assert isinstance(elem, type(list2[i]))

                    if isinstance(elem, list):
                        log_result = (
                            log_result
                            and check_same_list(elem, list2[i])
                        )
                    else:
                        log_result = log_result and (
                            elem == list2[i]
                        )
            except AssertionError:
                log_result = False
    return log_result


def check_list_in_list(
    list_all: List[List], list_target: List
) -> bool:
    """
    檢查 list_target 是否位於 list_all 中
    """
    results = [
        check_same_list(la, list_target) for la in list_all
    ]
    return bool(np.any(results))
