"""
專門處理時間字串的解析
"""

# -*- coding: utf-8 -*-
import numpy as np
import datetime
import math
import pytz
import collections
import pandas as pd
from typing import Union, List, Tuple
import re
from dateutil.tz import tzutc

time_flag = [
    "%d-%m-%y %H.%M.%S.%f %p",
    "%Y-%m-%d %H:%M:%S.%f",  # '2021-10-1 10:00:00.00'
    "%Y-%m-%d %H:%M:%S",
    "%Y-%m-%dT%H:%M:%SZ",
    "%Y-%m-%dT%H:%M:%S",
    "%Y-%m-%dT%H.%M.%S",
    "%Y/%m/%d %H:%M:%S",
    "%Y/%m/%dT%H:%M:%S",
    "%d/%m/%Y %H:%M:%S",
    "%Y-%m-%d",
    "%Y/%m/%d",
    "%Y%m%d%H%M%S",
    "%Y%m%d%H",
    "%Y%m%d",
    "%Y%m",
    "%Y",
]
time_flag2 = {
    # "priority": ["%Y%m%d%H"],
    8: [
        "%Y%m%d",
        "%Y-%m-%d",
        "%Y/%m/%d",
    ],
    9: ["%Y-%m-%d"],
    14: ["%Y%m%d%H%M%S"],
    10: [
        "%Y%m%d%H",
        "%Y-%m-%d",
        "%Y/%m/%d",
    ],
    6: ["%Y%m"],
    4: ["%Y"],
    16: ["%Y-%m-%d %H:%M"],
    19: [
        "%Y-%m-%dT%H:%M:%S",
        "%Y-%m-%dT%H.%M.%S",
        "%Y/%m/%dT%H:%M:%S",
        "%Y-%m-%d %H:%M:%S",
        "%Y/%m/%d %H:%M:%S",
        "%d/%m/%Y %H:%M:%S",
    ],
    20: ["%Y-%m-%dT%H:%M:%SZ"],
    22: ["%Y-%m-%d %H:%M:%S.%f"],
    "others": [
        "%d-%m-%y %I.%M.%S.%f %p",  # %I(12小時制)
        "%Y-%m-%d %I:%M:%S.%f %p",
        "%Y/%m/%d %I:%M:%S.%f %p",
        "%d-%m-%y %I.%M.%S %p",
        "%Y-%m-%d %I:%M:%S %p",
        "%Y/%m/%d %I:%M:%S %p",
        "%Y-%m-%dT%H:%M:%S.%fZ",
    ],
}

"%Y-%m-%dT%H:%M:%S.%fZ"

tkey = ["others", 22, 20, 19, 16, 14, 10, 8, 6, 4]


class DFINDEX_STRING(Exception):
    """
    DataFrame 之 index 為 str
    """

    pass


class NotMatched(Exception):
    """
    如果完全沒有吻合
    """

    pass


def time_phrase_multi_core(
    time_str: str, time_flag3: List, **kwargs
):
    """
    查詢合適的 tflag 以解析時間
    """
    assert isinstance(time_str, str)
    assert isinstance(time_flag3, list)

    log_debug = kwargs.get("log_debug", False)
    root_logger = kwargs.get("root_logger", None)
    for tflag in time_flag3:
        try:
            # try tflag
            if log_debug:
                message = "Testing '{}' to phrase {}".format(
                    tflag,
                    time_str,
                )
                if root_logger is not None:
                    root_logger.debug(message)

            dt = datetime.datetime.strptime(time_str, tflag)
            if log_debug:
                message = "Phrasing {} with tflag '{}'".format(
                    time_str, tflag
                )
                if root_logger is not None:
                    root_logger.debug(message)

            return dt
        except ValueError:
            pass
    # 沒有吻合的 tflag
    raise NotMatched


