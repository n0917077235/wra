"""
Utility function and class
"""

# -*- coding: utf-8 -*-
import os
import sys
import inspect
import re
import math
import functools
import datetime
from typing import List, Union, Tuple, Optional
import warnings

# from numba import jit
import socket
import subprocess

# pylint: disable=ungrouped-imports
import matplotlib.pyplot as plt
import matplotlib.font_manager
import pandas as pd
import numpy as np
import file_utility as fut

warnings.filterwarnings("ignore")


class KrigingFail(Exception):
    """
    用來處理 Kriging 建模失敗的問題
    """

    pass


class SystemCallFail(Exception):
    """
    用來處理 System Call 失敗的問題
    """

    pass


# dict Query
# 輸入 key 名稱
# 如 key in keys(), 回傳數值
# 如 key not in keys(), 回傳 None
dict_query = lambda kwargs, key: (
    kwargs[key] if key in kwargs.keys() else None
)


def list_query_missing_action(
    kwargs, flag: str, action_return=None
):
    """
    1. 以 dict_query 查詢回應值
    2. 如缺乏該資料，改以回傳 [None, None]
    """
    # result = dict_query(kwargs, flag)
    result = action_return
    if flag in kwargs.keys():
        result = kwargs[flag]
    return result


# @jit
def create_line_bar(size: int, symbol: str) -> str:
    """
    # 建立分隔線字串
    """
    line_bar = ""
    # pylint: disable=unused-variable
    for _ in range(size):
        line_bar += symbol
    return line_bar


def func_name_show(func):
    """
    for debug purpose
    顯示函式檔案與函式名稱
    """

    @functools.wraps(func)
    def func_name_show_core(*args, **kwargs):
        if "log_debug" in kwargs.keys():
            if kwargs["log_debug"]:
                ind = 0
                ind_width = 2
                if "ind" in kwargs.keys():
                    ind = kwargs["ind"]
                if "ind_width" in kwargs.keys():
                    ind_width = kwargs["ind_width"]

                print(
                    "{}--> Calling {}::{}".format(
                        create_line_bar(ind * ind_width, " "),
                        func.__code__.co_filename,
                        func.__name__,
                    )
                )
        result = func(*args, **kwargs)
        return result

    return func_name_show_core


def func_bound(func):
    """
    for debug purpose
    show function name, arguments and other valuable information
    用來描述所在位置的函式特徵
    """

    def dsme_func(*args, **kwargs):
        line_length = 70
        line = create_line_bar(line_length, ">")
        print(line)  # 輸出一行線
        print(line)  # 輸出一行線
        result = func(*args, **kwargs)

        print("<<< End of '{}'".format(func.__name__))
        line = create_line_bar(line_length, "<")
        print(line)  # 輸出一行線
        print(line)  # 輸出一行線
        return result

    return dsme_func


def describe_func(func):
    """
    for debug purpose
    show function name, arguments and other valuable information
    用來描述所在位置的函式特徵
    """

    def dsme_func(*args, **kwargs):
        line_length = 70
        line = create_line_bar(line_length, ">")
        print(line)  # 輸出一行線
        print(line)  # 輸出一行線
        print(
            ">>> Function name: {} (in '{}'')".format(
                func.__name__,
                os.path.basename(func.__code__.co_filename),
            )
        )
        print(">>> Positional arguments: {}".format(args))
        print(">>> Keyword arguments: {}".format(kwargs))
        result = func(*args, **kwargs)

        print("<<< End of '{}'".format(func.__name__))
        line = create_line_bar(line_length, "<")
        print(line)  # 輸出一行線
        print(line)  # 輸出一行線
        return result

    return dsme_func


