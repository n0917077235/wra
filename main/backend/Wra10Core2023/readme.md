# main

backend新增了LineBotService功能

# LineBotService

一個基於 **.NET 6** 的後端服務，主要有以下兩種功能：
- **LINE Bot Webhook**：接收與回應 LINE 感測器資料查詢
- **Scheduled Tasks（定時排程）**：自動執行資料檢查與推播通知(施工中)

其中**Scheduled Tasks（定時排程）**內有CCTV斷線通知與地震通報兩項


## 環境需求
- Windows系統
- .NET 6 SDK
- SQL Server
- Visual Studio / VS Code

## 啟動條件
需要**appsettings.Development.json**設定檔案，其中包含資料庫連線資訊以及Line Message API設定資料。
若無法自行填上，請詢問工程師取得

## 啟動方法
將專案clone至本地後，終端機輸入dotnet run啟動伺服器，便會聆聽webhook API並且自動執行兩種定時排程

## 備註
- 伺服器使用的port可在Program.cs修改

- LINE Bot Webhook功能(施工中):
    - 待完善感測器查詢歷史資料功能，例如: 感測器歷史資料視覺化呈現、監視器影像縮時呈現 
    - 使用需要配合Line Message API綁定webhook，並且有https加密需求

- Scheduled Tasks（定時排程）:
    - 兩項功能的定時頻率都待配合舊方案

- CCTV斷線通知、感測器斷線通知功能(施工中):
    - 待完善通報功能，配合line警報規則文件修改中...
    - 判斷CCTV斷線預計讀取影像系統的圖片時間，因影像系統在十河局內網中，目前本地無法測試，測試功能呼叫十河局網頁API取得對應監視器最新照片

- 地震通報:
    - 將地震資料整合為圖片，並儲存於本地，透過wwwroot公開，讓line發送