def time_phrase_multi(  # noqa: C901
    time_str: str,
    **kwargs,
) -> datetime.datetime:
    """
    以多種 tflag 解析時間字串
    """
    try:
        assert isinstance(
            time_str, (str, np.datetime64, datetime.datetime)
        )
    except AssertionError as e:
        raise TypeError(
            "{} / {}".format(time_str, type(time_str))
        ) from e

    if isinstance(time_str, (np.datetime64, datetime.datetime)):
        # 無須處理 datetime 型別
        return time_str
    elif isinstance(time_str, str):
        try:
            # 如果符合 Now, Today, Tomorrow 等字樣
            return timestr2dt(time_str)
        except ValueError:
            # 不符合者, 以 tflag 來解析
            if (
                time_str.find("+") >= 0
            ):  # 用來處理 +0800的部份 time shift
                sepline = time_str.split("+")
                # 解析無 local tz 資訊的 dt
                dt = time_phrase_multi(sepline[0], **kwargs)
                if sepline[1] in ["08:00", "0800"]:
                    return pytz.timezone("Asia/Taipei").localize(
                        dt
                    )  # 加上 tz
            # 循序由長至短試誤處理時間格式
            for tk in tkey:  # 由長至短進行試誤處理
                try:
                    if isinstance(tk, int):
                        if (len(time_str) == tk) or (
                            len(time_str) == tk - 1
                        ):  # 比對長度
                            return time_phrase_multi_core(
                                time_str,
                                time_flag2[tk],
                                **kwargs,
                            )
                    elif tk in ["others", "priority"]:
                        if tk == "priority":
                            try:
                                assert len(time_str) == 10
                            except AssertionError:
                                raise NotMatched

                        # 不比對長度
                        return time_phrase_multi_core(
                            time_str, time_flag2[tk], **kwargs
                        )
                except NotMatched:
                    # 沒有吻合, pass, 選擇下一個 tk
                    pass

            ####################################################################
            message = "No available tflag could be phrased '{}' (add more tflags) / length={}".format(
                time_str, len(time_str)
            )
            root_logger = kwargs.get("root_logger", None)
            if root_logger is not None:
                root_logger.error(message)
            raise IndexError(message)


def check_dt_between(
    dt: datetime.datetime, date_range: Tuple[datetime.datetime]
) -> bool:
    """
    檢查 dt 是否在 date_range 內
    """
    assert isinstance(date_range, tuple)
    assert len(date_range) == 2
    for _ in range(len(date_range)):
        assert isinstance(date_range[_], datetime.datetime)
    return (dt >= date_range[0]) and (dt < date_range[1])


def yyyymmddhh_process(time_flag: str) -> datetime.datetime:
    """
    1998010124, 採用 1-24 小時制度
    需要另行處理
    """
    assert isinstance(time_flag, str), "'{}' / {}".format(
        time_flag, type(time_flag)
    )
    assert len(time_flag) == 10, "'{}' / {}".format(
        time_flag, len(time_flag)
    )
    return datetime.datetime(
        int(time_flag[:4]),  # yy
        int(time_flag[4:6]),  # mm
        int(time_flag[6:8]),  # dd
        int(time_flag[8:]) - 1,  # hh
        0,
        0,
        0,
    )


def time_phrase_multi_export(
    time_str: str, tflag_export: str = "%Y-%m-%dT%H:%M:%SZ"
) -> str:
    """
    以多種 tflag 解析時間字串, 並以不同的結果輸出
    """
    return time_phrase_multi(time_str).strftime(tflag_export)


def phrase_dt_with124(string1: str) -> datetime.datetime:
    """
    時間解析, %Y%m%d%H, 10 碼數字
    但小時為: 1-24, 但 datetime 設定為 0-23
    當小時為 24 時, 改設定為 23 時, 再增加一小時, 讓日期自動進一位
    """
    try:
        dt = datetime.datetime.strptime(string1, "%Y%m%d%H")
        return dt
    except ValueError:
        # datetime 接受之小時資訊為 0 ~ 23, 但如果為 1 ~ 24 制度者，
        # 改以下列方式
        if string1[-2:] == "24":
            dt = datetime.datetime.strptime(
                string1[:-1] + "3", "%Y%m%d%H"
            ) + datetime.timedelta(seconds=3600)
        else:
            raise ValueError(
                "!!! Not 1-24 time system: {}".format(string1)
            )
        return dt


