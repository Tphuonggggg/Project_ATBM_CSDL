-- =============================================================
-- CSC12001 - AN TOAN BAO MAT DU LIEU TRONG HTTT
-- PHAN HE 2: UNG DUNG QUAN LY DU LIEU Y TE
-- FILE: OLS_setup.sql
-- YEU CAU 2: PHAT TAN THONG BAO DUNG ORACLE LABEL SECURITY
-- =============================================================
--
-- Mo hinh nhan:
--   LEVEL : COMPARTMENT : GROUP
--
-- LEVEL:
--   GD   = Ban Giam doc
--   LDK  = Lanh dao khoa
--   NV   = Nhan vien
--
-- COMPARTMENT:
--   TH = Khoa Tieu hoa
--   TK = Khoa Than kinh
--   TM = Khoa Tim mach
--
-- GROUP:
--   HCM = Co so Ho Chi Minh
--   HN  = Co so Ha Noi
--   HP  = Co so Hai Phong
--
-- Luu y thiet ke:
--   Cac thong bao gui TOAN VIEN / TOAN BO CAP BAC duoc gan nhan
--   khong kem compartment/group, vi neu gan tat ca khoa/co so len
--   mot dong thi user chi thuoc mot khoa/co so co the khong doc duoc.
-- =============================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== 0. CHON PDB XEPDB1 VA BAT OLS NEU CAN =====

BEGIN
    EXECUTE IMMEDIATE 'ALTER SESSION SET CONTAINER = XEPDB1';
    DBMS_OUTPUT.PUT_LINE('Current container switched to XEPDB1.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Skip ALTER SESSION SET CONTAINER: ' || SQLERRM);
END;
/

