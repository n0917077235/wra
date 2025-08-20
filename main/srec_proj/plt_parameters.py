"""
Parameters for Matplotlib
"""

import os
import sys
import matplotlib.pyplot as plt
from matplotlib.colors import ListedColormap
import numpy as np
import math
from typing import List

sys.path.append(os.path.join("..", "..", "srcs"))
import jutility as jut
import cmap_define

figsize = (12, 9)
# 繪圖, 中文
plt.rcParams["font.sans-serif"] = ["SimSun"]
# plt.rcParams["font.sans-serif"] = ["Microsoft JhengHei"]
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
        # raise AttributeError("{}".format(type(axs))) from e


def random_pick_uv(
    umat, vmat, xlist, ylist, mat_size
) -> np.ndarray:
    """
    隨機挑選 umat & vmat
    """
    xv, yv = np.meshgrid(
        np.arange(len(xlist)), np.arange(len(ylist))
    )

    xv = xv.reshape(-1)
    yv = yv.reshape(-1)
    rand_number = np.random.rand(mat_size)  # 隨機函數
    quiver_data = []
    for r in rand_number:
        index = min(
            int(math.floor(r * xv.shape[0])),
            xv.shape[0] - 1,
        )
        xindex = xv[index]
        yindex = yv[index]
        try:
            mat_sub = [
                xlist[xindex],
                ylist[yindex],
                umat[yindex, xindex],
                vmat[yindex, xindex],
            ]
        except IndexError as e:
            raise IndexError(
                "{} / {} | {} / {} | {}".format(
                    umat.shape,
                    yindex,
                    xindex,
                    len(ylist),
                    len(xlist),
                )
            ) from e
        if not math.isnan(umat[yindex, xindex]):
            quiver_data.append(mat_sub)
    return np.array(quiver_data)


def decrease_alpha_colormap(
    cmap, min_alpha: float = 0.0, max_alpha: float = 1.0
):
    """
    建立逐步調降 alpha 的 colormap
    cmap 外部輸入的 colormap
    """
    # Get the colormap colors
    my_cmap = cmap(np.arange(cmap.N))

    # Define the alphas in the range from 0 to 1 (default value)
    alphas = np.linspace(min_alpha, max_alpha, cmap.N)

    # Define the background as white
    # 白色的三原色設定
    BG = np.asarray(
        [
            1.0,
            1.0,
            1.0,
        ]
    )
    # Mix the colors with the background
    for i in range(cmap.N):
        my_cmap[i, :-1] = my_cmap[i, :-1] * alphas[i] + BG * (
            1.0 - alphas[i]
        )
    # Create new colormap which mimics the alpha values
    return ListedColormap(my_cmap)


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


def merge_images(
    fig_fnames: List,
    export_fname: str,
    log_horizontal: bool = True,
    **kwargs,
):
    """
    log_horizontal = True
    side by side
        convert image1.png image2.png +append joined_horizontal.png
    log_horizontal = False
    one above the other
        convert image1.png image2.png -append joined_vertical.png
    """
    command = "docker pull bsjacky/tool_convert:latest"
    try:
        jut.system_call_running(command, **kwargs)
    except jut.SystemCallFail:
        pass

    op = "+"
    if not log_horizontal:
        op = "-"
    command = "docker run -v {}:/container2 ".format(
        os.environ["DIND_USER_HOME"]
    )
    command += "-v /var/run/docker.sock:/var/run/docker.sock "
    command += "bsjacky/tool_convert:latest /bin/bash -c "
    inflist_str = ""
    for fig_fname in fig_fnames:
        inflist_str += "{} ".format(fig_fname)
    command += (
        "'cd /container2 ; convert {} {}append {}'".format(
            inflist_str, op, export_fname
        )
    )
    os.system(command)


def plot_fig_detail(ax, **kwargs):
    """
    針對 title, xlabel, ylabel 等進行處理
    """
    fig_params = {
        "fig_title": ax.set_title,
        "fig_legend": ax.legend,
        "fig_xlabel": ax.set_xlabel,
        "fig_ylabel": ax.set_ylabel,
        "fig_xlim": ax.set_xlim,
        "fig_ylim": ax.set_ylim,
    }
    for key, elem in fig_params.items():
        if key in kwargs:
            try:
                elem(
                    *kwargs[key][0],  # args
                    **kwargs[key][1],  # kwargs
                )
            except ValueError as e:
                raise ValueError(
                    "{}: {}".format(key, kwargs[key])
                ) from e


##########################################
# 繪製 Ridge Plot
def ridgeline(
    df,
    overlap=0,
    row_list=None,
    fill: bool = True,
    labels=None,
    n_points=150,
    **kwargs,
):
    """
    Creates a standard ridgeline plot.

    data, list of lists.
    overlap, overlap between distributions. 1 max overlap, 0 no overlap.
    fill, matplotlib color to fill the distributions.
    n_points, number of points to evaluate each distribution function.
    labels, values to place on the y axis to describe the distributions.
    """
    ax = kwargs["ax"]
    try:
        assert overlap >= 0
    except AssertionError as e:
        raise ValueError("overlap must be in [0 1]") from e
    ys = []
    if row_list is None:
        row_list = list(set(df.loc[:, kwargs["row"]].values))
    for i, row in enumerate(row_list):
        df_sub = df[df[kwargs["row"]] == row]
        xx = np.array(df_sub.loc[:, kwargs["x"]].values)
        y = i * overlap
        ys.append(y)
        if fill:
            if "color" in kwargs:
                plt.fill_between(
                    xx,
                    np.array(df_sub.loc[:, kwargs["y"]].values)
                    + y,
                    np.array(df_sub.loc[:, kwargs["y2"]].values)
                    + y,
                    zorder=df.shape[0] - i + 1,
                    color=kwargs["color"],
                )
            else:
                plt.fill_between(
                    xx,
                    np.array(df_sub.loc[:, kwargs["y"]].values)
                    + y,
                    np.array(df_sub.loc[:, kwargs["y2"]].values)
                    + y,
                    zorder=df.shape[0] - i + 1,
                    color=cmap_define.bmh_colors[
                        i % len(cmap_define.bmh_colors)
                    ],
                )
        plt.plot(
            xx,
            np.array(df_sub.loc[:, kwargs["y"]].values) + y,
            c="k",
            zorder=df.shape[0] - i + 1,
        )

    if isinstance(labels, list):
        assert len(ys) == len(labels)
        plt.yticks(ys, labels)
    else:
        ax.set_ylabel("")
        ax.set_yticklabels([])
