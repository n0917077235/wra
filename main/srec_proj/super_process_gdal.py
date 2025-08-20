"""
# load super file
# 以 gdal 進行處理
"""

# -*- coding: utf-8 -*-
import os
import sys
import geopandas as gpd
import matplotlib.pyplot as plt
import numpy as np
import GDAL_SF

sys.path.append(os.path.join("..", "..", "jlib", "srcs"))
# import plt_parameters
import TimeConsume
import jlib_logging
import dem_utility
import plt_parameters
import jutility as jut
import netcdf_analysis as NA
import tif_operation as tif_op

mytc = TimeConsume.TimeConsume("GDAL processing")

sf_fname = sys.argv[1]  # super file name
log_debug = False
program = "gdal_process"
root_logger = jlib_logging.logger_setup(
    program,
    filename=os.path.join(
        os.path.join("logging", program),
    ),
    log_append=True,
)

# 鄉鎮圖
# debug, 解決路徑問題
town_shp_fname = "../../../../GIS/Taiwan_town_twd97.shp"
town_shp = gpd.read_file(town_shp_fname)


def dem_process(dem_fname: str, gdal_sf, **kwargs):
    """
    dem_fname
    gdal_sf
    """
    # 地表高程
    mydem = dem_utility.DEM_Utility(dem_fname, **kwargs)
    # coor_inform = CI.readcell_inform(gdal_sf.coor_fname)
    cell_inform = gdal_sf.cell_inform
    mydem.load_dem(
        tic_lim=[
            np.min(cell_inform.xedges),
            np.max(cell_inform.xedges),
            np.min(cell_inform.yedges),
            np.max(cell_inform.yedges),
        ],  # [xmin, xmax, ymin, ymax]
    )

    #######################################################################
    # data array export
    # --> NC file
    xlist = np.array(
        [
            mydem.determine_loc_from_index([i, 0])[0]
            for i in range(mydem.dem_data.shape[1])
        ]
    )
    assert abs(np.min(xlist) - np.min(cell_inform.xedges)) < 20
    assert abs(np.max(xlist) - np.max(cell_inform.xedges)) < 20
    ylist = np.array(
        [
            mydem.determine_loc_from_index([0, j])[1]
            for j in range(mydem.dem_data.shape[0])
        ]
    )
    assert abs(np.min(ylist) - np.min(cell_inform.yedges)) < 20
    assert abs(np.max(ylist) - np.max(cell_inform.yedges)) < 20
    NA.export_netcdf(
        [ylist, xlist],
        mydem.dem_data,
        ["Y", "X"],
        os.path.join(
            gdal_sf.data_path,
            "dem_{}.nc".format(gdal_sf.proj_name),
        ),
    )

    #######################################################################
    # plot figure
    # 日照陰影圖
    plt_parameters.assign_fig_detail(None, "fig_style", **kwargs)
    _fig, ax = plt.subplots(
        1, figsize=kwargs.get("figsize", (9, 8))
    )
    GDAL_SF.tif_extra_pre_plot(
        ax,
        tif_extra_pre=gdal_sf.tif_extra_pre,
        root_logger=root_logger,
    )
    try:
        mydem.plot_hillshade(
            _fig,
            ax,
            **{
                key: gdal_sf.hillshade[key]
                for key in gdal_sf.hillshade
            },
        )
    except TypeError as e:
        if root_logger is not None:
            root_logger.error(gdal_sf.hillshade, exc_info=True)
        raise TypeError(gdal_sf.hillshade) from e

    # 如有額外定義圖匡, 進行繪製
    GDAL_SF.gis_extra_post_plot(ax, **kwargs)

    ax.set_title("{} (DEM)".format(gdal_sf.cname), fontsize=12)
    plt.tight_layout()
    fig_fname = os.path.join(
        gdal_sf.data_path, "dem_{}".format(gdal_sf.proj_name)
    )
    jut.save_fig(fig_fname)

    # 坡地圖
    plt_parameters.assign_fig_detail(None, "fig_style", **kwargs)
    _fig, ax = plt.subplots(
        1, figsize=kwargs.get("figsize", (9, 8))
    )
    GDAL_SF.tif_extra_pre_plot(
        ax,
        tif_extra_pre=gdal_sf.tif_extra_pre,
        root_logger=root_logger,
    )
    mydem.plot_slope(
        _fig,
        ax,
        **gdal_sf.slope_vlim,
    )
    # 如有額外定義圖匡, 進行繪製
    GDAL_SF.gis_extra_post_plot(ax, **kwargs)

    ax.set_title(
        "{} (DEM slope)".format(gdal_sf.cname), fontsize=12
    )

    plt.tight_layout()
    fig_fname = os.path.join(
        gdal_sf.data_path,
        "dem_slope_{}".format(gdal_sf.proj_name),
    )
    jut.save_fig(fig_fname)


