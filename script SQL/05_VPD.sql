-- =============================================================================
-- FILE: 05_VPD.sql
-- ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
-- CHỨC NĂNG:
--   - Cài đặt chính sách bảo mật Virtual Private Database (VPD) mức dòng/cột.
--   - Xây dựng package PKG_VPD_PH2 chứa các hàm điều kiện lọc dữ liệu (predicates)
--     cho từng bảng chính (NHANVIEN, BENHNHAN, HSBA, HSBA_DV, DONTHUOC).
--   - Gán chính sách VPD bằng DBMS_RLS.ADD_POLICY vào các bảng gốc để đảm bảo:
--     + Bác sĩ chỉ xem/sửa hồ sơ, chỉ chỉ định dịch vụ y tế cho bệnh nhân mình điều trị.
--     + Kỹ thuật viên chỉ xem và cập nhật kết quả dịch vụ được giao cho chính mình.
--     + Bệnh nhân chỉ xem và sửa địa chỉ/tiền sử cá nhân của chính mình.
--   - Tạo các view ứng dụng nghiệp vụ tương thích cho WinForms và gán thay thế
--     các Trigger (Instead of Triggers) phục vụ cập nhật thông tin an toàn.
--   - Thu hồi quyền trực tiếp trên bảng gốc và gán quyền tối thiểu trên View cho các Role.
-- TÀI KHOẢN THỰC THI: SYS hoặc tài khoản quản trị có quyền EXECUTE trên DBMS_RLS (CQ09)
-- THỨ TỰ THỰC THI: Bước 5 trong chuỗi thiết lập.
-- =============================================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

ALTER SESSION SET CONTAINER = XEPDB1;
ALTER SESSION SET CURRENT_SCHEMA = CQ09;

PROMPT ===== 1. GO CAC VPD POLICY CU NEU TON TAI =====

DECLARE
    PROCEDURE drop_policy_if_exists(p_object VARCHAR2, p_policy VARCHAR2) IS
    BEGIN
        DBMS_RLS.DROP_POLICY(
            object_schema => 'CQ09',
            object_name   => p_object,
            policy_name   => p_policy
        );
        DBMS_OUTPUT.PUT_LINE('Dropped policy: ' || p_policy || ' on ' || p_object);
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE IN (-28102, -28103) THEN
                DBMS_OUTPUT.PUT_LINE('Policy does not exist: ' || p_policy || ' on ' || p_object);
            ELSE
                DBMS_OUTPUT.PUT_LINE('Skip drop policy ' || p_policy || ' on ' || p_object || ': ' || SQLERRM);
            END IF;
    END;
BEGIN
    drop_policy_if_exists('NHANVIEN',  'POL_VPD_NHANVIEN');
    drop_policy_if_exists('BENHNHAN',  'POL_VPD_BENHNHAN');
    drop_policy_if_exists('HSBA',      'POL_VPD_HSBA');
    drop_policy_if_exists('HSBA_DV',   'POL_VPD_HSBA_DV');
    drop_policy_if_exists('DONTHUOC',  'POL_VPD_DONTHUOC');
END;
/

PROMPT ===== 2. TAO CAC HAM POLICY VPD =====

CREATE OR REPLACE PACKAGE PKG_VPD_PH2 AS
    FUNCTION nhanvien_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2;
    FUNCTION benhnhan_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2;
    FUNCTION hsba_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2;
    FUNCTION hsba_dv_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2;
    FUNCTION donthuoc_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2;
END PKG_VPD_PH2;
/

