-- =============================================================================
-- FILE: 00_run_all.sql
-- ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
-- CHỨC NĂNG:
--   - File master chạy tự động toàn bộ quy trình cấu hình cơ sở dữ liệu y tế.
--   - Chạy lần lượt các bước từ 01 đến 07 theo đúng trình tự để tránh lỗi
--     phụ thuộc dữ liệu và phân quyền.
-- TÀI KHOẢN THỰC THI: SYS AS SYSDBA
-- HƯỚNG DẪN CHẠY: Mở SQL Developer hoặc SQL*Plus bằng SYSDBA và gõ:
--   @00_run_all.sql
-- =============================================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT =========================================================================
PROMPT CHUẨN BỊ THỰC THI TOÀN BỘ SCRIPT CẤU HÌNH CSDL Y TẾ - NHÓM CQ09
PROMPT =========================================================================

-- Tự động chuyển container sang PDB XEPDB1
ALTER SESSION SET CONTAINER = XEPDB1;

PROMPT [1/7] Chạy 01_StoredProcedures.sql (Tạo schema CQ09 & Procedures quản trị)...
@01_StoredProcedures.sql

PROMPT [2/7] Chạy 02_schema_data.sql (Tạo bảng & Dữ liệu mẫu)...
@02_schema_data.sql

PROMPT [3/7] Chạy 03_role.sql (Tạo các Role & Oracle User nghiệp vụ)...
@03_role.sql

PROMPT [4/7] Chạy 04_RBAC.sql (Thiết lập phân quyền RBAC truyền thống qua View)...
@04_RBAC.sql

PROMPT [5/7] Chạy 05_VPD.sql (Áp dụng chính sách bảo mật mức dòng/cột VPD)...
@05_VPD.sql

PROMPT [6/7] Chạy 06_OLS_setup.sql (Thiết lập Oracle Label Security cho THONGBAO)...
@06_OLS_setup.sql

PROMPT [7/7] Chạy 07_audit_setup.sql (Cấu hình Nhật ký hệ thống Standard & FGA)...
@07_audit_setup.sql

PROMPT =========================================================================
PROMPT HỆ THỐNG ĐÃ ĐƯỢC THIẾT LẬP THÀNH CÔNG VỚI ĐẦY ĐỦ VPD, OLS, AUDIT!
PROMPT Bạn có thể khởi chạy ứng dụng WinForms để bắt đầu demo.
PROMPT =========================================================================
