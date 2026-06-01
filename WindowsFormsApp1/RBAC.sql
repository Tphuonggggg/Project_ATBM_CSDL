-- =============================================================
-- CSC12001 - AN TOAN BAO MAT DU LIEU TRONG HTTT
-- PHAN HE 2: UNG DUNG QUAN LY DU LIEU Y TE
-- FILE: RBAC.sql
-- CẤU HÌNH CƠ CHẾ KIỂM SOÁT TRUY CẬP DỰA TRÊN VAI TRÒ (RBAC VÀ VIEW)
-- =============================================================

-- =============================================================
-- 1. RBAC CHO BỆNH NHÂN (TC#5)
-- Bệnh nhân chỉ xem được thông tin của chính mình (qua view)
-- và chỉ cập nhật các cột địa chỉ, tiền sử bệnh, dị ứng thuốc.
-- =============================================================

-- Tạo view hiển thị thông tin cá nhân của bệnh nhân đăng nhập
CREATE OR REPLACE VIEW vw_benhnhan AS
SELECT * 
FROM BENHNHAN
WHERE MABN = SYS_CONTEXT('USERENV', 'SESSION_USER');

-- Thu hồi toàn bộ quyền trực tiếp trên bảng gốc để tránh bypass view (bọc PL/SQL tránh lỗi ORA-01927)
BEGIN
    EXECUTE IMMEDIATE 'REVOKE ALL ON BENHNHAN FROM RL_BENHNHAN';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- Cấp quyền SELECT trên view cá nhân cho role bệnh nhân
GRANT SELECT ON vw_benhnhan TO RL_BENHNHAN;

-- Cấp quyền UPDATE trên các cột được phép sửa thông qua view (đảm bảo bảo mật mức dòng)
GRANT UPDATE(
    SONHA,
    TENDUONG,
    QUANHUYEN,
    TINHTP,
    TIENSUBENH,
    TIENSUBENHGD,
    DIUNGTHUOC
)
ON vw_benhnhan
TO RL_BENHNHAN;


-- =============================================================
-- 2. RBAC CHO KỸ THUẬT VIÊN (TC#4)
-- Kỹ thuật viên chỉ xem dịch vụ do mình phụ trách (qua view)
-- và ghi kết quả thực hiện tại cột KETQUA của dịch vụ đó.
-- =============================================================

-- Tạo view hiển thị dịch vụ y tế do KTV đăng nhập phụ trách
CREATE OR REPLACE VIEW vw_ktv_HSBA_DV AS 
SELECT * 
FROM HSBA_DV 
WHERE MAKTV = SYS_CONTEXT('USERENV', 'SESSION_USER');

-- Thu hồi toàn bộ quyền trực tiếp trên bảng gốc để tránh bypass (bọc PL/SQL tránh lỗi ORA-01927)
BEGIN
    EXECUTE IMMEDIATE 'REVOKE ALL ON HSBA_DV FROM RL_KYTHUATVIEN';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- Cấp quyền SELECT trên view cho role kỹ thuật viên
GRANT SELECT ON vw_ktv_HSBA_DV TO RL_KYTHUATVIEN;

-- Cấp quyền UPDATE cột kết quả qua view (đảm bảo bảo mật mức dòng)
GRANT UPDATE(KETQUA) ON vw_ktv_HSBA_DV TO RL_KYTHUATVIEN;


-- =============================================================
-- 3. RBAC CHO NHÂN VIÊN TỰ XEM / SỬA THÔNG TIN CÁ NHÂN (TC#5)
-- Nhân viên chỉ xem được thông tin của chính mình (qua view)
-- và chỉ cập nhật quê quán (QUEQUAN) và số điện thoại (SODT).
-- =============================================================

-- Tạo view hiển thị thông tin cá nhân của nhân viên đăng nhập
CREATE OR REPLACE VIEW vw_nhanvien_canhan AS
SELECT *
FROM NHANVIEN
WHERE MANV = SYS_CONTEXT('USERENV', 'SESSION_USER');

-- Thu hồi quyền UPDATE trực tiếp trên bảng gốc đối với các vai trò nhân viên (bọc PL/SQL tránh lỗi ORA-01927)
BEGIN
    EXECUTE IMMEDIATE 'REVOKE UPDATE ON NHANVIEN FROM RL_DIEUPHOI';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'REVOKE UPDATE ON NHANVIEN FROM RL_BACSI';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'REVOKE UPDATE ON NHANVIEN FROM RL_KYTHUATVIEN';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

-- Cấp quyền SELECT trên view cá nhân cho các vai trò nhân viên
GRANT SELECT ON vw_nhanvien_canhan TO RL_DIEUPHOI;
GRANT SELECT ON vw_nhanvien_canhan TO RL_BACSI;
GRANT SELECT ON vw_nhanvien_canhan TO RL_KYTHUATVIEN;

-- Cấp quyền UPDATE quê quán và số điện thoại qua view cá nhân
GRANT UPDATE(QUEQUAN, SODT) ON vw_nhanvien_canhan TO RL_DIEUPHOI;
GRANT UPDATE(QUEQUAN, SODT) ON vw_nhanvien_canhan TO RL_BACSI;
GRANT UPDATE(QUEQUAN, SODT) ON vw_nhanvien_canhan TO RL_KYTHUATVIEN;
