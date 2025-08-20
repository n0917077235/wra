"""
IIOTC 標準化 logging 
"""

import os
import logging
import logging.config
import re
import math
from typing import Dict, Union

import file_utility as fut
import jutility as jut

# 輸出格式
BASE_FORMAT = "[%(name)s][%(levelname)-6s] %(message)s"
FILE_FORMAT = "[%(asctime)s]" + BASE_FORMAT
encoding = "utf-8"


class stringFilter(logging.Filter):
    """
    只保留含有 .py:: 字串的 message
    """

    def filter(self, record):
        if re.compile(".py::").search(record.msg):
            return True
        return False


class stringFilter_DEBUG_begin(logging.Filter):
    """
    只保留含有 .py:: 字串的 message
    """

    def filter(self, record):
        if re.compile("DEBUG:").search(record.msg):
            return False
        return True


def logger_init():
    """
    初始化 logger
    """
    logging.shutdown()
    # pass


def logger_setup(
    program_name: str,
    filename: str = "debug",
    log_append: bool = True,
    log_with_time: bool = True,
    log_init: bool = False,
) -> logging.Logger:
    """
    logging 參數設定
    program_name 為程序名稱
    logging_path: 輸出路徑

    產生
        1. filename + ".log" 包含所有層級資訊
        2. filename + "-error.log" 則只輸出 error 與 critical 兩個層級

    教學文件
    https://titangene.github.io/article/python-logging.html
    """
    if log_init:
        logger_init()  # 初始化
    logging.basicConfig(level=logging.INFO)
    open_type = "a"
    if not log_append:
        open_type = "w"  # delete old & create a new one
    try:
        os.makedirs(os.path.dirname(filename), exist_ok=True)
    except FileNotFoundError:
        pass

    format_str = ""
    if log_with_time:
        format_str += "%(asctime)s "
    format_str += "[%(levelname)s] [%(filename)s:%(funcName)s:%(lineno)d]: %(message)s"
    formatter: logging.Formatter = logging.Formatter(
        format_str,
        datefmt="%Y-%m-%dT%H:%M:%S",
    )

    #####################################################
    # logging.captureWarnings(False)
    root_logger = logging.getLogger(program_name)
    root_logger.setLevel(logging.DEBUG)

    # 檔案輸出
    fh = logging.FileHandler(
        "{}.log".format(filename), open_type, encoding
    )
    fh.setLevel(logging.DEBUG)
    fh.setFormatter(formatter)
    root_logger.addHandler(fh)

    # 螢幕輸出
    # ch = logging.StreamHandler(sys.stdout)
    ch = logging.StreamHandler()
    ch.setLevel(logging.INFO)
    ch.setFormatter(formatter)
    root_logger.addHandler(ch)

    # 檔案輸出 WARNING
    # 強置 append
    # 用來追蹤錯誤碼
    fh3 = logging.FileHandler(
        "{}-warning.log".format(filename), "w", encoding
    )
    fh3.setLevel(logging.WARNING)
    fh3.setFormatter(formatter)
    root_logger.addHandler(fh3)

    # 檔案輸出 ERROR
    # 強置 append
    # 用來追蹤錯誤碼
    fh2 = logging.FileHandler(
        "{}-error.log".format(filename), "w", encoding
    )
    fh2.setLevel(logging.ERROR)
    fh2.setFormatter(formatter)
    root_logger.addHandler(fh2)
    return root_logger


def logger_close(root_logger: logging.Logger):
    """
    關閉 logger
    """
    for handler in root_logger.handlers:
        if isinstance(handler, logging.FileHandler):
            handler.close()


def check_logger_status(
    logger: logging.Logger, **kwargs
) -> Dict:
    """
    檢查 logger 狀態
    """
    root_logger = kwargs.get("root_logger", None)
    logger_status: Dict = {}
    for handler in logger.handlers:
        logger_status[handler.level] = {}
        logger_status[handler.level][
            "formatter"
        ] = handler.formatter
        if isinstance(handler, logging.FileHandler):
            logger_status[handler.level]["name"] = handler.name
            # bytes
            file_size = os.path.getsize(handler.baseFilename)
            logger_status[handler.level][
                "filename"
            ] = handler.baseFilename
            logger_status[handler.level]["filesize"] = (
                file_size / math.pow(1024, 2)
            )
            if root_logger is not None:
                root_logger.debug(
                    "{}: {} / {} MB".format(
                        handler.level,
                        handler.baseFilename,
                        round(
                            logger_status[handler.level][
                                "filesize"
                            ],
                            0,
                        ),
                    )
                )
    return logger_status

