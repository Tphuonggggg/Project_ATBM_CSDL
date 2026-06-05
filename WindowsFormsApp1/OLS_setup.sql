-- =============================================================
-- CSC12001 - AN TOAN BAO MAT DU LIEU TRONG HTTT
-- PHAN HE 2: UNG DUNG QUAN LY DU LIEU Y TE
-- FILE: OLS_setup.sql 
-- THIẾT LẬP CƠ CHẾ PHÁT TÁN THÔNG BÁO KHẨN DÙNG OLS (YÊU CẦU 2)
-- =============================================================

-- =============================================================
-- HƯỚNG DẪN CHẠY:
--   1. Đăng nhập bằng tài khoản SYS AS SYSDBA trỏ vào PDB XEPDB1.
--   2. Thực thi file này để cấu hình OLS cho bảng CQ09.THONGBAO.
--   3. Lệnh chạy: @d:\CODE\Project_ATBM\Project_ATBM_CSDL\WindowsFormsApp1\OLS_setup.sql
-- =============================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

-- Tự động chuyển sang container PDB XEPDB1 để tránh chạy trên CDB$ROOT (Sửa lỗi ORA-12425)
ALTER SESSION SET CONTAINER = XEPDB1;

PROMPT ===== BƯỚC PHỤ: CẤP QUYỀN THỪA KẾ PRIVILEGES CHO LBACSYS (SỬA LỖI ORA-06598) =====
BEGIN
    EXECUTE IMMEDIATE 'GRANT INHERIT PRIVILEGES ON USER SYS TO LBACSYS';
    DBMS_OUTPUT.PUT_LINE('Granted INHERIT PRIVILEGES on SYS to LBACSYS successfully.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('INHERIT PRIVILEGES grant already exists or skipped.');
END;
/

PROMPT ===== 1. TẠO CHÍNH SÁCH BẢO MẬT OLS (OLS POLICY) =====
DECLARE
    v_count INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM DBA_SA_POLICIES WHERE POLICY_NAME = 'OLS_THONGBAO_POLICY';
    IF v_count = 0 THEN
        LBACSYS.SA_SYSDBA.CREATE_POLICY (
            policy_name     => 'OLS_THONGBAO_POLICY',
            column_name     => 'ROW_LABEL',
            default_options => 'READ_CONTROL, WRITE_CONTROL, CHECK_CONTROL'
        );
        DBMS_OUTPUT.PUT_LINE('Created OLS Policy: OLS_THONGBAO_POLICY');
    ELSE
        DBMS_OUTPUT.PUT_LINE('OLS Policy already exists.');
    END IF;
END;
/


PROMPT ===== 2. TẠO CÁC THÀNH PHẦN NHÃN (LEVELS, COMPARTMENTS, GROUPS) =====
BEGIN
    -- 2.1. Thiết lập Cấp độ (LEVELS)
    BEGIN
        LBACSYS.SA_COMPONENTS.CREATE_LEVEL('OLS_THONGBAO_POLICY', 30, 'GD', 'Ban Giam Doc');
        LBACSYS.SA_COMPONENTS.CREATE_LEVEL('OLS_THONGBAO_POLICY', 20, 'LD', 'Lanh Dao Khoa');
        LBACSYS.SA_COMPONENTS.CREATE_LEVEL('OLS_THONGBAO_POLICY', 10, 'NV', 'Nhan Vien');
        DBMS_OUTPUT.PUT_LINE('Created Levels: GD (30), LD (20), NV (10)');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('OLS Levels already exist or unique constraint violated. Skipping.');
    END;

    -- 2.2. Thiết lập Bộ phận/Khoa (COMPARTMENTS)
    BEGIN
        LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('OLS_THONGBAO_POLICY', 100, 'TH', 'Khoa Tieu Hoa');
        LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('OLS_THONGBAO_POLICY', 200, 'TK', 'Khoa Than Kinh');
        LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('OLS_THONGBAO_POLICY', 300, 'TM', 'Khoa Tim Mach');
        DBMS_OUTPUT.PUT_LINE('Created Compartments: TH (Tieu Hoa), TK (Than Kinh), TM (Tim Mach)');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('OLS Compartments already exist. Skipping.');
    END;

    -- 2.3. Thiết lập Địa lý/Cơ sở (GROUPS)
    BEGIN
        LBACSYS.SA_COMPONENTS.CREATE_GROUP('OLS_THONGBAO_POLICY', 1000, 'HCM', 'Co so Ho Chi Minh');
        LBACSYS.SA_COMPONENTS.CREATE_GROUP('OLS_THONGBAO_POLICY', 2000, 'HP', 'Co so Hai Phong');
        LBACSYS.SA_COMPONENTS.CREATE_GROUP('OLS_THONGBAO_POLICY', 3000, 'HN', 'Co so Ha Noi');
        DBMS_OUTPUT.PUT_LINE('Created Groups: HCM, HP, HN');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('OLS Groups already exist. Skipping.');
    END;
END;
/


PROMPT ===== 3. ĐỊNH NGHĨA DANH SÁCH NHÃN HỢP LỆ (DATA LABELS) =====
BEGIN
    BEGIN
        LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 100, 'NV');
    EXCEPTION WHEN OTHERS THEN NULL; END;

    BEGIN
        LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 110, 'GD');
    EXCEPTION WHEN OTHERS THEN NULL; END;

    BEGIN
        LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 120, 'LD');
    EXCEPTION WHEN OTHERS THEN NULL; END;

    BEGIN
        LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 130, 'LD:TH');
    EXCEPTION WHEN OTHERS THEN NULL; END;

    BEGIN
        LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 140, 'NV:TH:HCM');
    EXCEPTION WHEN OTHERS THEN NULL; END;

    BEGIN
        LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 150, 'NV:TH:HN');
    EXCEPTION WHEN OTHERS THEN NULL; END;

    BEGIN
        LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 160, 'LD:TH,TK:HP');
    EXCEPTION WHEN OTHERS THEN NULL; END;
    
    DBMS_OUTPUT.PUT_LINE('OLS Data Labels checked/created.');
