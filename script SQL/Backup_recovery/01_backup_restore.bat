@echo off
setlocal

rem =============================================================
rem YEU CAU 4 - DATA PUMP BACKUP/RESTORE MENU
rem Chay tren may co expdp/impdp va Oracle client/server.
rem Khong hardcode password: nguoi dung nhap khi chay.
rem =============================================================

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set STAMP=%%i

set "DEFAULT_USER=CQ09"
set "DEFAULT_SCHEMA=CQ09"
set "DEFAULT_CONNECT=localhost:1521/XEPDB1"

echo.
echo ===== YEU CAU 4 - BACKUP / RESTORE CQ09 =====
echo 1. Backup schema CQ09 bang expdp
echo 2. Restore schema CQ09 bang impdp
echo.
set /p CHOICE=Chon 1 hoac 2: 

if "%CHOICE%"=="1" goto BACKUP
if "%CHOICE%"=="2" goto RESTORE

echo Lua chon khong hop le.
goto END

:ASK_CONNECT
set "ORACLE_USER=%DEFAULT_USER%"
set "ORACLE_SCHEMA=%DEFAULT_SCHEMA%"
set "ORACLE_CONNECT_SERVICE=%DEFAULT_CONNECT%"

set /p ORACLE_USER=Nhap Oracle user [%DEFAULT_USER%]: 
if "%ORACLE_USER%"=="" set "ORACLE_USER=%DEFAULT_USER%"

set /p ORACLE_PASSWORD=Nhap password cho %ORACLE_USER%: 
if "%ORACLE_PASSWORD%"=="" (
    echo Password khong duoc de trong.
    goto END
)

set /p ORACLE_SCHEMA=Nhap schema [%DEFAULT_SCHEMA%]: 
if "%ORACLE_SCHEMA%"=="" set "ORACLE_SCHEMA=%DEFAULT_SCHEMA%"

set /p ORACLE_CONNECT_SERVICE=Nhap connect string [%DEFAULT_CONNECT%]: 
if "%ORACLE_CONNECT_SERVICE%"=="" set "ORACLE_CONNECT_SERVICE=%DEFAULT_CONNECT%"

set "ORACLE_CONNECT=%ORACLE_USER%/%ORACLE_PASSWORD%@%ORACLE_CONNECT_SERVICE%"
exit /b 0

:BACKUP
call :ASK_CONNECT
set "DUMP_NAME=%ORACLE_SCHEMA%_backup_%STAMP%.dmp"
set "LOG_NAME=%ORACLE_SCHEMA%_backup_%STAMP%.log"

echo.
echo Backup schema %ORACLE_SCHEMA% to DATA_PUMP_DIR/%DUMP_NAME%
expdp "%ORACLE_CONNECT%" schemas=%ORACLE_SCHEMA% directory=DATA_PUMP_DIR dumpfile=%DUMP_NAME% logfile=%LOG_NAME% reuse_dumpfiles=y
goto END

:RESTORE
call :ASK_CONNECT
set /p DUMP_NAME=Nhap ten dump file trong DATA_PUMP_DIR: 
if "%DUMP_NAME%"=="" (
    echo Ten dump file khong duoc de trong.
    goto END
)
set "LOG_NAME=%ORACLE_SCHEMA%_restore_%STAMP%.log"

echo.
echo Restore schema %ORACLE_SCHEMA% from DATA_PUMP_DIR/%DUMP_NAME%
impdp "%ORACLE_CONNECT%" schemas=%ORACLE_SCHEMA% directory=DATA_PUMP_DIR dumpfile=%DUMP_NAME% logfile=%LOG_NAME% table_exists_action=replace
goto END

:END
echo.
echo Hoan tat. Kiem tra file dump/log trong DATA_PUMP_DIR cua Oracle server.
pause
endlocal
