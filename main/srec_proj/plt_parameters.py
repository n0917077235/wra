"""
Parameters for Matplotlib
"""

import os
import sys
import matplotlib.pyplot as plt
import numpy as np
import math
from typing import List

sys.path.append(os.path.join("..", "..", "srcs"))

figsize = (12, 9)
# 繪圖, 中文
plt.rcParams["font.sans-serif"] = ["SimSun"]
plt.rcParams["axes.unicode_minus"] = False
plt.style.use("bmh")


def get_subax(axs, subfig_index: int):
    """
    對於多子圖的 axs, 循序抓出 ax
    """
    try:
        axs = axs.reshape(-1)
        return axs[subfig_index]
    except AttributeError:
        return axs


def assign_fig_detail(ax, flag, **kwargs):
    """
    繪圖 設定

    如果 kwargs 不包含 flag, 則回傳 None
    None 則不設定

    另外, legend 部分
        額外增設 log_legend=False, 用來關閉與刪除 legend
    """
    assert isinstance(flag, str)
    
    def set_legend(*args, **kwargs):
        """
        legend 設定 shadow = True
        """
        if kwargs.get("log_legend", True):
            ax.legend(*args, **{
                key: elem 
                for key, elem in kwargs.items()
                if key not in ["log_legend"]
            })
        else:
            # 刪除 legend
            ax.get_legend().remove()
    fig_params = {
        "fig_title": ax.set_title,
        "fig_xlabel": ax.set_xlabel,
        "fig_ylabel": ax.set_ylabel,
        "fig_xscale": ax.set_xscale,
        "fig_yscale": ax.set_yscale,
        "fig_xlim": ax.set_xlim,
        "fig_ylim": ax.set_ylim,
        "legend": set_legend,
        "fig_style": plt.style.use,
    }
    assert flag in list(fig_params.keys())


    if flag in kwargs:
        content = kwargs.get(flag, None)
        fig_params[flag](*content[0], **content[1])
    return ax
