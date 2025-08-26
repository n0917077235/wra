等震度圖程式操作與使用說明

1. 於 C:\ 解壓縮 srec_proj.zip 為 C:\srec_proj 目錄

2. C:\srec_proj 下應該有2個必要目錄
  a. 目錄 C:\srec_proj\Input
  b. 目錄 C:\srec_proj\Output

3. C:\srec_proj 下應該有1個必要檔案
  a. 檔案 C:\srec_proj\config.txt

4. C:\srec_proj\Input 下應該有3個必要檔案
  a. 檔案 C:\srec_proj\Input\coor_taipei.txt
  b. 檔案 C:\srec_proj\Input\coor_taipei_wgs84
  c. 檔案 C:\srec_proj\Input\sample.txt

5. 設定檔 C:\srec_proj\config.txt 定義環境參數
  — config.txt —
  input folder = “C:\srec_proj\Input”
  output folder = “C:\srec_proj\Output”
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

6. 設定檔 C:\srec_proj\Input\coor_taipei_wgs84 定義推估範圍，勿更改
  — config.txt —
  120.680 121.859448 0.002
  24.3000 25.3500 0.002
  — end of file —

7. 輸入檔 C:\srec_proj\Input\sample.txt
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
  c. 安裝並啟動Docker Desktop
  https://www.docker.com/products/docker-desktop/
  當時正常可用的版本為4.33.1 (161083)，若正常完畢，可用CMD下「docker
  version」測試，回應正確版號，沒有錯誤訊息即可

8. 執行 docker 命令
  docker run --rm -v C:\srec_proj:/container
  bsjacky/numerical_dem@sha256:b1278534e7004e9f198f707a2d0e93ff
  d82ff26f8858ce3c213d5c27638ebb4a /bin/bash -c "cd /container ;
  python srec_interpolate.py sample.txt log_plot"
  - --rm 運作結束之後，清除剛剛運作的container
  - -v 掛載本地目錄路徑至容器內指定目錄路徑
  - IMAGE@DIGEST 指定運行的 docker image 的版本
  - /bin/bash 是當容器運行後，第一個執行的指令
  - /bin/bash -c “XXX” 是把 “XXX” 裡面的內容，作為 bash 的 input
  - sample.txt 作為 python 第一個引數，約定好的命名方式
  - log_plot 作為 python 第二個引數，是指繪出圖檔，不加則不繪
  第一次使用此指令，將會從 Docker HUB 將 Docker Image 下載回近端，此
  Image 大約為 31GB，請耐心等待