BEGIN
    LBACSYS.CONFIGURE_OLS;
    DBMS_OUTPUT.PUT_LINE('CONFIGURE_OLS executed.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('CONFIGURE_OLS skipped/already configured: ' || SQLERRM);
END;
/

BEGIN
    LBACSYS.OLS_ENFORCEMENT.ENABLE_OLS;
    DBMS_OUTPUT.PUT_LINE('OLS enforcement enabled.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ENABLE_OLS skipped/already enabled: ' || SQLERRM);
END;
/

PROMPT ===== 1. CAP QUYEN HE THONG CAN THIET =====

BEGIN
    EXECUTE IMMEDIATE 'GRANT INHERIT PRIVILEGES ON USER SYS TO LBACSYS';
    DBMS_OUTPUT.PUT_LINE('Granted INHERIT PRIVILEGES on SYS to LBACSYS.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('INHERIT PRIVILEGES grant skipped: ' || SQLERRM);
END;
/

BEGIN
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON LBACSYS.LBAC_POLICY_ADMIN TO CQ09';
    EXECUTE IMMEDIATE 'GRANT EXECUTE ON LBACSYS.SA_SESSION TO CQ09';
    DBMS_OUTPUT.PUT_LINE('Granted OLS helper execute privileges to CQ09.');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('OLS helper grants skipped: ' || SQLERRM);
END;
/

PROMPT ===== 2. DON POLICY CU NEU DA CHAY TRUOC DO =====

BEGIN
    BEGIN
        LBACSYS.LBAC_POLICY_ADMIN.DISABLE_TABLE_POLICY(
            policy_name => 'OLS_THONGBAO_POLICY',
            schema_name => 'CQ09',
            table_name  => 'THONGBAO'
        );
        DBMS_OUTPUT.PUT_LINE('Disabled old OLS table policy.');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('No enabled old table policy or disable skipped: ' || SQLERRM);
    END;

    BEGIN
        LBACSYS.LBAC_POLICY_ADMIN.REMOVE_TABLE_POLICY(
            policy_name => 'OLS_THONGBAO_POLICY',
            schema_name => 'CQ09',
            table_name  => 'THONGBAO'
        );
        DBMS_OUTPUT.PUT_LINE('Removed old OLS table policy.');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('No old table policy or remove skipped: ' || SQLERRM);
    END;

    BEGIN
        LBACSYS.SA_SYSDBA.DROP_POLICY('OLS_THONGBAO_POLICY', TRUE);
        DBMS_OUTPUT.PUT_LINE('Dropped old OLS policy and old label column.');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('No old OLS policy or drop skipped: ' || SQLERRM);
    END;
END;
/

DECLARE
    PROCEDURE drop_column_if_exists(p_column VARCHAR2) IS
        v_count NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_count
        FROM ALL_TAB_COLUMNS
        WHERE OWNER = 'CQ09'
          AND TABLE_NAME = 'THONGBAO'
          AND COLUMN_NAME = UPPER(p_column);

        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE CQ09.THONGBAO DROP COLUMN ' || p_column;
            DBMS_OUTPUT.PUT_LINE('Dropped old OLS label column: ' || p_column);
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Drop column skipped for ' || p_column || ': ' || SQLERRM);
    END;
BEGIN
    drop_column_if_exists('ROW_LABEL');
    drop_column_if_exists('LABEL_TAG');
END;
/

PROMPT ===== 3. TAO POLICY OLS CHO BANG CQ09.THONGBAO =====

BEGIN
    LBACSYS.SA_SYSDBA.CREATE_POLICY(
        policy_name     => 'OLS_THONGBAO_POLICY',
        column_name     => 'LABEL_TAG',
        default_options => 'READ_CONTROL, WRITE_CONTROL, CHECK_CONTROL'
    );
    DBMS_OUTPUT.PUT_LINE('Created OLS_THONGBAO_POLICY with label column LABEL_TAG.');
END;
/

PROMPT ===== 4. TAO LEVEL, COMPARTMENT, GROUP =====

BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL('OLS_THONGBAO_POLICY', 30, 'GD',  'Ban Giam doc');
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL('OLS_THONGBAO_POLICY', 20, 'LDK', 'Lanh dao khoa');
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL('OLS_THONGBAO_POLICY', 10, 'NV',  'Nhan vien');

    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('OLS_THONGBAO_POLICY', 100, 'TH', 'Khoa Tieu hoa');
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('OLS_THONGBAO_POLICY', 200, 'TK', 'Khoa Than kinh');
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT('OLS_THONGBAO_POLICY', 300, 'TM', 'Khoa Tim mach');

    LBACSYS.SA_COMPONENTS.CREATE_GROUP('OLS_THONGBAO_POLICY', 1000, 'HCM', 'Co so Ho Chi Minh');
    LBACSYS.SA_COMPONENTS.CREATE_GROUP('OLS_THONGBAO_POLICY', 2000, 'HN',  'Co so Ha Noi');
    LBACSYS.SA_COMPONENTS.CREATE_GROUP('OLS_THONGBAO_POLICY', 3000, 'HP',  'Co so Hai Phong');

    DBMS_OUTPUT.PUT_LINE('Created OLS components: levels, compartments, groups.');
END;
/

PROMPT ===== 5. TAO CAC NHAN DU LIEU VA NHAN USER CAN DUNG =====

DECLARE
    PROCEDURE create_label_if_needed(p_tag NUMBER, p_label VARCHAR2) IS
    BEGIN
        LBACSYS.SA_LABEL_ADMIN.CREATE_LABEL('OLS_THONGBAO_POLICY', p_tag, p_label);
        DBMS_OUTPUT.PUT_LINE('Created label ' || p_tag || ': ' || p_label);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('Label skipped ' || p_tag || ' (' || p_label || '): ' || SQLERRM);
    END;
BEGIN
    -- Data labels t1 -> t7.
    create_label_if_needed(100, 'NV');
    create_label_if_needed(110, 'GD');
    create_label_if_needed(120, 'LDK');
    create_label_if_needed(130, 'LDK:TH');
    create_label_if_needed(140, 'NV:TH:HCM');
    create_label_if_needed(150, 'NV:TH:HN');
    create_label_if_needed(160, 'LDK:TH,TK:HP');

    -- User labels u1 -> u8.
    create_label_if_needed(201, 'GD:TH,TK,TM:HCM,HN,HP');
    create_label_if_needed(202, 'LDK:TM:HCM');
    create_label_if_needed(203, 'LDK:TK:HN');
    create_label_if_needed(204, 'NV:TK:HCM');
    create_label_if_needed(205, 'NV:TM:HCM');
    create_label_if_needed(206, 'LDK:TH,TK,TM:HCM,HN,HP');
    create_label_if_needed(207, 'NV:TH:HN');
END;
/

PROMPT ===== 6. AP POLICY LEN BANG CQ09.THONGBAO =====

BEGIN
    LBACSYS.LBAC_POLICY_ADMIN.APPLY_TABLE_POLICY(
        policy_name   => 'OLS_THONGBAO_POLICY',
        schema_name   => 'CQ09',
        table_name    => 'THONGBAO',
        table_options => 'READ_CONTROL, WRITE_CONTROL, CHECK_CONTROL'
    );
    DBMS_OUTPUT.PUT_LINE('Applied OLS policy to CQ09.THONGBAO.');
END;
/

PROMPT ===== 7. TAO USER DEMO OLS U1 -> U8 VA CAP QUYEN DOC BANG =====

DECLARE
    PROCEDURE create_or_reset_user(p_user VARCHAR2) IS
        v_count NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_count
        FROM DBA_USERS
        WHERE USERNAME = UPPER(p_user);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE 'CREATE USER ' || p_user || ' IDENTIFIED BY "ATBM123" ACCOUNT UNLOCK';
            DBMS_OUTPUT.PUT_LINE('Created demo user: ' || p_user);
        ELSE
            EXECUTE IMMEDIATE 'ALTER USER ' || p_user || ' IDENTIFIED BY "ATBM123" ACCOUNT UNLOCK';
            DBMS_OUTPUT.PUT_LINE('Reset/unlocked demo user: ' || p_user);
        END IF;

        EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO ' || p_user;
        EXECUTE IMMEDIATE 'GRANT SELECT ON CQ09.THONGBAO TO ' || p_user;
    END;
BEGIN
    create_or_reset_user('u1');
    create_or_reset_user('u2');
    create_or_reset_user('u3');
    create_or_reset_user('u4');
    create_or_reset_user('u5');
    create_or_reset_user('u6');
    create_or_reset_user('u7');
    create_or_reset_user('u8');
END;
/

PROMPT ===== 8. GAN NHAN DOC CHO USER U1 -> U8 =====

BEGIN
    -- Anh xa user demo voi nhom nghiep vu:
    -- u1: Ban Giam doc toan vien.
    -- u2: Lanh dao khoa Tim mach tai HCM.
    -- u3: Lanh dao khoa Than kinh tai HN.
    -- u4: Nhan vien khoa Than kinh tai HCM.
    -- u5: Nhan vien khoa Tim mach tai HCM.
    -- u6: Lanh dao phong/khoa Tim mach tai HCM.
    -- u7: Lanh dao cap LDK phu trach tat ca khoa/co so.
    -- u8: Nhan vien khoa Tieu hoa tai HN.
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u1', 'GD:TH,TK,TM:HCM,HN,HP');
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u2', 'LDK:TM:HCM');
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u3', 'LDK:TK:HN');
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u4', 'NV:TK:HCM');
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u5', 'NV:TM:HCM');
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u6', 'LDK:TM:HCM');
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u7', 'LDK:TH,TK,TM:HCM,HN,HP');
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS('OLS_THONGBAO_POLICY', 'u8', 'NV:TH:HN');

    -- CQ09 la schema owner dung de nap/xem toan bo du lieu khi demo quan tri.
    LBACSYS.SA_USER_ADMIN.SET_USER_PRIVS('OLS_THONGBAO_POLICY', 'CQ09', 'FULL');
    DBMS_OUTPUT.PUT_LINE('Assigned OLS labels to u1 -> u8 and FULL privilege to CQ09.');
END;
/

PROMPT ===== 9. NAP DU LIEU THONG BAO T1 -> T7 =====

BEGIN
    LBACSYS.LBAC_POLICY_ADMIN.DISABLE_TABLE_POLICY(
        policy_name => 'OLS_THONGBAO_POLICY',
        schema_name => 'CQ09',
        table_name  => 'THONGBAO'
    );
END;
/

DELETE FROM CQ09.THONGBAO;

INSERT INTO CQ09.THONGBAO(MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, LABEL_TAG)
VALUES (
    't1',
    N'Thông báo toàn viện: Họp định kỳ tháng về quy trình an toàn bệnh viện',
    TIMESTAMP '2026-07-07 08:00:00',
    N'Hội trường lớn tại toàn bộ cơ sở',
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'NV')
);

INSERT INTO CQ09.THONGBAO(MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, LABEL_TAG)
VALUES (
    't2',
    N'Thông báo Ban Giám đốc: Họp chiến lược mở rộng bệnh viện năm 2027',
    TIMESTAMP '2026-07-11 14:00:00',
    N'Phòng họp Ban Giám đốc - cơ sở Hồ Chí Minh',
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'GD')
);

