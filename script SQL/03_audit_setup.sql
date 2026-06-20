-- =============================================================
-- CSC12001 - AN TOAN BAO MAT DU LIEU TRONG HTTT
-- PHAN HE 2 - YEU CAU 3: KIEM TOAN / AUDIT
-- File: 03_audit_setup.sql
--
-- Oracle 21c Unified Auditing version.
--
-- Chay bang SYS AS SYSDBA tren PDB XEPDB1 sau khi da chay:
--   StoredProcedures.sql, schema_data.sql, role.sql, RBAC.sql, VPD.sql
--
-- Muc tieu:
--   1. Tao Unified Audit Policy cho Table, View, Procedure, Function.
--   2. Cau hinh chinh sach theo BY user cho cac user demo:
--      BS001, BS002, KTV01, KTV02, BN000001.
--   3. Audit rieng thao tac thanh cong va that bai.
--   4. Giu Fine-Grained Audit (FGA) cho cac cot nghiep vu nhay cam.
--
-- Ghi chu Oracle 21c:
--   - Unified audit records duoc doc tu UNIFIED_AUDIT_TRAIL.
--   - Pure Unified Auditing: FGA records nam trong UNIFIED_AUDIT_TRAIL.
--   - Mixed mode: FGA records nam trong DBA_FGA_AUDIT_TRAIL.
-- =============================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== 0. CHON PDB XEPDB1 VA KIEM TRA UNIFIED AUDITING =====

BEGIN
    EXECUTE IMMEDIATE 'ALTER SESSION SET CONTAINER = XEPDB1';
    DBMS_OUTPUT.PUT_LINE('Current container switched to XEPDB1.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Skip ALTER SESSION SET CONTAINER: ' || SQLERRM);
END;
/

PROMPT Unified Auditing option:
SELECT parameter, value
FROM v$option
WHERE parameter = 'Unified Auditing';

PROMPT ===== 1. CAP QUYEN CAN THIET CHO CQ09 =====

