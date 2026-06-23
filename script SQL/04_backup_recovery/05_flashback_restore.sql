-- =============================================================================
-- FILE: 05_flashback_restore.sql
-- ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
-- CHỨC NĂNG:
--   - Khôi phục nhanh dữ liệu về trạng thái trước sự cố sử dụng Flashback Query.
--   - Cơ chế tự động:
--     + Tự quét tìm log FGA mới nhất liên quan đến từ khóa 'SU CO DEMO'.
--     + Tự xác định mốc thời gian an toàn (lấy mốc sự cố trừ đi 10 giây).
--     + Đọc dữ liệu cũ tại mốc thời gian đó (AS OF TIMESTAMP) và ghi đè khôi
--       phục lại cột LIEUDUNG của bảng CQ09.DONTHUOC.
--   - Cơ chế thủ công: Nếu không tìm thấy log FGA, cho phép cấu hình tham số
--     RESTORE_TS thủ công để khôi phục.
-- TÀI KHOẢN THỰC THI: CQ09 (Quản trị viên dự án)
-- THỨ TỰ THỰC THI: Chạy sau khi đã xác định được mốc thời gian sự cố (Bước 5).
-- =============================================================================

SET DEFINE ON;
SET SERVEROUTPUT ON;
SET VERIFY OFF;
SET LINESIZE 220;

DEFINE RESTORE_TS = "2026-06-09 10:29:00"
DEFINE TARGET_MAHSBA = "HSBA2024001"
DEFINE TARGET_NGAYDT = "2024-01-10"
DEFINE TARGET_TENTHUOC = "Metformin 1000mg"

VARIABLE V_RESTORE_TS VARCHAR2(19);

BEGIN
    EXECUTE IMMEDIATE 'ALTER SESSION SET CONTAINER = XEPDB1';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

PROMPT ===== KIEM TRA USER DANG CHAY SCRIPT =====

SELECT SYS_CONTEXT('USERENV', 'SESSION_USER') AS SESSION_USER,
       SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') AS CURRENT_SCHEMA,
       SYS_CONTEXT('USERENV', 'CON_NAME') AS CON_NAME
FROM dual;

DECLARE
    v_user VARCHAR2(128);
BEGIN
    v_user := SYS_CONTEXT('USERENV', 'SESSION_USER');
    IF v_user NOT IN ('SYS', 'SYSTEM', 'CQ09') THEN
        RAISE_APPLICATION_ERROR(
            -20001,
            'Hay chay script nay bang SYS AS SYSDBA hoac CQ09. User hien tai la ' || v_user ||
            ', khong co quyen doc log FGA va phuc hoi CQ09.DONTHUOC.'
        );
    END IF;
END;
/

DECLARE
    v_ts VARCHAR2(19);
BEGIN
    SELECT TO_CHAR(CAST(MAX(timestamp) AS TIMESTAMP) - INTERVAL '10' SECOND, 'YYYY-MM-DD HH24:MI:SS')
    INTO v_ts
    FROM dba_fga_audit_trail
    WHERE object_schema = 'CQ09'
      AND object_name = 'DONTHUOC'
      AND policy_name = 'FGA_CQ09_DONTHUOC_UPDATE'
      AND db_user = 'BS001'
      AND DBMS_LOB.INSTR(sql_text, 'SU CO DEMO') > 0;

    :V_RESTORE_TS := NVL(v_ts, '&RESTORE_TS');
EXCEPTION
    WHEN OTHERS THEN
        :V_RESTORE_TS := '&RESTORE_TS';
        DBMS_OUTPUT.PUT_LINE('Khong tu lay duoc FGA timestamp, dung RESTORE_TS thu cong: ' || SQLERRM);
END;
/

PRINT V_RESTORE_TS;

PROMPT ===== DU LIEU HIEN TAI =====
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.DONTHUOC
WHERE MAHSBA = '&TARGET_MAHSBA'
  AND NGAYDT = DATE '&TARGET_NGAYDT'
  AND TENTHUOC = N'&TARGET_TENTHUOC';

PROMPT ===== DU LIEU TAI THOI DIEM FLASHBACK =====
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.DONTHUOC AS OF TIMESTAMP TO_TIMESTAMP(:V_RESTORE_TS, 'YYYY-MM-DD HH24:MI:SS')
WHERE MAHSBA = '&TARGET_MAHSBA'
  AND NGAYDT = DATE '&TARGET_NGAYDT'
  AND TENTHUOC = N'&TARGET_TENTHUOC';

PROMPT ===== PHUC HOI LIEUDUNG TU FLASHBACK =====

UPDATE CQ09.DONTHUOC d
SET d.LIEUDUNG = (
    SELECT old.LIEUDUNG
    FROM CQ09.DONTHUOC AS OF TIMESTAMP TO_TIMESTAMP(:V_RESTORE_TS, 'YYYY-MM-DD HH24:MI:SS') old
    WHERE old.MAHSBA = d.MAHSBA
      AND old.NGAYDT = d.NGAYDT
      AND old.TENTHUOC = d.TENTHUOC
)
WHERE d.MAHSBA = '&TARGET_MAHSBA'
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

PROMPT ===== DU LIEU SAU PHUC HOI =====
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.DONTHUOC
WHERE MAHSBA = '&TARGET_MAHSBA'
  AND NGAYDT = DATE '&TARGET_NGAYDT'
  AND TENTHUOC = N'&TARGET_TENTHUOC';
