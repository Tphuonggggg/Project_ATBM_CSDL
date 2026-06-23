-- =============================================================================
-- FILE: 07_audit_setup.sql
-- ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
-- CHỨC NĂNG:
--   - Cấu hình giải pháp nhật ký hệ thống (Audit) mức độ thường và nâng cao.
--   - Cấp các quyền cần thiết về Audit và FGA cho schema CQ09.
--   - Cấu hình Standard Audit:
--     + Theo dõi thao tác đọc thành công (SELECT SUCCESSFUL) trên bảng BENHNHAN.
--     + Theo dõi thao tác sửa lỗi thất bại (UPDATE UNSUCCESSFUL) trên HSBA, DONTHUOC.
--     + Theo dõi thao tác trên các View nghiệp vụ y tế.
--   - Cấu hình Fine-Grained Audit (FGA) bằng DBMS_FGA.ADD_POLICY để theo dõi
--     sát sao khi các cột nhạy cảm như (CHANDOAN, DIEUTRI, KETLUAN) hay (TENTHUOC, LIEUDUNG) bị chỉnh sửa.
-- TÀI KHOẢN THỰC THI: SYS AS SYSDBA
-- THỨ TỰ THỰC THI: Bước 7 trong chuỗi thiết lập.
-- =============================================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== 0. CHON PDB XEPDB1 VA KIEM TRA AUDIT_TRAIL =====

BEGIN
    EXECUTE IMMEDIATE 'ALTER SESSION SET CONTAINER = XEPDB1';
    DBMS_OUTPUT.PUT_LINE('Current container switched to XEPDB1.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Skip ALTER SESSION SET CONTAINER: ' || SQLERRM);
END;
/

SHOW PARAMETER audit_trail;

PROMPT Neu audit_trail = NONE, chay lenh sau bang SYS AS SYSDBA roi restart Oracle:
PROMPT ALTER SYSTEM SET audit_trail = DB, EXTENDED SCOPE = SPFILE;

PROMPT ===== 1. CAP QUYEN CAN THIET CHO CQ09 =====

BEGIN
    EXECUTE IMMEDIATE 'GRANT AUDIT SYSTEM TO CQ09';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON DBMS_FGA TO CQ09';
    EXECUTE IMMEDIATE 'GRANT SELECT ANY DICTIONARY TO CQ09';
    DBMS_OUTPUT.PUT_LINE('Granted audit/FGA helper privileges to CQ09.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Grant skipped or already granted: ' || SQLERRM);
END;
/

PROMPT ===== 2. TAO PROCEDURE/FUNCTION DEMO DE STANDARD AUDIT EXECUTE =====

CREATE OR REPLACE PROCEDURE CQ09.P_AUDIT_DEMO_MARK(
    p_note IN VARCHAR2 DEFAULT NULL
) AUTHID DEFINER AS
BEGIN
    DBMS_OUTPUT.PUT_LINE('AUDIT DEMO PROCEDURE: ' || NVL(p_note, 'no note'));
END;
/

CREATE OR REPLACE FUNCTION CQ09.F_AUDIT_DEMO_USER
RETURN VARCHAR2
AUTHID DEFINER AS
BEGIN
    RETURN SYS_CONTEXT('USERENV', 'SESSION_USER');
END;
/

BEGIN
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK TO RL_DIEUPHOI';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK TO RL_BACSI';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK TO RL_KYTHUATVIEN';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK TO RL_BENHNHAN';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON CQ09.F_AUDIT_DEMO_USER TO RL_DIEUPHOI';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON CQ09.F_AUDIT_DEMO_USER TO RL_BACSI';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON CQ09.F_AUDIT_DEMO_USER TO RL_KYTHUATVIEN';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON CQ09.F_AUDIT_DEMO_USER TO RL_BENHNHAN';
    DBMS_OUTPUT.PUT_LINE('Granted execute on demo procedure/function to business roles.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Demo execute grants skipped: ' || SQLERRM);
END;
/

PROMPT ===== 3. DON STANDARD AUDIT CU DE SCRIPT CHAY LAI DUOC =====

NOAUDIT ALL ON CQ09.BENHNHAN;
NOAUDIT ALL ON CQ09.HSBA;
NOAUDIT ALL ON CQ09.DONTHUOC;
NOAUDIT ALL ON CQ09.HSBA_DV;
NOAUDIT ALL ON CQ09.VW_BENHNHAN;
NOAUDIT ALL ON CQ09.VW_BACSI_HSBA;
NOAUDIT ALL ON CQ09.VW_BACSI_DONTHUOC;
NOAUDIT ALL ON CQ09.VW_KTV_HSBA_DV;
NOAUDIT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK;
NOAUDIT EXECUTE ON CQ09.F_AUDIT_DEMO_USER;

PROMPT ===== 4. STANDARD AUDIT CHO TABLE, VIEW, PROCEDURE, FUNCTION =====

-- Table: ghi SELECT thanh cong tren BENHNHAN.
AUDIT SELECT ON CQ09.BENHNHAN
BY ACCESS
WHENEVER SUCCESSFUL;

