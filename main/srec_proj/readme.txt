等震度圖程式操作與使用說明

1. 有2個必要目錄
  a. 目錄 \Input
  b. 目錄 \Output

2. 有1個必要檔案
  a. 檔案 config.txt

3. \Input 下應該有3個必要檔案
  a. 檔案 \Input\coor_taipei.txt
  b. 檔案 \Input\coor_taipei_wgs84
  c. 檔案 \Input\sample.txt

4. 設定檔 config.txt 定義環境參數
  — config.txt —
  input="Input"
  output="Output"
  levels=1.5,2.5,3.5,4.55,5.5,6.0,6.5,7.0
  gis_extra_post=embankment_10.shp,maroon,-,1.5,0.6,Taiwan_county_t
  wd97.shp,black,--,1.,0.6 # 定義額外的 GIS 圖層,
  # 0, shape file name
  # 1, line color
  # 2, line type
  # 3, line width
  # 4, alpha
  annotate_extra_post={"text":"台北市
  ","xycoords":"data","x":305000,"y":2770000,
  "xtext":305000,"ytext":2770000,"color":"black","fontsize":16}
  — end of file —

5. 設定檔 \Input\coor_taipei_wgs84 定義推估範圍，勿更改
  — coor_taipei_wgs84.txt —
  120.680 121.859448 0.002
  24.3000 25.3500 0.002
  — end of file —

6. 輸入檔 \Input\sample.txt
  — sample.txt —
  N,E,震度
  24.99792500,121.44094900,2
  25.02895900,121.45558900,3
  25.02895900,121.45558900,3
  24.99519700,121.43533300,2
  …
  — end of file —
  a. 「N,E,震度」是固定的，不可更改
  b. 「sample.txt」的主檔名將用於產出後檔案的主檔名
  