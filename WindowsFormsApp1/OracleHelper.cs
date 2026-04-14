using System;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace WindowsFormsApp1
{
    internal static class OracleHelper
    {
        public static async Task<DataTable> QueryAsync(string connectionString, string sql)
        {
            if (sql == null) throw new ArgumentNullException(nameof(sql));
            return await Task.Run(() =>
            {
                using (var conn = OracleConnectionManager.CreateConnection(connectionString))
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

        public static async Task<int> ExecuteNonQueryAsync(string connectionString, string sql)
        {
            if (sql == null) throw new ArgumentNullException(nameof(sql));
            return await Task.Run(() =>
            {
                using (var conn = OracleConnectionManager.CreateConnection(connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.BindByName = true;
                    cmd.CommandText = sql;
                    return cmd.ExecuteNonQuery();
                }
            });
        }

        public static string QuoteIdentifier(string identifier)
        {
            if (identifier == null) return "\"\"";
            var s = identifier.Trim().Replace("\"", "\"\"");
            return $"\"{s}\"";
        }

        public static string QuoteLiteral(string literal)
        {
            if (literal == null) return "''";
            var s = literal.Replace("'", "''");
            return $"'{s}'";
        }

        // IDENTIFIED BY "password"
        public static string QuotePassword(string password)
        {
            if (password == null) return "\"\"";
            var s = password.Replace("\"", "\"\"");
            return $"\"{s}\"";
        }

        // Execute stored procedure with output parameters
        public static async Task<(int result, string errorMsg)> ExecProcedureAsync(
            string connectionString, 
            string procedureName, 
            params (string paramName, object paramValue, OracleParamType paramType)[] parameters)
        {
            return await Task.Run(() =>
            {
                using (var conn = OracleConnectionManager.CreateConnection(connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.BindByName = true;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = procedureName;

                    // Add parameters
                    foreach (var param in parameters)
                    {
                        var oracleParam = cmd.CreateParameter();
                        oracleParam.ParameterName = param.paramName;
                        oracleParam.Value = param.paramValue ?? DBNull.Value;

                        if (param.paramType == OracleParamType.Output)
                        {
                            oracleParam.Direction = ParameterDirection.Output;
                            oracleParam.Size = 4000; // For string output
                        }
                        else
                        {
                            oracleParam.Direction = ParameterDirection.Input;
                        }

                        cmd.Parameters.Add(oracleParam);
                    }

                    cmd.ExecuteNonQuery();

                    // Get output parameters
                    var resultObj = cmd.Parameters["p_result"]?.Value;
                    var errorMsgObj = cmd.Parameters["p_error_msg"]?.Value;

                    int result = resultObj != null && resultObj != DBNull.Value ? (int)resultObj : -1;
                    string errorMsg = errorMsgObj != null && errorMsgObj != DBNull.Value ? errorMsgObj.ToString() : "";

                    return (result, errorMsg);
                }
            });
        }
    }

    // Enum for parameter type
    public enum OracleParamType
    {
        Input,
        Output
    }
}