-- Table: ghi UPDATE that bai tren cac bang nhay cam.
AUDIT UPDATE ON CQ09.HSBA
BY ACCESS
WHENEVER NOT SUCCESSFUL;

AUDIT UPDATE ON CQ09.DONTHUOC
BY ACCESS
WHENEVER NOT SUCCESSFUL;

-- Table: ghi INSERT/UPDATE/DELETE that bai tren HSBA_DV.
AUDIT INSERT, UPDATE, DELETE ON CQ09.HSBA_DV
BY ACCESS
WHENEVER NOT SUCCESSFUL;

-- View: ghi thao tac qua cac view nghiep vu cua phan he 2.
AUDIT SELECT, UPDATE ON CQ09.VW_BENHNHAN
BY ACCESS;

AUDIT SELECT, UPDATE ON CQ09.VW_BACSI_HSBA
BY ACCESS;

AUDIT SELECT, INSERT, UPDATE, DELETE ON CQ09.VW_BACSI_DONTHUOC
BY ACCESS;

AUDIT SELECT, UPDATE ON CQ09.VW_KTV_HSBA_DV
BY ACCESS;

-- Procedure/function: ghi EXECUTE tren doi tuong demo.
AUDIT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK
BY ACCESS;

AUDIT EXECUTE ON CQ09.F_AUDIT_DEMO_USER
BY ACCESS;

PROMPT ===== 5. DON FGA POLICY CU =====

DECLARE
    PROCEDURE drop_fga_if_exists(p_object VARCHAR2, p_policy VARCHAR2) IS
    BEGIN
        DBMS_FGA.DROP_POLICY(
            object_schema => 'CQ09',
            object_name   => p_object,
            policy_name   => p_policy
        );
        DBMS_OUTPUT.PUT_LINE('Dropped FGA policy ' || p_policy || ' on ' || p_object);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('No old FGA policy or drop skipped: ' || p_policy || ' - ' || SQLERRM);
    END;
BEGIN
    drop_fga_if_exists('DONTHUOC', 'FGA_CQ09_DONTHUOC_UPDATE');
    drop_fga_if_exists('HSBA',     'FGA_CQ09_HSBA_UPDATE');
END;
/

PROMPT ===== 6. FINE-GRAINED AUDIT CHO CAC COT NHAY CAM =====

BEGIN
    DBMS_FGA.ADD_POLICY(
        object_schema      => 'CQ09',
        object_name        => 'DONTHUOC',
        policy_name        => 'FGA_CQ09_DONTHUOC_UPDATE',
        audit_column       => 'MAHSBA,NGAYDT,TENTHUOC,LIEUDUNG',
        audit_column_opts  => DBMS_FGA.ANY_COLUMNS,
        statement_types    => 'UPDATE',
        audit_trail        => DBMS_FGA.DB + DBMS_FGA.EXTENDED,
        enable             => TRUE
    );
    DBMS_OUTPUT.PUT_LINE('Created FGA_CQ09_DONTHUOC_UPDATE.');
END;
/

BEGIN
    DBMS_FGA.ADD_POLICY(
        object_schema      => 'CQ09',
        object_name        => 'HSBA',
        policy_name        => 'FGA_CQ09_HSBA_UPDATE',
        audit_column       => 'CHANDOAN,DIEUTRI,KETLUAN',
        audit_column_opts  => DBMS_FGA.ANY_COLUMNS,
        statement_types    => 'UPDATE',
        audit_trail        => DBMS_FGA.DB + DBMS_FGA.EXTENDED,
        enable             => TRUE
    );
    DBMS_OUTPUT.PUT_LINE('Created FGA_CQ09_HSBA_UPDATE.');
END;
/

PROMPT ===== 7. KIEM TRA CAU HINH AUDIT/FGA =====

SELECT owner, object_name, object_type, status
FROM dba_objects
WHERE owner = 'CQ09'
  AND object_name IN ('P_AUDIT_DEMO_MARK', 'F_AUDIT_DEMO_USER')
ORDER BY object_name;

PROMPT Standard object audit options:
SELECT owner, object_name, object_type, sel, ins, upd, del, exe
FROM dba_obj_audit_opts
WHERE owner = 'CQ09'
ORDER BY object_name;

PROMPT FGA policies:
SELECT object_schema, object_name, policy_name, policy_text, enabled,
       policy_column, policy_column_options
FROM dba_audit_policies
WHERE object_schema = 'CQ09'
ORDER BY object_name, policy_name;

PROMPT ===== 8. CAU TRUY VAN DOC LOG SAU KHI TEST =====

PROMPT Standard audit:
SELECT username, action_name, owner, obj_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       returncode
FROM dba_audit_trail
WHERE owner = 'CQ09'
ORDER BY timestamp DESC
FETCH FIRST 20 ROWS ONLY;

PROMPT Fine-grained audit:
SELECT db_user, object_schema, object_name, policy_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       sql_text
FROM dba_fga_audit_trail
WHERE object_schema = 'CQ09'
ORDER BY timestamp DESC
FETCH FIRST 20 ROWS ONLY;

PROMPT ===== HOAN TAT YEU CAU 3: AUDIT SETUP =====