CREATE OR REPLACE PACKAGE BODY PKG_VPD_PH2 AS
    FUNCTION session_user_name RETURN VARCHAR2 IS
    BEGIN
        RETURN UPPER(SYS_CONTEXT('USERENV', 'SESSION_USER'));
    END;

    FUNCTION is_schema_owner RETURN BOOLEAN IS
    BEGIN
        RETURN session_user_name IN ('CQ09', 'SYS', 'SYSTEM');
    END;

    FUNCTION has_role(p_role VARCHAR2) RETURN BOOLEAN IS
    BEGIN
        RETURN SYS_CONTEXT('SYS_SESSION_ROLES', UPPER(p_role)) = 'TRUE';
    END;

    FUNCTION is_benhnhan RETURN BOOLEAN IS
    BEGIN
        RETURN has_role('RL_BENHNHAN') OR session_user_name LIKE 'BN%';
    END;

    FUNCTION is_bacsi RETURN BOOLEAN IS
    BEGIN
        RETURN has_role('RL_BACSI') OR session_user_name LIKE 'BS%';
    END;

    FUNCTION is_ktv RETURN BOOLEAN IS
    BEGIN
        RETURN has_role('RL_KYTHUATVIEN') OR session_user_name LIKE 'KTV%';
    END;

    FUNCTION is_dieuphoi RETURN BOOLEAN IS
    BEGIN
        RETURN has_role('RL_DIEUPHOI') OR session_user_name LIKE 'NV%';
    END;

    FUNCTION nhanvien_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
        IF is_schema_owner OR is_dieuphoi THEN
            RETURN '1=1';
        ELSIF is_bacsi THEN
            -- Bac si xem ho so ca nhan va danh sach KTV de chi dinh dich vu.
            RETURN 'MANV = SYS_CONTEXT(''USERENV'', ''SESSION_USER'') OR MANV LIKE ''KTV%''';
        ELSIF is_ktv THEN
            RETURN 'MANV = SYS_CONTEXT(''USERENV'', ''SESSION_USER'')';
        ELSE
            RETURN '1=0';
        END IF;
    END;

    FUNCTION benhnhan_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
        IF is_schema_owner OR is_dieuphoi THEN
            RETURN '1=1';
        ELSIF is_benhnhan THEN
            RETURN 'MABN = SYS_CONTEXT(''USERENV'', ''SESSION_USER'')';
        ELSIF is_bacsi THEN
            RETURN 'EXISTS (SELECT 1 FROM CQ09.HSBA h ' ||
                   'WHERE h.MABN = BENHNHAN.MABN ' ||
                   'AND h.MABS = SYS_CONTEXT(''USERENV'', ''SESSION_USER''))';
        ELSE
            RETURN '1=0';
        END IF;
    END;

    FUNCTION hsba_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
        IF is_schema_owner OR is_dieuphoi THEN
            RETURN '1=1';
        ELSIF is_bacsi THEN
            RETURN 'MABS = SYS_CONTEXT(''USERENV'', ''SESSION_USER'')';
        ELSIF is_benhnhan THEN
            RETURN 'MABN = SYS_CONTEXT(''USERENV'', ''SESSION_USER'')';
        ELSIF is_ktv THEN
            RETURN 'EXISTS (SELECT 1 FROM CQ09.HSBA_DV dv ' ||
                   'WHERE dv.MAHSBA = HSBA.MAHSBA ' ||
                   'AND dv.MAKTV = SYS_CONTEXT(''USERENV'', ''SESSION_USER''))';
        ELSE
            RETURN '1=0';
        END IF;
    END;

    FUNCTION hsba_dv_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
        IF is_schema_owner OR is_dieuphoi THEN
            RETURN '1=1';
        ELSIF is_bacsi THEN
            RETURN 'EXISTS (SELECT 1 FROM CQ09.HSBA h ' ||
                   'WHERE h.MAHSBA = HSBA_DV.MAHSBA ' ||
                   'AND h.MABS = SYS_CONTEXT(''USERENV'', ''SESSION_USER''))';
        ELSIF is_ktv THEN
            RETURN 'MAKTV = SYS_CONTEXT(''USERENV'', ''SESSION_USER'')';
        ELSIF is_benhnhan THEN
            RETURN 'EXISTS (SELECT 1 FROM CQ09.HSBA h ' ||
                   'WHERE h.MAHSBA = HSBA_DV.MAHSBA ' ||
                   'AND h.MABN = SYS_CONTEXT(''USERENV'', ''SESSION_USER''))';
        ELSE
            RETURN '1=0';
        END IF;
    END;

    FUNCTION donthuoc_predicate(p_schema VARCHAR2, p_object VARCHAR2) RETURN VARCHAR2 IS
    BEGIN
        IF is_schema_owner OR is_dieuphoi THEN
            RETURN '1=1';
        ELSIF is_bacsi THEN
            RETURN 'EXISTS (SELECT 1 FROM CQ09.HSBA h ' ||
                   'WHERE h.MAHSBA = DONTHUOC.MAHSBA ' ||
                   'AND h.MABS = SYS_CONTEXT(''USERENV'', ''SESSION_USER''))';
        ELSIF is_benhnhan THEN
            RETURN 'EXISTS (SELECT 1 FROM CQ09.HSBA h ' ||
                   'WHERE h.MAHSBA = DONTHUOC.MAHSBA ' ||
                   'AND h.MABN = SYS_CONTEXT(''USERENV'', ''SESSION_USER''))';
        ELSE
            RETURN '1=0';
        END IF;
    END;
