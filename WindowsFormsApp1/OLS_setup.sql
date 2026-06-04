-- =============================================================
-- CSC12001 - AN TOAN BAO MAT DU LIEU TRONG HTTT
-- PHAN HE 2: UNG DUNG QUAN LY DU LIEU Y TE
-- FILE: OLS_setup.sql (ĐÃ SỬA LỖI COMPILER VÀ PHÂN QUYỀN)
-- THIẾT LẬP CƠ CHẾ PHÁT TÁN THÔNG BÁO KHẨN DÙNG OLS (YÊU CẦU 2)
-- =============================================================

-- =============================================================
-- BƯỚC 0: KÍCH HOẠT OLS TRÊN CẢ CONTAINER (CDB) VÀ PLUGGABLE DB (PDB)
-- =============================================================
-- Các lỗi ORA-12458 xuất hiện do OLS chưa thực sự được kích hoạt hoặc chưa khởi động lại DB.
-- Bạn cần chạy các lệnh sau bằng tài khoản SYS AS SYSDBA trước:
--
-- 1. Kết nối vào Container Root và kích hoạt:
--    CONN sys/ATBM123 AS SYSDBA;
--    ALTER SESSION SET CONTAINER = CDB$ROOT;
--    EXEC LBACSYS.CONFIGURE_OLS;
--    EXEC LBACSYS.OLS_ENFORCEMENT.ENABLE_OLS;
--
-- 2. Kết nối vào Pluggable Database (ví dụ XEPDB1 hoặc PDB của bạn) và kích hoạt:
--    ALTER SESSION SET CONTAINER = XEPDB1; -- Thay XEPDB1 bằng tên PDB của bạn nếu khác
--    EXEC LBACSYS.CONFIGURE_OLS;
--    EXEC LBACSYS.OLS_ENFORCEMENT.ENABLE_OLS;
--
-- 3. Khởi động lại Oracle Database (ở mức CDB/Root) để các thay đổi có hiệu lực:
--    ALTER SESSION SET CONTAINER = CDB$ROOT;
--    SHUTDOWN IMMEDIATE;
--    STARTUP;
--
-- 4. Kiểm tra lại trạng thái (phải trả về TRUE và ENABLED):
--    SELECT VALUE FROM V$OPTION WHERE PARAMETER = 'Oracle Label Security';
--    SELECT STATUS FROM DBA_OLS_STATUS WHERE NAME = 'OLS';
-- =============================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== BƯỚC PHỤ: CẤP QUYỀN THỪA KẾ PRIVILEGES CHO LBACSYS (SỬA LỖI ORA-06598) =====
-- Lỗi ORA-06598 xảy ra trên Oracle 12c+ khi chạy các lệnh của LBACSYS từ tài khoản SYS
DECLARE
    v_current_user VARCHAR2(100);
BEGIN
    SELECT USER INTO v_current_user FROM DUAL;
    IF v_current_user = 'SYS' THEN
        EXECUTE IMMEDIATE 'GRANT INHERIT PRIVILEGES ON USER SYS TO LBACSYS';
        DBMS_OUTPUT.PUT_LINE('Granted INHERIT PRIVILEGES on SYS to LBACSYS successfully.');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Skipped INHERIT PRIVILEGES grant or it already exists.');
END;
/

PROMPT ===== 1. TẠO CHÍNH SÁCH BẢO MẬT OLS (OLS POLICY) =====
DECLARE
    v_count INT;