def phrase_timedelta(tdelta: datetime.timedelta) -> str:
    """
    輸入 timedelta, 回傳 6t, 3d 等字串
    """
    assert isinstance(tdelta, datetime.timedelta)
    seconds = tdelta.seconds
    days = tdelta.days

    tflag = ""
    if days > 0:
        tflag += "{}d".format(int(days))
    op = ""
    if seconds > 0:
        hour = int(math.floor(seconds / 3600))
        minute = int(math.floor((seconds - hour * 3600) / 60))
        second = seconds - hour * 3600 - minute * 60

        if len(tflag) > 0:
            op = "+"
        if hour > 0:
            tflag += "{}{}h".format(op, hour)

        if len(tflag) > 0:
            op = "+"
        if minute > 0:
            tflag += "{}{}t".format(op, minute)

        if len(tflag) > 0:
            op = "+"
        if second > 0:
            tflag += "{}{}s".format(op, second)
    return tflag


def calc_data_freq(df: Union[pd.DataFrame, pd.Series]) -> str:
    """
    輸入 pd.DataFrame or pd.Series, 依據 index
    評估其採樣頻率

    1. 針對 index 排序
    2. 找出出現機率最大之 timedelta
    3. 將 timedelta 轉換為字串形式之 tflag
    """
    try:
        assert isinstance(df, (pd.DataFrame, pd.Series))
        assert df.shape[0] > 1
    except AssertionError as e:
        raise TypeError(
            "!!! Wrong type: {} / {}".format(type(df), df)
        ) from e

    # 找出數據的 freq
    df = df.sort_index()
    data_freq_list = [
        phrase_timedelta(df.index[t + 1] - df.index[t])
        for t in range(
            min(df.shape[0] - 1, 100)
        )  # 如數據較少者, 則直接以數據數量 - 1
    ]

    # 計算數量
    counter = collections.Counter(data_freq_list)
    df_count = pd.DataFrame.from_dict(
        counter, orient="index", columns=["count"]
    ).sort_values(by=["count"], ascending=False)
    try:
        data_freq = df_count.index[0]
    except IndexError as e:
        raise IndexError(
            "!!! {} / {}".format(df.head(), df_count.head())
        ) from e
    return data_freq


def timeflag_timedelta_core(
    timeflag_simplex: str,
) -> datetime.timedelta:
    """
    只處理簡單格式 [整數 + 時間標記]
    """
    check_list = ["hz", "m", "w", "d", "h", "t", "s", "y"]
    timeflag_simplex_sep: List = [[-999, ""]]
    for elem in check_list:
        if re.compile(elem).search(timeflag_simplex.lower()):
            try:
                timeflag_simplex_sep.append(
                    [
                        float(
                            timeflag_simplex.lower().replace(
                                elem, ""
                            )
                        ),
                        elem,
                    ]
                )
            except ValueError:
                pass
    try:
        timeflag_simplex_sep = timeflag_simplex_sep[1]
    except IndexError as e:
        raise IndexError(
            "{} / {}".format(
                timeflag_simplex, timeflag_simplex_sep
            )
        ) from e

    td_result = datetime.timedelta(seconds=0)
    timeflag_simplex_sep[1] = timeflag_simplex_sep[1].lower()

    tphrase_params = {
        "y": {"days": 365 * timeflag_simplex_sep[0]},
        "m": {"days": 30 * timeflag_simplex_sep[0]},
        "w": {"days": 7 * timeflag_simplex_sep[0]},
        "d": {"days": timeflag_simplex_sep[0]},
        "h": {"hours": timeflag_simplex_sep[0]},
        "t": {"minutes": timeflag_simplex_sep[0]},
        "s": {"seconds": timeflag_simplex_sep[0]},
    }

    if timeflag_simplex_sep[1] == "hz":
        step_length = 1.0 / timeflag_simplex_sep[0]
        if step_length >= 0.001:
            td_result = datetime.timedelta(
                milliseconds=int(step_length * 1000)
            )
        elif step_length >= 1e-6:
            td_result = datetime.timedelta(
                microseconds=int(step_length * 1e6)
            )
    else:
        # 以 dict 來取代 if-elif的寫法
        assert timeflag_simplex_sep[1] in tphrase_params.keys()
        td_result = datetime.timedelta(
            **tphrase_params[timeflag_simplex_sep[1]]
        )
    return td_result


