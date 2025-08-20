"""
處理 tif 檔案的輸入, 繪圖
"""

import os

# import matplotlib.pyplot as plt
from typing import List, Tuple
import tempfile
import rasterio
from rasterio.merge import merge
from rasterio.plot import show
from rasterio.windows import Window


class tif_meta_system:
    """
    TIF Meta System
        from list file
        define tif data path
    """

    def __init__(self, tif_flist: List, **kwargs):
        """
        Initialization
        """
        self.tif_multi_load(tif_flist, **kwargs)

    def tif_multi_load(self, tif_flist: List, **kwargs):
        """
        給予 tif list, 循序 load tiff & define the ticks of tiff
        define self.images
        """
        root_logger = kwargs.get("root_logger", None)
        crs = kwargs.get("crs", "EPSG:3826")
        assert isinstance(tif_flist, list)
        if root_logger is not None:
            root_logger.debug(
                "Multi-load tif files: {}".format(
                    [
                        os.path.basename(fname)  # 簡化輸出
                        for fname in tif_flist
                    ]
                )
            )

        images: List = []
        for tif_fname_merge in tif_flist:
            assert isinstance(tif_fname_merge, str)
            assert os.path.exists(
                tif_fname_merge
            ), "TIF file '{}' does not exist!".format(
                tif_fname_merge
            )

            # Open each image and read its predefined location
            src = rasterio.open(tif_fname_merge, crs=crs)
            images.append(src)
            profile = src.profile
        # Merge the images based on their location
        self.merged_data, self.merged_transform = merge(images)
        if root_logger is not None:
            loc1 = self.merged_transform * (0, 0)
            loc2 = self.merged_transform * (
                self.merged_data.shape[-1],
                self.merged_data.shape[-2],
            )
            self.merged_extent = [
                min(loc1[0], loc2[0]),
                max(loc1[0], loc2[0]),
                min(loc1[1], loc2[1]),
                max(loc1[1], loc2[1]),
            ]
            root_logger.debug(
                "Original shape: {}".format(
                    self.merged_data.shape
                )
            )
            root_logger.debug(
                "Original extent: {}".format(self.merged_extent)
            )

        trim_extent = kwargs.get("trim_extent", None)
        if trim_extent is not None:
            # 設定 sliced window, 切割為較小範圍的圖片
            if root_logger is not None:
                root_logger.debug(
                    "Trim extent: {}".format(trim_extent)
                )
            extent_index = []
            for loc in [
                (trim_extent[0], trim_extent[2]),
                (trim_extent[1], trim_extent[3]),
            ]:
                loc_index = self.query_pixel_index(loc)
                extent_index += list(loc_index)
            if root_logger is not None:
                root_logger.debug(
                    "Trim extent index: {}".format(extent_index)
                )
            # trim window

            # 將 merged file 寫出
            tmpFilename = ""
            with tempfile.NamedTemporaryFile(
                "w+t", delete=False
            ) as fp:
                tmpFilename = fp.name

            profile = images[0].profile
            profile.update(
                {
                    "transform": self.merged_transform,  # Use the same transform as the input file
                    "width": self.merged_data.shape[-1],
                    "height": self.merged_data.shape[-2],
                    "count": 3,
                }
            )
            with rasterio.open(
                tmpFilename,
                "w",
                **profile,
            ) as dst:
                # create a new temporay TIFF
                dst.write(self.merged_data)

            Window_params = (
                min(extent_index[0], extent_index[2]),
                min(extent_index[1], extent_index[3]),
                abs(extent_index[2] - extent_index[0]),
                abs(extent_index[3] - extent_index[1]),
            )
            win = Window(*Window_params)
            with rasterio.open(tmpFilename) as src:
                # 座標轉換
                self.merged_transform = src.window_transform(win)
                # 擷取框內資料
                self.merged_data = src.read()[
                    :,
                    win.row_off : win.row_off + win.height,
                    win.col_off : win.col_off + win.width,
                ]

            # 刪除站存檔
            os.unlink(tmpFilename)

    def query_pixel_index(self, loc: Tuple) -> Tuple[int, ...]:
        """
        輸入座標, 回傳對應的 pixel index
        """
        assert isinstance(loc, tuple)
        assert len(loc) == 2
        loc_index = ~self.merged_transform * loc
        loc_index2: Tuple[int, ...] = tuple(
            [int(elem) for elem in loc_index]
        )

        return loc_index2

    def tif_multi_plot(self, ax, **kwargs):
        """
        整並多張 tif 圖片
        Plot the merged image
        """

        # Plot the merged image using rasterio
        show(
            self.merged_data,
            transform=self.merged_transform,
            ax=ax,
            **{
                flag: elem
                for flag, elem in kwargs.items()
                if flag
                not in ["root_logger", "grid", "fig_title"]
            },
        )

        if kwargs.get("grid", False):
            # 繪製格線
            ax.grid(linestyle="--", color="k")

        if kwargs.get("fig_title", None) is not None:
            # 繪製圖片標題
            ax.set_title(kwargs.get("fig_title", None))
        ax.set_xlabel(r"$X$")
        ax.set_ylabel(r"$Y$")
        return ax