END PKG_VPD_PH2;
/

SHOW ERRORS PACKAGE PKG_VPD_PH2;
SHOW ERRORS PACKAGE BODY PKG_VPD_PH2;

PROMPT ===== 3. GAN VPD POLICY LEN BANG GOC =====

BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema     => 'CQ09',
        object_name       => 'NHANVIEN',
        policy_name       => 'POL_VPD_NHANVIEN',
        function_schema   => 'CQ09',
        policy_function   => 'PKG_VPD_PH2.NHANVIEN_PREDICATE',
        statement_types   => 'SELECT,UPDATE',
        update_check      => TRUE,
        policy_type       => DBMS_RLS.DYNAMIC
    );

    DBMS_RLS.ADD_POLICY(
        object_schema     => 'CQ09',
        object_name       => 'BENHNHAN',
        policy_name       => 'POL_VPD_BENHNHAN',
        function_schema   => 'CQ09',
        policy_function   => 'PKG_VPD_PH2.BENHNHAN_PREDICATE',
        statement_types   => 'SELECT,UPDATE',
        update_check      => TRUE,
        policy_type       => DBMS_RLS.DYNAMIC
    );

    DBMS_RLS.ADD_POLICY(
        object_schema     => 'CQ09',
        object_name       => 'HSBA',
        policy_name       => 'POL_VPD_HSBA',
        function_schema   => 'CQ09',
        policy_function   => 'PKG_VPD_PH2.HSBA_PREDICATE',
        statement_types   => 'SELECT,INSERT,UPDATE,DELETE',
        update_check      => TRUE,
        policy_type       => DBMS_RLS.DYNAMIC
    );

    DBMS_RLS.ADD_POLICY(
        object_schema     => 'CQ09',
        object_name       => 'HSBA_DV',
        policy_name       => 'POL_VPD_HSBA_DV',
        function_schema   => 'CQ09',
        policy_function   => 'PKG_VPD_PH2.HSBA_DV_PREDICATE',
        statement_types   => 'SELECT,INSERT,UPDATE,DELETE',
        update_check      => TRUE,
        policy_type       => DBMS_RLS.DYNAMIC
    );

    DBMS_RLS.ADD_POLICY(
        object_schema     => 'CQ09',
        object_name       => 'DONTHUOC',
        policy_name       => 'POL_VPD_DONTHUOC',
        function_schema   => 'CQ09',
        policy_function   => 'PKG_VPD_PH2.DONTHUOC_PREDICATE',
        statement_types   => 'SELECT,INSERT,UPDATE,DELETE',
        update_check      => TRUE,
        policy_type       => DBMS_RLS.DYNAMIC
    );
END;
/

PROMPT ===== 4. TAO VIEW TUONG THICH VOI UNG DUNG =====

CREATE OR REPLACE VIEW VW_BENHNHAN AS
SELECT
    MABN, TENBN, PHAI, NGAYSINH, CCCD,
    SONHA, TENDUONG, QUANHUYEN, TINHTP,
    TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC
