-- =============================================================================
-- FILE: 09_audit_read_logs.sql
-- ĐỀ TÀI: ĐỒ ÁN AN TOÀN BẢO MẬT HỆ THỐNG THÔNG TIN
-- CHỨC NĂNG:
--   - Truy vấn và hiển thị nhật ký kiểm toán hệ thống (Audit Logs) đã ghi nhận.
--   - Đọc Standard Audit từ bảng dba_audit_trail (các hành vi truy vấn và các hành vi lỗi).
--   - Đọc Fine-Grained Audit từ bảng dba_fga_audit_trail (các câu lệnh SQL chi tiết
--     can thiệp vào các cột nhạy cảm).
--   - Phân tích các lệnh lỗi quan trọng (Returncode khác 0) để phát hiện tấn công/truy cập trái phép.
-- TÀI KHOẢN THỰC THI: CQ09 (Quản trị viên dự án)
-- THỨ TỰ THỰC THI: Chạy sau khi test các thao tác nghiệp vụ để kiểm tra log (Bước 9).
-- =============================================================================

SET DEFINE OFF;
SET LINESIZE 260;
SET PAGESIZE 200;
SET LONG 20000;
SET LONGCHUNKSIZE 20000;

COLUMN audit_time FORMAT A19;
COLUMN dbusername FORMAT A12;
COLUMN audit_type FORMAT A18;
COLUMN policy_name FORMAT A34;
COLUMN action_name FORMAT A14;
COLUMN object_full_name FORMAT A34;
COLUMN status FORMAT A10;
COLUMN return_code FORMAT 999999;
COLUMN total FORMAT 999999;
COLUMN sql_text_short FORMAT A95;
COLUMN enabled_option FORMAT A16;
COLUMN entity_name FORMAT A18;
COLUMN success FORMAT A7;
COLUMN failure FORMAT A7;

BEGIN
    EXECUTE IMMEDIATE 'ALTER SESSION SET CONTAINER = XEPDB1';
EXCEPTION
    WHEN OTHERS THEN NULL;
END;
/

PROMPT ===== 1. POLICY DANG ENABLE =====

SELECT policy_name,
       enabled_option,
       entity_name,
       success,
       failure
FROM audit_unified_enabled_policies
WHERE policy_name LIKE 'UA_CQ09_%'
ORDER BY policy_name, entity_name;

PROMPT ===== 2. TONG HOP LOG THEO POLICY / USER / THANH CONG-THAT BAI =====

SELECT NVL(unified_audit_policies, fga_policy_name) AS policy_name,
       dbusername,
       CASE WHEN return_code = 0 THEN 'SUCCESS' ELSE 'FAILED' END AS status,
       COUNT(*) AS total
FROM unified_audit_trail
WHERE dbusername IN ('BS001', 'BS002', 'KTV01', 'KTV02', 'BN000001')
  AND (
        unified_audit_policies LIKE '%UA_CQ09_%'
        OR fga_policy_name LIKE 'FGA_CQ09_%'
      )
GROUP BY NVL(unified_audit_policies, fga_policy_name),
         dbusername,
         CASE WHEN return_code = 0 THEN 'SUCCESS' ELSE 'FAILED' END
ORDER BY policy_name, dbusername, status;

PROMPT ===== 3. CHI TIET 100 LOG MOI NHAT CUA NHOM =====

SELECT TO_CHAR(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       dbusername,
       audit_type,
       NVL(unified_audit_policies, fga_policy_name) AS policy_name,
       action_name,
       object_schema || '.' || object_name AS object_full_name,
       CASE WHEN return_code = 0 THEN 'SUCCESS' ELSE 'FAILED' END AS status,
       return_code,
       REPLACE(REPLACE(DBMS_LOB.SUBSTR(sql_text, 95, 1), CHR(10), ' '), CHR(13), ' ') AS sql_text_short
FROM unified_audit_trail
WHERE dbusername IN ('BS001', 'BS002', 'KTV01', 'KTV02', 'BN000001')
  AND (
        unified_audit_policies LIKE '%UA_CQ09_%'
        OR fga_policy_name LIKE 'FGA_CQ09_%'
      )
ORDER BY event_timestamp DESC
FETCH FIRST 100 ROWS ONLY;

PROMPT ===== 4. RIENG CAC THAO TAC THAT BAI / VI PHAM QUYEN =====
PROMPT RETURN_CODE = 0 la thanh cong; RETURN_CODE khac 0 la ma loi Oracle.

SELECT TO_CHAR(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       dbusername,
       audit_type,
       NVL(unified_audit_policies, fga_policy_name) AS policy_name,
       action_name,
       object_schema || '.' || object_name AS object_full_name,
       return_code,
       REPLACE(REPLACE(DBMS_LOB.SUBSTR(sql_text, 95, 1), CHR(10), ' '), CHR(13), ' ') AS sql_text_short
FROM unified_audit_trail
WHERE dbusername IN ('BS001', 'BS002', 'KTV01', 'KTV02', 'BN000001')
  AND return_code <> 0
  AND (
        unified_audit_policies LIKE '%UA_CQ09_%'
        OR object_schema = 'CQ09'
      )
ORDER BY event_timestamp DESC
FETCH FIRST 50 ROWS ONLY;

PROMPT ===== 5A. RIENG FGA - PURE UNIFIED AUDITING - UNIFIED_AUDIT_TRAIL =====

SELECT TO_CHAR(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       dbusername,
       audit_type,
       fga_policy_name AS policy_name,
       action_name,
       object_schema || '.' || object_name AS object_full_name,
       CASE WHEN return_code = 0 THEN 'SUCCESS' ELSE 'FAILED' END AS status,
       return_code,
       REPLACE(REPLACE(DBMS_LOB.SUBSTR(sql_text, 95, 1), CHR(10), ' '), CHR(13), ' ') AS sql_text_short
FROM unified_audit_trail
WHERE audit_type = 'FineGrainedAudit'
  AND object_schema = 'CQ09'
  AND fga_policy_name LIKE 'FGA_CQ09_%'
ORDER BY event_timestamp DESC
FETCH FIRST 50 ROWS ONLY;

PROMPT ===== 5B. RIENG FGA - MIXED MODE - DBA_FGA_AUDIT_TRAIL =====

SELECT TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       db_user AS dbusername,
       'FineGrainedAudit' AS audit_type,
       policy_name,
       statement_type AS action_name,
       object_schema || '.' || object_name AS object_full_name,
       'SUCCESS' AS status,
       0 AS return_code,
       REPLACE(REPLACE(TO_CHAR(SUBSTR(sql_text, 1, 95)), CHR(10), ' '), CHR(13), ' ') AS sql_text_short
FROM dba_fga_audit_trail
WHERE object_schema = 'CQ09'
  AND policy_name LIKE 'FGA_CQ09_%'
ORDER BY timestamp DESC
FETCH FIRST 50 ROWS ONLY;

PROMPT ===== 6. RAW VIEW HO TRO DOI CHIEU NHANH - UNIFIED_AUDIT_TRAIL =====

SELECT TO_CHAR(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       dbusername,
       audit_type,
       unified_audit_policies,
       fga_policy_name,
       action_name,
       object_schema,
       object_name,
       return_code
FROM unified_audit_trail
WHERE dbusername IN ('BS001', 'BS002', 'KTV01', 'KTV02', 'BN000001')
  AND (
        object_schema = 'CQ09'
        OR unified_audit_policies LIKE '%UA_CQ09_%'
        OR fga_policy_name LIKE 'FGA_CQ09_%'
      )
ORDER BY event_timestamp DESC
FETCH FIRST 100 ROWS ONLY;