def timestr2dt(timestr: str) -> datetime.datetime:
    """
    NOW 為現在時間
    NOW - 3h, 表示以現在時間回溯 3 小時
    NOW + 3h, 表示以現在時間向後 3 小時
    TODAY, 今天的 00:00:00 AM
    YESTERDAY, 昨天的 00:00:00 AM
    TOMORROW, 明天的 00:00:00 AM

    以 +/- 串接
    例如
        TODAY +/- 3h
        TODAY +/- 3h +/- 30t
        前者為 datetime, 第二欄為 timedelta
    """

    # sepline = timestr.split(" ")
    # sepline = [elem for elem in sepline if elem != ""]
    # 以 + or - 作為分隔符號, 但保留 + or - 符號
    def split_func(sepline: List, sep: str) -> List:
        """
        sepline 為 List of str
        sep 為分隔符號,
        例如
            sepline = ["NOW+2d-4h], sep = "+"
            --> ["NOW", "+", "2d-4h"]
            如果以此運作, sep 改為 "-"
            --> ["NOW", "+", "2d", "-", "4h"]
        """
        sepline = [elem.replace(" ", "") for elem in sepline]
        sepline_result = [elem for elem in sepline]
        for i, elem in enumerate(sepline):
            elem = sepline[i]
            # 切割
            sepline2 = elem.split(sep)
            if len(sepline2) > 1:
                # 先去除該位置的內容
                sepline_result.pop(i)

                # 反向塞入結果
                for j in range(len(sepline2)):
                    k = len(sepline2) - j - 1
                    sepline_result.insert(i, sepline2[k])
                    if k > 0:
                        sepline_result.insert(i, sep)
        return sepline_result

    sepline = [timestr]
    sepline = split_func(sepline, "+")  # +
    sepline = split_func(sepline, "-")  # -

    terms = [sepline[i] for i in range(2, len(sepline), 2)]
    op_list = [sepline[i] for i in range(1, len(sepline), 2)]
    base_term = sepline[0]

    try:
        assert len(terms) == len(op_list)
        assert base_term.upper() in [
            "NOW",
            "TODAY",
            "YESTERDAY",
            "TOMORROW",
        ]
    except AssertionError as e:
        raise ValueError(
            "{} --> {}".format(timestr, base_term)
        ) from e

    dt_now: datetime.datetime = datetime.datetime.now()
    dt_today: datetime.datetime = datetime.datetime(
        dt_now.year, dt_now.month, dt_now.day, 0, 0, 0, 0
    )
    dt_result: datetime.datetime = dt_now
    if base_term.upper() == "TODAY":
        dt_result = dt_today
    elif base_term.upper() == "NONE":
        dt_result = dt_now
    elif base_term.upper() == "YESTERDAY":
        dt_result = dt_today - datetime.timedelta(days=1)
    elif base_term.upper() == "TOMORROW":
        dt_result = dt_today + datetime.timedelta(days=1)
    for i in range(len(terms)):
        # timeflag2timedelta 換算出時間差異
        dt_result += timeflag2timedelta(
            "{}{}".format(
                op_list[i],
                terms[i],
            )
        )
    return dt_result


