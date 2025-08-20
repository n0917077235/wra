"""
繪圖的分級與塗色
    PM 25 
    降雨量
"""

import pandas as pd
import numpy as np
from matplotlib import colors
from scipy.interpolate import interp1d

from typing import Union, Tuple, Dict

# 風速部份, 參考蒲氏風速分級
# http://windexpert.ce.tku.edu.tw/newcode/windtable.php
data_limits = {
    "pm25": (0, 80),
    "precipitation_tendays": (0, 600),
    "wind_speed": (0, 17.2),
}
data_label = {
    "wind_speed": [
        "1級 (無風)",
        "2級 (輕風)",
        "3級 (微風)",
        "4級 (和風)",
        "5級 (清風)",
        "6級 (強風)",
        "7級 (疾風)",
        "8級 (輕颱)",
    ],
}
data_params = {
    "pm25": {
        0: [(0, 12), "lightgreen", 1],
        1: [(12, 24), "lime", 1],
        2: [(24, 36), "darkgreen", 1],
        3: [(36, 42), "yellow", 1],
        4: [(42, 48), "gold", 1],
        5: [(48, 54), "orange", 1],
        6: [(54, 59), "tomato", 1],
        7: [(59, 65), "red", 1],
        8: [(65, 71), "darkred", 1],
        9: [(71, 80), "darkviolet", 1],
    },
    "precipitation_tendays": {  # 旬降雨量
        0: [(0, 1.5), "#fdfdfd", 1],
        1: [(1.5, 3.0), "#04e9e7", 1],
        2: [(3.0, 9.0), "#019ff4", 1],
        3: [(9.0, 15), "#0300f4", 1],
        4: [(15, 22.5), "#02fd02", 1],
        5: [(22.5, 30), "#01c501", 1],
        6: [(30, 45), "#008e00", 1],
        7: [(45, 60), "#fdf802", 1],
        8: [(60, 75), "#e5bc00", 1],
        9: [(75, 90), "#fd9500", 1],
        10: [(90, 120), "#fd0000", 1],
        11: [(120, 150), "#d40000", 1],
        12: [(150, 195), "#bc0000", 1],
        13: [(195, 240), "#f800fd", 1],
        14: [(240, 420), "#9854c6", 1],
        15: [(420, 600), "#49189e", 1],
    },
    "wind_speed": {  # 旬降雨量
        0: [(0, 1.6), "darkred", 0.1],  # 1級
        1: [(1.6, 3.4), "orangered", 0.4],  # 2級
        2: [(3.4, 5.5), "khaki", 0.7],  # 3級
        3: [(5.5, 8.0), "lime", 0.85],  # 4級
        4: [(8.0, 10.8), "aqua", 0.9],  # 5級
        5: [(10.8, 13.9), "blue", 1],  # 6級
        6: [(13.9, 17.2), "indigo", 1],  # 7級
    },
}


bmh_colors = [
    "#348ABD",
    "#A60628",
    "#7A68A6",
    "#467821",
    "#D55E00",
    "#CC79A7",
    "#56B4E9",
    "#009E73",
    "#F0E442",
    "#0072B2",
]


def assert_bounds(
    bounds: Tuple[Union[int, float], Union[int, float]]
):
    """
    bounds 只能為 list 或 tuple 型別
    內容長度為 2
    內含只能是整數或實數
    """
    assert isinstance(bounds, tuple)
    assert len(bounds) == 2
    for i in range(2):
        assert isinstance(bounds[i], (int, float))


def check_between(
    val: Union[int, float, np.ndarray],
    bounds: Tuple[Union[int, float], Union[int, float]],
) -> Union[bool, np.ndarray]:
    """
    確認傳入值是否位於 bounds 中
    會傳 bool
    """
    try:
        assert_bounds(bounds)
        assert isinstance(
            val, (int, float, np.ndarray, np.int64, np.float32)
        )
    except AssertionError as e:
        raise TypeError(
            "!!! {} / {}".format(val, type(val))
        ) from e
    return (val - bounds[0]) * (val - bounds[1]) <= 0


def linear_mapping(
    values: Union[int, float, np.ndarray],
    bounds1: Tuple[Union[int, float], Union[int, float]],
    bounds2: Tuple[Union[int, float], Union[int, float]],
) -> Union[float, np.ndarray]:
    """
    原本值域為 bound1, 線性投射為 bound2
    """
    assert isinstance(values, (int, float, np.ndarray))
    assert_bounds(bounds1)
    assert_bounds(bounds2)

    if isinstance(values, (int, float)):
        # 整數或實數
        assert check_between(values, bounds1)
    else:
        # np.ndarray
        assert check_between(np.max(values), bounds1)
        assert check_between(np.min(values), bounds1)

    return (values - bounds1[0]) / (bounds1[1] - bounds1[0]) * (
        bounds2[1] - bounds2[0]
    ) + bounds2[0]