def code_trace_export(debug_fname):
    """
    debug_fname: debug file, 寫入檔案名稱
    備註: 如若不為 str, 則不啟動 code trace 功能
    追蹤函式名稱, 進入引數, 以及離開點
    """

    def trace(func):
        @functools.wraps(func)
        def location(func) -> str:
            file_location: str = ""
            try:
                file_location = "{}".format(func.__qualname__)
            except Exception:
                file_location = "::{}".format(func.__name__)
            return file_location

        @functools.wraps(func)
        # pylint: disable=too-many-branches
        def wrapper(*args, **kwargs):
            dt1 = datetime.datetime.now()
            if isinstance(debug_fname, str):
                # pylint: disable=unspecified-encoding
                with open(debug_fname, "a") as f:
                    line_length = 70
                    line = create_line_bar(line_length, ">")
                    f.write("{}\n".format(line))  # 輸出一行線
                    # 該 function 所在位置的檔案
                    # inspect.stack()[1].filename
                    message = ">>> Trace: {}".format(
                        location(func)
                    )  # pylint: disable=too-many-format-args
                    f.write("{}\n".format(message))
                    # args
                    f.write("  -> Positional arguments: \n")
                    for i in range(len(args)):
                        if isinstance(args[i], (list, tuple)):
                            length = min(5, len(args[i]))
                            f.write(
                                "     {}. {}\n".format(
                                    i, args[i][:length]
                                )
                            )
                        elif isinstance(args[i], str):
                            length = min(64, len(args[i]))
                            f.write(
                                "     {}. {}\n".format(
                                    i, args[i][:length]
                                )
                            )
                        elif isinstance(
                            args[i], (pd.DataFrame, pd.Series)
                        ):
                            f.write(
                                "     {}. {}\n".format(
                                    i, args[i].head()
                                )
                            )
                        elif isinstance(args[i], dict):
                            keys = args[i].keys()
                            if len(keys) > 0:
                                f.write(
                                    "     {}. Dict\n".format(i)
                                )
                        else:
                            f.write(
                                "     {}. {}\n".format(
                                    i, args[i]
                                )
                            )
                    # kwargs
                    f.write("  -> Keyword arguments: \n")
                    kw_keys = kwargs.keys()
                    for key in kw_keys:
                        if isinstance(
                            kwargs[key], (list, tuple)
                        ):
                            length = min(5, len(kwargs[key]))
                            f.write(
                                "     {}. {}: {}\n".format(
                                    i, key, kwargs[key][:length]
                                )
                            )
                        elif isinstance(kwargs[key], str):
                            length = min(64, len(kwargs[key]))
                            f.write(
                                "     {}. {}: {}\n".format(
                                    i, key, kwargs[key][:length]
                                )
                            )
                        elif isinstance(
                            kwargs[key],
                            (pd.DataFrame, pd.Series),
                        ):
                            f.write(
                                "     {}. {}: {}\n".format(
                                    i, key, kwargs[key].head()
                                )
                            )
                        else:
                            f.write(
                                "     {}. {}: {}\n".format(
                                    i, key, kwargs[key]
                                )
                            )
                    f.write("  -> enter at {}\n".format(dt1))

            result = func(*args, **kwargs)

            if isinstance(debug_fname, str):
                # pylint: disable=unspecified-encoding
                with open(debug_fname, "a") as f:
                    dt2 = datetime.datetime.now()
                    f.write("     exist at {}\n".format(dt2))
                    f.write(
                        "  -> 耗費時間: {}\n".format(dt2 - dt1)
                    )
                    f.write(
                        "<<< End of {}\n".format(location(func))
                    )
                    line = create_line_bar(line_length, "<")
                    f.write("{}\n".format(line))  # 輸出一行線
            return result

        return wrapper

    return trace


def message_export(
    message: str,
    fdebug=None,
    log_debug: bool = False,
    log_EOL: bool = True,
):
    """
    訊息輸出: 螢幕輸出 + debug file log 輸出
    """
    if log_debug:
        print(message)

    if fdebug is not None:
        EOL = "\n"
        if not log_EOL:
            EOL = ""

        message += EOL
        # try:
        fdebug.write(message)
        # except AttributeError as e:
        #    print (type(fdebug))
        #    jut.warning_message2(e)
        #    sys.exit()
    return fdebug


# @jit
def warning_message(fdebug=None):
    """
    輸出系統警示資訊
    """
    message = create_line_bar(75, "!") + "\n"
    message += "    %s\n" % str(sys.exc_info())
    message_export(message, fdebug=fdebug, log_debug=True)
    return fdebug


def save_fig_woclose(
    fig_fname,
    dpi: int = 150,
    fig=None,
    log_show: bool = False,
    **kwargs,  # pylint: disable=unused-argument
):
    """
    # 將螢幕輸出成果，存成 .png
    # 不關閉檔案

    modified date: 加入功能, 關閉繪圖時間
        外部輸入 kwargs["log_time_export“]
    """

    def sequence_export(fig, *args, **kwargs):
        """
        測試兩種不同的輸出法
        """
        # 照片存檔
        try:
            fig.savefig(*args, **kwargs)
        except AttributeError:
            plt.savefig(*args, **kwargs)

    if isinstance(fig_fname, str):
        png_name = fig_fname + ".png"

        log_time_export = kwargs.get(
            "log_time_export", True
        )  # 預設輸出繪圖時間
        if log_time_export:
            # 輸出繪圖時間
            plt.annotate(
                "Plotted on \n"
                + datetime.datetime.now().strftime(
                    "%Y-%m-%d %H:%M"
                ),
                xy=(0.005, 1),
                xytext=(10, -10),
                xycoords="figure fraction",
                textcoords="offset points",
                va="top",
                fontsize=6,
            )

        try:
            # 照片存檔
            sequence_export(fig, png_name, dpi=dpi)
        except FileNotFoundError:
            # 建立目錄
            os.makedirs(
                os.path.dirname(fig_fname), exist_ok=True
            )
            # 再次照片存檔
            sequence_export(fig, png_name, dpi=dpi)

    if log_show:
        # 螢幕輸出
        plt.show()


def save_fig(fig_fname: Union[str, None], **kwargs):
    """
    繪圖寫入檔案
    if fig_fname is None, 螢幕輸出
    if type of fig_fname is str, export to file
    """
    assert isinstance(fig_fname, str) or (fig_fname is None)
    plt.tight_layout()

    if fig_fname is None:
        kwargs["log_show"] = True
    save_fig_woclose(fig_fname, **kwargs)
    plt.cla()
    plt.clf()
    plt.close()