def timeflag2timedelta(
    timeflag_complex: str,
) -> datetime.timedelta:
    """
    從 time flag 來產生 timedelta
    例如 5d 為 5天, 1t 為 1分鐘, 5h 為 5小時
    年 Y
    月 m
    週 w
    天 d
    小時 h
    分鐘 t
    秒 s
    複合處理 5d+3h 代表 5天3小時
    """
    assert isinstance(timeflag_complex, str)

    # 將複雜時間標記串, 拆解成為 list
    # sepline = timeflag_complex.split("+")
    if timeflag_complex[0] not in ["+", "-"]:
        timeflag_complex = "+" + timeflag_complex
    try:
        sepline = re.split(r"[+-]", timeflag_complex)
    except re.error:
        sepline = [timeflag_complex]
    op_index = [
        m.start() for m in re.finditer("[+-]", timeflag_complex)
    ]
    if len(sepline) > len(op_index):
        # 如果兩者數量不符合
        # +1d --> ["", "1d"], 刪除第一個數據
        sepline = sepline[1:]

    # 累加複雜時間標記串的時間差
    def determine_op(tstr: str, op_index: int) -> int:
        """
        判斷正負號運算
        """
        assert isinstance(tstr, str)
        assert isinstance(op_index, int)
        assert op_index < len(tstr)
        assert tstr[op_index] in ["+", "-"]

        op = 1
        if tstr[op_index] == "-":
            op = -1
        return op

    index = 0
    td_result = timeflag_timedelta_core(
        sepline[index]
    ) * determine_op(timeflag_complex, op_index[index])
    for index, elem in enumerate(sepline[1:]):
        index2 = index + 1
        td_result += timeflag_timedelta_core(
            elem
        ) * determine_op(timeflag_complex, op_index[index2])
    return td_result


def check_data_is_navie(
    data: Union[pd.DataFrame, datetime.datetime, List]
) -> bool:
    """
    確認傳入的 data (pd.DataFrame or datetime.datetime) 是否為 navie
    """
    try:
        assert isinstance(
            data,
            (pd.DataFrame, pd.Series, datetime.datetime, list),
        )
    except AssertionError as e:
        raise AssertionError(
            "!!! type: {}".format(type(data))
        ) from e

    # @jit, assert 不可使用 jit
    def check_datetime_is_navie(dt: datetime.datetime) -> bool:
        """
        輸入的必須為 datetime
        """
        try:
            assert isinstance(dt, datetime.datetime)
        except AssertionError as e:
            if isinstance(dt, str):
                # 用來處理, 如果 df.index 為 str
                raise DFINDEX_STRING()
            raise AssertionError(
                "!!! {} / type: {}".format(dt, type(dt))
            ) from e
        return (dt.tzinfo is None) or (
            dt.tzinfo.utcoffset(dt) is None
        )

    log_result = True
    if isinstance(data, pd.DataFrame):
        try:
            log_result = check_datetime_is_navie(data.index[0])
        except DFINDEX_STRING:
            log_result = check_datetime_is_navie(
                pd.to_datetime(data.index)[0]
            )
        except IndexError as e:
            # 資料缺少, data.index[[0] 不存在
            raise IndexError(data) from e
    elif isinstance(data, datetime.datetime):
        log_result = check_datetime_is_navie(data)
    elif isinstance(data, list):
        log_result = False
        # 只要有一個 navie 就回傳 True
        for elem in data:
            log_result = log_result or (
                check_datetime_is_navie(elem)
            )
    return log_result


def check_data_tzinfo_consist(
    data: Union[pd.DataFrame, datetime.datetime, List],
    tzinfo: str,
) -> bool:
    """
    確認 tzinfo 是否一致
    """
    assert isinstance(
        data, (pd.DataFrame, datetime.datetime, list)
    )

    # @jit
    def check_datetime_tzinfo_consist(
        dt: datetime.datetime, tzinfo: str
    ) -> bool:
        """
        輸入的必須為 datetime
        """
        assert isinstance(dt, datetime.datetime)
        log_result = dt.tzinfo == tzinfo
        assert isinstance(log_result, bool)
        return log_result

    log_result: bool = True
    if isinstance(data, pd.DataFrame):
        log_result = check_datetime_tzinfo_consist(
            data.index[0], tzinfo
        )
    elif isinstance(data, datetime.datetime):
        log_result = check_datetime_tzinfo_consist(data, tzinfo)
    elif isinstance(data, list):
        log_result = True
        # 只要有一個 不一致 就回傳 True
        for elem in data:
            log_result = log_result and bool(
                check_datetime_tzinfo_consist(elem, tzinfo),
            )
    return log_result