if __name__ == "__main__":
    root_logger.info("# GDAL process: {}".format(sf_fname))
    log_quiet = True
    gdal_sf = GDAL_SF.GDAL_SF(
        sf_fname,
        root_logger=root_logger,
        log_quiet=log_quiet,
        proj_logger_append=False,
    )

    # Rasterization
    gdal_sf.shp_rasterizing(
        calc_types=["landuse", "soil"],
        log_parallel_process=True,
    )  # Rasterization

    if gdal_sf.tif_extra_pre is not None:
        # 繪製 TIF 圖
        if gdal_sf.proj_logger is not None:
            gdal_sf.proj_logger.debug(
                "TIF background plot: {}".format(
                    gdal_sf.tif_extra_pre
                )
            )

        plt.style.use("bmh")
        _fig, ax = plt.subplots(1, figsize=(12, 9))
        # load multiple TIFFs
        # merge
        my_tms = tif_op.tif_meta_system(
            gdal_sf.tif_extra_pre["tif_flist"],
            trim_extent=gdal_sf.tif_extra_pre["extent"],
            root_logger=gdal_sf.proj_logger,
        )
        # Plot
        my_tms.tif_multi_plot(
            ax,
            **{
                "alpha": gdal_sf.tif_extra_pre["alpha"],
                "fig_title": "{} (底圖)".format(gdal_sf.cname),
                "root_logger": gdal_sf.proj_logger,
            },
        )

        # 產生繪圖
        jut.save_fig(
            os.path.join(
                gdal_sf.data_path,
                "TIF_background_{}".format(gdal_sf.proj_name),
            )
        )

    if gdal_sf.proj_logger is not None:
        gdal_sf.proj_logger.debug("DEM process")
    # 地表高程處理
    # 1. 切割網格; 2. 建立日照陰影圖
    kwargs = {
        "root_logger": gdal_sf.proj_logger,
        "fig_style": "bmh",
        "figsize": (12, 9),
        "gis_extra_post": gdal_sf.gis_extra_post,
        "gis_extra_post_filter_type": gdal_sf.gis_extra_post_filter_type,
        "tif_extra_pre": gdal_sf.tif_extra_pre,
        "cell_inform": gdal_sf.cell_inform,
    }
    dem_process(
        os.path.join(
            "..", "..", "..", "..", "GIS", "tw20mdtm2.img"
        ),
        gdal_sf,
        **kwargs,
    )
    # 表層土壤處理過程
    gdal_sf.soil_process(**kwargs)
    # 土地利用處理過程
    gdal_sf.luse_process(**kwargs)

    # fut.remove_files_folder(os.path.join("raster_data", "soil"))
    # fut.remove_files_folder(
    #    os.path.join("raster_data", "landuse")
    # )

    # 後處理
    luse_nc_fname = os.path.join(
        gdal_sf.data_path, "luse_{}.nc".format(gdal_sf.proj_name)
    )
    soil_nc_fname = os.path.join(
        gdal_sf.data_path,
        "soil_{}_significant.nc".format(gdal_sf.proj_name),
    )
    luse_tuple = NA.read_ncband(luse_nc_fname)
    soil_tuple = NA.read_ncband(soil_nc_fname)
    for data_fname in [luse_nc_fname, soil_nc_fname]:
        nc_data = NA.read_ncband(data_fname)

mytc.TimeConsume_Calc("GDAL Process")
mytc.export()
