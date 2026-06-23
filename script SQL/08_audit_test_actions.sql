-- =============================================================================
-- FILE: 08_audit_test_actions.sql
-- ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
-- CHỨC NĂNG:
--   - Chạy các câu lệnh test thử nghiệm (gồm hành động hợp pháp và bất hợp pháp)
--     để hệ thống Oracle phát sinh log kiểm toán (Audit Log).
--   - Đăng nhập dưới dạng các user BS001, KTV01, BN000001 để:
--     + Xem/sửa hồ sơ bệnh án, đơn thuốc, cập nhật kết quả dịch vụ đúng vai trò.
--     + Gọi thử thủ tục P_AUDIT_DEMO_MARK và hàm F_AUDIT_DEMO_USER.
--     + Thực hiện các hành vi bất hợp pháp (ví dụ: Bệnh nhân cố tình sửa HSBA,
--       xóa chỉ định dịch vụ).
-- TÀI KHOẢN THỰC THI: CONNECT chuyển đổi linh hoạt qua lệnh SQL*Plus (SYS, BS001, KTV01, BN000001).
-- THỨ TỰ THỰC THI: Chạy sau khi đã cấu hình Audit thành công (Bước 8).
-- =============================================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;
WHENEVER SQLERROR CONTINUE;

PROMPT ===== 1. BAC SI BS001 DOC/SUA HSBA VA DON THUOC HOP LE =====
CONNECT BS001/ATBM123@localhost:1521/XEPDB1

SELECT MAHSBA, MABN, CHANDOAN, KETLUAN
FROM CQ09.VW_BACSI_HSBA
WHERE MAHSBA = 'HSBA2024001';

UPDATE CQ09.VW_BACSI_HSBA
SET CHANDOAN = N'AUDIT TEST - cap nhat chan doan boi BS001',
    DIEUTRI  = N'AUDIT TEST - cap nhat dieu tri boi BS001',
    KETLUAN  = N'AUDIT TEST - cap nhat ket luan boi BS001'
WHERE MAHSBA = 'HSBA2024001';

UPDATE CQ09.VW_BACSI_DONTHUOC
SET LIEUDUNG = N'AUDIT TEST - lieu dung da bi thay doi boi BS001'
WHERE MAHSBA = 'HSBA2024001'
  AND NGAYDT = DATE '2024-01-10'
  AND TENTHUOC = N'Metformin 1000mg';

BEGIN
    CQ09.P_AUDIT_DEMO_MARK('BS001 execute procedure demo');
END;
/

SELECT CQ09.F_AUDIT_DEMO_USER() AS SESSION_USER_FROM_FUNCTION
FROM dual;

COMMIT;

PROMPT ===== 2. KY THUAT VIEN KTV01 CAP NHAT KET QUA HOP LE =====
CONNECT KTV01/ATBM123@localhost:1521/XEPDB1

SELECT MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA
FROM CQ09.VW_KTV_HSBA_DV
WHERE ROWNUM <= 5;

UPDATE CQ09.VW_KTV_HSBA_DV
SET KETQUA = N'AUDIT TEST - KTV01 cap nhat ket qua dich vu'
WHERE MAHSBA = 'HSBA2024001'
  AND NGAYDV = DATE '2024-01-10';

COMMIT;

PROMPT ===== 3. BENH NHAN BN000001 DOC/SUA THONG TIN CA NHAN HOP LE =====
CONNECT BN000001/ATBM123@localhost:1521/XEPDB1

SELECT MABN, TENBN, SONHA, TENDUONG, QUANHUYEN, TINHTP
FROM CQ09.VW_BENHNHAN;

UPDATE CQ09.VW_BENHNHAN
SET SONHA = '12A',
    TENDUONG = N'AUDIT TEST - Nguyen Trai'
WHERE MABN = 'BN000001';

COMMIT;

PROMPT ===== 4. TEST BAT HOP PHAP: BENH NHAN CO TINH SUA HSBA =====
PROMPT Lenh sau du kien bi loi va duoc Standard Audit ghi WHENEVER NOT SUCCESSFUL.

UPDATE CQ09.HSBA
SET CHANDOAN = N'AUDIT TEST - sua trai phep'
WHERE MAHSBA = 'HSBA2024002';

PROMPT ===== 5. TEST BAT HOP PHAP: BENH NHAN CO TINH XOA HSBA_DV =====
PROMPT Lenh sau du kien bi loi va duoc Standard Audit ghi WHENEVER NOT SUCCESSFUL.

DELETE FROM CQ09.HSBA_DV
WHERE MAHSBA = 'HSBA2024001';

PROMPT ===== 6. DOC LOG AUDIT SAU TEST =====
PROMPT Hay chay 09_audit_read_logs.sql bang CQ09.

WHENEVER SQLERROR EXIT;