INSERT INTO CQ09.THONGBAO(MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, LABEL_TAG)
VALUES (
    't3',
    N'Thông báo lãnh đạo khoa: Đánh giá chất lượng chuyên môn quý 2',
    TIMESTAMP '2026-07-05 09:30:00',
    N'Phòng họp trực tuyến liên cơ sở',
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'LDK')
);

INSERT INTO CQ09.THONGBAO(MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, LABEL_TAG)
VALUES (
    't4',
    N'Thông báo lãnh đạo Khoa Tiêu hóa: Cập nhật phác đồ điều trị nội soi mới',
    TIMESTAMP '2026-07-01 15:00:00',
    N'Phòng hội thảo chuyên đề Khoa Tiêu hóa',
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'LDK:TH')
);

INSERT INTO CQ09.THONGBAO(MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, LABEL_TAG)
VALUES (
    't5',
    N'Thông báo nhân viên Khoa Tiêu hóa tại HCM: Đào tạo nội bộ kỹ thuật xét nghiệm',
    TIMESTAMP '2026-07-02 08:30:00',
    N'Phòng LAB vi sinh - cơ sở Hồ Chí Minh',
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'NV:TH:HCM')
);

INSERT INTO CQ09.THONGBAO(MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, LABEL_TAG)
VALUES (
    't6',
    N'Thông báo nhân viên Khoa Tiêu hóa tại Hà Nội: Tập huấn quy trình chẩn đoán HP',
    TIMESTAMP '2026-07-10 10:00:00',
    N'Hội trường giảng dạy - cơ sở Hà Nội',
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'NV:TH:HN')
);

