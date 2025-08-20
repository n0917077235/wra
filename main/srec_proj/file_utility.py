"""
for File Utility
"""

import pathlib
import os
import platform
from typing import List, Union, Dict, Optional
import re
import gc
import shutil
import datetime
import pandas as pd
import time_phrase
import jutility as jut


class Found(Exception):
    """
    專用於雙層迴圈的搜尋 & break
    """

    # pylint: disable=unnecessary-pass
    pass


# @jit
def reformulate_path(my_path):
    """
    由於Windows與Linux等其他作業系統，其檔案符號系統不一致
    解決不一致問題
    """
    my_path = my_path.replace("\\", "/")
    sepline = my_path.split("/")
    sepline = [
        elem
        for elem in my_path.split("/")
        if elem != ""  # 去除空 str
    ]
    return os.path.join(*tuple(sepline))


def find_empty_folder(dir_path: str, **kwargs) -> List:
    """
    找出目錄中的空目錄
    """
    root_logger = kwargs.get("root_logger", None)
    if root_logger is not None:
        root_logger.debug("目錄: {}".format(dir_path))

    file_result: List = []
    # pylint: disable=unused-variable
    for dirpath_sub, dirnames_sub, filenames in os.walk(
        dir_path
    ):
        if root_logger is not None:
            root_logger.debug(
                "-- {} / {} / {}".format(
                    dirpath_sub, dirnames_sub, filenames
                )
            )
        if len(dirnames_sub) == 0 and len(filenames) == 0:
            file_result.append(dirpath_sub)
            if root_logger is not None:
                root_logger.debug(
                    "   --> {}, is dir? {}".format(
                        dirpath_sub, os.path.isdir(dirpath_sub)
                    )
                )
    return file_result


def remove_file(fname: Union[str, List], **kwargs):
    """
    刪除檔案
        增加功能: 如偵測為目錄, 則刪除下方檔案與目錄本身
        增加功能: 如 fname 為 str, 則進行單次處理
                如為 list, 則進行多次處理
    """
    root_logger = kwargs.get("root_logger", None)
    try:
        assert isinstance(fname, (str, list))
    except AssertionError as e:
        message = "!!! Filename={} / type={}".format(
            fname, type(fname)
        )
        if root_logger is not None:
            root_logger.error(message, exc_info=True)
        raise TypeError(message) from e

    if isinstance(fname, list):
        for ff in fname:
            # 遞迴呼叫
            remove_file(ff)
    elif isinstance(fname, str):
        try:
            if os.path.isfile(fname):
                pathlib.Path(fname).unlink()
            else:
                # 如果為 folder
                remove_files_folder(fname)
        except FileNotFoundError as e:
            message = (
                "!!! Filename={} doest not exist!!!".format(
                    fname
                )
            )
            if root_logger is not None:
                root_logger.error(message, exc_info=True)
            raise FileNotFoundError(message) from e


def remove_files_folder(folder: str, **kwargs):
    """
    刪除目錄中的檔案, 刪除目錄
        folder 限定為 folder, 不可為 file
    """
    root_logger = kwargs.get("root_logger", None)
    try:
        assert isinstance(folder, str)
    except AssertionError as e:
        message = "!!! type of {} should be str, but {}".format(
            folder, type(folder)
        )
        if root_logger is not None:
            root_logger.error(message, exc_info=True)
        raise TypeError(message) from e

    try:
        assert os.path.exists(folder)
        assert os.path.isdir(folder)
    except AssertionError as e:
        if not os.path.exists(folder):
            message = "!!! {} does not exists!!!".format(folder)
            if root_logger is not None:
                root_logger.error(message, exc_info=True)
            raise FileNotFoundError(message) from e
        elif os.path.isfile(folder):
            message = "!!! {} is not a directory!!! Is a file? {}".format(
                folder, os.path.isfile(folder)
            )
            if root_logger is not None:
                root_logger.error(message, exc_info=True)
            raise NotADirectoryError(message) from e
    shutil.rmtree(folder)  # 刪除下方的目錄結構, 以及目錄本身