# 不可使用 jit
def get_fig_available_font():
    """
    找出系統內可用之字體
    """
    flist = sorted(
        [
            f.name
            for f in matplotlib.font_manager.fontManager.ttflist
        ]
    )
    print("# Available fonds: {}".format(flist))
    return flist


# @jit
def seperate_path_file(my_path):
    """
    由於Windows與Linux等其他作業系統，其檔案符號系統不一致
    解決不一致問題
    """
    my_path = my_path.replace("\\", "/")
    sepline = my_path.split("/")
    my_path = "."
    if len(sepline) > 1:
        my_path = sepline[0]
        for i in range(1, len(sepline) - 1):
            my_path = os.path.join(my_path, sepline[i])
    my_file = sepline[-1]
    return my_path, my_file


# @jit
def export_formated_tstr(t: int) -> str:
    """
    t index --> formated string
    三碼, 補0
    """
    result: str = ""
    if t < 10:
        result = "00" + str(t)
    elif t < 100:
        result = "0" + str(t)
    else:
        result = str(t)
    return result


def convert_list_of_list2tuple(list_of_list: List):
    """
    # Convert a list of list to a list of tuple
    """
    assert isinstance(
        list_of_list, type([])
    ), "輸入型別錯誤: " + str(type(list_of_list))

    try:
        assert len(list_of_list) > 0
    except AssertionError as e:
        raise AssertionError(
            "!!! 輸入List尺寸不足: " + str(len(list_of_list))
        ) from e

    # 內部是否為 list or tuple
    assert isinstance(list_of_list[0], type([])) | isinstance(
        list_of_list[0], type(())
    ), "輸入型別錯誤: " + str(type(list_of_list[0]))

    mat: List = []
    for i in range(len(list_of_list)):
        mat.append(tuple(list_of_list[i]))
    return mat


def check_xor(A: bool, B: bool) -> bool:
    """
    True xor False --> True
    False xor True --> True
    True xor True --> False
    False xor False --> False
    """
    assert isinstance(A, bool)
    assert isinstance(B, bool)
    return A ^ B


def Check_Duplicated(data_list) -> bool:
    """
    檢驗 data_list 是否重複
    1. 檢查是否有重複
    2. 實質迴圈檢查內容
    True, 表示存在重複
    False, 則無重複
    """
    data_list2: List = list(data_list)
    assert len(data_list2) > 0

    try:
        return len(data_list2) != len(list(set(data_list2)))
    except TypeError:
        # TypeError: unhashable type: 'list'
        # 如 list 的內容物, 為另外一個 list, set 無法運作
        # 則改為迴圈處置
        for elem in data_list2:
            if data_list2.count(elem) > 1:
                return True
    return False


def check_contain_zh(word: str) -> bool:
    """
    確認字串中, 是否包含中文字符
    """
    try:
        assert isinstance(word, str)
        match = re.findall("[\u4e00-\u9fa5]+", word)
        return len(match) > 0
    except AssertionError as e:
        raise TypeError(
            "Wrong type: {}".format(type(word))
        ) from e


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


# @jit
def process_thousand_colon(str_val_tc: str) -> float:
    """
    千分號處理

    Ex.:
        "123,456.2" --> 123456.2
    """
    try:
        assert isinstance(str_val_tc, str)
    except AssertionError as e:
        raise TypeError(
            "{} / {}".format(str_val_tc, type(str_val_tc))
        ) from e

    str_val = str_val_tc.replace(",", "")

    try:
        assert check_isfloat(str_val)  # 必須為可轉換實數之字串
    except AssertionError as e:
        raise TypeError(
            "Something still wrong from '{}' to '{}'".format(
                str_val_tc, str_val
            )
        ) from e
    return transform2float(str_val)


def merge_negetive(sepline: List) -> List:
    """
    ["-", "2", "-", "3"] --> ["-2", "-3"]
    """
    sepline2 = np.array(sepline)
    arg_negetive = np.argwhere(sepline2 == "-")

    sepline3 = sepline.copy()
    for i in range(arg_negetive.shape[0]):
        j = arg_negetive.shape[0] - i - 1
        sepline_temp = sepline3.copy()
        if check_isfloat(sepline3[arg_negetive[j, 0] + 1]):
            # 為可轉換為實數的字串
            sepline_temp = sepline3[: arg_negetive[j, 0]] + [
                sepline3[arg_negetive[j, 0]]
                + sepline3[arg_negetive[j, 0] + 1]
            ]
            if arg_negetive[j, 0] + 2 < len(sepline3):
                sepline_temp = (
                    sepline_temp
                    + sepline3[arg_negetive[j, 0] + 2 :]
                )
        sepline3 = sepline_temp
    return sepline3


def check_isnan(elem) -> bool:
    """
    排除 string 後，檢查是否為 NaN
    """
    # pylint: disable=no-else-return
    if isinstance(elem, str):
        return elem.lower() == "nan"
    elif elem is None:
        return True
    elif not isinstance(elem, float):
        return False
    return math.isnan(elem)


