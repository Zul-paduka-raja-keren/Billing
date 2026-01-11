@echo off
set DB_PATH=C:\WarnetBilling\Server\Data\billing.db
set BACKUP_DIR=C:\WarnetBilling\Server\Data\Backups
set TIMESTAMP=%date:~-4%%date:~3,2%%date:~0,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%

mkdir "%BACKUP_DIR%" 2>nul

echo Backing up database...
copy "%DB_PATH%" "%BACKUP_DIR%\billing_%TIMESTAMP%.db"

if %errorlevel% equ 0 (
    echo Backup successful: %BACKUP_DIR%\billing_%TIMESTAMP%.db
) else (
    echo Backup failed!
)

pause