def close_remain_open_hdf5():
    """
    從 gc 找出 hdf5 相關的檔案
    close 檔案
    """
    for obj in gc.get_objects():  # Browse through ALL objects
        my_type = str(type(obj))
        if my_type.lower().find("hdf5") >= 0:
            # try:
            obj.close()
            # except:
            #    pass  # Was already closed


def remove_empty_folder(
    dir_path: str,
    log_recursive: bool = False,
    **kwargs,
):
    """
    刪除空目錄
    1. 如果非空目錄, 則不刪除
    2. if log_recursive:
        刪除底層的空子目錄
    """

    def remove_empty_core(dir_path, **kwargs) -> pd.DataFrame:
        """
        find_empty_folder 的延伸功能,
        建立 df, 並順道計算節點長度
        """
        empty_folder = find_empty_folder(
            dir_path, **kwargs
        )  # 找出所有的空目錄

        for folder in empty_folder:
            # 找出第一層的空目錄
            shutil.rmtree(folder)
        return empty_folder

    empty_folder = remove_empty_core(dir_path, **kwargs)
    if log_recursive:
        while len(empty_folder) > 0:
            # 遞迴刪除空目錄
            empty_folder = remove_empty_core(dir_path, **kwargs)
    # else:
    #    empty_folder = remove_empty_core(dir_path, **kwargs)


def filter_search_result(
    file_result: List,
    regular_flags: str,
    log_reverse: bool = False,
    # log_debug: bool = False,
    **kwargs,
) -> List:
    """
    從目錄清單, 進一步篩選
    20210902 08:20 AM, 加入功能 log_reverse
    代表反向選取, 保留不符合條件者
    """
    root_logger = kwargs.get("root_logger", None)
    log_debug = kwargs.get("log_debug", False)
    file_result2 = []
    # pylint: disable=broad-except
    try:
        assert isinstance(regular_flags, str)
        assert isinstance(file_result, list)
        assert isinstance(log_reverse, bool)

        if log_debug:
            if root_logger is not None:
                root_logger.debug("# func::filter_search_result")
                root_logger.debug(
                    "   -- 正規化表示法: {}; log_reverse: {}".format(
                        regular_flags, log_reverse
                    )
                )
        for fpath in file_result:
            search_result = re.compile(regular_flags).search(
                fpath
            )
            if not log_reverse:
                # 留下符合條件者
                if search_result:  # regular expression
                    if log_debug:
                        if root_logger is not None:
                            root_logger.debug(
                                "   --> Mtched result: {}".format(
                                    fpath
                                )
                            )
                    file_result2.append(fpath)
            else:
                # 排除符合條件者
                if not bool(search_result):  # regular expression
                    if log_debug:
                        if root_logger is not None:
                            root_logger.debug(
                                "   --> Mtched result: {}".format(
                                    fpath
                                )
                            )
                    file_result2.append(fpath)
    except AssertionError as e:
        message = "輸入錯誤型別 '{}' / '{}' / '{}'".format(
            file_result, regular_flags, log_reverse
        )
        if root_logger is not None:
            root_logger.error(message, exc_info=True)
        raise TypeError(message) from e
    return file_result2


def check_file_ismatch(
    path_whole: str,
    regular_flags,
) -> bool:
    """
    評估是否符合條件
    regular_flags 為正規化的條件
    """
    check_result = False
    if regular_flags is None:
        # 無設定正規化搜尋的參數
        check_result = True
    else:
        # 有設定正規化搜尋
        search_result = re.compile(regular_flags).search(
            path_whole
        )
        if bool(search_result):  # regular expression
            # 吻合
            check_result = True
    return check_result


