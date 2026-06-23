-- =============================================================================
-- FILE: 03_demo_su_co.sql
-- ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
-- CHỨC NĂNG:
--   - Giả lập một sự cố thay đổi dữ liệu trái phép trong hệ thống.
--   - Đăng nhập dưới quyền bác sĩ BS001 để sửa sai cột LIEUDUNG (liều dùng thuốc)
--     trên đơn thuốc thuộc bệnh án HSBA2024001 thông qua view nghiệp vụ.
--   - Hệ thống Fine-Grained Audit (FGA) cấu hình trước đó sẽ âm thầm ghi lại log.
-- TÀI KHOẢN THỰC THI: CONNECT chuyển đổi tự động sang BS001/ATBM123.
-- THỨ TỰ THỰC THI: Chạy để bắt đầu kịch bản demo sao lưu/khôi phục (Bước 3).
-- =============================================================================

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
