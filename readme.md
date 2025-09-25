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
- Run `npm run serve`
- Open browser, go to `http://localhost:8080/v2`

# Deploy
* Install python 3.12.10 on server
* Install node.js v22 on server

* Build backend
  - `cd main\backend`
  - Delete all files and directories in `bin\Release`
  - In visual studio, right click on project name > Publish
  - Use FolderProfile5.pubxml
  - Configure connection strings in `appsettings.json` as required
  - Copy the build result indicated in Visual Studio output messages, to server:       
    `C:\ancad\backend`. The folder should contain `Wra10Core2023.exe` 

* Compile frontend
  - In `frontend\.env`, set `VUE_APP_API_URL` to the public url of backend.
  - `cd main\frontend && npm run build`
  - Copy the resulting static files in `dist` to server: `C:\ancad\backend\Webpage`

* Copy the folder `srec_proj` to server: `C:\ancad\srec_proj` 

* On the server, `C:\ancad` should contain 2 folders:
  - `backend`
  - `srec_proj`

* Install IIS on server
* Add a website, set the directory to an empty folder
* Right click on the website, select 'Add application'. 
* Set alias to `v2`. Select directory `C:\ancad\backend`
* Site is now online at `http://localhost/v2`

# Test isoseismal map generation
* Open a browser, go to:
  `{VUE_APP_API_URL}/Earthquake/Simulate?action=insert&time=20250917080000&key=9d5i2tzbx3ilrke2it5o6o`

* If simulated data points are generated, you will see a message.

* To clear the simulated data points, go to:
  `{VUE_APP_API_URL}/Earthquake/Simulate?action=delete&time=20250917080000&key=9d5i2tzbx3ilrke2it5o6o`
  
* If data points are deleted, you will see a message.
