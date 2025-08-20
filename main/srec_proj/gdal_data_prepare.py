"""
# 處理 shape file --> raster file
"""

# -*- coding: utf-8 -*-
import sys
import os
from typing import Union

sys.path.append(os.path.join("..", "..", "jlib", "srcs"))
import jutility as jut
import gdal_utility as jgut

data_path = sys.argv[1]
filter_flags: Union[None, str] = sys.argv[2]
if filter_flags == "None":  # 篩選檔案
    filter_flags = None
attribute_name = sys.argv[3]

nc_fname_whole = sys.argv[4]  # 輸出路徑
cell_define = sys.argv[5]

# 可設定, 可不設定
plot_param = None
if len(sys.argv) > 6:
    plot_param = sys.argv[6]  # 繪圖設定檔

log_debug = True

if __name__ == "__main__":
    export_path, nc_fname = os.path.split(nc_fname_whole)
    if log_debug:
        print("# 處理路徑: {}".format(data_path))
        print("  <-- 正規化表示法: {}".format(filter_flags))

        print("# 輸出檔名: {}".format(nc_fname_whole))
        print("  --> 目錄: {}".format(export_path))
        print("  --> nc file name: {}".format(nc_fname))

    # 建立路徑
    os.system("mkdir -p {}".format(export_path))

    # 找出路徑中的檔案 .shp
    shp_list = jut.search_files_in_dir(
        data_path, regular_flags="shp$", log_recursive=True
    )
    if filter_flags is not None:
        shp_list = jut.filter_search_result(
            shp_list, filter_flags
        )

    # 循序處理
    gdal_nc = jgut.gdal_utility(
        nc_fname_whole, cell_define, log_debug=log_debug
    )
    for shp_fname in shp_list:
        if log_debug:
            print(
                "      |--> 待處理 shape file: {}".format(
                    shp_fname
                )
            )
        gdal_nc.gdal_rasterizing(
            shp_fname, attribute=attribute_name
        )

        if plot_param is not None:
            gdal_nc.load_band_data(nc_fname_whole)
            gdal_nc.plot_GIS_data(plot_param)