def transform2float(str_val: str) -> float:
    """
    將可以轉換成為 float 的字串進行轉換
    "123,456" --> 123456

    """
    val: float = np.NaN
    # 轉成 float
    try:
        val = float(str_val)
    except ValueError:
        # 刪除數字中的 * 註解符號 & ,千分號
        for flag in ["*", ","]:
            if str_val.find(flag) >= 0:
                str_val = str_val.replace(flag, "")
        val = float(str_val)
    return val


def get_Duplicated_elem_inList(lst: List) -> List:
    """
    Check if given list contains any duplicates
    return True, means duplicated element in list exists!
    """
    result: List = []
    for elem in lst:
        if lst.count(elem) > 1:
            if elem not in result:
                result.append(elem)
        if len(result) > 20:  # 如重複量超過 20 組，則中止輸出
            return result
    return result


# @jit
def replace_list(lst: List, elem, new_elem) -> List:
    """
    # 替換 List 中特定字串
    """
    return [new_elem if (x.find(elem) >= 0) else x for x in lst]


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


def indent_move(ind_num: int, space_size: int):
    """
    依據層次，來提供 indent 的縮排空格
    """
    indent_msg = ""
    for _ in range(ind_num * space_size):
        indent_msg += " "
    return indent_msg


def count_indent_level(sentence: str, space_size: int):
    """
    確認 sentence 字串的 indent level
    """
    # 前頭的空格數
    leading_spaces = len(sentence) - len(sentence.lstrip())
    try:
        assert leading_spaces % space_size == 0
    except AssertionError as e:
        raise TypeError(
            "--> {} / '{}'".format(leading_spaces, sentence)
        ) from e
    return int(leading_spaces / space_size)


def export_two_ways(message: str, fout=None):
    """
    螢幕輸出 / 檔案輸出
    """
    if fout is None:
        print(message)
    else:
        fout.write("{}\n".format(message))


def check_int(s: str):
    """
    確認是否可轉換為整數字串
    """
    if s[0] in ("-", "+"):
        return s[1:].isdigit()
    return s.isdigit()


def export_message_dual(
    message,
    fname: Optional[str],
    log_show: bool = True,
    encoding="utf-8",
):
    """
    # 雙軌輸出

    log_show 可用來關閉螢幕輸出
    """
    if log_show:
        print(message)

    if fname is not None:
        # pylint: disable=bad-option-value
        # pylint: disable=unspecified-encoding
        with open(fname, "a", encoding=encoding) as fdebug:
            fdebug.write("{}\n".format(message))


def check_in_float_list(
    val: Union[int, float], list_sequence: List, criteria: float
) -> bool:
    """
    檢查 val 是否位於 float_list 中
    """
    if val in list_sequence:
        # 如果進到內圈, 表示符合
        # 為了加速, 直接回傳 True
        return True

    # 比較迴圈內的數值差
    compare_result = [
        elem
        for elem in list_sequence
        if abs(val - elem) < criteria
    ]
    return len(compare_result) > 0


def check_lists_equal(list1: List, list2: List, **kwargs):
    """
    比較兩 List 相等
    https://www.geeksforgeeks.org/python-check-if-two-lists-are-identical/
    """
    try:
        assert isinstance(list1, (list, np.ndarray))
        assert isinstance(list2, (list, np.ndarray))

        list1a = list(list1)
        list2a = list(list2)
        list1a.sort()
        list2a.sort()
    except AssertionError as e:
        root_logger = kwargs.get("root_logger", None)
        message = "!!! data type: {} / {}".format(
            type(list1), type(list2)
        )
        if root_logger is not None:
            root_logger.error(message)
        raise TypeError(message) from e
    return list1a == list2a


def check_outdated_IO(check_item: str, message: str, **kwargs):
    """
    偵測是否使用過時用法
    """
    if check_item in kwargs:
        root_logger = kwargs.get("root_logger", None)
        code_loc = get_frameinfo(trace_back_no=2)
        message2 = (
            "from {}: 呼叫 method '{}', {} 已過時, {}".format(
                code_loc,
                get_frame(
                    trace_back_no=1
                ).f_code.co_name,  # 本行所在函式名稱
                check_item,
                message,
            )
        )
        if root_logger is not None:
            root_logger.warning(message2)
        else:
            print(message2)


def get_frame(trace_back_no: int = 0):
    """
    取得呼叫函式的位置, 並回傳 frame
        trace_back_no 用來回溯的層數
    """
    currentframe = inspect.currentframe()
    if trace_back_no > 0:
        assert currentframe is not None
        # typehint: ignore
        # 排除 currentframe is None 的 mypy 警告
        for _ in range(
            trace_back_no + 1
        ):  # +1, 是為了處理 get_frame
            # 回溯
            currentframe = (
                currentframe.f_back
            )  # typehint: ignore # noqa: union-attr
    return currentframe