FROM BENHNHAN;

CREATE OR REPLACE VIEW VW_NHANVIEN_CANHAN AS
SELECT
    MANV, HOTEN, PHAI, NGAYSINH, CMND,
    QUEQUAN, SODT, VAITRO, CHUYENKHOA
FROM NHANVIEN;

CREATE OR REPLACE VIEW VW_KTV_HSBA_DV AS
SELECT
    MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA
FROM HSBA_DV;

CREATE OR REPLACE VIEW VW_BACSI_HSBA AS
SELECT
    h.MAHSBA,
    h.MABN,
    b.TENBN,
    h.NGAY,
    h.MAKHOA,
    h.CHANDOAN,
    h.DIEUTRI,
    h.KETLUAN
FROM HSBA h
JOIN BENHNHAN b ON b.MABN = h.MABN;

CREATE OR REPLACE VIEW VW_BACSI_BENHNHAN AS
SELECT
    MABN,
    TENBN,
    PHAI,
    NGAYSINH,
    CCCD,
    SONHA,
    TENDUONG,
    QUANHUYEN,
    TINHTP,
    TIENSUBENH,
    TIENSUBENHGD,
    DIUNGTHUOC
FROM BENHNHAN;

CREATE OR REPLACE VIEW VW_BACSI_DONTHUOC AS
SELECT
    MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM DONTHUOC;

CREATE OR REPLACE VIEW VW_BACSI_HSBA_DV AS
SELECT
    MAHSBA,
    LOAIDV,
    NGAYDV,
    MAKTV,
    CAST(NULL AS NVARCHAR2(100)) AS HOTEN_KTV,
    KETQUA
FROM HSBA_DV;

CREATE OR REPLACE VIEW VW_KTV_LIST AS
SELECT MANV, HOTEN
FROM NHANVIEN
WHERE MANV LIKE 'KTV%';

PROMPT ===== 5. TAO TRIGGER CHO CAC VIEW DML CUA BAC SI =====

CREATE OR REPLACE TRIGGER TRG_IOU_VW_BACSI_HSBA
INSTEAD OF UPDATE ON VW_BACSI_HSBA
FOR EACH ROW
BEGIN
    UPDATE HSBA
    SET CHANDOAN = :NEW.CHANDOAN,
        DIEUTRI  = :NEW.DIEUTRI,
        KETLUAN  = :NEW.KETLUAN
    WHERE MAHSBA = :OLD.MAHSBA
      AND (
            MABS = SYS_CONTEXT('USERENV', 'SESSION_USER')
            OR SYS_CONTEXT('SYS_SESSION_ROLES', 'RL_DIEUPHOI') = 'TRUE'
          );

    IF SQL%ROWCOUNT = 0 THEN
        RAISE_APPLICATION_ERROR(-20031, 'Khong du quyen cap nhat HSBA nay.');
    END IF;
END;
/

CREATE OR REPLACE TRIGGER TRG_IOI_VW_BACSI_HSBA_DV
INSTEAD OF INSERT ON VW_BACSI_HSBA_DV
FOR EACH ROW
DECLARE
    v_allowed NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_allowed
    FROM HSBA
    WHERE MAHSBA = :NEW.MAHSBA
      AND (
            MABS = SYS_CONTEXT('USERENV', 'SESSION_USER')
            OR SYS_CONTEXT('SYS_SESSION_ROLES', 'RL_DIEUPHOI') = 'TRUE'
          );

    IF v_allowed = 0 THEN
        RAISE_APPLICATION_ERROR(-20032, 'Khong du quyen them chi dinh dich vu cho HSBA nay.');
    END IF;

    INSERT INTO HSBA_DV(MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA)
    VALUES (:NEW.MAHSBA, :NEW.LOAIDV, :NEW.NGAYDV, :NEW.MAKTV, NULL);
END;
/