def tz_transform_multiple(  # noqa: C901
    data: Union[pd.DataFrame, datetime.datetime, List],
    tzinfo: str,
) -> pd.DataFrame:
    """
    如果寫入的時間為 2013-7-31 00:00:00 (navie)
    我的數據應該為 Asia/Taipei
    經過轉換, 應該要呈現為 2013-7-30 16:00:00+08:00
    """
    try:
        assert isinstance(
            data, (pd.DataFrame, datetime.datetime, list)
        )
        assert isinstance(tzinfo, str)
    except AssertionError as e:
        raise AssertionError(
            "!!! {} type: {} / {}".format(
                data, type(data), type(tzinfo)
            )
        ) from e

    tz = pytz.timezone(tzinfo)
    if check_data_is_navie(data):
        # 確認為 navie or tzinfo 不一致, 才處理
        # 若否, 則 pass

        # shift
        """
        time_difference = tz.utcoffset(
            datetime.datetime.utcnow()
        ).total_seconds()
        """

        if isinstance(data, pd.DataFrame):
            # pd.DataFrame
            try:
                data = data.tz_localize(tzinfo)
            except (TypeError, DFINDEX_STRING):
                data.index = pd.to_datetime(data.index)
                data = data.tz_localize(tzinfo)
        elif isinstance(data, datetime.datetime):
            data = tz.localize(data)
        elif isinstance(data, list):
            for i in range(len(data)):
                data[i] = tz_transform_multiple(data[i], tzinfo)
    elif not check_data_tzinfo_consist(data, tzinfo):
        if isinstance(data, pd.DataFrame):
            # pd.DataFrame
            data.index = data.index.tz_convert(tzinfo)
        elif isinstance(data, datetime.datetime):
            # data = tz.localize(data)
            # convert from tz1 to tz2
            data = data.astimezone(tz)
        elif isinstance(data, list):
            for i in range(len(data)):
                data[i] = tz_transform_multiple(data[i], tzinfo)
    return data


def tz_remove_multiple(
    data: Union[pd.DataFrame, datetime.datetime, List],
) -> pd.DataFrame:
    """
    from aware to navie
    2021-10-1 10:00:00+08:00 -> 2021-10-1 18:00:00
    """
    try:
        assert isinstance(
            data, (pd.DataFrame, datetime.datetime, list)
        )
    except AssertionError as e:
        raise TypeError("{}".format(type(data))) from e

    time_difference: Union[float, int] = 0  # for tzutc()
    tz_params = {
        "pytz.FixedOffset(480)": "Asia/Taipei",
        "UTC+08:00": "Asia/Taipei",
    }
    #
    try:
        if not check_data_is_navie(data):
            if isinstance(data, pd.DataFrame):
                tzinfo = data.index[0].tzinfo
                time_difference = 0  # for tzutc()
                if tzinfo != tzutc():
                    try:
                        tz = pytz.timezone(str(tzinfo))
                    except pytz.exceptions.UnknownTimeZoneError:
                        # print (tz_params, tzinfo, data.index[0])
                        tz = pytz.timezone(
                            tz_params[str(tzinfo)]
                        )
                    # shift
                    time_difference = tz.utcoffset(
                        datetime.datetime.utcnow()
                    ).total_seconds()

                data.index = data.index.tz_localize(None)
                data.index += datetime.timedelta(
                    seconds=time_difference
                )

            elif isinstance(data, datetime.datetime):
                try:
                    tz = pytz.timezone(str(data.tzinfo))
                except pytz.exceptions.UnknownTimeZoneError:
                    if str(data.tzinfo) == "tzutc()":
                        tz = pytz.timezone("UTC")
                    else:
                        tz = pytz.timezone(
                            tz_params[str(data.tzinfo)]
                        )
                # shift
                time_difference = tz.utcoffset(
                    datetime.datetime.utcnow()
                ).total_seconds()

                # pd.DataFrame
                data = data.replace(
                    tzinfo=None
                ) + datetime.timedelta(seconds=time_difference)

            elif isinstance(data, list):
                for i in range(len(data)):
                    data[i] = tz_remove_multiple(data[i])
    except IndexError:
        # IndexError: index 0 is out of bounds for axis 0 with size 0
        # 表示為空的 pd.DataFrame
        # 無需處理就丟出來
        return data
    return data


