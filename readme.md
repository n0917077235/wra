# Folders
main: 監測網站系統
eform: 維護電子表單

# Setup environment
- Get `appsettings.Development.json` and replace the file in `main\backend\Wra10Core2023`.
  The file in on RD server:
  `\\192.168.10.128\Resource\David\docs\build_wra10\appsettings.Development.json`

  If you do not have access to RD server, ask somemone for this file

- Install node.js v22

# Development and testing (Windows 11)

## First, run backend
- In visual studio, open solution `Wra10Core2024`
- Simply press F5, or Ctrl+F5 (if you want to save power when developing on a laptop)

## Setup python environment (等震度圖)
- Install python 3.12.10
- Go to `main\srec_proj`
- Start a virtual environment and activate it:
```
python -m venv .venv
.venv\Scripts\activate
```

- Install dependencies:
`python -m pip install -r requirements.txt`

- Run: `python srec_interpolate.py sample.txt log_plot`
- Output file is in `main\srec_proj\Output`

## Then, run frontend
- `cd main\frontend`
- Open file `.env`, set `VUE_APP_DEBUG=1`
  If you want to connect to the backend at WRA, set `VUE_APP_DEBUG=0`
- Run `npm run serve`
- Open browser, go to `http://localhost:8080`

# Deploy
* Install python 3.12.10 on server
* Install node.js v22 on server

* Build backend
  - `cd main\backend`
  - Delete all files and directories in `bin\Release`
  - Run `dotnet build -c Release --self-contained -r win-x64`
  - The build result can be found in `main\backend\Wra10Core2023\bin\Release\net6.0-windows10.0.17763.0\win-x64`
  - Copy the result to server: `C:\main\backend`. The folder should contain `Wra10Core2023.exe` 

* Compile frontend
  - In `frontend\.env`, set `VUE_APP_DEBUG=0`. Set `VUE_APP_API_URL_DEBUG` to public url.
  - `cd main\frontend && npm run build`
  - Copy the resulting static files in `dist` to server: `C:\main\backend\Webpage`

* Copy the folder `srec_proj` to server: `C:\main\srec_proj` 

* On the server, `C:\main` should contain 2 folders:
  - `backend`
  - `srec_proj`

* Start backend: `cd C:\main\backend && Wra10Core2023.exe`
* Use IIS reverse proxy to direct HTTP requests to http://localhost:5000
