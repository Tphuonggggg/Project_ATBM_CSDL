-- =============================================================
-- YEU CAU 4 - DEMO FLASH RESTORE THU CONG
--
-- Muc dich:
--   1. Gay su co bang app WinForms: dang nhap BS001 va sua sai LIEUDUNG.
--   2. Chay script nay bang CQ09 de doc audit log, preview flashback,
--      va chi restore khi nguoi demo nhap YES.
--
-- Khac voi ban chay lien tuc cu:
--   - Script nay KHONG tu update/gay su co nua.
--   - Su co duoc thao tac tren giao dien app de dung kich ban do an.
--
-- Gioi han heuristic:
--   - RESTORE_TS tu dong = audit timestamp moi nhat cua update demo tru 10 giay.
--   - Tru 10 giay chi la buffer demo, khong phai moc recovery chinh xac
--     neu co nhieu update gan nhau.
--   - Flashback Query phu thuoc UNDO_RETENTION du lon va undo chua bi ghi de.
-- =============================================================

SET DEFINE ON;
SET SERVEROUTPUT ON;
SET VERIFY OFF;
SET LINESIZE 220;
SET PAGESIZE 100;
WHENEVER SQLERROR EXIT SQL.SQLCODE;

DEFINE TARGET_MAHSBA = "HSBA2024001"
DEFINE TARGET_NGAYDT = "2024-01-10"
DEFINE TARGET_TENTHUOC = "Metformin 1000mg"
DEFINE RESTORE_TS_FALLBACK = "2026-06-21 11:00:00"

VARIABLE V_RESTORE_TS VARCHAR2(19);

PROMPT ===== GIAI DOAN 0: GAY SU CO BANG APP =====
PROMPT 1. Mo app WinForms.
PROMPT 2. Dang nhap BS001.
PROMPT 3. Sua sai LIEUDUNG cua don thuoc muc tieu.
PROMPT 4. Quay lai SQL*Plus/SQL Developer va chay tiep script nay bang CQ09.
PROMPT.

CONNECT CQ09/ATBM123@localhost:1521/XEPDB1

PROMPT ===== GIAI DOAN 1: KIEM TRA USER VA DU LIEU HIEN TAI =====
SELECT SYS_CONTEXT('USERENV', 'SESSION_USER') AS session_user,
       SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') AS current_schema,
       SYS_CONTEXT('USERENV', 'CON_NAME') AS con_name
FROM dual;

SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.DONTHUOC
WHERE MAHSBA = '&TARGET_MAHSBA'
  AND NGAYDT = DATE '&TARGET_NGAYDT'
  AND TENTHUOC = N'&TARGET_TENTHUOC';

PROMPT ===== GIAI DOAN 2: DOC AUDIT LOG FGA =====
COLUMN db_user FORMAT A14;
COLUMN object_name FORMAT A20;
COLUMN policy_name FORMAT A32;
COLUMN audit_time FORMAT A19;
COLUMN suggested_restore_ts FORMAT A19;
COLUMN sql_text FORMAT A110;

SELECT db_user,
       object_schema,
       object_name,
       policy_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       TO_CHAR(CAST(timestamp AS TIMESTAMP) - INTERVAL '10' SECOND, 'YYYY-MM-DD HH24:MI:SS') AS suggested_restore_ts,
       sql_text
FROM dba_fga_audit_trail
WHERE object_schema = 'CQ09'
  AND object_name = 'DONTHUOC'
  AND policy_name = 'FGA_CQ09_DONTHUOC_UPDATE'
ORDER BY timestamp DESC
FETCH FIRST 20 ROWS ONLY;

PROMPT ===== GIAI DOAN 3: TU TINH RESTORE_TS =====
DECLARE
    v_ts VARCHAR2(19);
BEGIN
    SELECT TO_CHAR(CAST(MAX(timestamp) AS TIMESTAMP) - INTERVAL '10' SECOND, 'YYYY-MM-DD HH24:MI:SS')
    INTO v_ts
    FROM dba_fga_audit_trail
    WHERE object_schema = 'CQ09'
      AND object_name = 'DONTHUOC'
      AND policy_name = 'FGA_CQ09_DONTHUOC_UPDATE'
      AND db_user = 'BS001';

    :V_RESTORE_TS := NVL(v_ts, '&RESTORE_TS_FALLBACK');
EXCEPTION
    WHEN OTHERS THEN
        :V_RESTORE_TS := '&RESTORE_TS_FALLBACK';
        DBMS_OUTPUT.PUT_LINE('Khong tu lay duoc FGA timestamp, dung fallback: ' || SQLERRM);
END;
/

PRINT V_RESTORE_TS;

PROMPT ===== GIAI DOAN 4: PREVIEW DU LIEU TAI RESTORE_TS =====
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.DONTHUOC AS OF TIMESTAMP TO_TIMESTAMP(:V_RESTORE_TS, 'YYYY-MM-DD HH24:MI:SS')
WHERE MAHSBA = '&TARGET_MAHSBA'
  AND NGAYDT = DATE '&TARGET_NGAYDT'
  AND TENTHUOC = N'&TARGET_TENTHUOC';

PROMPT.
PROMPT Neu preview dung gia tri cu thi nhap YES de restore.
PROMPT Neu chua dung, nhap NO roi chon RESTORE_TS thu cong hoac dung giao dien Recovery trong app.
ACCEPT CONFIRM_RESTORE CHAR PROMPT 'Nhap YES de Flash Restore: '

PROMPT ===== GIAI DOAN 5: FLASH RESTORE CO XAC NHAN =====
UPDATE CQ09.DONTHUOC d
SET d.LIEUDUNG = (
    SELECT old.LIEUDUNG
    FROM CQ09.DONTHUOC AS OF TIMESTAMP TO_TIMESTAMP(:V_RESTORE_TS, 'YYYY-MM-DD HH24:MI:SS') old
    WHERE old.MAHSBA = d.MAHSBA
      AND old.NGAYDT = d.NGAYDT
      AND old.TENTHUOC = d.TENTHUOC
)
WHERE UPPER('&CONFIRM_RESTORE') = 'YES'
  AND d.MAHSBA = '&TARGET_MAHSBA'
  AND d.NGAYDT = DATE '&TARGET_NGAYDT'
  AND d.TENTHUOC = N'&TARGET_TENTHUOC'
  AND EXISTS (
      SELECT 1
      FROM CQ09.DONTHUOC AS OF TIMESTAMP TO_TIMESTAMP(:V_RESTORE_TS, 'YYYY-MM-DD HH24:MI:SS') old
      WHERE old.MAHSBA = d.MAHSBA
        AND old.NGAYDT = d.NGAYDT
        AND old.TENTHUOC = d.TENTHUOC
  );

COMMIT;

PROMPT ===== GIAI DOAN 6: SAU FLASH RESTORE =====
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.DONTHUOC
WHERE MAHSBA = '&TARGET_MAHSBA'
  AND NGAYDT = DATE '&TARGET_NGAYDT'
  AND TENTHUOC = N'&TARGET_TENTHUOC';

PROMPT ===== HOAN TAT DEMO FLASH RESTORE THU CONG =====
