import sys
import traceback


def except_stdout(e, log_show: bool = True):
    error_class = e.__class__.__name__  # 取得錯誤類型
    errMsg: str = ""
    if len(e.args) >= 1:
        # print (e.args)
        detail = e.args[0]  # 取得詳細內容
        # pylint: disable=unused-variable
        cl, exc, tb = sys.exc_info()  # 取得Call Stack
        lastCallStack = traceback.extract_tb(tb)[
            -1
        ]  # 取得Call Stack的最後一筆資料
        fileName = lastCallStack[0]  # 取得發生的檔案名稱
        lineNum = lastCallStack[1]  # 取得發生的行號
        funcName = lastCallStack[2]  # 取得發生的函數名稱
        errMsg = 'File "{}", line {}, in {}: [{}] {}'.format(
            fileName, lineNum, funcName, error_class, detail
        )
        if log_show:
            print(errMsg)
    else:
        # pylint: disable=unused-variable
        cl, exc, tb = sys.exc_info()  # 取得Call Stack
        lastCallStack = traceback.extract_tb(tb)[
            -1
        ]  # 取得Call Stack的最後一筆資料
        fileName = lastCallStack[0]  # 取得發生的檔案名稱
        lineNum = lastCallStack[1]  # 取得發生的行號
        funcName = lastCallStack[2]  # 取得發生的函數名稱
        errMsg = 'File "{}", line {}, in {}: [{}]'.format(
            fileName, lineNum, funcName, error_class
        )
        if log_show:
            print(errMsg)
    return errMsg