DECLARE
    PROCEDURE grant_if_possible(p_sql VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE p_sql;
        DBMS_OUTPUT.PUT_LINE('OK: ' || p_sql);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Grant skipped: ' || p_sql || ' - ' || SQLERRM);
    END;
BEGIN
    grant_if_possible('GRANT AUDIT SYSTEM TO CQ09');
    grant_if_possible('GRANT AUDIT_ADMIN TO CQ09');
    grant_if_possible('GRANT AUDIT_VIEWER TO CQ09');
    grant_if_possible('GRANT EXECUTE ON DBMS_FGA TO CQ09');
    grant_if_possible('GRANT SELECT ANY DICTIONARY TO CQ09');
END;
/

PROMPT ===== 2. TAO PROCEDURE/FUNCTION DEMO DE AUDIT EXECUTE =====

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

DECLARE
    PROCEDURE grant_if_possible(p_sql VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE p_sql;
        DBMS_OUTPUT.PUT_LINE('OK: ' || p_sql);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Grant skipped: ' || p_sql || ' - ' || SQLERRM);
    END;
BEGIN
    grant_if_possible('GRANT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK TO RL_DIEUPHOI');
    grant_if_possible('GRANT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK TO RL_BACSI');
    grant_if_possible('GRANT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK TO RL_KYTHUATVIEN');
    grant_if_possible('GRANT EXECUTE ON CQ09.P_AUDIT_DEMO_MARK TO RL_BENHNHAN');
    grant_if_possible('GRANT EXECUTE ON CQ09.F_AUDIT_DEMO_USER TO RL_DIEUPHOI');
    grant_if_possible('GRANT EXECUTE ON CQ09.F_AUDIT_DEMO_USER TO RL_BACSI');
    grant_if_possible('GRANT EXECUTE ON CQ09.F_AUDIT_DEMO_USER TO RL_KYTHUATVIEN');
    grant_if_possible('GRANT EXECUTE ON CQ09.F_AUDIT_DEMO_USER TO RL_BENHNHAN');
END;
/

PROMPT ===== 3. DON UNIFIED AUDIT POLICY CU DE SCRIPT CHAY LAI DUOC =====

DECLARE
    c_users CONSTANT VARCHAR2(200) := 'BS001, BS002, KTV01, KTV02, BN000001';

    PROCEDURE run_ddl(p_sql VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE p_sql;
        DBMS_OUTPUT.PUT_LINE('OK: ' || p_sql);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Skip: ' || p_sql || ' - ' || SQLERRM);
    END;

    PROCEDURE drop_policy_if_exists(p_policy VARCHAR2) IS
    BEGIN
        run_ddl('NOAUDIT POLICY ' || p_policy || ' BY ' || c_users);
        run_ddl('NOAUDIT POLICY ' || p_policy);
        run_ddl('DROP AUDIT POLICY ' || p_policy);
    END;
BEGIN
    drop_policy_if_exists('UA_CQ09_TABLE_SUCCESS');
    drop_policy_if_exists('UA_CQ09_VIEW_SUCCESS');
    drop_policy_if_exists('UA_CQ09_PROC_EXEC');
    drop_policy_if_exists('UA_CQ09_FUNC_EXEC');
    drop_policy_if_exists('UA_CQ09_FAILED_ATTEMPTS');
END;
/

PROMPT ===== 4. TAO 5 UNIFIED AUDIT POLICY =====

CREATE AUDIT POLICY UA_CQ09_TABLE_SUCCESS
    ACTIONS
        SELECT ON CQ09.BENHNHAN,
        UPDATE ON CQ09.BENHNHAN,
        SELECT ON CQ09.HSBA,
        UPDATE ON CQ09.HSBA,
        SELECT ON CQ09.DONTHUOC,
        INSERT ON CQ09.DONTHUOC,
        UPDATE ON CQ09.DONTHUOC,
        DELETE ON CQ09.DONTHUOC,
        SELECT ON CQ09.HSBA_DV,
        INSERT ON CQ09.HSBA_DV,
        UPDATE ON CQ09.HSBA_DV,
        DELETE ON CQ09.HSBA_DV;

CREATE AUDIT POLICY UA_CQ09_VIEW_SUCCESS
    ACTIONS
        SELECT ON CQ09.VW_BENHNHAN,
        UPDATE ON CQ09.VW_BENHNHAN,
        SELECT ON CQ09.VW_BACSI_HSBA,
        UPDATE ON CQ09.VW_BACSI_HSBA,
        SELECT ON CQ09.VW_BACSI_DONTHUOC,
        INSERT ON CQ09.VW_BACSI_DONTHUOC,
        UPDATE ON CQ09.VW_BACSI_DONTHUOC,
        DELETE ON CQ09.VW_BACSI_DONTHUOC,
        SELECT ON CQ09.VW_KTV_HSBA_DV,
        UPDATE ON CQ09.VW_KTV_HSBA_DV;

CREATE AUDIT POLICY UA_CQ09_PROC_EXEC
    ACTIONS EXECUTE ON CQ09.P_AUDIT_DEMO_MARK;

CREATE AUDIT POLICY UA_CQ09_FUNC_EXEC
    ACTIONS EXECUTE ON CQ09.F_AUDIT_DEMO_USER;

CREATE AUDIT POLICY UA_CQ09_FAILED_ATTEMPTS
    ACTIONS
        SELECT ON CQ09.BENHNHAN,
        UPDATE ON CQ09.BENHNHAN,
        SELECT ON CQ09.HSBA,
        UPDATE ON CQ09.HSBA,
        DELETE ON CQ09.HSBA,
        SELECT ON CQ09.DONTHUOC,
        INSERT ON CQ09.DONTHUOC,
        UPDATE ON CQ09.DONTHUOC,
        DELETE ON CQ09.DONTHUOC,
        SELECT ON CQ09.HSBA_DV,
        INSERT ON CQ09.HSBA_DV,
        UPDATE ON CQ09.HSBA_DV,
        DELETE ON CQ09.HSBA_DV,
        SELECT ON CQ09.VW_BENHNHAN,
        UPDATE ON CQ09.VW_BENHNHAN,
        SELECT ON CQ09.VW_BACSI_HSBA,
        UPDATE ON CQ09.VW_BACSI_HSBA,
        SELECT ON CQ09.VW_BACSI_DONTHUOC,
        INSERT ON CQ09.VW_BACSI_DONTHUOC,
        UPDATE ON CQ09.VW_BACSI_DONTHUOC,
        DELETE ON CQ09.VW_BACSI_DONTHUOC,
        SELECT ON CQ09.VW_KTV_HSBA_DV,
        UPDATE ON CQ09.VW_KTV_HSBA_DV;

PROMPT ===== 5. BAT POLICY THEO BY USER VA THANH CONG/THAT BAI =====

AUDIT POLICY UA_CQ09_TABLE_SUCCESS
    BY BS001, BS002, KTV01, KTV02, BN000001
    WHENEVER SUCCESSFUL;

AUDIT POLICY UA_CQ09_VIEW_SUCCESS
    BY BS001, BS002, KTV01, KTV02, BN000001
    WHENEVER SUCCESSFUL;

AUDIT POLICY UA_CQ09_PROC_EXEC
    BY BS001, BS002, KTV01, KTV02, BN000001
    WHENEVER SUCCESSFUL;

AUDIT POLICY UA_CQ09_FUNC_EXEC
    BY BS001, BS002, KTV01, KTV02, BN000001
    WHENEVER SUCCESSFUL;

AUDIT POLICY UA_CQ09_FAILED_ATTEMPTS
    BY BS001, BS002, KTV01, KTV02, BN000001
    WHENEVER NOT SUCCESSFUL;

PROMPT ===== 6. DON FGA POLICY CU =====

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

PROMPT ===== 7. FINE-GRAINED AUDIT CHO CAC COT NHAY CAM =====

-- Oracle 21c audit mode note:
--   - Pure Unified Auditing: FGA records nam trong UNIFIED_AUDIT_TRAIL
--     voi AUDIT_TYPE = 'FineGrainedAudit' va FGA_POLICY_NAME.
--   - Mixed mode (v$option 'Unified Auditing' = FALSE): FGA records nam trong
--     DBA_FGA_AUDIT_TRAIL. DBMS_FGA.DB + DBMS_FGA.EXTENDED giup ghi SQL_TEXT.

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

PROMPT ===== 8. KIEM TRA CAU HINH UNIFIED AUDIT/FGA =====

PROMPT Demo procedure/function:
SELECT owner, object_name, object_type, status
FROM dba_objects
WHERE owner = 'CQ09'
  AND object_name IN ('P_AUDIT_DEMO_MARK', 'F_AUDIT_DEMO_USER')
ORDER BY object_name;

PROMPT Unified audit policies:
SELECT policy_name, audit_option, object_schema, object_name, object_type
FROM audit_unified_policies
WHERE policy_name LIKE 'UA_CQ09_%'
ORDER BY policy_name, object_schema, object_name, audit_option;

PROMPT Enabled unified audit policies:
SELECT policy_name, enabled_option, entity_name, success, failure
FROM audit_unified_enabled_policies
WHERE policy_name LIKE 'UA_CQ09_%'
ORDER BY policy_name, entity_name;

PROMPT FGA policies:
SELECT object_schema, object_name, policy_name, policy_text, enabled,
       policy_column, policy_column_options
FROM dba_audit_policies
WHERE object_schema = 'CQ09'
  AND policy_name LIKE 'FGA_CQ09_%'
ORDER BY object_name, policy_name;

PROMPT ===== HOAN TAT YEU CAU 3: UNIFIED AUDIT SETUP =====
