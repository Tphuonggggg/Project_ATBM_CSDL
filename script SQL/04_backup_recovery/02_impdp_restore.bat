@echo off
rem ============================================================================
rem FILE: 02_impdp_restore.bat
rem ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
rem CHỨC NĂNG:
rem   - Phục hồi (Import) lại schema CQ09 từ file dump đã backup trước đó sử dụng
rem     Oracle Data Pump (impdp).
rem   - Chế độ ghi đè bảng cũ nếu đã tồn tại (table_exists_action=replace).
rem HƯỚNG DẪN CHẠY: Chạy trực tiếp trong môi trường CMD/PowerShell trên máy Oracle Server.
rem   Yêu cầu nhập tên file dump tương ứng (ví dụ: CQ09_backup_20260609_103000.dmp).
rem THỨ TỰ THỰC THI: Sử dụng khi cần khôi phục lại toàn bộ dữ liệu từ file dump backup.
rem ============================================================================
setlocal

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