END;
/


PROMPT ===== 4. ÁP DỤNG CHÍNH SÁCH OLS LÊN BẢNG CQ09.THONGBAO =====
DECLARE
    v_applied INT;
BEGIN
    SELECT COUNT(*) INTO v_applied FROM DBA_SA_TABLE_POLICIES 
    WHERE POLICY_NAME = 'OLS_THONGBAO_POLICY' AND SCHEMA_NAME = 'CQ09' AND TABLE_NAME = 'THONGBAO';
    
    IF v_applied = 0 THEN
        LBACSYS.LBAC_POLICY_ADMIN.APPLY_TABLE_POLICY (
            policy_name     => 'OLS_THONGBAO_POLICY',
            schema_name     => 'CQ09', 
            table_name      => 'THONGBAO',
            table_options   => 'READ_CONTROL, WRITE_CONTROL, CHECK_CONTROL'
        );
        EXECUTE IMMEDIATE 'GRANT SELECT ON CQ09.THONGBAO TO public';
        DBMS_OUTPUT.PUT_LINE('Applied OLS Policy to table CQ09.THONGBAO and granted SELECT to public.');
    ELSE
        EXECUTE IMMEDIATE 'GRANT SELECT ON CQ09.THONGBAO TO public';
        DBMS_OUTPUT.PUT_LINE('OLS Policy is already applied. Ensured SELECT grant to public.');
    END IF;
END;
/


PROMPT ===== 5. TẠO CÁC USER NGHIỆP VỤ ĐỂ DEMO OLS (u1 -> u8) =====
DECLARE
    PROCEDURE create_demo_user(p_user VARCHAR2) IS
        v_user_count INT;
    BEGIN
        SELECT COUNT(*) INTO v_user_count FROM DBA_USERS WHERE USERNAME = UPPER(p_user);
        IF v_user_count = 0 THEN
            EXECUTE IMMEDIATE 'CREATE USER ' || p_user || ' IDENTIFIED BY "ATBM123" ACCOUNT UNLOCK';
            EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO ' || p_user;
            DBMS_OUTPUT.PUT_LINE('Created demo user: ' || p_user);
        ELSE
            DBMS_OUTPUT.PUT_LINE('Demo user ' || p_user || ' already exists.');
        END IF;
    END;
BEGIN
    create_demo_user('u1');
    create_demo_user('u2');
    create_demo_user('u3');
    create_demo_user('u4');
    create_demo_user('u5');
    create_demo_user('u6');
    create_demo_user('u7');
    create_demo_user('u8');
END;
/


PROMPT ===== 6. GÁN NHÃN VÀ ĐẶC QUYỀN CHO NGƯỜI DÙNG (USER LABELS & PRIVILEGES) =====
DECLARE
    v_con_name VARCHAR2(100);
