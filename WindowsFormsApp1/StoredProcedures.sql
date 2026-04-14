-- =====================================================
-- STORED PROCEDURES FOR ORACLE DB ADMIN SERVICE
-- =====================================================

-- ===== USER MANAGEMENT PROCEDURES =====

-- Create a new user
CREATE OR REPLACE PROCEDURE sp_create_user(
    p_username IN VARCHAR2,
    p_password IN VARCHAR2,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    EXECUTE IMMEDIATE 'CREATE USER "' || REPLACE(p_username, '"', '""') || '" IDENTIFIED BY "' || REPLACE(p_password, '"', '""') || '"';
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_create_user;
/

-- Alter user password
CREATE OR REPLACE PROCEDURE sp_alter_user_password(
    p_username IN VARCHAR2,
    p_new_password IN VARCHAR2,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    EXECUTE IMMEDIATE 'ALTER USER "' || REPLACE(p_username, '"', '""') || '" IDENTIFIED BY "' || REPLACE(p_new_password, '"', '""') || '"';
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_alter_user_password;
/

-- Drop user
CREATE OR REPLACE PROCEDURE sp_drop_user(
    p_username IN VARCHAR2,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    EXECUTE IMMEDIATE 'DROP USER "' || REPLACE(p_username, '"', '""') || '" CASCADE';
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_drop_user;
/

-- Lock/Unlock user account
CREATE OR REPLACE PROCEDURE sp_set_user_lock(
    p_username IN VARCHAR2,
    p_locked IN NUMBER,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
    v_lock_status VARCHAR2(20);
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    IF p_locked = 1 THEN
        v_lock_status := 'LOCK';
    ELSE
        v_lock_status := 'UNLOCK';
    END IF;
    
    EXECUTE IMMEDIATE 'ALTER USER "' || REPLACE(p_username, '"', '""') || '" ACCOUNT ' || v_lock_status;
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_set_user_lock;
/

-- ===== ROLE MANAGEMENT PROCEDURES =====

-- Create role
CREATE OR REPLACE PROCEDURE sp_create_role(
    p_role_name IN VARCHAR2,
    p_is_password_role IN NUMBER,
    p_role_password IN VARCHAR2,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
    v_sql VARCHAR2(1000);
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    IF p_is_password_role = 1 THEN
        v_sql := 'CREATE ROLE "' || REPLACE(p_role_name, '"', '""') || '" IDENTIFIED BY "' || REPLACE(p_role_password, '"', '""') || '"';
    ELSE
        v_sql := 'CREATE ROLE "' || REPLACE(p_role_name, '"', '""') || '"';
    END IF;
    
    EXECUTE IMMEDIATE v_sql;
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_create_role;
/

-- Drop role
CREATE OR REPLACE PROCEDURE sp_drop_role(
    p_role_name IN VARCHAR2,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    EXECUTE IMMEDIATE 'DROP ROLE "' || REPLACE(p_role_name, '"', '""') || '"';
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_drop_role;
/

-- ===== PRIVILEGE GRANT PROCEDURES =====

-- Grant role to user/role
CREATE OR REPLACE PROCEDURE sp_grant_role(
    p_grantee IN VARCHAR2,
    p_role IN VARCHAR2,
    p_with_admin_option IN NUMBER,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
    v_sql VARCHAR2(1000);
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    v_sql := 'GRANT "' || REPLACE(p_role, '"', '""') || '" TO "' || REPLACE(p_grantee, '"', '""') || '"';
    
    IF p_with_admin_option = 1 THEN
        v_sql := v_sql || ' WITH ADMIN OPTION';
    END IF;
    
    EXECUTE IMMEDIATE v_sql;
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_grant_role;
/

-- Revoke role from user/role
CREATE OR REPLACE PROCEDURE sp_revoke_role(
    p_grantee IN VARCHAR2,
    p_role IN VARCHAR2,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    EXECUTE IMMEDIATE 'REVOKE "' || REPLACE(p_role, '"', '""') || '" FROM "' || REPLACE(p_grantee, '"', '""') || '"';
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_revoke_role;
/

-- Grant system privilege
CREATE OR REPLACE PROCEDURE sp_grant_system_privilege(
    p_grantee IN VARCHAR2,
    p_privilege IN VARCHAR2,
    p_with_admin_option IN NUMBER,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
    v_sql VARCHAR2(1000);
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    v_sql := 'GRANT ' || p_privilege || ' TO "' || REPLACE(p_grantee, '"', '""') || '"';
    
    IF p_with_admin_option = 1 THEN
        v_sql := v_sql || ' WITH ADMIN OPTION';
    END IF;
    
    EXECUTE IMMEDIATE v_sql;
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_grant_system_privilege;
/

-- Revoke system privilege
CREATE OR REPLACE PROCEDURE sp_revoke_system_privilege(
    p_grantee IN VARCHAR2,
    p_privilege IN VARCHAR2,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    EXECUTE IMMEDIATE 'REVOKE ' || p_privilege || ' FROM "' || REPLACE(p_grantee, '"', '""') || '"';
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_revoke_system_privilege;
/

-- Grant object privilege (with column support)
CREATE OR REPLACE PROCEDURE sp_grant_object_privilege(
    p_grantee IN VARCHAR2,
    p_privilege IN VARCHAR2,
    p_object_owner IN VARCHAR2,
    p_object_name IN VARCHAR2,
    p_columns_csv IN VARCHAR2,
    p_with_grant_option IN NUMBER,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
    v_sql VARCHAR2(2000);
    v_privilege_upper VARCHAR2(100);
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    v_privilege_upper := UPPER(TRIM(p_privilege));
    
    -- Build the GRANT statement
    IF (v_privilege_upper = 'SELECT' OR v_privilege_upper = 'UPDATE') AND p_columns_csv IS NOT NULL AND TRIM(p_columns_csv) != '' THEN
        -- Grant with columns
        v_sql := 'GRANT ' || UPPER(p_privilege) || '(' || p_columns_csv || ') ON ' || 
                 UPPER(p_object_owner) || '.' || UPPER(p_object_name) || ' TO "' || 
                 REPLACE(p_grantee, '"', '""') || '"';
    ELSE
        -- Grant without columns
        v_sql := 'GRANT ' || UPPER(p_privilege) || ' ON ' || 
                 UPPER(p_object_owner) || '.' || UPPER(p_object_name) || ' TO "' || 
                 REPLACE(p_grantee, '"', '""') || '"';
    END IF;
    
    IF p_with_grant_option = 1 THEN
        v_sql := v_sql || ' WITH GRANT OPTION';
    END IF;
    
    EXECUTE IMMEDIATE v_sql;
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_grant_object_privilege;
/

-- Revoke object privilege (with column support)
CREATE OR REPLACE PROCEDURE sp_revoke_object_privilege(
    p_grantee IN VARCHAR2,
    p_privilege IN VARCHAR2,
    p_object_owner IN VARCHAR2,
    p_object_name IN VARCHAR2,
    p_columns_csv IN VARCHAR2,
    p_result OUT NUMBER,
    p_error_msg OUT VARCHAR2
) AS
    v_sql VARCHAR2(2000);
    v_privilege_upper VARCHAR2(100);
BEGIN
    p_result := 0;
    p_error_msg := '';
    
    v_privilege_upper := UPPER(TRIM(p_privilege));
    
    -- Build the REVOKE statement
    IF (v_privilege_upper = 'SELECT' OR v_privilege_upper = 'UPDATE') AND p_columns_csv IS NOT NULL AND TRIM(p_columns_csv) != '' THEN
        -- Revoke with columns
        v_sql := 'REVOKE ' || UPPER(p_privilege) || '(' || p_columns_csv || ') ON ' || 
                 UPPER(p_object_owner) || '.' || UPPER(p_object_name) || ' FROM "' || 
                 REPLACE(p_grantee, '"', '""') || '"';
    ELSE
        -- Revoke without columns
        v_sql := 'REVOKE ' || UPPER(p_privilege) || ' ON ' || 
                 UPPER(p_object_owner) || '.' || UPPER(p_object_name) || ' FROM "' || 
                 REPLACE(p_grantee, '"', '""') || '"';
    END IF;
    
    EXECUTE IMMEDIATE v_sql;
    
    p_result := 1;
EXCEPTION
    WHEN OTHERS THEN
        p_result := -1;
        p_error_msg := SQLERRM;
END sp_revoke_object_privilege;
/