def get_frameinfo(trace_back_no: int = 0) -> str:
    """
    取得呼叫函式的位置
    """
    assert isinstance(trace_back_no, int)
    currentframe = get_frame(
        trace_back_no + 1
    )  # +1, 是為了處理 get_frameinfo
    if currentframe is not None:
        # 排除 currentframe is None 的 mypy 警告
        # typehint: ignore
        code_loc = "{}::{}::{}".format(
            os.path.basename(
                inspect.getframeinfo(
                    currentframe
                ).filename  # typehint: ignore
            ),
            currentframe.f_code.co_name,  # typehint: ignore
            inspect.getframeinfo(
                currentframe
            ).lineno,  # typehint: ignore
        )
    else:
        # is None
        code_loc = "None"
    return code_loc


def get_code_location(trace_back_no: int = 1) -> str:
    """
    輸出程式碼所在位置, 以 str 輸出

    使用時, 以 inspect.currentframe() 輸入所在位置

    debug, 更名 get_code_location --> get_frameinfo_current
    """
    return "[{}]".format(
        get_frameinfo(trace_back_no=trace_back_no + 1)
    )


##############################################################
# 查詢 localhost_ip 對外 IP
def query_localhost_IP():
    """
    查詢 localhost 對外 IP
    """
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.settimeout(0)
    try:
        # doesn't even have to be reachable
        s.connect(("10.254.254.254", 1))
        IP = s.getsockname()[0]
    except Exception:
        """
        無法連線
        """
        IP = "127.0.0.1"
    finally:
        s.close()
    return IP


def process_suicide(program_name: str, root_logger=None):
    """
    程序自殺
    """
    if root_logger is not None:
        root_logger.info(
            "# Process '{}' suicide! pid={}".format(
                program_name, os.getpid()
            )
        )
    sys.exit()


def std_output(stdout: str, linesize: int = 15) -> List[str]:
    """
    也可以適用於 stdout or stderr
    linesize = 15, 表示如果輸出資訊高於兩倍 linesize,
        則只輸出前後各 linesize 的資訊, 中間補上 ...
        如為負值, 則代表不管幾行, 都全部輸出

    輸出 List
    """
    stdout_sepline = stdout[2:-1].split("\\n")
    log_reduce = False
    if linesize > 0:
        log_reduce = len(stdout_sepline) > (linesize * 2)
    if log_reduce:
        # 製作縮減版
        stdout_sepline2 = [
            stdout_sepline[ll] for ll in range(linesize)
        ]
        stdout_sepline2 += ["..." for _ in range(3)]
        stdout_sepline2 += [
            stdout_sepline[len(stdout_sepline) - linesize + ll]
            for ll in range(linesize)
        ]
        return stdout_sepline2
    # 全輸出
    return stdout_sepline


def command2clist(command: str, **kwargs) -> List:
    """
    cd abc --> [["cd", "abc"]]

    1. 加入環境變數 $DIND_USER_HOME
        "${}".format(flag) --> os.environ[flag]
    2. '' & "" 包覆內容, 不拆解處理
    pipeline
    3. ; 符號, 屬於不相關的兩個指令
        ls ; pwd --> [["ls"], ["pwd:]]
    4. | 符號, 將前一個命令丟給後面的指令
        ls | grep abc --> [["ls"], "|", ["grep", "abc"]]
    5. 混合 ; 與 |
        ls ; ls | grep abc
        --> [["ls"], [["ls"], "|", ["grep", "abc"]]]
    """
    root_logger = kwargs.get("root_logger", None)
    command = command.replace("\t", " ")

    # $DIND_USER_HOME
    # 處理環境變數
    for flag in ["DIND_USER_HOME"]:
        if command.find("${}".format(flag)) >= 0:
            # 需要使用該環境變數
            try:
                command = command.replace(
                    "${}".format(flag),
                    os.environ[flag],
                )
            except KeyError:
                message = "!!! 環境變數 '{}' 不存在".format(flag)
                if root_logger is not None:
                    root_logger.error(message, exc_info=True)
                sys.exit(1)
                # raise KeyError(message) from e

    # 依據 | 與 ;, 拆解成多個指令
    # 從 list of str --> list of list
    # 內層 list 代表一獨立指令 (可以包含 | 進行 pipeline, 但不能有 ;)
    pipe_command = command2list(command, **kwargs)
    split_index = [
        (i, elem)
        for i, elem in enumerate(pipe_command)
        if elem in [";", "|"]
    ]
    if len(split_index) == 0:
        pipe_command2 = [pipe_command]
    else:
        pipe_command2 = [pipe_command[: split_index[0][0]]]
        pipe_command2.append(split_index[0][1])  # 以 ; 符號隔開
        for j in range(1, len(split_index)):
            pipe_command2.append(
                pipe_command[
                    split_index[j - 1][0] + 1 : split_index[j][0]
                ]
            )
            pipe_command2.append(
                split_index[j][1]
            )  # 以 ; 符號隔開
        pipe_command2.append(
            pipe_command[split_index[-1][0] + 1 :]
        )

    def operation_apostrophe(mylist: List, **kwargs) -> List:
        """
        針對單引號或雙引號被拆解到 list 的不同 elem 中,
        重新合併
        """
        # root_logger = kwargs.get("root_logger", None)
        sep = kwargs.get("sep", " ")
        # 針對 '' or "" 包覆內容處理, 不拆解
        matched_count1 = np.array(
            [elem.count("'") for elem in mylist]
        )
        matched_count2 = np.array(
            [elem.count('"') for elem in mylist]
        )
        matched_count = matched_count1 + matched_count2

        # 封閉 '' or "" 的內容
        # ' or " 的一端
        closed_index = np.argwhere(matched_count == 1)
        if len(closed_index) == 0:
            # 不需要整併單雙引號
            return mylist
        mylist2 = []
        i = 0
        pre_list = mylist[: closed_index[i][0]]
        if closed_index[i][0] == 0:
            # 最前頭
            pre_list = []
        post_list = mylist[closed_index[i + 1][0] + 1 :]
        if closed_index[i + 1][0] + 1 == len(mylist):
            # 最後頭
            post_list = []

        middle_term = ""
        for j in range(
            closed_index[i][0], closed_index[i + 1][0] + 1
        ):
            middle_term += "{}".format(mylist[j])
            if j != closed_index[i + 1][0]:
                middle_term += sep
        mylist2 = pre_list + [middle_term] + post_list

        if len(closed_index) > 2:
            # 有多個 '' or ""
            # 重新整併
            # 遞迴呼叫
            return operation_apostrophe(mylist2, **kwargs)

        return mylist2

    def remove_values_from_list(the_list, val):
        return [value for value in the_list if value != val]

    def operation_remove_empty_str(
        mylist: List, **kwargs
    ) -> List:
        """
        清除空的字串
        """
        return remove_values_from_list(mylist, "")

    if root_logger is not None:
        root_logger.debug(
            "{} --> {}".format(command, pipe_command2)
        )
    return pipe_command2


