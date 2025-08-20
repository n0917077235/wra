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
import jutility as jut


class Found(Exception):
    """
    專用於雙層迴圈的搜尋 & break
    """

    # pylint: disable=unnecessary-pass
    pass


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

