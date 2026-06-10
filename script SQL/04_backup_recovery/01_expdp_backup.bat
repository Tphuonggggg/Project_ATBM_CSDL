@echo off
setlocal

rem Backup schema CQ09 bang Oracle Data Pump.
rem Chay file nay trong CMD/PowerShell tren may co expdp va Oracle client/server.

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set STAMP=%%i

set ORACLE_CONNECT=CQ09/ATBM123@localhost:1521/XEPDB1
set ORACLE_SCHEMA=CQ09
set DUMP_NAME=CQ09_backup_%STAMP%.dmp
set LOG_NAME=CQ09_backup_%STAMP%.log

echo Backup schema %ORACLE_SCHEMA% to DATA_PUMP_DIR/%DUMP_NAME%
expdp %ORACLE_CONNECT% schemas=%ORACLE_SCHEMA% directory=DATA_PUMP_DIR dumpfile=%DUMP_NAME% logfile=%LOG_NAME% reuse_dumpfiles=y

echo.
echo Done. Kiem tra file dump/log trong DATA_PUMP_DIR cua Oracle server.
pause
