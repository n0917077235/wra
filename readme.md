# Folders
main: 監測網站系統
eform: 維護電子表單

# Setup environment
- Get `appsettings.Development.json` and replace the file in `main\backend\Wra10Core2023`.
  The file in on RD server:
  `\\192.168.10.128\Resource\David\docs\build_wra10\appsettings.Development.json`

  If you do not have access to RD server, ask somemone for this file

- Install node.js v22

# Development and testing

## First, run backend
- In visual studio, open solution `Wra10Core2024`
- Simply press F5 in visual studio

## Then, run frontend
- `cd main\frontend`
- Open file `.env`, set `VUE_APP_DEBUG=1`
  If you want to connect to the backend at WRA, set `VUE_APP_DEBUG=0`
- Run `npm run serve`
- Open browser, go to `http://localhost:8080`

# Deploy

## For backend
- `cd main\backend`
- Delete all files and directories in `bin\Release`
- Run `dotnet build -c Release --self-contained -r win-x64`
- The build result can be found in `main\backend\Wra10Core2023\bin\Release`
- Copy the result to VM

## Frontend
In `main\frontend\.env`, set `VUE_APP_DEBUG=0`