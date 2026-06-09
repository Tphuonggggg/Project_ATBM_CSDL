-- =============================================================
-- YEU CAU 4 - KIEM TRA AUDIT LOG SAU SU CO
--
-- Chay bang SYS AS SYSDBA hoac CQ09.
-- =============================================================

SET DEFINE OFF;
SET LINESIZE 220;
SET PAGESIZE 100;
COLUMN db_user FORMAT A14;
COLUMN object_name FORMAT A20;
COLUMN policy_name FORMAT A32;
COLUMN audit_time FORMAT A19;
COLUMN sql_text FORMAT A110;

BEGIN
    EXECUTE IMMEDIATE 'ALTER SESSION SET CONTAINER = XEPDB1';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

PROMPT ===== FGA LOG CHO DONTHUOC =====

SELECT db_user,
       object_schema,
       object_name,
       policy_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       sql_text
FROM dba_fga_audit_trail
WHERE object_schema = 'CQ09'
  AND object_name = 'DONTHUOC'
ORDER BY timestamp DESC
FETCH FIRST 20 ROWS ONLY;

PROMPT ===== STANDARD AUDIT LOI NEU CO =====

SELECT username,
       action_name,
       obj_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       returncode
FROM dba_audit_trail
WHERE owner = 'CQ09'
  AND returncode <> 0
ORDER BY timestamp DESC
FETCH FIRST 20 ROWS ONLY;