# @jit
def find_all_files(dir_path: str, **kwargs) -> pd.DataFrame:
    """
    輸入目錄位址，找出目錄下的所有檔案結構
        log_onlyfile 則僅呈現
        回傳一 DataFrame, 包含三欄資訊，一欄為檔案名稱，一欄為是否為 parent folder，一欄為是否為 is folder

    find all fils from dir [dir_path]
    """
    log_onlyfile: bool = kwargs.get("log_onlyfile", True)
    try:
        assert isinstance(dir_path, str)
    except AssertionError as e:
        raise TypeError(
            "The type of dir_path should be str, not {}!".format(
                type(dir_path)
            )
        ) from e

    try:
        assert os.path.exists(dir_path)
    except AssertionError as e:
        raise FileNotFoundError(
            "{} does not exist!".format(dir_path)
        ) from e
    try:
        assert os.path.isdir(dir_path)
    except AssertionError as e:
        raise ValueError(
            "{} is not a directory!".format(dir_path)
        ) from e

    # 找出檔案中的路徑, 以及檔案
    mat = []
    # pylint: disable=unused-variable
    for dirpath_sub, dirnames_sub, filenames in os.walk(
        dir_path
    ):
        for filename in filenames:
            path_whole = os.path.join(dirpath_sub, filename)
            # 排除 directory, 只有檔案
            # 0. path
            # 1. is not chird folder, is parrent folder
            mat.append(
                [path_whole, dirpath_sub == dir_path, True]
            )

        if not log_onlyfile:
            # 加入目錄
            for dirname in dirnames_sub:
                mat.append(
                    [
                        os.path.join(dirpath_sub, dirname),
                        dirpath_sub == dir_path,
                        False,
                    ]
                )
    df = pd.DataFrame(
        mat, columns=["fname", "log_parrent_dir", "log_isfile"]
    )
    return df


# pylint: disable=broad-except
# pylint: disable=too-many-branches
# @jit # 不可加入
def search_files_in_dir(
    dir_path: str,
    regular_flags: Optional[str] = None,
    log_recursive: bool = False,
    **kwargs,
) -> List:
    """
    找出目錄中的檔案
    see https://codertw.com/%E7%A8%8B%E5%BC%8F%E8%AA%9E%E8%A8%80/362018/
    log_recursive = True, 遞迴找出更深入的檔案

    2022/03/09: 加入功能, dir_path 可以是 List 格式, 一次搜尋多個目錄

    """
    root_logger = kwargs.get("root_logger", None)
    log_debug = kwargs.get("log_debug", False)
    assert isinstance(regular_flags, str) or (
        regular_flags is None
    ), "{} / {}".format(regular_flags, type(regular_flags))
    assert isinstance(dir_path, (str, list))
    # pylint: disable=comparison-with-itself
    if log_debug:
        if root_logger is not None:
            root_logger.debug(
                " func::search_files_in_dir: {} | {}".format(
                    dir_path, regular_flags
                )
            )
    if isinstance(dir_path, list):
        # list 版本

        flist_all = []
        # 組合多個目錄進行搜尋
        for dp in dir_path:
            flist_all += search_files_in_dir(
                dp,
                regular_flags=regular_flags,
                log_recursive=log_recursive,
                log_debug=log_debug,
                **kwargs,
            )
        # 透過 set 排除重複資訊
        flist_all = list(set(flist_all))
        if log_debug:
            if root_logger is not None:
                export_result = flist_all[
                    : min(10, len(flist_all))
                ]
                root_logger.debug(
                    "Results: {}".format(export_result)
                )
        return flist_all
    elif isinstance(dir_path, str):
        # str 版本
        if dir_path == "":
            # 轉為 local folder
            dir_path = "."

        # find all files
        df_flist: pd.DataFrame = find_all_files(dir_path)
        if df_flist.shape[0] == 0:
            # 空目錄, 無檔案
            return []

        # is matched
        df_flist.loc[:, "log_matched"] = [
            check_file_ismatch(fname, regular_flags)
            for i, fname in enumerate(
                df_flist.loc[:, "fname"].values
            )
        ]
        df_flist2 = df_flist[
            df_flist["log_matched"]
        ]  # 挑出符合者

        if not log_recursive:
            # 剔除遞迴之子目錄
            try:
                df_flist2 = df_flist2[
                    df_flist2["log_parrent_dir"]
                ]
            except KeyError as e:
                message = "{} / {} / {}".format(
                    dir_path, "log_parrent_dir", df_flist2
                )
                if root_logger is not None:
                    root_logger.debug(message)
                raise KeyError(message) from e
        file_result = list(df_flist2.loc[:, "fname"].values)
        if log_debug:
            if root_logger is not None:
                export_result = file_result[
                    : min(10, len(file_result))
                ]
                root_logger.debug(
                    "Results: {}".format(export_result)
                )
        return file_result