def find_quote(string: str) -> bool:
    """
    ^' ^" '$ "$ 各文字包含一個, 表示多個文字組成的命令列
        總數應為複數
    """
    log_list = np.array(
        [
            bool(re.compile(flag).search(string))
            for flag in ['^"', "^'", '"$', "'$"]
        ]
    )
    return bool(np.any(log_list))


def closed_quote(string: str) -> bool:
    """
    字串前後都是 ' 或 "
    表示被包起來

    回傳包起來的字串
    """

    log_result = False
    if find_quote(string):
        # 表示內部存在 ' 或 "
        for flag in ["'", '"']:
            if string.find(flag) >= 0:
                count_number = string.count(flag)
                # 偶數個數, closed quote
                # 奇數個數, open quote
                log_result = count_number % 2 == 0
                if log_result:
                    return True
    return log_result


def command2list(command: str, **kwargs) -> List:
    """
    Command 字串 --> List
        以空格隔開
    """
    root_logger = kwargs.get("root_logger", None)
    assert isinstance(command, str)

    command_list = command.split()
    # 確認是否被
    # print ("Before:", command_list)
    if bool(
        np.any(
            [
                find_quote(elem) and (not closed_quote(elem))
                for elem in command_list
            ]
        )
    ):
        # 表示內部存在 ' 或 "
        # 想辦法整併
        quote_index = []
        for i, elem in enumerate(command_list):
            if find_quote(elem):
                if not closed_quote(elem):
                    quote_index.append(i)
        # quote_index = [
        #    i
        #    for i, elem in enumerate(command_list)
        #    if find_quote(elem) and (not closed_quote(elem))
        # ]
        while len(quote_index) != 0:
            try:
                assert len(quote_index) % 2 == 0  # 偶數個數
            except AssertionError as e:
                if root_logger is not None:
                    root_logger.error(
                        "{} / {}".format(
                            len(quote_index), quote_index
                        )
                    )
                    for i, elem in enumerate(command_list):
                        root_logger.error(
                            "  --> {}. {} / {} / {}".format(
                                i,
                                elem,
                                find_quote(elem),
                                (not closed_quote(elem)),
                            )
                        )
                raise AssertionError(quote_index) from e
            for j in range(1, len(quote_index)):
                if (
                    command_list[quote_index[0]][0]
                    == command_list[quote_index[j]][-1]
                ):
                    # quote mark should be closed
                    merged_string = ""
                    for k in range(
                        quote_index[0], quote_index[j] + 1
                    ):
                        merged_string += "{} ".format(
                            command_list[k]
                        )
                    merged_string = merged_string[
                        :-1
                    ]  # 排除最後面的空行
                    command_list_temp = (
                        command_list[: quote_index[0]]
                        + [merged_string]
                        + command_list[quote_index[j] + 1 :]
                    )

                    command_list = command_list_temp
                    break
            quote_index = [
                i
                for i, elem in enumerate(command_list)
                if find_quote(elem) and (not closed_quote(elem))
            ]
    return command_list


