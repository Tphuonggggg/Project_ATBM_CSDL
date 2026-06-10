-- =============================================================
-- YEU CAU 4 - DEMO TAO SU CO
--
-- Chay bang SQL*Plus/SQLcl. Script dang nhap BS001 va sua sai lieu dung
-- tren don thuoc qua view nghiep vu. FGA cua yeu cau 3 se ghi log.
-- =============================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;
WHENEVER SQLERROR EXIT SQL.SQLCODE;

PROMPT ===== DANG NHAP BS001 =====
CONNECT BS001/ATBM123@localhost:1521/XEPDB1

PROMPT ===== THOI DIEM TRUOC SU CO - DUNG DE FLASHBACK NEU CAN =====
SELECT TO_CHAR(SYSTIMESTAMP, 'YYYY-MM-DD HH24:MI:SS') AS TIME_BEFORE_ERROR
FROM dual;

PROMPT Du lieu truoc su co:
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.VW_BACSI_DONTHUOC
WHERE MAHSBA = 'HSBA2024001'
  AND NGAYDT = DATE '2024-01-10'
  AND TENTHUOC = N'Metformin 1000mg';

UPDATE CQ09.VW_BACSI_DONTHUOC
SET LIEUDUNG = N'SU CO DEMO - sai lieu nguy hiem, can phuc hoi'
WHERE MAHSBA = 'HSBA2024001'
  AND NGAYDT = DATE '2024-01-10'
  AND TENTHUOC = N'Metformin 1000mg';

COMMIT;

PROMPT Du lieu sau su co:
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.VW_BACSI_DONTHUOC
WHERE MAHSBA = 'HSBA2024001'
  AND NGAYDT = DATE '2024-01-10'
  AND TENTHUOC = N'Metformin 1000mg';

PROMPT Hay chay 04_check_audit_log.sql de lay thoi diem audit va SQL_TEXT.
