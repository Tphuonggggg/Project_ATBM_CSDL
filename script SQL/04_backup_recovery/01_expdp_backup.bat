@echo off
rem ============================================================================
rem FILE: 01_expdp_backup.bat
rem ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
rem CHỨC NĂNG:
rem   - Sao lưu (Export) toàn bộ schema CQ09 ra file dump sử dụng Oracle Data Pump (expdp).
rem   - File backup được đặt tên tự động kèm theo ngày giờ hệ thống.
rem HƯỚNG DẪN CHẠY: Chạy trực tiếp trong môi trường CMD/PowerShell trên máy Oracle Server.
rem THỨ TỰ THỰC THI: Chạy sau khi đã chuẩn bị quyền ở bước 00 (Bước 2).
rem ============================================================================
setlocal

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