def search_file_in_multi_dirs(
    data_path: Union[str, List], **kwargs
) -> List:
    """
    一次搜尋多個路徑的結果, 統一將搜尋結果以 List 整併輸出
    """
    data_paths = []
    if isinstance(data_path, str):
        data_paths = [data_path]
    elif isinstance(data_path, list):
        data_paths = data_path
    assert isinstance(data_paths, list)
    assert len(data_paths) > 0

    flist = []
    for dp in data_paths:
        if os.path.exists(dp):
            flist += search_files_in_dir(dp, **kwargs)
    return flist


def merge_multi_well_data(
    flist: List,
    flag: str = "Obs",
    log_nonmatched: bool = True,
) -> pd.DataFrame:
    """
    用來處理可能有一組, 多組或全無資料的檔案讀取
        a. 資料格式為時間序列
        b. 多組資料則增加欄位, 結合
    回傳 pd.DataFrame

    log_nonmatched: 為如果無符合條件的檔案, 要如何處理
    True, 則允許無符合條件的檔案, 回傳一個空的 pd.DataFrame
    False, 則回傳錯誤訊息
    """
    assert isinstance(flist, list)

    _df_data = pd.DataFrame([])
    if len(flist) == 1:
        # 讀取單一檔案
        _df_data = (
            pd.read_hdf(flist[0])
            .resample("1d")
            .mean()
            .interpolate()
        )
        _df_data = _df_data.rename(
            columns={_df_data.columns[0]: flag}
        )
    elif len(flist) > 1:
        # 多口觀測井, 廢井後重建
        for fname in flist:
            df = pd.read_hdf(fname)
            # 接起來
            _df_data = pd.concat(
                [
                    _df_data,
                    df.rename(columns={df.columns[0]: flag}),
                ]
            )

        # 內插補齊
        _df_data = _df_data.resample("1d").mean().interpolate()
    else:
        # 缺資料
        if not log_nonmatched:
            # 回傳錯誤訊息
            raise IndexError(
                "Matched file is None: {}".format(flist)
            )
    return _df_data


def path_leaf(path: str):
    """
    basename
    """
    # head, tail = ntpath.split(path)
    # return tail or ntpath.basename(head)
    return os.path.basename(path)


def creation_date(path_to_file: str) -> datetime.datetime:
    """
    取出檔案建立日期
    Try to get the date that a file was created, falling back to when it was
    last modified if that isn't possible.
    See http://stackoverflow.com/a/39501288/1709587 for explanation.
    """
    try:
        assert isinstance(path_to_file, str)
    except AssertionError as e:
        raise TypeError(
            "{} / {} / {}".format(
                path_to_file,
                type(path_to_file),
                os.path.exists(path_to_file),
            )
        ) from e

    try:
        assert os.path.exists(path_to_file)
    except AssertionError:
        raise FileNotFoundError(path_to_file)

    mtime: float = -99999.0
    if platform.system() == "Windows":
        mtime = os.path.getctime(path_to_file)
    else:
        mtime = os.stat(path_to_file).st_mtime
    dt_create = datetime.datetime.fromtimestamp(mtime)
    return dt_create


