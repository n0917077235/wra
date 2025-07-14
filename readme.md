# Folders
main: 監測網站系統

# Test main site on VM

## First, run backend
- `cd main\backend`
- Run `call dotnet build -c Release --self-contained -r win-x64`
- Copy the build result, which can be found in `main\backend\Wra10Core2023\bin\Release`, to the VM
- Run Wra10Core2023.exe on VM

## Then, run frontend
- Install node.js v22 on VM
- Zip folder `main\frontend`, except `node_modules`
- Copy the zip file to VM via RDP
- Unzip on VM, run `npm install`
- Open file `.env`, change first line to `VUE_APP_API_URL= http://localhost:5000/api`
- Run `npm run serve`
