-- =============================================================
-- YEU CAU 4 - SETUP BACKUP/RESTORE VA BACKUP TU DONG
--
-- Chay bang SYS AS SYSDBA tren PDB XEPDB1.
-- Script nay:
--   1. Cap quyen Data Pump/Flashback can thiet cho CQ09.
--   2. Kiem tra DATA_PUMP_DIR ton tai trong PDB XEPDB1 roi grant READ/WRITE.
--   3. Tao CQ09.BACKUP_LOG.
--   4. Tao procedure CQ09.PRC_AUTO_EXPORT_SCHEMA dung DBMS_DATAPUMP.
--   5. Tao DBMS_SCHEDULER job chay export schema CQ09 hang ngay.
-- =============================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;
SET LINESIZE 220;
SET PAGESIZE 100;

BEGIN
    EXECUTE IMMEDIATE 'ALTER SESSION SET CONTAINER = XEPDB1';
EXCEPTION
    WHEN OTHERS THEN
        NULL;
END;
/

PROMPT ===== 1. KIEM TRA PDB VA DATA_PUMP_DIR =====

SELECT SYS_CONTEXT('USERENV', 'SESSION_USER') AS session_user,
       SYS_CONTEXT('USERENV', 'CON_NAME') AS con_name
FROM dual;

DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM dba_directories
    WHERE directory_name = 'DATA_PUMP_DIR';

    IF v_count = 0 THEN
        RAISE_APPLICATION_ERROR(
            -20000,
            'DATA_PUMP_DIR khong ton tai trong PDB ' ||
            SYS_CONTEXT('USERENV', 'CON_NAME') ||
            '. Hay tao/kiem tra directory truoc khi grant.'
        );
    END IF;

    DBMS_OUTPUT.PUT_LINE('DATA_PUMP_DIR exists in ' || SYS_CONTEXT('USERENV', 'CON_NAME') || '.');
END;
/

PROMPT ===== 2. CAP QUYEN BACKUP/RESTORE CHO CQ09 =====

BEGIN
    EXECUTE IMMEDIATE 'GRANT DATAPUMP_EXP_FULL_DATABASE TO CQ09';
    EXECUTE IMMEDIATE 'GRANT DATAPUMP_IMP_FULL_DATABASE TO CQ09';
    EXECUTE IMMEDIATE 'GRANT EXEMPT ACCESS POLICY TO CQ09';
    EXECUTE IMMEDIATE 'GRANT CREATE JOB TO CQ09';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON SYS.DBMS_DATAPUMP TO CQ09';
    EXECUTE IMMEDIATE 'GRANT READ, WRITE ON DIRECTORY DATA_PUMP_DIR TO CQ09';
    DBMS_OUTPUT.PUT_LINE('Granted Data Pump, scheduler, EXEMPT ACCESS POLICY, and DATA_PUMP_DIR privileges to CQ09.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Grant skipped or failed: ' || SQLERRM);
        RAISE;
END;
/

PROMPT ===== 3. TAO BANG CQ09.BACKUP_LOG =====

BEGIN
    EXECUTE IMMEDIATE '
        CREATE TABLE CQ09.BACKUP_LOG (
            run_time   TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            dump_file  VARCHAR2(128),
            status     VARCHAR2(30) NOT NULL,
            error_msg  VARCHAR2(4000)
        )';
    DBMS_OUTPUT.PUT_LINE('Created CQ09.BACKUP_LOG.');
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN
            DBMS_OUTPUT.PUT_LINE('CQ09.BACKUP_LOG already exists.');
        ELSE
            RAISE;
        END IF;
END;
/

PROMPT ===== 4. TAO PROCEDURE AUTO EXPORT =====

CREATE OR REPLACE PROCEDURE CQ09.PRC_AUTO_EXPORT_SCHEMA
AUTHID DEFINER
AS
    v_handle    NUMBER;
    v_job_state VARCHAR2(30);
    v_stamp     VARCHAR2(15) := TO_CHAR(SYSTIMESTAMP, 'YYYYMMDD_HH24MISS');
    v_dump_file VARCHAR2(128) := 'CQ09_auto_' || v_stamp || '.dmp';
    v_log_file  VARCHAR2(128) := 'CQ09_auto_' || v_stamp || '.log';
    v_logged_failure BOOLEAN := FALSE;

    PROCEDURE write_log(p_status VARCHAR2, p_error_msg VARCHAR2) IS
        PRAGMA AUTONOMOUS_TRANSACTION;
    BEGIN
        INSERT INTO CQ09.BACKUP_LOG(run_time, dump_file, status, error_msg)
        VALUES (
            SYSTIMESTAMP,
            v_dump_file,
            SUBSTR(p_status, 1, 30),
            SUBSTR(p_error_msg, 1, 4000)
        );
        COMMIT;
    END write_log;