INSERT INTO CQ09.THONGBAO(MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM, LABEL_TAG)
VALUES (
    't7',
    N'Thông báo lãnh đạo Khoa Tiêu hóa và Thần kinh tại Hải Phòng: Xử lý sự cố nhiễm khuẩn',
    TIMESTAMP '2026-07-08 16:30:00',
    N'Phòng họp chỉ huy khẩn cấp - cơ sở Hải Phòng',
    CHAR_TO_LABEL('OLS_THONGBAO_POLICY', 'LDK:TH,TK:HP')
);

BEGIN
    LBACSYS.LBAC_POLICY_ADMIN.ENABLE_TABLE_POLICY(
        policy_name => 'OLS_THONGBAO_POLICY',
        schema_name => 'CQ09',
        table_name  => 'THONGBAO'
    );
END;
/

COMMIT;

PROMPT ===== 10. KIEM TRA NHAN VA DU LIEU MAU =====

SELECT
    MATHONGBAO,
    LABEL_TO_CHAR(LABEL_TAG) AS NHAN_OLS,
    DIADIEM
FROM CQ09.THONGBAO
ORDER BY MATHONGBAO;

PROMPT ===== 11. HAU KIEM POLICY / TABLE POLICY / LABEL / USER LABELS =====

DECLARE
    v_count NUMBER;

    PROCEDURE check_count(p_name VARCHAR2, p_sql VARCHAR2, p_expected NUMBER) IS
    BEGIN
        EXECUTE IMMEDIATE p_sql INTO v_count;
        IF v_count >= p_expected THEN
            DBMS_OUTPUT.PUT_LINE('OK   - ' || p_name || ': ' || v_count);
        ELSE
            DBMS_OUTPUT.PUT_LINE('FAIL - ' || p_name || ': ' || v_count || ', expected >= ' || p_expected);
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('FAIL - ' || p_name || ': ' || SQLERRM);
    END;
BEGIN
    check_count(
        'Policy OLS_THONGBAO_POLICY ton tai',
        'SELECT COUNT(*) FROM DBA_SA_POLICIES WHERE POLICY_NAME = ''OLS_THONGBAO_POLICY''',
        1
    );

    check_count(
        'Policy da apply len CQ09.THONGBAO',
        'SELECT COUNT(*) FROM DBA_SA_TABLE_POLICIES WHERE POLICY_NAME = ''OLS_THONGBAO_POLICY'' AND SCHEMA_NAME = ''CQ09'' AND TABLE_NAME = ''THONGBAO''',
        1
    );

    check_count(
        'Cot LABEL_TAG ton tai tren CQ09.THONGBAO',
        'SELECT COUNT(*) FROM ALL_TAB_COLUMNS WHERE OWNER = ''CQ09'' AND TABLE_NAME = ''THONGBAO'' AND COLUMN_NAME = ''LABEL_TAG''',
        1
    );

    check_count(
        'Da gan label cho du 8 user demo u1 -> u8',
        'SELECT COUNT(DISTINCT USER_NAME) FROM DBA_SA_USER_LABELS WHERE POLICY_NAME = ''OLS_THONGBAO_POLICY'' AND USER_NAME IN (''U1'',''U2'',''U3'',''U4'',''U5'',''U6'',''U7'',''U8'')',
        8
    );
END;
/

PROMPT ===== TEST NHANH SAU KHI CHAY SCRIPT =====
PROMPT CONNECT u1/ATBM123@XEPDB1
PROMPT SELECT MATHONGBAO, NOIDUNG FROM CQ09.THONGBAO ORDER BY MATHONGBAO;
PROMPT CONNECT u4/ATBM123@XEPDB1
PROMPT SELECT MATHONGBAO, NOIDUNG FROM CQ09.THONGBAO ORDER BY MATHONGBAO;

PROMPT ===== HOAN THANH OLS SETUP CHO YEU CAU 2 =====