BEGIN
    SELECT COUNT(*) INTO v_count FROM DBA_SA_POLICIES WHERE POLICY_NAME = 'OLS_THONGBAO_POLICY';
    IF v_count = 0 THEN
        -- Gọi trực tiếp qua schema LBACSYS để tránh lỗi thiếu Synonym
        LBACSYS.SA_SYS_DBA.CREATE_POLICY (
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
    -- 2.1. Thiết lập Cấp độ (LEVELS) - Càng cao càng nhạy cảm
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL('OLS_THONGBAO_POLICY', 30, 'GD', 'Ban Giam Doc');
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL('OLS_THONGBAO_POLICY', 20, 'LD', 'Lanh Dao Khoa');
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL('OLS_THONGBAO_POLICY', 10, 'NV', 'Nhan Vien');
    DBMS_OUTPUT.PUT_LINE('Created Levels: GD (30), LD (20), NV (10)');

    -- 2.2. Thiết lập Bộ phận/Khoa (COMPARTMENTS)
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('OLS_THONGBAO_POLICY', 100, 'TH', 'Khoa Tieu Hoa');
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('OLS_THONGBAO_POLICY', 200, 'TK', 'Khoa Than Kinh');
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('OLS_THONGBAO_POLICY', 300, 'TM', 'Khoa Tim Mach');
    DBMS_OUTPUT.PUT_LINE('Created Compartments: TH (Tieu Hoa), TK (Than Kinh), TM (Tim Mach)');

    -- 2.3. Thiết lập Địa lý/Cơ sở (GROUPS)
    LBACSYS.SA_COMPONENTS.CREATE_GROUP('OLS_THONGBAO_POLICY', 1000, 'HCM', 'Co so Ho Chi Minh');
    LBACSYS.SA_COMPONENTS.CREATE_GROUP('OLS_THONGBAO_POLICY', 2000, 'HP', 'Co so Hai Phong');
    LBACSYS.SA_COMPONENTS.CREATE_GROUP('OLS_THONGBAO_POLICY', 3000, 'HN', 'Co so Ha Noi');
    DBMS_OUTPUT.PUT_LINE('Created Groups: HCM, HP, HN');
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -20000 OR SQLCODE = -12411 THEN
            DBMS_OUTPUT.PUT_LINE('One or more OLS components already exist. Ignoring.');
        ELSE
            RAISE;
        END IF;
END;
/


PROMPT ===== 3. ĐỊNH NGHĨA DANH SÁCH NHÃN HỢP LỆ (DATA LABELS) =====
BEGIN
    -- Tạo các nhãn dữ liệu đại diện để gán cho các dòng thông báo (t1 -> t7)
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 100, 'NV');                  -- t1: Toàn bộ nhân viên
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 110, 'GD');                  -- t2: Toàn bộ Ban giám đốc
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 120, 'LD');                  -- t3: Các lãnh đạo khoa
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 130, 'LD:TH');               -- t4: Lãnh đạo Khoa tiêu hóa
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 140, 'NV:TH:HCM');            -- t5: Nhân viên Khoa tiêu hóa ở HCM
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 150, 'NV:TH:HN');             -- t6: Nhân viên Khoa tiêu hóa ở HN
    LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', 160, 'LD:TH,TK:HP');          -- t7: Lãnh đạo Khoa tiêu hóa & Thần kinh ở HP
    DBMS_OUTPUT.PUT_LINE('Created Data Labels (t1 -> t7) successfully.');
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -20000 OR SQLCODE = -12413 THEN
            DBMS_OUTPUT.PUT_LINE('Data labels already exist. Ignoring.');
        ELSE
            RAISE;
        END IF;
END;
/


PROMPT ===== 4. ÁP DỤNG CHÍNH SÁCH OLS LÊN BẢNG THONGBAO =====
DECLARE
    v_applied INT;
BEGIN
    -- Kiểm tra bảng THONGBAO đã được áp dụng policy chưa
    SELECT COUNT(*) INTO v_applied FROM DBA_SA_TABLE_POLICIES 
    WHERE POLICY_NAME = 'OLS_THONGBAO_POLICY' AND TABLE_NAME = 'THONGBAO';
    
    IF v_applied = 0 THEN
        LBACSYS.SA_POLICY_ADMIN.APPLY_TABLE_POLICY (
            policy_name     => 'OLS_THONGBAO_POLICY',
            schema_name     => SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA'), -- Lấy schema hiện tại thay vì USER
            table_name      => 'THONGBAO',
            table_options   => 'READ_CONTROL, WRITE_CONTROL, CHECK_CONTROL',
            label_column    => 'ROW_LABEL'
        );
        -- Cấp quyền SELECT cho public để cơ chế OLS tự lọc dòng dữ liệu khi các user u1->u8 truy vấn
        EXECUTE IMMEDIATE 'GRANT SELECT ON THONGBAO TO public';
        DBMS_OUTPUT.PUT_LINE('Applied OLS Policy and granted SELECT to public.');
    ELSE
        -- Đảm bảo quyền SELECT vẫn được cấp trong trường hợp chạy lại
        EXECUTE IMMEDIATE 'GRANT SELECT ON THONGBAO TO public';
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


