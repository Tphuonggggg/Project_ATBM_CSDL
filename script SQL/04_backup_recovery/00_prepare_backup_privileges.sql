-- =============================================================================
-- FILE: 00_prepare_backup_privileges.sql
-- ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
-- CHỨC NĂNG:
--   - Cấp các quyền cần thiết để sao lưu dữ liệu cho schema CQ09:
--     + Quyền backup/restore qua Data Pump (DATAPUMP_EXP_FULL_DATABASE, DATAPUMP_IMP_FULL_DATABASE).
--     + Quyền EXEMPT ACCESS POLICY để Data Pump bypass qua VPD/OLS, tránh lỗi
--       export thiếu dòng hoặc cảnh báo ORA-39181.
-- TÀI KHOẢN THỰC THI: SYS AS SYSDBA
-- THỨ TỰ THỰC THI: Chạy trước khi thực hiện export/import Data Pump (Bước 1).
-- =============================================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

BEGIN
    EXECUTE IMMEDIATE 'ALTER SESSION SET CONTAINER = XEPDB1';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'GRANT DATAPUMP_EXP_FULL_DATABASE TO CQ09';
    EXECUTE IMMEDIATE 'GRANT DATAPUMP_IMP_FULL_DATABASE TO CQ09';
    EXECUTE IMMEDIATE 'GRANT EXEMPT ACCESS POLICY TO CQ09';
    DBMS_OUTPUT.PUT_LINE('Granted Data Pump and EXEMPT ACCESS POLICY privileges to CQ09.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Grant skipped or failed: ' || SQLERRM);
END;
/

SELECT privilege
FROM dba_sys_privs
WHERE grantee = 'CQ09'
  AND privilege = 'EXEMPT ACCESS POLICY';

SELECT granted_role
FROM dba_role_privs
WHERE grantee = 'CQ09'
  AND granted_role IN ('DATAPUMP_EXP_FULL_DATABASE', 'DATAPUMP_IMP_FULL_DATABASE')
ORDER BY granted_role;
