"""
從卡氏座標 --> 極座標計算
"""

from typing import Union, List, Tuple
import numpy as np
import math


def sin_cos2rad(
    sin_val: Union[float, int], cos_val: Union[float, int]
) -> float:
    """
    輸入 sin 數值與 cos 數值，回傳弧度
    """
    for val in [sin_val, cos_val]:
        assert isinstance(val, (float, int)), "請輸入實數或整數"
    rad_list = [np.arcsin(sin_val), np.arccos(cos_val)]
    result = -1e9
    if sin_val >= 0:
        result = rad_list[1]  # 以 cos 值為主
    else:
        if cos_val >= 0:
            # 第四象限
            result = 2 * math.pi + rad_list[0]
        else:
            # 第三象限
            result = math.pi - rad_list[0]
    return result


def assert_test(point: Union[List, Tuple]):
    assert isinstance(point, (list, tuple)), "請輸入list或tuple"
    assert len(point) == 2, "Require 2 value, but got {}".format(
        len(point)
    )
    for val in point:
        assert isinstance(
            val, (float, int)
        ), "Require float or int, but got {}".format(type(val))


class Polar_coord:
    def __init__(self, centre_point: Union[List, Tuple]):
        """
        輸入極座標中心
        """
        assert_test(centre_point)  # assert test
        self.centre_point = centre_point

    def cart_to_polar(self, point: Union[List, Tuple]) -> Tuple:
        """
        輸入卡氏座標點，回傳極座標
        """
        assert_test(point)  # assert test
        x, y = point
        cx, cy = self.centre_point
        r = ((x - cx) ** 2 + (y - cy) ** 2) ** 0.5

        sin_val = (y - cy) / r
        cos_val = (x - cx) / r
        theta = sin_cos2rad(sin_val, cos_val)
        return (r, theta)
