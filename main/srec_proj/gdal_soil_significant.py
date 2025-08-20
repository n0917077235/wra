"""
# 功用
# 取出代表性土壤
"""

# -*- coding: utf-8 -*-
import os
import sys

sys.path.append(os.path.join("..", "..", "jlib", "srcs"))
import jutility as jut
import gdal_utility as jgut

data_path = sys.argv[1]
regular_flag = sys.argv[2]
# 搜尋目錄內的檔案
flist = jut.search_files_in_dir(
    data_path, regular_flags=regular_flag
)
flist = jut.filter_search_result(
    flist, regular_flags=".nc$"
)  # 限縮為 .nc

cell_define = sys.argv[3]
nc_fname_out = sys.argv[4]

# 可設定, 可不設定
plot_param = None
if len(sys.argv) > 5:
    plot_param = sys.argv[5]  # 繪圖設定檔

log_debug = False

if __name__ == "__main__":
    print("   -- 代表性土壤功能")
    print("   -- Data Path: {}".format(data_path))
    print("      Regular Expression: {}".format(regular_flag))
    for m in range(len(flist)):
        print("      {}. {}".format(m + 1, flist[m]))
    print("   -- Cell Information: {}".format(cell_define))

    # ===================================================================
    # ===================================================================
    # ===================================================================
    soil_list = []
    for m in range(len(flist)):
        print("      {}. Loading {}".format(m + 1, flist[m]))
        gdal_nc = jgut.gdal_utility(
            flist[m], cell_define, log_debug=log_debug
        )
        gdal_nc.load_band_data(flist[m])
        soil_list.append(gdal_nc)

    # gdal_nc = jgut.gdal_utility(
    #    flist[m], cell_define, log_debug=log_debug
    # )
    soil_list[0].significant_soil(soil_list)  # 代表性土壤
    soil_list[0].export_band_data(nc_fname_out)  # 輸出

    if plot_param is not None:
        nc_fname_significant = nc_fname_out
        gdal_nc = jgut.gdal_utility(
            nc_fname_significant,
            cell_define,
            log_debug=log_debug,
        )
        # gdal_nc.plot_gdaldem(plot_param)  # 繪圖輸出
        # soil_list[0].load_band_data(nc_fname)
        soil_list[0].plot_GIS_data(
            plot_param,
            plot_fname=nc_fname_significant.replace(".nc", ""),
        )
