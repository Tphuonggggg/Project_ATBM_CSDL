@echo off
setlocal

rem Restore schema CQ09 tu file dump Data Pump.
rem Nhap ten file dump da tao trong DATA_PUMP_DIR, vi du CQ09_backup_20260609_103000.dmp

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set STAMP=%%i

set ORACLE_CONNECT=CQ09/ATBM123@localhost:1521/XEPDB1
set ORACLE_SCHEMA=CQ09
set /p DUMP_NAME=Nhap ten dump file trong DATA_PUMP_DIR: 
set LOG_NAME=CQ09_restore_%STAMP%.log

echo Restore schema %ORACLE_SCHEMA% from DATA_PUMP_DIR/%DUMP_NAME%
impdp %ORACLE_CONNECT% schemas=%ORACLE_SCHEMA% directory=DATA_PUMP_DIR dumpfile=%DUMP_NAME% logfile=%LOG_NAME% table_exists_action=replace

echo.
echo Done. Kiem tra restore log trong DATA_PUMP_DIR cua Oracle server.
pause
