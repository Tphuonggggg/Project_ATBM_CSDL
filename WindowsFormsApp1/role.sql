-- =============================================================
-- CSC12001 - AN TOAN BAO MAT DU LIEU TRONG HTTT
-- PHAN HE 2: UNG DUNG QUAN LY DU LIEU Y TE
-- FILE 02: TAO USER / ROLE
-- Luu y:
--   1. Chay SAU file Script_SQL_completed_schema_data.sql
--   2. Chay bang user co quyen CREATE USER, CREATE ROLE, GRANT ANY ROLE
--   3. File nay CHI tao user/role va gan role. KHONG cap quyen nghiep vu tren bang.
--   4. Password demo cua tat ca user: ATBM123
-- =============================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== 1. TAO LAI CAC ROLE PHAN HE 2 =====

DECLARE
    PROCEDURE drop_role_if_exists(p_role VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE 'DROP ROLE ' || p_role;
        DBMS_OUTPUT.PUT_LINE('Dropped role: ' || p_role);
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE = -1919 THEN
                DBMS_OUTPUT.PUT_LINE('Role does not exist: ' || p_role);
            ELSE
                RAISE;
            END IF;
    END;

    PROCEDURE create_role_if_not_exists(p_role VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE 'CREATE ROLE ' || p_role;
        DBMS_OUTPUT.PUT_LINE('Created role: ' || p_role);
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE = -1921 THEN
                DBMS_OUTPUT.PUT_LINE('Role already exists: ' || p_role);
            ELSE
                RAISE;
            END IF;
    END;
BEGIN
    -- Drop de dam bao role sach, khong con quyen bang cu tu lan chay truoc.
    drop_role_if_exists('RL_DIEUPHOI');
    drop_role_if_exists('RL_BACSI');
    drop_role_if_exists('RL_KYTHUATVIEN');
    drop_role_if_exists('RL_BENHNHAN');

    create_role_if_not_exists('RL_DIEUPHOI');
    create_role_if_not_exists('RL_BACSI');
    create_role_if_not_exists('RL_KYTHUATVIEN');
    create_role_if_not_exists('RL_BENHNHAN');
END;
/

PROMPT ===== 2. CAP QUYEN DANG NHAP CO BAN =====

-- Cap qua role
GRANT CREATE SESSION TO RL_DIEUPHOI;
GRANT CREATE SESSION TO RL_BACSI;
GRANT CREATE SESSION TO RL_KYTHUATVIEN;
GRANT CREATE SESSION TO RL_BENHNHAN;

-- Quyen xem view tu anh xa tai khoan, chi de kiem tra user dang nhap la ai.
-- Khong phai quyen nghiep vu tren cac bang BENHNHAN/HSBA/HSBA_DV/DONTHUOC.
GRANT SELECT ON V_MY_ACCOUNT TO RL_DIEUPHOI;
GRANT SELECT ON V_MY_ACCOUNT TO RL_BACSI;
GRANT SELECT ON V_MY_ACCOUNT TO RL_KYTHUATVIEN;
GRANT SELECT ON V_MY_ACCOUNT TO RL_BENHNHAN;

PROMPT ===== 3. TAO ORACLE USER CHO NHANVIEN =====

DECLARE
    v_sql VARCHAR2(4000);

    PROCEDURE create_or_update_user(p_username VARCHAR2) IS
    BEGIN
        v_sql := 'CREATE USER ' || p_username || ' IDENTIFIED BY "ATBM123" ACCOUNT UNLOCK';
        EXECUTE IMMEDIATE v_sql;
        DBMS_OUTPUT.PUT_LINE('Created user: ' || p_username);
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE = -1920 THEN
                EXECUTE IMMEDIATE 'ALTER USER ' || p_username || ' IDENTIFIED BY "ATBM123" ACCOUNT UNLOCK';
                DBMS_OUTPUT.PUT_LINE('User exists, reset password/unlock: ' || p_username);
            ELSE
                RAISE;
            END IF;
    END;
BEGIN
    FOR r IN (
        SELECT MANV, VAITRO
        FROM NHANVIEN
        ORDER BY
            CASE VAITRO
                WHEN N'Điều phối viên' THEN 1
                WHEN N'Bác sĩ/Y sĩ' THEN 2
                WHEN N'Kỹ thuật viên' THEN 3
                ELSE 4
            END,
            MANV
    ) LOOP
        create_or_update_user(r.MANV);

        -- Cap CREATE SESSION truc tiep de dam bao user dang nhap duoc trong moi moi truong Oracle.
        EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO ' || r.MANV;

        IF r.VAITRO = N'Điều phối viên' THEN
            EXECUTE IMMEDIATE 'GRANT RL_DIEUPHOI TO ' || r.MANV;
        ELSIF r.VAITRO = N'Bác sĩ/Y sĩ' THEN
            EXECUTE IMMEDIATE 'GRANT RL_BACSI TO ' || r.MANV;
        ELSIF r.VAITRO = N'Kỹ thuật viên' THEN
            EXECUTE IMMEDIATE 'GRANT RL_KYTHUATVIEN TO ' || r.MANV;
        END IF;

        EXECUTE IMMEDIATE 'ALTER USER ' || r.MANV || ' DEFAULT ROLE ALL';
    END LOOP;
END;
/

PROMPT ===== 4. TAO ORACLE USER CHO BENHNHAN =====

DECLARE
    v_sql VARCHAR2(4000);

    PROCEDURE create_or_update_user(p_username VARCHAR2) IS
    BEGIN
        v_sql := 'CREATE USER ' || p_username || ' IDENTIFIED BY "ATBM123" ACCOUNT UNLOCK';
        EXECUTE IMMEDIATE v_sql;
        DBMS_OUTPUT.PUT_LINE('Created user: ' || p_username);
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE = -1920 THEN
                EXECUTE IMMEDIATE 'ALTER USER ' || p_username || ' IDENTIFIED BY "ATBM123" ACCOUNT UNLOCK';
                DBMS_OUTPUT.PUT_LINE('User exists, reset password/unlock: ' || p_username);
            ELSE
                RAISE;
            END IF;
    END;
BEGIN
    FOR r IN (
        SELECT MABN
        FROM BENHNHAN
        ORDER BY MABN
    ) LOOP
        create_or_update_user(r.MABN);

        -- Cap CREATE SESSION truc tiep de dam bao user dang nhap duoc.
        EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO ' || r.MABN;

        EXECUTE IMMEDIATE 'GRANT RL_BENHNHAN TO ' || r.MABN;
        EXECUTE IMMEDIATE 'ALTER USER ' || r.MABN || ' DEFAULT ROLE ALL';
    END LOOP;
END;
/

PROMPT ===== 5. KIEM TRA USER / ROLE DA TAO =====

SELECT 'ROLE' AS DOI_TUONG, ROLE AS TEN, NULL AS THONG_TIN
FROM DBA_ROLES
WHERE ROLE IN ('RL_DIEUPHOI','RL_BACSI','RL_KYTHUATVIEN','RL_BENHNHAN')
ORDER BY ROLE;

SELECT GRANTED_ROLE, COUNT(*) AS SO_USER
FROM DBA_ROLE_PRIVS
WHERE GRANTED_ROLE IN ('RL_DIEUPHOI','RL_BACSI','RL_KYTHUATVIEN','RL_BENHNHAN')
GROUP BY GRANTED_ROLE
ORDER BY GRANTED_ROLE;

SELECT USERNAME, ACCOUNT_STATUS
FROM DBA_USERS
WHERE USERNAME IN (
    SELECT MANV FROM NHANVIEN
    UNION
    SELECT MABN FROM BENHNHAN
)
ORDER BY USERNAME;

PROMPT ===== HOAN TAT FILE USER / ROLE =====

PROMPT Test nhanh:
PROMPT   CONNECT BS001/ATBM123
PROMPT   SELECT * FROM <SCHEMA_OWNER>.V_MY_ACCOUNT;
PROMPT   CONNECT KTV01/ATBM123
PROMPT   SELECT * FROM <SCHEMA_OWNER>.V_MY_ACCOUNT;
PROMPT   CONNECT BN000001/ATBM123
PROMPT   SELECT * FROM <SCHEMA_OWNER>.V_MY_ACCOUNT;
