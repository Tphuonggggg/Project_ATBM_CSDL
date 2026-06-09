-- =============================================================
-- YEU CAU 4 - CHUAN BI QUYEN BACKUP/RESTORE CHO DATA PUMP
--
-- Chay bang SYS AS SYSDBA tren PDB XEPDB1 truoc khi chay expdp/impdp.
-- EXEMPT ACCESS POLICY giup Data Pump backup duoc du lieu dang bi VPD/OLS
-- bao ve, tranh canh bao ORA-39181 va nguy co export thieu dong.
-- =============================================================

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