def file_filter_recent(
    fname: str, tm_flag: str, **kwargs
) -> bool:
    """
    檢查檔案建立時間, tm_flag為時間
    確認建立時間是否在 tm_flag 建立

    tm_flag: 1s, 5t, 3h, 4d, 1w
    如在時間內, 回傳 True
    反之, 舊檔案, 回傳 False
    """
    try:
        # 檢查檔案是否存在
        assert os.path.exists(fname)
    except AssertionError as e:
        raise FileNotFoundError(
            "'{}' doesn't exist!".format(fname)
        ) from e
    delta_time = time_phrase.timeflag2timedelta(tm_flag.lower())
    dt_birthtime = creation_date(fname)  # 辨別建置日期
    dt_now = datetime.datetime.now()

    log_check: bool = dt_now - delta_time < dt_birthtime
    root_logger = kwargs.get("root_logger", None)
    if root_logger is not None:
        root_logger.debug(
            "# Recent File Filter: {} / {}".format(
                fname, tm_flag
            )
        )
        root_logger.debug(
            "  --> 建置日期: {}".format(dt_birthtime)
        )
        root_logger.debug("  --> 目前日期: {}".format(dt_now))
        root_logger.debug(
            "  --> 時間間隔: {}".format(dt_now - dt_birthtime)
        )
        root_logger.debug(
            "  --> Before {}? {}".format(
                dt_now - delta_time, log_check
            )
        )
    return log_check


def search_recent_file(
    dir_path: str,
    tm_flag: str,  # 時間標記
    regular_flags: Optional[str] = None,
    log_recursive: bool = False,
    log_recent_reverse: bool = False,
    log_debug: bool = False,
    **kwargs,
) -> List:
    """
    提供目錄, 設定時間戳記, 找出時間內建立的檔案

    regular_flags 用來正規化篩選檔案
    log_recursive 是否遞迴找出下層目錄

    log_recent_reverse 是否反向輸出較舊的檔案
    """
    assert isinstance(regular_flags, str) or (
        regular_flags is None
    )
    # 找出目錄內的檔案
    flist = search_files_in_dir(
        dir_path,
        regular_flags=regular_flags,
        log_recursive=log_recursive,
        log_debug=log_debug,
        **kwargs,
    )

    df_flist = pd.DataFrame(flist, columns=["fname"])
    df_flist.loc[:, "log_recent"] = [
        file_filter_recent(
            fname, tm_flag, log_debug=log_debug
        )  # 以建檔時間來挑選
        for fname in df_flist.loc[:, "fname"].values
    ]

    df_flist2 = df_flist[df_flist["log_recent"]]
    if log_recent_reverse:
        # reverse, 選舊的
        df_flist2 = df_flist[~df_flist["log_recent"]]

    if df_flist2.shape[0] == 0:
        # 如果無檔案, 則回傳空 List
        return []
    return list(df_flist2.loc[:, "fname"].values)


def blocks(files, size=65536):
    while True:
        b = files.read(size)
        if not b:
            break
        yield b


def count_filenum(fname: str, **kwargs) -> int:
    """
    計算檔案行數
    """
    assert isinstance(fname, str)
    assert os.path.exists(fname)
    assert os.path.isfile(fname)
    root_logger = kwargs.get("root_logger", None)
    if root_logger is not None:
        root_logger.debug(
            "Counting number of line in a text file: {}".format(
                fname
            )
        )

    lines_count: int = 0

    read_size = kwargs.get("read_size", 1024 * 1024)
    with open(
        fname,
        "r",
        errors="ignore",
    ) as f:
        while True:
            chunk = f.read(read_size)
            if not chunk:
                break
            lines_count += chunk.count("\n")

    if root_logger is not None:
        root_logger.debug(
            "Counted number of lines: {}".format(lines_count)
        )
    return lines_count


def trans_liststr2str(content: List[str]) -> str:
    """
    將 List of str --> str
    以換行符號串接
    """
    merged_str = ""
    for elem in content:
        assert isinstance(elem, str)
        merged_str += "{}\n".format(elem)
    return merged_str


def check_hdd_status(path: str) -> Dict:
    """
    輸入目錄路徑
    回傳硬碟狀態

    單位: GB
    """
    assert isinstance(path, str)
    assert os.path.exists(path)
    assert os.path.isdir(path)

    total, used, free = shutil.disk_usage(path)
    result = {
        "total": total // (2**30),  # 單位 GB
        "used": used // (2**30),
        "free": free // (2**30),
    }
    return result
