using System;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace WindowsFormsApp1
{
    internal static class OracleSql
    {
        public static async Task<DataTable> QueryAsync(string connectionString, string sql)
        {
            return await Task.Run(() =>
            {
                using (var conn = OracleDb.CreateConnection(connectionString))
                using (var cmd = conn.CreateCommand())
                using (var da = new OracleDataAdapter(cmd))
                {
                    conn.Open();
                    cmd.BindByName = true;
                    cmd.CommandText = sql;

                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            });
        }

        public static async Task<int> ExecuteAsync(string connectionString, string sql)
        {
            return await Task.Run(() =>
            {
                using (var conn = OracleDb.CreateConnection(connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.BindByName = true;
                    cmd.CommandText = sql;
                    return cmd.ExecuteNonQuery();
                }
            });
        }

        public static string Q(string identifier)
        {
            if (identifier == null) return "\"\"";
            var s = identifier.Trim().Replace("\"", "\"\"");
            return $"\"{s}\"";
        }

        public static string QLit(string literal)
        {
            if (literal == null) return "''";
            var s = literal.Replace("'", "''");
            return $"'{s}'";
        }

        /// <summary>
        /// Mật khẩu trong IDENTIFIED BY phải dùng nháy kép (không dùng nháy đơn như chuỗi SQL thường).
        /// </summary>
        public static string QPassword(string literal)
        {
            if (literal == null) return "\"\"";
            var s = literal.Replace("\"", "\"\"");
            return $"\"{s}\"";
        }
    }
}