BEGIN
    SELECT sys_context('USERENV', 'CON_NAME') INTO v_con_name FROM dual;
    DBMS_OUTPUT.PUT_LINE('Current DB Container context: ' || v_con_name);

    -- 6.1. Gán nhãn đọc ghi OLS cho các user từ u1 -> u8
    BEGIN
        LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u1', 'GD:TH,TK,TM:HCM,HP,HN');
        LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u2', 'LD:TM:HCM');
        LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u3', 'LD:TK:HN');
        LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u4', 'NV:TK:HCM');
        LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u5', 'NV:TM:HCM');
        LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u6', 'LD:TM:HCM');
        LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u7', 'LD:TH,TK,TM:HCM,HP,HN');
        LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u8', 'NV:TH:HN');
        DBMS_OUTPUT.PUT_LINE('Assigned OLS labels to users u1 -> u8 successfully.');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Warning: Could not set OLS labels for users: ' || SQLERRM);
    END;
    
    -- 6.2. Cấp đặc quyền 'FULL' OLS cho CQ09
    BEGIN
        LBACSYS.SA_USER_ADMIN.SET_USER_PRIVS('OLS_THONGBAO_POLICY', 'CQ09', 'FULL');
        DBMS_OUTPUT.PUT_LINE('Granted OLS FULL privilege to CQ09 successfully.');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Warning: Could not set OLS FULL privilege for CQ09: ' || SQLERRM);
    END;
    
    -- 6.3. Cấp trực tiếp các quyền hệ thống cho CQ09 để thực hiện Phân hệ 1 (Tránh lỗi ORA-01031 trong PL/SQL DDL)
    BEGIN
        EXECUTE IMMEDIATE 'GRANT CREATE USER, ALTER USER, DROP USER TO CQ09 WITH ADMIN OPTION';
        EXECUTE IMMEDIATE 'GRANT CREATE ROLE, DROP ANY ROLE TO CQ09 WITH ADMIN OPTION';
        EXECUTE IMMEDIATE 'GRANT GRANT ANY PRIVILEGE, GRANT ANY ROLE TO CQ09 WITH ADMIN OPTION';
        EXECUTE IMMEDIATE 'GRANT SELECT ANY DICTIONARY TO CQ09';
        DBMS_OUTPUT.PUT_LINE('Granted direct system privileges to CQ09 successfully.');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Warning: Could not grant direct system privileges to CQ09: ' || SQLERRM);
    END;
END;
/


PROMPT ===== 7. NẠP DỮ LIỆU THÔNG BÁO VỚI NHÃN BẢO MẬT (t1 -> t7) =====

-- Tạm thời tắt chính sách bảo mật OLS trên bảng CQ09.THONGBAO để nạp dữ liệu mẫu
BEGIN
    LBACSYS.LBAC_POLICY_ADMIN.DISABLE_TABLE_POLICY('OLS_THONGBAO_POLICY', 'CQ09', 'THONGBAO');
END;
/

-- Trước khi nạp, xóa thông báo cũ trong schema CQ09
DELETE FROM CQ09.THONGBAO;

-- Thực hiện insert dữ liệu đi kèm chỉ định nhãn OLS tương ứng cho từng dòng
INSERT INTO CQ09.THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't1', 
    N'Thông báo toàn viện: Họp định kỳ tháng về phòng chống cháy nổ', 
    TIMESTAMP '2026-07-07 08:00:00', 
    N'Hội trường lớn của toàn bộ các Cơ sở', 
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'NV')
);

INSERT INTO CQ09.THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't2', 
    N'Thông báo Ban Giám Đốc: Thảo luận chiến lược mở rộng bệnh viện 2027', 
    TIMESTAMP '2026-07-11 14:00:00', 
    N'Phòng họp Vip lầu 10 - CS HCM', 
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'GD')
);

INSERT INTO CQ09.THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't3', 
    N'Thông báo Lãnh đạo Khoa: Đánh giá chất lượng chuyên môn Quý 2', 
    TIMESTAMP '2026-07-05 09:30:00', 
    N'Phòng họp liên cơ sở trực tuyến', 
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'LD')
);

INSERT INTO CQ09.THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't4', 
    N'Thông báo Khoa Tiêu Hóa: Cập nhật phác đồ điều trị nội soi dạ dày mới', 
    TIMESTAMP '2026-07-01 15:00:00', 
    N'Phòng hội thảo chuyên đề khoa Tiêu Hóa', 
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'LD:TH')
);

INSERT INTO CQ09.THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't5', 
    N'Thông báo Khoa Tiêu Hóa tại HCM: Đào tạo nội bộ KTV Xét nghiệm Tiêu hóa', 
    TIMESTAMP '2026-07-02 08:30:00', 
    N'Phòng LAB vi sinh lầu 2 - CS HCM', 
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'NV:TH:HCM')
);

INSERT INTO CQ09.THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't6', 
    N'Thông báo Khoa Tiêu Hóa tại HN: Tập huấn quy trình chẩn đoán khuẩn HP mới', 
    TIMESTAMP '2026-07-10 10:00:00', 
    N'Hội trường giảng dạy - CS Hà Nội', 
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'NV:TH:HN')
);

INSERT INTO CQ09.THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't7', 
    N'Thông báo khẩn liên khoa Hải Phòng: Xử lý sự cố nhiễm khuẩn khoa Tiêu Hóa & Thần Kinh', 
    TIMESTAMP '2026-07-08 16:30:00', 
    N'Phòng họp chỉ huy khẩn cấp - CS Hải Phòng', 
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'LD:TH,TK:HP')
);

-- Kích hoạt lại chính sách bảo mật OLS sau khi nạp xong dữ liệu
BEGIN
    LBACSYS.LBAC_POLICY_ADMIN.ENABLE_TABLE_POLICY('OLS_THONGBAO_POLICY', 'CQ09', 'THONGBAO');
END;
/

COMMIT;
PROMPT ===== HOÀN THÀNH SETUP CHÍNH SÁCH OLS =====