def determine_cmap(
    cmap_dict: Dict,
    blimit: Tuple[Union[int, float], Union[int, float]],
) -> np.ndarray:
    """
    https://www.ettoday.net/news/20161122/815959.htm
    依據台灣 PM25 的指標等級與繪圖方式
    建立對應的 cmap
    階段式色階

    """
    vindex: np.ndarray = np.arange(256)
    vindex = linear_mapping(  # type: ignore
        vindex,  # [0, 1, 2, ..., 255]
        (0, 255),
        blimit,
    )

    data_colors = np.zeros((256, 4))
    for _key, item in cmap_dict.items():
        color_rgb = list(colors.to_rgba(item[1]))
        for j in range(3):
            data_colors[:, j] = np.where(
                check_between(vindex, item[0]),
                color_rgb[j],
                data_colors[:, j],
            )
        data_colors[:, 3] = item[2]
    pm25_cmap = colors.ListedColormap(data_colors)
    return pm25_cmap


def determine_cmap_continue(
    cmap_dict: Dict,
    blimit: Tuple[Union[int, float], Union[int, float]],
    log_export: bool = False,
    **kwargs,
):
    """
    https://www.ettoday.net/news/20161122/815959.htm
    依據台灣 PM25 的指標等級與繪圖方式
    建立對應的 cmap
    階段式色階
    """
    vindex = linear_mapping(
        np.arange(256),  # [0, 1, 2, ..., 255]
        (0, 255),
        blimit,
    )

    # pylint: disable=unnecessary-dict-index-lookup
    cmap_dict_size = len(cmap_dict)
    color_points = {
        **{
            cmap_dict[0][0][0]: list(
                colors.to_rgba(cmap_dict[0][1])
            )[:3]
            + [cmap_dict[0][2]],
            cmap_dict[cmap_dict_size - 1][0][1]: list(
                colors.to_rgba(cmap_dict[cmap_dict_size - 1][1])
            )[:3]
            + [cmap_dict[cmap_dict_size - 1][2]],
        },
        **{  # 變成三段
            (cmap_dict[key][0][0] * 2 + cmap_dict[key][0][1])
            / 3: list(colors.to_rgba(cmap_dict[key][1]))[:3]
            + [cmap_dict[key][2]]
            for key, _elem in cmap_dict.items()
        },
        **{
            (cmap_dict[key][0][0] + cmap_dict[key][0][1] * 2)
            / 3: list(colors.to_rgba(cmap_dict[key][1]))[:3]
            + [cmap_dict[key][2]]
            for key, _elem in cmap_dict.items()
        },
    }

    data_colors = np.zeros((256, 4))
    xlist = sorted(list(color_points.keys()))
    for j in range(4):
        ylist = np.array([color_points[x][j] for x in xlist])
        f = interp1d(xlist, ylist)
        data_colors[:, j] = f(vindex)

    if log_export:
        # 產生 R, G, B, Alpha 資料
        df = pd.DataFrame(
            data_colors, columns=["R", "G", "B", "Alpha"]
        )
        pm25_data = np.linspace(blimit[0], blimit[1], 256)
        df.loc[:, "PM25"] = pm25_data

        for c in ["R", "G", "B"]:
            df.loc[:, c] *= 256
            df = df.astype({c: "int32"})

        df2 = pd.DataFrame(
            [[148, 0, 211, 1.0, 350.4]],
            columns=["R", "G", "B", "Alpha", "PM25"],
            index=[256],
        )
        df3 = pd.DataFrame(
            [[148, 0, 211, 1.0, 500.4]],
            columns=["R", "G", "B", "Alpha", "PM25"],
            index=[257],
        )
        df = pd.concat(
            [
                df,
                df2,
                df3,
            ]
        )
        if "data_name" in kwargs:
            df.to_csv("{}.csv".format(kwargs["data_name"]))
    data_cmap = colors.ListedColormap(data_colors)
    return data_cmap


if __name__ == "__main__":
    # 取得 cmap 色階
    for flag in ["pm25", "precipitation_tendays"]:
        cmap1 = determine_cmap(
            data_params[flag],
            data_limits[flag],
        )

        cmap2 = determine_cmap_continue(
            data_params[flag],
            data_limits[flag],
            log_export=True,
            data_name=flag,
        )
        print(flag, cmap1, cmap2)