CREATE OR REPLACE TRIGGER TRG_IOU_VW_BACSI_HSBA_DV
INSTEAD OF UPDATE ON VW_BACSI_HSBA_DV
FOR EACH ROW
BEGIN
    UPDATE HSBA_DV dv
    SET MAKTV = :NEW.MAKTV
    WHERE dv.MAHSBA = :OLD.MAHSBA
      AND dv.LOAIDV = :OLD.LOAIDV
      AND dv.NGAYDV = :OLD.NGAYDV
      AND EXISTS (
            SELECT 1
            FROM HSBA h
            WHERE h.MAHSBA = dv.MAHSBA
              AND (
                    h.MABS = SYS_CONTEXT('USERENV', 'SESSION_USER')
                    OR SYS_CONTEXT('SYS_SESSION_ROLES', 'RL_DIEUPHOI') = 'TRUE'
                  )
          );

    IF SQL%ROWCOUNT = 0 THEN
        RAISE_APPLICATION_ERROR(-20033, 'Khong du quyen cap nhat chi dinh dich vu nay.');
    END IF;
END;
/

CREATE OR REPLACE TRIGGER TRG_IOD_VW_BACSI_HSBA_DV
INSTEAD OF DELETE ON VW_BACSI_HSBA_DV
FOR EACH ROW
BEGIN
    DELETE FROM HSBA_DV dv
    WHERE dv.MAHSBA = :OLD.MAHSBA
      AND dv.LOAIDV = :OLD.LOAIDV
      AND dv.NGAYDV = :OLD.NGAYDV
      AND EXISTS (
            SELECT 1
            FROM HSBA h
            WHERE h.MAHSBA = dv.MAHSBA
              AND (
                    h.MABS = SYS_CONTEXT('USERENV', 'SESSION_USER')
                    OR SYS_CONTEXT('SYS_SESSION_ROLES', 'RL_DIEUPHOI') = 'TRUE'
                  )
          );

    IF SQL%ROWCOUNT = 0 THEN
        RAISE_APPLICATION_ERROR(-20034, 'Khong du quyen xoa chi dinh dich vu nay.');
    END IF;
END;
/

SHOW ERRORS TRIGGER TRG_IOU_VW_BACSI_HSBA;
SHOW ERRORS TRIGGER TRG_IOI_VW_BACSI_HSBA_DV;
SHOW ERRORS TRIGGER TRG_IOU_VW_BACSI_HSBA_DV;
SHOW ERRORS TRIGGER TRG_IOD_VW_BACSI_HSBA_DV;

PROMPT ===== 6. CAP QUYEN TOI THIEU CHO ROLE =====