def pipeline_split(command_list: List) -> Tuple:
    """
    command_list 可能是多個指令以 ; 或 | 串再一起
    ; 則當成多個獨立命令
    | 則需要 pipeline 一起輸入

    pstart_list & pend_list 則是用來定義每一個 ; 分隔的獨立命令起始
    """
    # 找尋 ';' 分開符號
    spilt_index = [
        i for i, x in enumerate(command_list) if x == ";"
    ]

    pstart_list = []
    pend_list = []

    pstart_list = [0]
    if len(spilt_index) == 0:
        # 無 ; 分隔
        pend_list = [len(command_list) - 1]
    else:
        for i in range(len(spilt_index)):
            pstart_list.append(spilt_index[i] + 1)
            pend_list.append(spilt_index[i] - 1)
        pend_list.append(len(command_list) - 1)

    try:
        assert len(pstart_list) == len(pend_list)
    except AssertionError:
        raise AssertionError(
            "{} / {}".format(pstart_list, pend_list)
        )
    return (pstart_list, pend_list)


def system_call_running(
    command: str, **kwargs
) -> Tuple[bool, Optional[List]]:
    """
    System Call
    執行 system call

    回傳 bool, list
    1. bool
        如成功執行, 回傳 True
        如有錯誤, 回傳 False
    2. 螢幕輸出內容
        以 \n 換行隔開

    1. 可以接受 pipeline 寫法
    2. 額外處理 docker 的處置, 如果 /bin/bash -c 後面包在 "" 中的指令
        視為無 docker
        docker 外部不可使用 pipeline
        docker /bin/bash -c 內部則可以使用 pipeline

    log_show_detail --> 用來控制是否螢幕輸出 command 的結果

    debug 無法處理末尾為 > /dev/null 的命令列
        --> 解決方法 刪除末尾命令列
    """
    root_logger = kwargs.get("root_logger", None)
    if root_logger is not None:
        root_logger.debug("{} / {}".format(command, kwargs))
        message = "  --> Caller's location: {}".format(
            get_frameinfo(trace_back_no=1)
        )
        root_logger.debug(message)

    def post_screen_export(p, **kwargs) -> List[str]:
        """
        螢幕輸出 command 的回饋資訊
        """
        log_show_detail = kwargs.get("log_show_detail", False)
        return_message = str(p.stdout)[2:-1].split("\\n")

        if log_show_detail:
            # 螢幕輸出, stdout 內容
            for l, line in enumerate(  # noqa: E741
                return_message
            ):  # noqa: E741
                print("  --> {}. {}".format(l, line))
        assert isinstance(return_message, list)
        return return_message

    success_returncode = [0, 88]

    def check_returncode(returncode: int, p, **kwargs) -> bool:
        """
        檢查 returncode 是否為錯誤
        """
        # 88: Socket operation on non-socket
        assert isinstance(returncode, int)
        root_logger = kwargs.get("root_logger", None)
        # ignore_returncode 也視為成功運算
        success_returncode2 = success_returncode + kwargs.get(
            "ignore_returncode", []
        )
        if root_logger is not None:
            root_logger.debug(
                "  return code {} / {}".format(
                    returncode,
                    success_returncode2,
                )
            )
            if "ignore_returncode" in kwargs:
                root_logger.debug(
                    "  --> ignore_returncode: {}".format(
                        kwargs.get("ignore_returncode")
                    )
                )

        if returncode not in success_returncode2:
            # 輸出 stderr 內容
            # 輸出 stderr
            message_error = ""
            if root_logger is not None:
                root_logger.error(
                    ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>"
                )
                for line in std_output(
                    str(p.stderr), linesize=-1
                ):
                    root_logger.error("  ==> {}".format(line))
                    message_error += "{}\n".format(line)
            raise SystemCallFail(message_error)
        return True

    def error_condition_check(p, command: str, **kwargs) -> bool:
        """
        確認是否錯誤, 如有錯誤, 輸出錯誤
        """
        root_logger = kwargs.get("root_logger", None)

        if root_logger is not None:
            root_logger.debug("# Command: {}".format(command))
            # 輸出 stdout 內容
            # 輸出 stdout
            for line in std_output(str(p.stdout), linesize=15):
                root_logger.debug("  ==> {}".format(line))
        return check_returncode(p.returncode, p, **kwargs)

    def remove_quotation_mark(command_list: List) -> List:
        """
        去除最後一個指令的 ' 或 " 等前後的 quotation mark
        """
        assert isinstance(command_list, list)
        for flag in ["'", '"']:
            if (command_list[-1][0] == flag) and (
                command_list[-1][-1] == flag
            ):
                command_list[-1] = command_list[-1][1:]
                command_list[-1] = command_list[-1][:-1]
        return command_list

    #######################################################################
    #######################################################################
    #######################################################################
    log_bg = kwargs.get("log_bg", False)
    root_logger = kwargs.get("root_logger", None)
    if kwargs.get("shell_mode", False):
        # Shell mode
        if log_bg:
            # Background mode
            # 會自動加入 nohup
            command = "nohup {} &".format(command)
        returncode = subprocess.call(
            command,
            shell=True,
            stdout=open(
                "stdout_message", "w"
            ),  # 錯誤的話，寫入 stdout_message
        )
        lines = []
        try:
            with open("stdout_message", "r") as f:
                lines = f.readlines()
            lines = [line.strip() for line in lines]
            fut.remove_file("stdout_message")

            if root_logger is not None:
                root_logger.debug(
                    "command: {}, output: {} / {}".format(
                        command, returncode, lines
                    )
                )
        except FileNotFoundError:
            # FileNotFoundError: [Errno 2] No such file or directory: 'stdout_message'
            # 表示沒有錯誤
            pass
        return returncode in success_returncode, lines
    else:
        # 以下為 非 shell mode 運算
        # 運算前資訊
        if root_logger is not None:
            root_logger.debug(create_line_bar(40, "<"))
            root_logger.debug(
                "System call command: {}".format(command)
            )

            message = "  --> Caller's location: {}".format(
                get_frameinfo(trace_back_no=1)
            )
            root_logger.debug(message)

        pipe_command2 = command2clist(command, **kwargs)
        if root_logger is not None:
            # 輸出命令列
            root_logger.debug(
                "  --> args: {}".format(pipe_command2)
            )

        # 建立 pipeline 起始與結束
        (pstart_list, pend_list) = pipeline_split(pipe_command2)

        # 執行命令列
        presult = False
        return_message: List[str] = []
        kwargs2 = {}
        func_bg = subprocess.run
        if log_bg:
            # Background mode
            kwargs = {"close_fds": True}
            func_bg = subprocess.Popen
        else:
            # not BG mode
            kwargs2 = {"capture_output": True}

        if root_logger is not None:
            root_logger.debug(
                "'subprocess.{}' with kwargs: {}".format(
                    func_bg.__name__, kwargs2
                )
            )
        # kwargs["shell"] = True
        for pindex in range(len(pstart_list)):
            pipe_command2_sub = pipe_command2[
                pstart_list[pindex] : pend_list[pindex] + 1
            ]
            if len(pipe_command2_sub) == 1:
                # 單一指令
                if root_logger is not None:
                    root_logger.debug(
                        "{}. {}".format(0, pipe_command2_sub[0])
                    )

                # 去除 前後的 ' & "
                pipe_command2_sub[0] = remove_quotation_mark(
                    pipe_command2_sub[0]
                )
                try:
                    p = func_bg(pipe_command2_sub[0], **kwargs2)
                except TypeError as e:
                    message = (
                        "{} ({}) / {} : {} / {} / {}".format(
                            pipe_command2_sub[0],
                            pipe_command2,
                            pstart_list[pindex],
                            pend_list[pindex] + 1,
                            kwargs,
                            kwargs2,
                        )
                    )
                    raise TypeError(message) from e
                if isinstance(p, subprocess.CompletedProcess):
                    presult = error_condition_check(
                        p,
                        pipe_command2_sub[0],
                        **kwargs2,
                        **kwargs,
                    )
                    return_message = post_screen_export(
                        p, **kwargs2, **kwargs
                    )
                else:
                    return False, None
            else:
                # 複合指令
                p1 = None
                if root_logger is not None:
                    root_logger.debug(
                        "{}. {}".format(
                            0, pipe_command2[pstart_list[pindex]]
                        )
                    )
                pipe_command2[pstart_list[pindex]] = (
                    remove_quotation_mark(
                        pipe_command2[pstart_list[pindex]]
                    )
                )

                p1 = subprocess.Popen(
                    pipe_command2[pstart_list[pindex]],
                    stdout=subprocess.PIPE,
                )
                p_pre = p1
                for pindex_sub in range(
                    pstart_list[pindex] + 1,
                    pend_list[pindex] + 1,
                ):
                    if root_logger is not None:
                        root_logger.debug(
                            "{}. {}".format(
                                pindex,
                                pipe_command2[
                                    pstart_list[pindex]
                                    + 1 : pend_list[pindex]
                                    + 1
                                ],
                            )
                        )
                    # pipeline 傳遞
                    # 將 p_pre.stdout 傳入
                    if pipe_command2[pindex_sub] != "|":
                        pipe_command2[pindex_sub] = (
                            remove_quotation_mark(
                                pipe_command2[pindex_sub]
                            )
                        )
                        p_current = func_bg(
                            pipe_command2[pindex_sub],
                            stdin=p_pre.stdout,
                            **{
                                key: elem
                                for key, elem in kwargs.items()
                                if key
                                not in [
                                    "root_logger",
                                    "log_show_detail",
                                    "shell_mode",
                                    "log_bg",
                                ]
                            },
                        )
                        p_pre = p_current

                # 判斷是否正確結束
                # 若無寫入 root_logger
                if isinstance(
                    p_pre, subprocess.CompletedProcess
                ):
                    presult = error_condition_check(
                        p_pre,
                        pipe_command2,
                        **kwargs,
                    )
                    return_message = post_screen_export(
                        p_pre, **kwargs
                    )
                else:
                    if root_logger is not None:
                        root_logger.error(
                            "無法正確執行之指令: {}".format(
                                pipe_command2_sub
                            )
                        )
                    return p_pre, None

                # pipeline 分隔線
                if root_logger is not None:
                    if pindex != len(pipe_command2) - 1:
                        root_logger.debug(
                            create_line_bar(40, "_")
                        )

    if root_logger is not None:
        root_logger.debug(create_line_bar(40, ">"))
    return presult, return_message