BEGIN
    v_handle := DBMS_DATAPUMP.OPEN(
        operation => 'EXPORT',
        job_mode  => 'SCHEMA',
        job_name  => 'CQ09_EXP_' || v_stamp,
        version   => 'COMPATIBLE'
    );

    DBMS_DATAPUMP.ADD_FILE(
        handle    => v_handle,
        filename  => v_dump_file,
        directory => 'DATA_PUMP_DIR',
        filetype  => DBMS_DATAPUMP.KU$_FILE_TYPE_DUMP_FILE,
        reusefile => 1
    );

    DBMS_DATAPUMP.ADD_FILE(
        handle    => v_handle,
        filename  => v_log_file,
        directory => 'DATA_PUMP_DIR',
        filetype  => DBMS_DATAPUMP.KU$_FILE_TYPE_LOG_FILE,
        reusefile => 1
    );

    DBMS_DATAPUMP.METADATA_FILTER(
        handle => v_handle,
        name   => 'SCHEMA_EXPR',
        value  => 'IN (''CQ09'')'
    );

    DBMS_DATAPUMP.START_JOB(v_handle);

    -- WAIT_FOR_JOB giu procedure cho den khi export that su ket thuc.
    -- Neu bo qua buoc nay, DBMS_SCHEDULER co the bao SUCCEEDED trong khi
    -- Data Pump worker van dang chay nen log scheduler khong phan anh dung.
    DBMS_DATAPUMP.WAIT_FOR_JOB(v_handle, v_job_state);

    IF v_job_state = 'COMPLETED' THEN
        write_log('SUCCESS', NULL);
    ELSE
        write_log(v_job_state, 'Data Pump job ended with state: ' || v_job_state);
        v_logged_failure := TRUE;
        RAISE_APPLICATION_ERROR(-20001, 'Data Pump job ended with state: ' || v_job_state);
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        BEGIN
            IF v_handle IS NOT NULL THEN
                DBMS_DATAPUMP.DETACH(v_handle);
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                NULL;
        END;

        IF NOT v_logged_failure THEN
            write_log('ERROR', SQLERRM);
        END IF;
        RAISE;
END;
/

SHOW ERRORS PROCEDURE CQ09.PRC_AUTO_EXPORT_SCHEMA;

PROMPT ===== 5. TAO DBMS_SCHEDULER JOB =====

BEGIN
    DBMS_SCHEDULER.DROP_JOB(
        job_name => 'CQ09.JOB_AUTO_EXPORT_SCHEMA',
        force    => TRUE
    );
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -27475 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    DBMS_SCHEDULER.CREATE_JOB(
        job_name        => 'CQ09.JOB_AUTO_EXPORT_SCHEMA',
        job_type        => 'STORED_PROCEDURE',
        job_action      => 'CQ09.PRC_AUTO_EXPORT_SCHEMA',
        start_date      => SYSTIMESTAMP,
        repeat_interval => 'FREQ=DAILY;BYHOUR=2;BYMINUTE=0;BYSECOND=0',
        enabled         => TRUE,
        comments        => 'Automatic daily Data Pump export for schema CQ09'
    );
    DBMS_OUTPUT.PUT_LINE('Created CQ09.JOB_AUTO_EXPORT_SCHEMA.');
END;
/

PROMPT ===== 6. KIEM TRA SAU SETUP =====

PROMPT System privileges:
SELECT privilege
FROM dba_sys_privs
WHERE grantee = 'CQ09'
  AND privilege IN ('EXEMPT ACCESS POLICY', 'CREATE JOB')
ORDER BY privilege;

PROMPT Granted roles:
SELECT granted_role
FROM dba_role_privs
WHERE grantee = 'CQ09'
  AND granted_role IN ('DATAPUMP_EXP_FULL_DATABASE', 'DATAPUMP_IMP_FULL_DATABASE')
ORDER BY granted_role;

PROMPT Directory privileges:
SELECT grantee, table_name AS directory_name, privilege
FROM dba_tab_privs
WHERE grantee = 'CQ09'
  AND table_name = 'DATA_PUMP_DIR'
ORDER BY privilege;

PROMPT Package privileges:
SELECT grantee, owner, table_name AS object_name, privilege
FROM dba_tab_privs
WHERE grantee = 'CQ09'
  AND owner = 'SYS'
  AND table_name = 'DBMS_DATAPUMP'
ORDER BY privilege;

PROMPT Backup log table:
SELECT owner, table_name
FROM dba_tables
WHERE owner = 'CQ09'
  AND table_name = 'BACKUP_LOG';

PROMPT Scheduler job:
SELECT owner, job_name, enabled, state, repeat_interval
FROM dba_scheduler_jobs
WHERE owner = 'CQ09'
  AND job_name = 'JOB_AUTO_EXPORT_SCHEMA';

PROMPT Recent automatic backup log:
SELECT TO_CHAR(run_time, 'YYYY-MM-DD HH24:MI:SS') AS run_time,
       dump_file,
       status,
       error_msg
FROM CQ09.BACKUP_LOG
ORDER BY run_time DESC
FETCH FIRST 10 ROWS ONLY;

PROMPT ===== HOAN TAT SETUP YEU CAU 4 =====