DECLARE
    PROCEDURE revoke_if_possible(p_sql VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE p_sql;
    EXCEPTION
        WHEN OTHERS THEN NULL;
    END;
BEGIN
    revoke_if_possible('REVOKE ALL ON NHANVIEN FROM RL_DIEUPHOI');
    revoke_if_possible('REVOKE ALL ON NHANVIEN FROM RL_BACSI');
    revoke_if_possible('REVOKE ALL ON NHANVIEN FROM RL_KYTHUATVIEN');
    revoke_if_possible('REVOKE ALL ON NHANVIEN FROM RL_BENHNHAN');

    revoke_if_possible('REVOKE ALL ON BENHNHAN FROM RL_DIEUPHOI');
    revoke_if_possible('REVOKE ALL ON BENHNHAN FROM RL_BACSI');
    revoke_if_possible('REVOKE ALL ON BENHNHAN FROM RL_KYTHUATVIEN');
    revoke_if_possible('REVOKE ALL ON BENHNHAN FROM RL_BENHNHAN');

    revoke_if_possible('REVOKE ALL ON HSBA FROM RL_DIEUPHOI');
    revoke_if_possible('REVOKE ALL ON HSBA FROM RL_BACSI');
    revoke_if_possible('REVOKE ALL ON HSBA FROM RL_KYTHUATVIEN');
    revoke_if_possible('REVOKE ALL ON HSBA FROM RL_BENHNHAN');

    revoke_if_possible('REVOKE ALL ON HSBA_DV FROM RL_DIEUPHOI');
    revoke_if_possible('REVOKE ALL ON HSBA_DV FROM RL_BACSI');
    revoke_if_possible('REVOKE ALL ON HSBA_DV FROM RL_KYTHUATVIEN');
    revoke_if_possible('REVOKE ALL ON HSBA_DV FROM RL_BENHNHAN');

    revoke_if_possible('REVOKE ALL ON DONTHUOC FROM RL_DIEUPHOI');
    revoke_if_possible('REVOKE ALL ON DONTHUOC FROM RL_BACSI');
    revoke_if_possible('REVOKE ALL ON DONTHUOC FROM RL_KYTHUATVIEN');
    revoke_if_possible('REVOKE ALL ON DONTHUOC FROM RL_BENHNHAN');
END;
/

-- Dieu phoi vien: tiep nhan benh nhan, tao HSBA va phan cong bac si/KTV.
-- Khong cap DELETE va khong cap quyen tren DONTHUOC.
GRANT SELECT ON NHANVIEN TO RL_DIEUPHOI;
GRANT SELECT, INSERT ON BENHNHAN TO RL_DIEUPHOI;
GRANT UPDATE(
    TENBN,
    PHAI,
    NGAYSINH,
    CCCD,
    SONHA,
    TENDUONG,
    QUANHUYEN,
    TINHTP,
    TIENSUBENH,
    TIENSUBENHGD,
    DIUNGTHUOC
) ON BENHNHAN TO RL_DIEUPHOI;
GRANT SELECT ON HSBA TO RL_DIEUPHOI;
GRANT INSERT(MAHSBA, MABN, NGAY, MABS, MAKHOA) ON HSBA TO RL_DIEUPHOI;
GRANT UPDATE(MAKHOA, MABS) ON HSBA TO RL_DIEUPHOI;
GRANT SELECT ON HSBA_DV TO RL_DIEUPHOI;
GRANT INSERT(MAHSBA, LOAIDV, NGAYDV, MAKTV) ON HSBA_DV TO RL_DIEUPHOI;
GRANT UPDATE(MAKTV) ON HSBA_DV TO RL_DIEUPHOI;

-- Bac si/Y si - TC#3.
GRANT SELECT ON NHANVIEN TO RL_BACSI;
GRANT UPDATE(QUEQUAN, SODT) ON NHANVIEN TO RL_BACSI;
GRANT SELECT ON BENHNHAN TO RL_BACSI;
GRANT UPDATE(TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC) ON BENHNHAN TO RL_BACSI;
GRANT SELECT ON HSBA TO RL_BACSI;
GRANT UPDATE(CHANDOAN, DIEUTRI, KETLUAN) ON HSBA TO RL_BACSI;
GRANT SELECT, INSERT, DELETE ON DONTHUOC TO RL_BACSI;
GRANT UPDATE(TENTHUOC, LIEUDUNG) ON DONTHUOC TO RL_BACSI;
GRANT SELECT ON HSBA_DV TO RL_BACSI;
GRANT INSERT(MAHSBA, LOAIDV, NGAYDV, MAKTV) ON HSBA_DV TO RL_BACSI;
GRANT UPDATE(MAKTV) ON HSBA_DV TO RL_BACSI;
GRANT DELETE ON HSBA_DV TO RL_BACSI;

-- Ky thuat vien - TC#4 va ho so ca nhan TC#5.
GRANT SELECT ON NHANVIEN TO RL_KYTHUATVIEN;
GRANT UPDATE(QUEQUAN, SODT) ON NHANVIEN TO RL_KYTHUATVIEN;
GRANT SELECT ON HSBA_DV TO RL_KYTHUATVIEN;
GRANT UPDATE(KETQUA) ON HSBA_DV TO RL_KYTHUATVIEN;

-- Benh nhan - TC#5.
GRANT SELECT ON BENHNHAN TO RL_BENHNHAN;
GRANT UPDATE(
    SONHA,
    TENDUONG,
    QUANHUYEN,
    TINHTP,
    TIENSUBENH,
    TIENSUBENHGD,
    DIUNGTHUOC
) ON BENHNHAN TO RL_BENHNHAN;
GRANT SELECT ON HSBA TO RL_BENHNHAN;
GRANT SELECT ON HSBA_DV TO RL_BENHNHAN;
GRANT SELECT ON DONTHUOC TO RL_BENHNHAN;

-- Quyen tren view tuong thich app.
GRANT SELECT ON VW_BENHNHAN TO RL_BENHNHAN;
GRANT UPDATE(
    SONHA,
    TENDUONG,
    QUANHUYEN,
    TINHTP,
    TIENSUBENH,
    TIENSUBENHGD,
    DIUNGTHUOC
) ON VW_BENHNHAN TO RL_BENHNHAN;

GRANT SELECT ON VW_NHANVIEN_CANHAN TO RL_DIEUPHOI;
GRANT SELECT ON VW_NHANVIEN_CANHAN TO RL_BACSI;
GRANT SELECT ON VW_NHANVIEN_CANHAN TO RL_KYTHUATVIEN;
GRANT UPDATE(QUEQUAN, SODT) ON VW_NHANVIEN_CANHAN TO RL_DIEUPHOI;
GRANT UPDATE(QUEQUAN, SODT) ON VW_NHANVIEN_CANHAN TO RL_BACSI;
GRANT UPDATE(QUEQUAN, SODT) ON VW_NHANVIEN_CANHAN TO RL_KYTHUATVIEN;

GRANT SELECT ON VW_KTV_HSBA_DV TO RL_KYTHUATVIEN;
GRANT UPDATE(KETQUA) ON VW_KTV_HSBA_DV TO RL_KYTHUATVIEN;

GRANT SELECT ON VW_BACSI_HSBA TO RL_BACSI;
GRANT UPDATE(CHANDOAN, DIEUTRI, KETLUAN) ON VW_BACSI_HSBA TO RL_BACSI;
GRANT SELECT ON VW_BACSI_BENHNHAN TO RL_BACSI;
GRANT UPDATE(TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC) ON VW_BACSI_BENHNHAN TO RL_BACSI;
GRANT SELECT, INSERT, DELETE ON VW_BACSI_DONTHUOC TO RL_BACSI;
GRANT UPDATE(TENTHUOC, LIEUDUNG) ON VW_BACSI_DONTHUOC TO RL_BACSI;
GRANT SELECT, DELETE ON VW_BACSI_HSBA_DV TO RL_BACSI;
GRANT INSERT(MAHSBA, LOAIDV, NGAYDV, MAKTV) ON VW_BACSI_HSBA_DV TO RL_BACSI;
GRANT UPDATE(MAKTV) ON VW_BACSI_HSBA_DV TO RL_BACSI;
GRANT SELECT ON VW_KTV_LIST TO RL_BACSI;

PROMPT ===== 7. KIEM TRA POLICY DA GAN =====

SELECT OBJECT_OWNER, OBJECT_NAME, POLICY_NAME, SEL, INS, UPD, DEL, ENABLE
FROM DBA_POLICIES
WHERE OBJECT_OWNER = 'CQ09'
  AND OBJECT_NAME IN ('NHANVIEN', 'BENHNHAN', 'HSBA', 'HSBA_DV', 'DONTHUOC')
ORDER BY OBJECT_NAME, POLICY_NAME;

PROMPT ===== HOAN TAT VPD_TC3.sql =====

PROMPT Test nhanh sau khi chay:
PROMPT   CONNECT BS001/ATBM123@XEPDB1
PROMPT   SELECT DISTINCT MABS FROM CQ09.HSBA;
PROMPT   SELECT * FROM CQ09.VW_BACSI_HSBA;
PROMPT   CONNECT KTV01/ATBM123@XEPDB1
PROMPT   SELECT DISTINCT MAKTV FROM CQ09.HSBA_DV;
PROMPT   CONNECT BN000001/ATBM123@XEPDB1
PROMPT   SELECT MABN, TENBN FROM CQ09.BENHNHAN;
