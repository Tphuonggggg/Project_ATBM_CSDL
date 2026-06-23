-- =============================================================================
-- FILE: 09_audit_read_logs.sql
-- ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
-- CHỨC NĂNG:
--   - Truy vấn và hiển thị nhật ký kiểm toán hệ thống (Audit Logs) đã ghi nhận.
--   - Đọc Standard Audit từ bảng dba_audit_trail (các hành vi truy vấn và các hành vi lỗi).
--   - Đọc Fine-Grained Audit từ bảng dba_fga_audit_trail (các câu lệnh SQL chi tiết
--     can thiệp vào các cột nhạy cảm).
--   - Phân tích các lệnh lỗi quan trọng (Returncode khác 0) để phát hiện tấn công/truy cập trái phép.
-- TÀI KHOẢN THỰC THI: CQ09 (Quản trị viên dự án)
-- THỨ TỰ THỰC THI: Chạy sau khi test các thao tác nghiệp vụ để kiểm tra log (Bước 9).
-- =============================================================================

SET DEFINE OFF;
SET LINESIZE 220;
SET PAGESIZE 200;
COLUMN username FORMAT A14;
COLUMN db_user FORMAT A14;
COLUMN action_name FORMAT A16;
COLUMN obj_name FORMAT A24;
COLUMN object_name FORMAT A24;
COLUMN policy_name FORMAT A32;
COLUMN audit_time FORMAT A19;
COLUMN returncode FORMAT 999999;
COLUMN sql_text FORMAT A90;

BEGIN
    EXECUTE IMMEDIATE 'ALTER SESSION SET CONTAINER = XEPDB1';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

PROMPT ===== STANDARD AUDIT - DBA_AUDIT_TRAIL =====

SELECT username,
       action_name,
       owner,
       obj_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       returncode
FROM dba_audit_trail
WHERE owner = 'CQ09'
   OR obj_name IN (
        'BENHNHAN', 'HSBA', 'DONTHUOC', 'HSBA_DV',
        'VW_BENHNHAN', 'VW_BACSI_HSBA', 'VW_BACSI_DONTHUOC',
        'VW_KTV_HSBA_DV', 'P_AUDIT_DEMO_MARK', 'F_AUDIT_DEMO_USER'
   )
ORDER BY timestamp DESC
FETCH FIRST 50 ROWS ONLY;

PROMPT ===== FINE-GRAINED AUDIT - DBA_FGA_AUDIT_TRAIL =====

SELECT db_user,
       object_schema,
       object_name,
       policy_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       sql_text
FROM dba_fga_audit_trail
WHERE object_schema = 'CQ09'
  AND object_name IN ('HSBA', 'DONTHUOC')
ORDER BY timestamp DESC
FETCH FIRST 50 ROWS ONLY;

PROMPT ===== CAC LENH LOI QUAN TRONG =====
PROMPT RETURNCODE = 0 la thanh cong; khac 0 la that bai.

SELECT username,
       action_name,
       obj_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       returncode
FROM dba_audit_trail
WHERE owner = 'CQ09'
  AND returncode <> 0
ORDER BY timestamp DESC
FETCH FIRST 30 ROWS ONLY;