PROMPT ===== 6. GÁN NHÃN BẢO MẬT CHO NGƯỜI DÙNG (USER LABELS) =====
BEGIN
    -- u1: Giám đốc (Đọc toàn bộ: Cấp GD, tất cả các khoa TH/TK/TM, tất cả cơ sở HCM/HP/HN)
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u1', 'GD:TH,TK,TM:HCM,HP,HN');

    -- u2: Lãnh đạo Khoa tim mạch tại Hồ Chí Minh
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u2', 'LD:TM:HCM');

    -- u3: Lãnh đạo Khoa thần kinh tại Hà Nội
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u3', 'LD:TK:HN');

    -- u4: Nhân viên thuộc Khoa thần kinh tại Hồ Chí Minh
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u4', 'NV:TK:HCM');

    -- u5: Nhân viên thuộc Khoa tim mạch tại Hồ Chí Minh
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u5', 'NV:TM:HCM');

    -- u6: Lãnh đạo phòng có thể đọc các thông báo của Khoa tim mạch tại Hồ Chí Minh
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u6', 'LD:TM:HCM');

    -- u7: Lãnh đạo phòng có thể đọc được toàn bộ thông báo phù hợp với cấp lãnh đạo phòng
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u7', 'LD:TH,TK,TM:HCM,HP,HN');

    -- u8: Nhân viên thuộc Khoa Tiêu hóa tại Hà Nội
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u8', 'NV:TH:HN');
    
    DBMS_OUTPUT.PUT_LINE('Assigned OLS labels to users u1 -> u8.');
END;
/


PROMPT ===== 7. NẠP DỮ LIỆU THÔNG BÁO VỚI NHÃN BẢO MẬT (t1 -> t7) =====
-- Trước khi nạp, xóa thông báo cũ để tránh trùng lặp
DELETE FROM THONGBAO;

-- Thực hiện insert dữ liệu đi kèm chỉ định nhãn OLS tương ứng cho từng dòng
INSERT INTO THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't1', 
    N'Thông báo toàn viện: Họp định kỳ tháng về phòng chống cháy nổ', 
    TIMESTAMP '2026-07-07 08:00:00', 
    N'Hội trường lớn của toàn bộ các Cơ sở', 
    LBACSYS.CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'NV')
);

INSERT INTO THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't2', 
    N'Thông báo Ban Giám Đốc: Thảo luận chiến lược mở rộng bệnh viện 2027', 
    TIMESTAMP '2026-07-11 14:00:00', 
    N'Phòng họp Vip lầu 10 - CS HCM', 
    LBACSYS.CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'GD')
);

INSERT INTO THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't3', 
    N'Thông báo Lãnh đạo Khoa: Đánh giá chất lượng chuyên môn Quý 2', 
    TIMESTAMP '2026-07-05 09:30:00', 
    N'Phòng họp liên cơ sở trực tuyến', 
    LBACSYS.CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'LD')
);

INSERT INTO THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't4', 
    N'Thông báo Khoa Tiêu Hóa: Cập nhật phác đồ điều trị nội soi dạ dày mới', 
    TIMESTAMP '2026-07-01 15:00:00', 
    N'Phòng hội thảo chuyên đề khoa Tiêu Hóa', 
    LBACSYS.CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'LD:TH')
);

INSERT INTO THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't5', 
    N'Thông báo Khoa Tiêu Hóa tại HCM: Đào tạo nội bộ KTV Xét nghiệm Tiêu hóa', 
    TIMESTAMP '2026-07-02 08:30:00', 
    N'Phòng LAB vi sinh lầu 2 - CS HCM', 
    LBACSYS.CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'NV:TH:HCM')
);

INSERT INTO THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't6', 
    N'Thông báo Khoa Tiêu Hóa tại HN: Tập huấn quy trình chẩn đoán khuẩn HP mới', 
    TIMESTAMP '2026-07-10 10:00:00', 
    N'Hội trường giảng dạy - CS Hà Nội', 
    LBACSYS.CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'NV:TH:HN')
);

INSERT INTO THONGBAO (MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, ROW_LABEL) 
VALUES (
    't7', 
    N'Thông báo khẩn liên khoa Hải Phòng: Xử lý sự cố nhiễm khuẩn khoa Tiêu Hóa & Thần Kinh', 
    TIMESTAMP '2026-07-08 16:30:00', 
    N'Phòng họp chỉ huy khẩn cấp - CS Hải Phòng', 
    LBACSYS.CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'LD:TH,TK:HP')
);

COMMIT;
PROMPT ===== HOÀN THÀNH SETUP CHÍNH SÁCH OLS =====
