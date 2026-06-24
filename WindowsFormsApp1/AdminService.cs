using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    internal sealed class AdminService
    {
        private const string AdminSchema = "CQ09";
        private readonly string _connectionString;

        public AdminService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private static string AdminProcedure(string procedureName) => AdminSchema + "." + procedureName;

        // ===== USERS =====
        public Task<DataTable> GetUsersAsync()
            => OracleHelper.QueryAsync(_connectionString, "select username, account_status, created from dba_users order by username");

        public async Task CreateUserAsync(string username, string password)
        {
            var u = (username ?? string.Empty).Trim();
            if (u.Length == 0) throw new InvalidOperationException("Username không được để trống.");
            if (string.IsNullOrEmpty(password)) throw new InvalidOperationException("Password không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_create_user"),
                ("p_username", u, OracleParamType.Input),
                ("p_password", password, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi tạo user: {errorMsg}");
        }

        public async Task AlterUserPasswordAsync(string username, string newPassword)
        {
            var u = (username ?? string.Empty).Trim();
            if (u.Length == 0) throw new InvalidOperationException("Username không được để trống.");
            if (string.IsNullOrEmpty(newPassword)) throw new InvalidOperationException("Password mới không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_alter_user_password"),
                ("p_username", u, OracleParamType.Input),
                ("p_new_password", newPassword, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi thay đổi password: {errorMsg}");
        }

        public async Task DropUserAsync(string username)
        {
            var u = (username ?? string.Empty).Trim();
            if (u.Length == 0) throw new InvalidOperationException("Username không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_drop_user"),
                ("p_username", u, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi xóa user: {errorMsg}");
        }
        public async Task SetUserLockAsync(string username, bool locked)
        {
            var u = (username ?? string.Empty).Trim();
            if (u.Length == 0) throw new InvalidOperationException("Username không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_set_user_lock"),
                ("p_username", u, OracleParamType.Input),
                ("p_locked", locked ? 1 : 0, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi khóa/mở khóa user: {errorMsg}");
        }

        // ===== ROLES =====
        public Task<DataTable> GetRolesAsync()
            => OracleHelper.QueryAsync(_connectionString, "select role, authentication_type from dba_roles order by role");

        public async Task CreateRoleAsync(string roleName, bool passwordRole, string rolePassword)
        {
            var r = (roleName ?? string.Empty).Trim();
            if (r.Length == 0) throw new InvalidOperationException("Role name không được để trống.");
            if (passwordRole && string.IsNullOrEmpty(rolePassword)) 
                throw new InvalidOperationException("Vui lòng nhập password cho role.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_create_role"),
                ("p_role_name", r, OracleParamType.Input),
                ("p_is_password_role", passwordRole ? 1 : 0, OracleParamType.Input),
                ("p_role_password", rolePassword ?? "", OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi tạo role: {errorMsg}");
        }

        public async Task DropRoleAsync(string roleName)
        {
            var r = (roleName ?? string.Empty).Trim();
            if (r.Length == 0) throw new InvalidOperationException("Role name không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_drop_role"),
                ("p_role_name", r, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi xóa role: {errorMsg}");
        }

        // ===== GRANT / REVOKE =====
        public async Task GrantRoleAsync(string grantee, string role, bool withAdminOption)
        {
            var g = (grantee ?? string.Empty).Trim();
            var r = (role ?? string.Empty).Trim();
            if (g.Length == 0) throw new InvalidOperationException("Grantee không được để trống.");
            if (r.Length == 0) throw new InvalidOperationException("Role không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_grant_role"),
                ("p_grantee", g, OracleParamType.Input),
                ("p_role", r, OracleParamType.Input),
                ("p_with_admin_option", withAdminOption ? 1 : 0, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi cấp role: {errorMsg}");
        }

        public async Task RevokeRoleAsync(string grantee, string role)
        {
            var g = (grantee ?? string.Empty).Trim();
            var r = (role ?? string.Empty).Trim();
            if (g.Length == 0) throw new InvalidOperationException("Grantee không được để trống.");
            if (r.Length == 0) throw new InvalidOperationException("Role không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_revoke_role"),
                ("p_grantee", g, OracleParamType.Input),
                ("p_role", r, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi thu hồi role: {errorMsg}");
        }

        public async Task GrantSystemPrivilegeAsync(string grantee, string systemPrivilege, bool withAdminOption)
        {
            var g = (grantee ?? string.Empty).Trim();
            var p = (systemPrivilege ?? string.Empty).Trim();
            if (g.Length == 0) throw new InvalidOperationException("Grantee không được để trống.");
            if (p.Length == 0) throw new InvalidOperationException("System privilege không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_grant_system_privilege"),
                ("p_grantee", g, OracleParamType.Input),
                ("p_privilege", p, OracleParamType.Input),
                ("p_with_admin_option", withAdminOption ? 1 : 0, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi cấp system privilege: {errorMsg}");
        }

        public async Task RevokeSystemPrivilegeAsync(string grantee, string systemPrivilege)
        {
            var g = (grantee ?? string.Empty).Trim();
            var p = (systemPrivilege ?? string.Empty).Trim();
            if (g.Length == 0) throw new InvalidOperationException("Grantee không được để trống.");
            if (p.Length == 0) throw new InvalidOperationException("System privilege không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_revoke_system_privilege"),
                ("p_grantee", g, OracleParamType.Input),
                ("p_privilege", p, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi thu hồi system privilege: {errorMsg}");
        }

        // objectName: OWNER.OBJECT (không quote bằng identifier cho toàn chuỗi, vì OWNER.OBJECT cần giữ dấu chấm)
        public async Task GrantObjectPrivilegeAsync(string grantee, string privilege, string objectName, string columnsCsv, bool withGrantOption)
        {
            var g = (grantee ?? string.Empty).Trim();
            var p = (privilege ?? string.Empty).Trim();
            var obj = (objectName ?? string.Empty).Trim();
            var cols = (columnsCsv ?? string.Empty).Trim();

            if (g.Length == 0) throw new InvalidOperationException("Grantee không được để trống.");
            if (p.Length == 0) throw new InvalidOperationException("Privilege không được để trống.");
            if (obj.Length == 0) throw new InvalidOperationException("Object name không được để trống (dạng OWNER.OBJECT).");

            var parsedObject = ParseObjectName(obj);
            ValidateObjectColumns(p, cols, false);

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_grant_object_privilege"),
                ("p_grantee", g, OracleParamType.Input),
                ("p_privilege", p, OracleParamType.Input),
                ("p_object_owner", parsedObject.Owner, OracleParamType.Input),
                ("p_object_name", parsedObject.Name, OracleParamType.Input),
                ("p_columns_csv", cols, OracleParamType.Input),
                ("p_with_grant_option", withGrantOption ? 1 : 0, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi cấp object privilege: {errorMsg}");
        }

        public async Task RevokeObjectPrivilegeAsync(string grantee, string privilege, string objectName, string columnsCsv)
        {
            var g = (grantee ?? string.Empty).Trim();
            var p = (privilege ?? string.Empty).Trim();
            var obj = (objectName ?? string.Empty).Trim();
            var cols = (columnsCsv ?? string.Empty).Trim();

            if (g.Length == 0) throw new InvalidOperationException("Grantee không được để trống.");
            if (p.Length == 0) throw new InvalidOperationException("Privilege không được để trống.");
            if (obj.Length == 0) throw new InvalidOperationException("Object name không được để trống (dạng OWNER.OBJECT).");

            var parsedObject = ParseObjectName(obj);
            ValidateObjectColumns(p, cols, true);

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, AdminProcedure("sp_revoke_object_privilege"),
                ("p_grantee", g, OracleParamType.Input),
                ("p_privilege", p, OracleParamType.Input),
                ("p_object_owner", parsedObject.Owner, OracleParamType.Input),
                ("p_object_name", parsedObject.Name, OracleParamType.Input),
                ("p_columns_csv", cols, OracleParamType.Input),
                ("p_result", 0, OracleParamType.Output),
                ("p_error_msg", "", OracleParamType.Output));

            if (result != 1)
                throw new InvalidOperationException($"Lỗi thu hồi object privilege: {errorMsg}");
        }

        // ===== VIEW PRIVS (truy vấn trực tiếp Oracle, không xử lý phía client) =====
        public Task<DataTable> GetSystemPrivilegesOfGranteeAsync(string granteeUpper)
        {
            var g = NormalizeGrantee(granteeUpper);
            var sql = $@"select grantee, privilege, admin_option
from dba_sys_privs
where grantee = {OracleHelper.QuoteLiteral(g)}
order by privilege";
            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        public Task<DataTable> GetRolePrivilegesOfGranteeAsync(string granteeUpper)
        {
            var g = NormalizeGrantee(granteeUpper);
            var sql = $@"select grantee, granted_role, admin_option, default_role
from dba_role_privs
where grantee = {OracleHelper.QuoteLiteral(g)}
order by granted_role";
            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        public Task<DataTable> GetObjectPrivilegesOfGranteeAsync(string granteeUpper)
        {
            var g = NormalizeGrantee(granteeUpper);
            // Gộp quyền trên object + quyền theo cột bằng UNION ALL, Oracle tự xử lý
            var sql = $@"select grantee, owner, table_name as object_name, cast(null as varchar2(128)) as column_name,
       privilege, grantable, 'OBJECT' as priv_level
from dba_tab_privs
where grantee = {OracleHelper.QuoteLiteral(g)}
union all
select grantee, owner, table_name as object_name, column_name,
       privilege, grantable, 'COLUMN' as priv_level
from dba_col_privs
where grantee = {OracleHelper.QuoteLiteral(g)}
order by owner, object_name, priv_level, column_name, privilege";
            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        private static string NormalizeGrantee(string grantee)
        {
            var g = (grantee ?? string.Empty).Trim().ToUpperInvariant();
            if (g.Length == 0) throw new InvalidOperationException("Vui lòng nhập tên user/role.");
            return g;
        }

        private static (string Owner, string Name) ParseObjectName(string objectName)
        {
            var obj = (objectName ?? string.Empty).Trim();
            var parts = obj.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string owner;
            string name;
            if (parts.Length == 1)
            {
                owner = AdminSchema;
                name = parts[0].Trim();
            }
            else if (parts.Length == 2)
            {
                owner = parts[0].Trim();
                name = parts[1].Trim();
            }
            else
            {
                throw new InvalidOperationException("Object name phải có dạng OBJECT hoặc OWNER.OBJECT.");
            }

            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Object name phải có dạng OBJECT hoặc OWNER.OBJECT.");

            return (owner.ToUpperInvariant(), name.ToUpperInvariant());
        }

        private static void ValidateObjectColumns(string privilege, string columnsCsv, bool isRevoke)
        {
            if (string.IsNullOrWhiteSpace(columnsCsv)) return;

            var p = (privilege ?? string.Empty).Trim().ToUpperInvariant();
            if (isRevoke)
                throw new InvalidOperationException("Oracle không hỗ trợ REVOKE theo danh sách cột trong cú pháp này. Hãy để trống Columns để thu hồi quyền trên object.");
            if (p == "SELECT")
                throw new InvalidOperationException("Oracle không hỗ trợ GRANT SELECT theo từng cột. Hãy để trống Columns để cấp SELECT toàn object, hoặc tạo VIEW chỉ gồm các cột cần cho phép.");
            if (p != "UPDATE" && p != "INSERT" && p != "REFERENCES")
                throw new InvalidOperationException("Columns chỉ áp dụng cho quyền UPDATE, INSERT hoặc REFERENCES.");
        }

        // ===== OBJECT BROWSER =====
        public Task<DataTable> GetOwnersAsync()
            => OracleHelper.QueryAsync(_connectionString, "select username from dba_users order by username");

        public Task<DataTable> GetObjectsAsync(string ownerUpper, string objectType)
        {
            var owner = (ownerUpper ?? string.Empty).Trim().ToUpperInvariant();
            if (owner.Length == 0) throw new InvalidOperationException("Owner không được để trống.");
            var t = (objectType ?? "TABLE").Trim().ToUpperInvariant();

            string sql;
            if (t == "TABLE")
                sql = $"select table_name as name, 'TABLE' as type from dba_tables where owner={OracleHelper.QuoteLiteral(owner)} order by table_name";
            else if (t == "VIEW")
                sql = $"select view_name as name, 'VIEW' as type from dba_views where owner={OracleHelper.QuoteLiteral(owner)} order by view_name";
            else if (t == "PROCEDURE" || t == "FUNCTION")
                sql = $"select object_name as name, object_type as type from dba_objects where owner={OracleHelper.QuoteLiteral(owner)} and object_type={OracleHelper.QuoteLiteral(t)} order by object_name";
            else
                sql = $"select object_name as name, object_type as type from dba_objects where owner={OracleHelper.QuoteLiteral(owner)} order by object_type, object_name";

            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        public Task<DataTable> GetColumnsAsync(string ownerUpper, string tableOrViewUpper)
        {
            var owner = (ownerUpper ?? string.Empty).Trim().ToUpperInvariant();
            var name = (tableOrViewUpper ?? string.Empty).Trim().ToUpperInvariant();
            if (owner.Length == 0) throw new InvalidOperationException("Owner không được để trống.");
            if (name.Length == 0) throw new InvalidOperationException("Tên object không được để trống.");
            var sql = $"select column_id, column_name, data_type, data_length, nullable from dba_tab_columns where owner={OracleHelper.QuoteLiteral(owner)} and table_name={OracleHelper.QuoteLiteral(name)} order by column_id";
            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        // ===== AUDIT LOGS =====
        public Task<DataTable> GetStandardAuditLogsAsync(string username = null)
        {
            var userCondition = BuildAuditUserCondition("dbusername", username);
            var sql = @"
                select to_char(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') as audit_time,
                       dbusername,
                       audit_type,
                       unified_audit_policies as policy_name,
                       action_name,
                       object_schema || '.' || object_name as object_name,
                       case when return_code = 0 then 'SUCCESS' else 'FAILED' end as status,
                       return_code,
                       replace(replace(dbms_lob.substr(sql_text, 120, 1), chr(10), ' '), chr(13), ' ') as sql_text
                from unified_audit_trail
                where " + userCondition + @"
                  and unified_audit_policies like '%UA_CQ09_%'
                order by event_timestamp desc
                fetch first 100 rows only";
            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        public async Task<DataTable> GetFgaAuditLogsAsync(string username = null)
        {
            var unifiedUserCondition = BuildAuditUserCondition("dbusername", username);
            var legacyUserCondition = BuildAuditUserCondition("db_user", username);

            var unifiedSql = @"
                select to_char(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') as audit_time,
                       dbusername,
                       audit_type,
                       fga_policy_name as policy_name,
                       action_name,
                       object_schema || '.' || object_name as object_name,
                       case when return_code = 0 then 'SUCCESS' else 'FAILED' end as status,
                       return_code,
                       replace(replace(dbms_lob.substr(sql_text, 120, 1), chr(10), ' '), chr(13), ' ') as sql_text,
                       'UNIFIED_AUDIT_TRAIL' as source
                from unified_audit_trail
                where audit_type = 'FineGrainedAudit'
                  and " + unifiedUserCondition + @"
                  and object_schema = 'CQ09'
                  and fga_policy_name in ('FGA_CQ09_DONTHUOC_UPDATE', 'FGA_CQ09_HSBA_UPDATE')
                order by event_timestamp desc
                fetch first 100 rows only";

            var legacySql = @"
                select to_char(timestamp, 'YYYY-MM-DD HH24:MI:SS') as audit_time,
                       db_user as dbusername,
                       'FineGrainedAudit' as audit_type,
                       policy_name,
                       statement_type as action_name,
                       object_schema || '.' || object_name as object_name,
                       'SUCCESS' as status,
                       0 as return_code,
                       to_char(substr(sql_text, 1, 120)) as sql_text,
                       'DBA_FGA_AUDIT_TRAIL' as source
                from dba_fga_audit_trail
                where object_schema = 'CQ09'
                  and " + legacyUserCondition + @"
                  and policy_name in ('FGA_CQ09_DONTHUOC_UPDATE', 'FGA_CQ09_HSBA_UPDATE')
                order by timestamp desc
                fetch first 100 rows only";

            var result = CreateFgaAuditTable();
            await MergeAuditRowsAsync(result, unifiedSql);
            await MergeAuditRowsAsync(result, legacySql);

            var view = result.DefaultView;
            view.Sort = "AUDIT_TIME DESC, SOURCE ASC";
            return view.ToTable();
        }

        private static string BuildAuditUserCondition(string columnName, string username)
        {
            var user = (username ?? string.Empty).Trim().ToUpperInvariant();
            if (user.Length == 0 || user == "TẤT CẢ" || user == "TAT CA" || user == "ALL")
                return columnName + " in ('BS001', 'BS002', 'KTV01', 'KTV02', 'BN000001')";

            switch (user)
            {
                case "BS001":
                case "BS002":
                case "KTV01":
                case "KTV02":
                case "BN000001":
                    return columnName + " = " + OracleHelper.QuoteLiteral(user);
                default:
                    return "1 = 0";
            }
        }

        private async Task MergeAuditRowsAsync(DataTable target, string sql)
        {
            try
            {
                var source = await OracleHelper.QueryAsync(_connectionString, sql);
                foreach (DataRow row in source.Rows)
                {
                    var newRow = target.NewRow();
                    foreach (DataColumn column in target.Columns)
                        newRow[column.ColumnName] = row.Table.Columns.Contains(column.ColumnName) ? row[column.ColumnName] : DBNull.Value;
                    target.Rows.Add(newRow);
                }
            }
            catch (Exception ex)
            {
                var errorRow = target.NewRow();
                errorRow["AUDIT_TIME"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                errorRow["AUDIT_TYPE"] = "FGA_SOURCE_ERROR";
                errorRow["POLICY_NAME"] = "READ_FGA_LOG";
                errorRow["STATUS"] = "FAILED";
                errorRow["RETURN_CODE"] = -1;
                errorRow["SQL_TEXT"] = ex.Message;
                errorRow["SOURCE"] = sql.IndexOf("dba_fga_audit_trail", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "DBA_FGA_AUDIT_TRAIL"
                    : "UNIFIED_AUDIT_TRAIL";
                target.Rows.Add(errorRow);
            }
        }

        private static DataTable CreateFgaAuditTable()
        {
            var table = new DataTable();
            table.Columns.Add("AUDIT_TIME", typeof(string));
            table.Columns.Add("DBUSERNAME", typeof(string));
            table.Columns.Add("AUDIT_TYPE", typeof(string));
            table.Columns.Add("POLICY_NAME", typeof(string));
            table.Columns.Add("ACTION_NAME", typeof(string));
            table.Columns.Add("OBJECT_NAME", typeof(string));
            table.Columns.Add("STATUS", typeof(string));
            table.Columns.Add("RETURN_CODE", typeof(decimal));
            table.Columns.Add("SQL_TEXT", typeof(string));
            table.Columns.Add("SOURCE", typeof(string));
            return table;
        }

        // ===== RECOVERY / FLASHBACK =====
        public async Task<DataTable> GetRecoverableAuditAsync()
        {
            var result = CreateRecoveryAuditTable();

            var donThuocUnifiedSql = @"
                select to_char(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') as audit_time,
                       dbusername as db_user,
                       'UNIFIED_AUDIT_TRAIL' as source,
                       'DONTHUOC' as recovery_type,
                       replace(replace(dbms_lob.substr(sql_text, 1000, 1), chr(10), ' '), chr(13), ' ') as sql_text
                from unified_audit_trail
                where audit_type = 'FineGrainedAudit'
                  and object_schema = 'CQ09'
                  and object_name = 'DONTHUOC'
                  and fga_policy_name = 'FGA_CQ09_DONTHUOC_UPDATE'
                order by event_timestamp desc
                fetch first 50 rows only";

            var donThuocLegacySql = @"
                select to_char(timestamp, 'YYYY-MM-DD HH24:MI:SS') as audit_time,
                       db_user,
                       'DBA_FGA_AUDIT_TRAIL' as source,
                       'DONTHUOC' as recovery_type,
                       to_char(substr(sql_text, 1, 1000)) as sql_text
                from dba_fga_audit_trail
                where object_schema = 'CQ09'
                  and object_name = 'DONTHUOC'
                  and policy_name = 'FGA_CQ09_DONTHUOC_UPDATE'
                order by timestamp desc
                fetch first 50 rows only";

            var hsbaFgaUnifiedSql = @"
                select to_char(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') as audit_time,
                       dbusername as db_user,
                       'UNIFIED_AUDIT_TRAIL' as source,
                       'HSBA' as recovery_type,
                       replace(replace(dbms_lob.substr(sql_text, 1000, 1), chr(10), ' '), chr(13), ' ') as sql_text
                from unified_audit_trail
                where audit_type = 'FineGrainedAudit'
                  and object_schema = 'CQ09'
                  and object_name = 'HSBA'
                  and fga_policy_name = 'FGA_CQ09_HSBA_UPDATE'
                order by event_timestamp desc
                fetch first 50 rows only";

            var hsbaFgaLegacySql = @"
                select to_char(timestamp, 'YYYY-MM-DD HH24:MI:SS') as audit_time,
                       db_user,
                       'DBA_FGA_AUDIT_TRAIL' as source,
                       'HSBA' as recovery_type,
                       to_char(substr(sql_text, 1, 1000)) as sql_text
                from dba_fga_audit_trail
                where object_schema = 'CQ09'
                  and object_name = 'HSBA'
                  and policy_name = 'FGA_CQ09_HSBA_UPDATE'
                order by timestamp desc
                fetch first 50 rows only";

            var hsbaViewUnifiedSql = @"
                select to_char(event_timestamp, 'YYYY-MM-DD HH24:MI:SS') as audit_time,
                       dbusername as db_user,
                       'UNIFIED_AUDIT_TRAIL_VIEW' as source,
                       'HSBA' as recovery_type,
                       replace(replace(dbms_lob.substr(sql_text, 1000, 1), chr(10), ' '), chr(13), ' ') as sql_text
                from unified_audit_trail
                where object_schema = 'CQ09'
                  and object_name = 'VW_BACSI_HSBA'
                  and action_name = 'UPDATE'
                  and unified_audit_policies like '%UA_CQ09_%'
                order by event_timestamp desc
                fetch first 50 rows only";

            await MergeRecoveryAuditRowsAsync(result, donThuocUnifiedSql);
            await MergeRecoveryAuditRowsAsync(result, donThuocLegacySql);
            await MergeRecoveryAuditRowsAsync(result, hsbaFgaUnifiedSql);
            await MergeRecoveryAuditRowsAsync(result, hsbaFgaLegacySql);
            await MergeRecoveryAuditRowsAsync(result, hsbaViewUnifiedSql);

            var view = result.DefaultView;
            view.Sort = "AUDIT_TIME DESC, SOURCE ASC";
            return view.ToTable();
        }

        private async Task MergeRecoveryAuditRowsAsync(DataTable target, string sql)
        {
            try
            {
                var source = await OracleHelper.QueryAsync(_connectionString, sql);
                foreach (DataRow row in source.Rows)
                {
                    var auditTime = row["AUDIT_TIME"]?.ToString() ?? "";
                    var sqlText = row["SQL_TEXT"]?.ToString() ?? "";
                    var recoveryType = (row["RECOVERY_TYPE"]?.ToString() ?? "").Trim().ToUpperInvariant();
                    var restoreTs = BuildSuggestedRestoreTs(auditTime);
                    var parsed = recoveryType == "HSBA"
                        ? ParseHsbaRecoveryKeyFromSql(sqlText)
                        : ParseDonThuocRecoveryKeyFromSql(sqlText);

                    if (string.IsNullOrWhiteSpace(parsed.Mahsba) || string.IsNullOrWhiteSpace(restoreTs))
                        continue;
                    if (recoveryType == "DONTHUOC" &&
                        (string.IsNullOrWhiteSpace(parsed.Ngaydt) || string.IsNullOrWhiteSpace(parsed.Tenthuoc)))
                        continue;

                    var newRow = target.NewRow();
                    newRow["RECOVERY_TYPE"] = recoveryType;
                    newRow["CAN_RESTORE"] = "YES";
                    newRow["AUDIT_TIME"] = auditTime;
                    newRow["DB_USER"] = row["DB_USER"]?.ToString() ?? "";
                    newRow["SOURCE"] = row["SOURCE"]?.ToString() ?? "";
                    newRow["SUGGESTED_RESTORE_TS"] = restoreTs;
                    newRow["MAHSBA"] = parsed.Mahsba;
                    newRow["NGAYDT"] = parsed.Ngaydt;
                    newRow["TENTHUOC"] = parsed.Tenthuoc;
                    newRow["RESTORE_KEY"] = BuildRestoreKey(recoveryType, parsed.Mahsba, parsed.Ngaydt, parsed.Tenthuoc);
                    newRow["SQL_TEXT"] = sqlText;
                    target.Rows.Add(newRow);
                }
            }
            catch (Exception ex)
            {
                // Recovery only lists rows that can be restored. Source errors stay out of
                // the grid so they are not mistaken for recoverable audit rows.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private static DataTable CreateRecoveryAuditTable()
        {
            var table = new DataTable();
            table.Columns.Add("RECOVERY_TYPE", typeof(string));
            table.Columns.Add("CAN_RESTORE", typeof(string));
            table.Columns.Add("AUDIT_TIME", typeof(string));
            table.Columns.Add("DB_USER", typeof(string));
            table.Columns.Add("SOURCE", typeof(string));
            table.Columns.Add("SUGGESTED_RESTORE_TS", typeof(string));
            table.Columns.Add("RESTORE_KEY", typeof(string));
            table.Columns.Add("MAHSBA", typeof(string));
            table.Columns.Add("NGAYDT", typeof(string));
            table.Columns.Add("TENTHUOC", typeof(string));
            table.Columns.Add("SQL_TEXT", typeof(string));
            return table;
        }

        private static string BuildSuggestedRestoreTs(string auditTime)
        {
            DateTime parsed;
            if (!DateTime.TryParseExact(auditTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return "";
            return parsed.AddSeconds(-10).ToString("yyyy-MM-dd HH:mm:ss");
        }

        private static (string Mahsba, string Ngaydt, string Tenthuoc) ParseDonThuocRecoveryKeyFromSql(string sqlText)
        {
            var mahsba = MatchSqlValue(sqlText, @"(?:[A-Z0-9_]+\.)?MAHSBA\s*=\s*'([^']*)'");
            var tenthuoc = MatchSqlValue(sqlText, @"(?:[A-Z0-9_]+\.)?TENTHUOC\s*=\s*N?'([^']*)'");
            var ngaydt = MatchSqlValue(sqlText, @"(?:[A-Z0-9_]+\.)?NGAYDT\s*=\s*TO_DATE\('([^']*)'\s*,\s*'DD/MM/YYYY'\)");
            if (!string.IsNullOrWhiteSpace(ngaydt))
            {
                DateTime parsedDate;
                if (DateTime.TryParseExact(ngaydt, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    ngaydt = parsedDate.ToString("yyyy-MM-dd");
            }
            else
            {
                ngaydt = MatchSqlValue(sqlText, @"(?:[A-Z0-9_]+\.)?NGAYDT\s*=\s*DATE\s*'([^']*)'");
            }
            return (mahsba, ngaydt, tenthuoc);
        }

        private static (string Mahsba, string Ngaydt, string Tenthuoc) ParseHsbaRecoveryKeyFromSql(string sqlText)
        {
            var mahsba = MatchSqlValue(sqlText, @"(?:[A-Z0-9_]+\.)?MAHSBA\s*=\s*'([^']*)'");
            return (mahsba, "", "");
        }

        private static string BuildRestoreKey(string recoveryType, string maHsba, string ngayDt, string tenThuoc)
        {
            if (string.Equals(recoveryType, "DONTHUOC", StringComparison.OrdinalIgnoreCase))
                return $"MAHSBA={maHsba}; NGAYDT={ngayDt}; TENTHUOC={tenThuoc}";
            return $"MAHSBA={maHsba}";
        }

        private static string MatchSqlValue(string text, string pattern)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Replace("''", "'") : "";
        }

        public Task<DataTable> GetCurrentDonThuocAsync(string maHsba, string ngayDt, string tenThuoc)
        {
            ValidateDonThuocKey(maHsba, ngayDt, tenThuoc);
            var sql = $@"
                select mahsba,
                       to_char(ngaydt, 'YYYY-MM-DD') as ngaydt,
                       tenthuoc,
                       lieudung
                from CQ09.DONTHUOC
                where mahsba = {OracleHelper.QuoteLiteral(maHsba.Trim())}
                  and ngaydt = date {OracleHelper.QuoteLiteral(ngayDt.Trim())}
                  and tenthuoc = N{OracleHelper.QuoteLiteral(tenThuoc.Trim())}";
            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        public Task<DataTable> GetFlashbackDonThuocAsync(string maHsba, string ngayDt, string tenThuoc, string restoreTs)
        {
            ValidateDonThuocKey(maHsba, ngayDt, tenThuoc);
            ValidateRestoreTs(restoreTs);
            var sql = $@"
                select mahsba,
                       to_char(ngaydt, 'YYYY-MM-DD') as ngaydt,
                       tenthuoc,
                       lieudung
                from CQ09.DONTHUOC as of timestamp to_timestamp({OracleHelper.QuoteLiteral(restoreTs.Trim())}, 'YYYY-MM-DD HH24:MI:SS')
                where mahsba = {OracleHelper.QuoteLiteral(maHsba.Trim())}
                  and ngaydt = date {OracleHelper.QuoteLiteral(ngayDt.Trim())}
                  and tenthuoc = N{OracleHelper.QuoteLiteral(tenThuoc.Trim())}";
            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        public async Task<int> FlashRestoreDonThuocAsync(string maHsba, string ngayDt, string tenThuoc, string restoreTs)
        {
            ValidateDonThuocKey(maHsba, ngayDt, tenThuoc);
            ValidateRestoreTs(restoreTs);
            var sql = $@"
                update CQ09.DONTHUOC d
                set d.LIEUDUNG = (
                    select old.LIEUDUNG
                    from CQ09.DONTHUOC as of timestamp to_timestamp({OracleHelper.QuoteLiteral(restoreTs.Trim())}, 'YYYY-MM-DD HH24:MI:SS') old
                    where old.MAHSBA = d.MAHSBA
                      and old.NGAYDT = d.NGAYDT
                      and old.TENTHUOC = d.TENTHUOC
                )
                where d.MAHSBA = {OracleHelper.QuoteLiteral(maHsba.Trim())}
                  and d.NGAYDT = date {OracleHelper.QuoteLiteral(ngayDt.Trim())}
                  and d.TENTHUOC = N{OracleHelper.QuoteLiteral(tenThuoc.Trim())}
                  and exists (
                      select 1
                      from CQ09.DONTHUOC as of timestamp to_timestamp({OracleHelper.QuoteLiteral(restoreTs.Trim())}, 'YYYY-MM-DD HH24:MI:SS') old
                      where old.MAHSBA = d.MAHSBA
                        and old.NGAYDT = d.NGAYDT
                        and old.TENTHUOC = d.TENTHUOC
                  )";
            var rows = await OracleHelper.ExecuteNonQueryAsync(_connectionString, sql);
            await OracleHelper.ExecuteNonQueryAsync(_connectionString, "commit");
            return rows;
        }

        public Task<DataTable> GetCurrentHsbaAsync(string maHsba)
        {
            ValidateHsbaKey(maHsba);
            var sql = $@"
                select mahsba,
                       chandoan,
                       dieutri,
                       ketluan
                from CQ09.HSBA
                where mahsba = {OracleHelper.QuoteLiteral(maHsba.Trim())}";
            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        public Task<DataTable> GetFlashbackHsbaAsync(string maHsba, string restoreTs)
        {
            ValidateHsbaKey(maHsba);
            ValidateRestoreTs(restoreTs);
            var sql = $@"
                select mahsba,
                       chandoan,
                       dieutri,
                       ketluan
                from CQ09.HSBA as of timestamp to_timestamp({OracleHelper.QuoteLiteral(restoreTs.Trim())}, 'YYYY-MM-DD HH24:MI:SS')
                where mahsba = {OracleHelper.QuoteLiteral(maHsba.Trim())}";
            return OracleHelper.QueryAsync(_connectionString, sql);
        }

        public async Task<int> FlashRestoreHsbaAsync(string maHsba, string restoreTs)
        {
            ValidateHsbaKey(maHsba);
            ValidateRestoreTs(restoreTs);
            var sql = $@"
                update CQ09.HSBA h
                set (h.CHANDOAN, h.DIEUTRI, h.KETLUAN) = (
                    select old.CHANDOAN, old.DIEUTRI, old.KETLUAN
                    from CQ09.HSBA as of timestamp to_timestamp({OracleHelper.QuoteLiteral(restoreTs.Trim())}, 'YYYY-MM-DD HH24:MI:SS') old
                    where old.MAHSBA = h.MAHSBA
                )
                where h.MAHSBA = {OracleHelper.QuoteLiteral(maHsba.Trim())}
                  and exists (
                      select 1
                      from CQ09.HSBA as of timestamp to_timestamp({OracleHelper.QuoteLiteral(restoreTs.Trim())}, 'YYYY-MM-DD HH24:MI:SS') old
                      where old.MAHSBA = h.MAHSBA
                  )";
            var rows = await OracleHelper.ExecuteNonQueryAsync(_connectionString, sql);
            await OracleHelper.ExecuteNonQueryAsync(_connectionString, "commit");
            return rows;
        }

        private static void ValidateDonThuocKey(string maHsba, string ngayDt, string tenThuoc)
        {
            if (string.IsNullOrWhiteSpace(maHsba))
                throw new InvalidOperationException("MAHSBA khong duoc de trong.");
            if (string.IsNullOrWhiteSpace(ngayDt))
                throw new InvalidOperationException("Ngay don thuoc khong duoc de trong.");
            if (string.IsNullOrWhiteSpace(tenThuoc))
                throw new InvalidOperationException("Ten thuoc khong duoc de trong.");
            DateTime parsed;
            if (!DateTime.TryParseExact(ngayDt.Trim(), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsed))
                throw new InvalidOperationException("Ngay don thuoc phai co dang YYYY-MM-DD.");
        }

        private static void ValidateHsbaKey(string maHsba)
        {
            if (string.IsNullOrWhiteSpace(maHsba))
                throw new InvalidOperationException("MAHSBA khong duoc de trong.");
        }

        private static void ValidateRestoreTs(string restoreTs)
        {
            if (string.IsNullOrWhiteSpace(restoreTs))
                throw new InvalidOperationException("Restore timestamp khong duoc de trong.");
            DateTime parsed;
            if (!DateTime.TryParseExact(restoreTs.Trim(), "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out parsed))
                throw new InvalidOperationException("Restore timestamp phai co dang YYYY-MM-DD HH24:MI:SS.");
        }
    }
}