def dt_standard_export(
    dt: Union[datetime.datetime, None]
) -> str:
    """
    時間資訊之標準化輸出
    1. navie --> %Y-%m-%d %H:%M:%S
    2. aware --> %Y-%m-%d %H:%M:%S+{tz shift}:00
    """
    assert isinstance(dt, datetime.datetime) or (dt is None)
    if dt is None:
        return "None"

    time_flag = "%Y-%m-%d %H:%M:%S"
    if not check_data_is_navie(dt):
        tz = pytz.timezone(str(dt.tzinfo))
        # shift
        shift_hours = int(
            tz.utcoffset(
                datetime.datetime.utcnow()
            ).total_seconds()
            / 3600
        )

        # aware
        time_flag += "+{:0>2d}:00".format(shift_hours)
    return dt.strftime(time_flag)


def time_integer_operator(
    dt: datetime.datetime, time_flag: str, method
) -> datetime.datetime:
    """
    時間整點校準
    method 可以是 round, math.flood & math.ceil: 四捨五入, 無條件捨去與無條件進位

    輸入任意時間點, 以及 time_flag (1h, 10T等文字字串), 如果為四捨五入
    如 time_flag = "1h", 回傳 01:00:00, 02:00:00 等
    如 time_flag = "10T", 回傳 01:00:00, 01:10:00
    """
    assert method in [round, math.floor, math.ceil]
    assert isinstance(dt, datetime.datetime)

    ts_delta = timeflag2timedelta(
        time_flag
    ).total_seconds()  # 轉換秒數
    ts = dt.timestamp()
    # 直接除, 並取四捨五入
    ts2 = int(method(ts / ts_delta) * ts_delta)
    dt2 = datetime.datetime.fromtimestamp(ts2)
    return dt2


def time_round(
    dt: datetime.datetime, time_flag: str
) -> datetime.datetime:
    """
    輸入任意時間點, 以及 time_flag (1h, 10T等文字字串)
    如 time_flag = "1h", 回傳 01:00:00, 02:00:00 等
    如 time_flag = "10T", 回傳 01:00:00, 01:10:00
    """
    return time_integer_operator(dt, time_flag, round)


def situ_date_merge(sepline: List) -> datetime.datetime:
    """
    Situ 數據的日期轉換

    2022/6/29 下午 03:27:23
    """
    assert isinstance(sepline, list)
    try:
        dt_str = (
            "{} {} {}".format(sepline[0], sepline[2], sepline[1])
            .replace("下午", "PM")
            .replace("上午", "AM")
        )
    except IndexError as e:
        raise IndexError(sepline) from e
    try:
        dt = datetime.datetime.strptime(
            dt_str, "%Y/%m/%d %H:%M:%S %p"
        )
    except ValueError as e:
        raise ValueError(
            "'{}' | '{}'".format(
                dt_str,
                "%Y/%m/%d %H:%M:%S %p",
            )
        ) from e
    if (dt_str.find("12:00:00") >= 0) and (
        dt_str.find("PM") >= 0
    ):
        # 不處理
        pass
    elif dt_str.find("PM") >= 0:
        dt += datetime.timedelta(hours=12)
    elif (dt_str.find("12:00:00") >= 0) and (
        dt_str.find("AM") >= 0
    ):
        dt -= datetime.timedelta(hours=12)
    return dt
