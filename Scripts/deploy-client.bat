@echo off
echo ================================
echo  Deploy Warnet Billing Client
echo ================================
echo.

set CLIENT_DIR=C:\WarnetBilling\Client
set BUILD_DIR=Client\bin\Release\net8.0-windows\publish

echo [1/4] Building Client...
cd Client
dotnet publish -c Release -r win-x64 --self-contained false
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo [2/4] Creating installation directory...
mkdir "%CLIENT_DIR%" 2>nul

echo.
echo [3/4] Copying files...
xcopy /E /I /Y "%BUILD_DIR%" "%CLIENT_DIR%"
copy config.json "%CLIENT_DIR%\" /Y

echo.
echo [4/4] Setting auto-start...
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "WarnetClient" /t REG_SZ /d "%CLIENT_DIR%\Client.exe" /f

echo.
echo ================================
echo  Deployment Complete!
echo ================================
echo Client installed to: %CLIENT_DIR%
echo Auto-start enabled
echo.
pause