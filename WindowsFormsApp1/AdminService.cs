using System;
using System.Data;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    internal sealed class AdminService
    {
        private readonly string _connectionString;

        public AdminService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // ===== USERS =====
        public Task<DataTable> GetUsersAsync()
            => OracleHelper.QueryAsync(_connectionString, "select username, account_status, created from dba_users order by username");

        public async Task CreateUserAsync(string username, string password)
        {
            var u = (username ?? string.Empty).Trim();
            if (u.Length == 0) throw new InvalidOperationException("Username không được để trống.");
            if (string.IsNullOrEmpty(password)) throw new InvalidOperationException("Password không được để trống.");

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_create_user",
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

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_alter_user_password",
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

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_drop_user",
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

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_set_user_lock",
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

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_create_role",
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

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_drop_role",
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

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_grant_role",
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

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_revoke_role",
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

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_grant_system_privilege",
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

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_revoke_system_privilege",
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

            // Parse owner and object name from "OWNER.OBJECT"
            var parts = obj.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var owner = parts.Length > 0 ? parts[0] : "";
            var objectName2 = parts.Length > 1 ? parts[1] : "";

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_grant_object_privilege",
                ("p_grantee", g, OracleParamType.Input),
                ("p_privilege", p, OracleParamType.Input),
                ("p_object_owner", owner, OracleParamType.Input),
                ("p_object_name", objectName2, OracleParamType.Input),
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

            // Parse owner and object name from "OWNER.OBJECT"
            var parts = obj.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var owner = parts.Length > 0 ? parts[0] : "";
            var objectName2 = parts.Length > 1 ? parts[1] : "";

            var (result, errorMsg) = await OracleHelper.ExecProcedureAsync(_connectionString, "sp_revoke_object_privilege",
                ("p_grantee", g, OracleParamType.Input),
                ("p_privilege", p, OracleParamType.Input),
                ("p_object_owner", owner, OracleParamType.Input),
                ("p_object_name", objectName2, OracleParamType.Input),
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
    }
}

