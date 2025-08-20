# -*- coding: utf-8 -*-
import tempfile
import os
import sys
from typing import Union, Optional

try:
    import magic
except ModuleNotFoundError:
    from bs4 import UnicodeDammit

# Jacky's Libary
import file_utility as fut
import jutility as jut

# 用於處理文字檔編碼問題


def check_file_encode(
    fname: str,
    *argv,
    read_size: int = 512,  # 無需全檔案都讀取
    **kwargs,  # root_logger
) -> str:
    """
    # 確認檔案編碼

    備註: big5 會輸出成 iso-8859-1
    """
    root_logger = kwargs.get("root_logger", None)
    if len(argv) > 0:
        # 舊款算法
        message = "Wrong encode_utility::check_file_encode() usage at {} ({}:{})".format(
            sys._getframe(1).f_code.co_name,
            sys._getframe(1).f_code.co_filename,
            sys._getframe(1).f_lineno,
        )
        if root_logger is not None:
            root_logger.error(message)

    # 改版為 magic
    encoding: str = ""
    with open(fname, "rb") as f:
        blob = f.read(read_size)
        try:
            m = magic.open(magic.MAGIC_MIME_ENCODING)
            m.load()
            encoding = m.buffer(blob)  # "utf-8" "us-ascii" etc
        except NameError:
            # 因為 ARM 無法使用 magic, 故改用 bs4
            suggestion = UnicodeDammit(blob)
            encoding = suggestion.original_encoding

    # debug, ptcp154 強制改為 utf-8
    if encoding == "ptcp154":
        encoding = "utf-8"
    if root_logger is not None:
        root_logger.debug("File encoding: {}".format(encoding))
    return encoding


def check_content_encode(content) -> str:
    """
    確認內容編碼

    1. 將 content 寫入 tempfile
    2. 確認該 tempfile 的 encoding
    """
    content2 = content
    if isinstance(content, list):
        # 將 list of str ->串接為 str
        content2 = fut.trans_liststr2str(content)
    with tempfile.NamedTemporaryFile("w+t") as fp:
        with open(fp.name, "w") as fw:
            try:
                fw.write(content2)
            except TypeError as e:
                raise TypeError(
                    "{} / {} / {}".format(
                        content2, type(content2), len(content2)
                    )
                ) from e
        return check_file_encode(fp.name)


class ENCODE_utility:
    """
    專門用於 file encode 的操作與查詢
    """

    # @jit
    def __init__(self, encode_fname: str, root_logger=None):
        self.encode_fname = encode_fname
        self.root_logger = root_logger

    # Check the encode of file
    # pylint: disable=no-else-return
    # @jit, 不可加入
    def check_file_encode(self, **kwargs) -> Union[str, None]:
        """Predict a file's encoding using chardet"""
        # Open the file as binary data
        return check_file_encode(self.encode_fname, **kwargs)

    # pylint: disable=too-many-branches
    # pylint: disable=broad-except
    # @jit, 不可加入
    def iconv2utf8(
        self,
        target_fname: Optional[str] = None,
        **kwargs,
    ):
        message = "    |- Encoding (ENCODE_utility.iconv2utf8): {} --> {}".format(
            self.encode_fname, target_fname
        )
        # root_logger = kwargs.get("root_logger", None)
        if self.root_logger is not None:
            self.root_logger.debug(message)

        ############################################################################
        jut.check_outdated_IO(
            "log_debug",  # check_item
            "請改用 root_logger",  # message
            root_logger=self.root_logger,
            **{
                key: elem
                for key, elem in kwargs.items()
                if key != "root_logger"
            },
        )
        ############################################################################

        if target_fname is not None:
            if os.path.isdir(target_fname):  # 如果為目錄
                # 若為目錄, 以原有檔名命名, 等同重編碼後搬移目錄
                fname = fut.path_leaf(self.encode_fname)
                target_fname = os.path.join(
                    target_fname, fname
                )  # 目錄 + fname
        else:
            # is None
            target_fname = self.encode_fname

        encode_type = self.check_file_encode()
        message = "       --> File encode (original): {}".format(
            encode_type
        )
        if self.root_logger is not None:
            self.root_logger.debug(message)

        command: str = ""
        if encode_type not in ["utf-8", "UTF-8-SIG"]:
            #  UTF-8 & UTF-8-SIG 以外的格式, 都要進行轉換
            command = "iconv -f {} -t utf-8 {}".format(
                encode_type,
                self.encode_fname,
            )

            with tempfile.NamedTemporaryFile() as tf:
                command += " > " + tf.name
                command += " ; cp -rf {} {}".format(
                    tf.name, target_fname
                )
                os.system(command)

            message1 = "       --> Encode process: \n"
            message1 += "           |- Command: {}".format(
                command
            )

            if self.root_logger is not None:
                self.root_logger.debug(message1)
        else:
            # 如為 UTF-8 等格式, 則不需要轉換格式
            # 但 target_fname 非 None 且 target_fname != self.encode_fname
            # 則搬移改名
            if target_fname is not None:
                if target_fname != self.encode_fname:
                    command = "cp -rf {} {} ".format(
                        self.encode_fname,
                        target_fname,
                    )

                    message1 = "       --> Rename process: \n"
                    message1 += (
                        "           |- Command: {}".format(
                            command
                        )
                    )

                    os.system(command)
                    if self.root_logger is not None:
                        self.root_logger.debug(message1)
