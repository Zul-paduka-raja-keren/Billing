@echo off
echo ================================
echo  Deploy Warnet Billing Server
echo ================================
echo.

REM Configuration
set SERVER_DIR=C:\WarnetBilling\Server
set BUILD_DIR=Server\bin\Release\net8.0-windows\publish

echo [1/4] Building Server...
cd Server
dotnet publish -c Release -r win-x64 --self-contained false
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo [2/4] Creating installation directory...
mkdir "%SERVER_DIR%" 2>nul

echo.
echo [3/4] Copying files...
xcopy /E /I /Y "%BUILD_DIR%" "%SERVER_DIR%"
copy config.json "%SERVER_DIR%\" /Y

echo.
echo [4/4] Creating desktop shortcut...
powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%USERPROFILE%\Desktop\Billing Server.lnk'); $s.TargetPath = '%SERVER_DIR%\Server.exe'; $s.WorkingDirectory = '%SERVER_DIR%'; $s.IconLocation = '%SERVER_DIR%\Server.exe,0'; $s.Save()"

echo.
echo ================================
echo  Deployment Complete!
echo ================================
echo Server installed to: %SERVER_DIR%
echo Desktop shortcut created
echo.
pause