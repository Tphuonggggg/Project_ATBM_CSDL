using System;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

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

                        if (param.paramType == OracleParamType.Output)
                        {
                            oracleParam.Direction = ParameterDirection.Output;
                            if (IsNumericOutputParameter(param.paramName, param.paramValue))
                            {
                                oracleParam.OracleDbType = OracleDbType.Decimal;
                            }
                            else
                            {
                                oracleParam.OracleDbType = OracleDbType.Varchar2;
                                oracleParam.Size = 4000;
                            }
                        }
                        else
                        {
                            oracleParam.Direction = ParameterDirection.Input;
                            if (param.paramValue is int || param.paramValue is long || param.paramValue is decimal)
                                oracleParam.OracleDbType = OracleDbType.Decimal;
                            else
                                oracleParam.OracleDbType = OracleDbType.Varchar2;
                        }

                        oracleParam.Value = param.paramValue ?? DBNull.Value;
                        cmd.Parameters.Add(oracleParam);
                    }

                    cmd.ExecuteNonQuery();

                    // Get output parameters
                    var resultObj = cmd.Parameters["p_result"]?.Value;
                    var errorMsgObj = cmd.Parameters["p_error_msg"]?.Value;

                    int result = ConvertOracleNumber(resultObj);
                    string errorMsg = ConvertOracleString(errorMsgObj);

                    return (result, errorMsg);
                }
            });
        }

        private static bool IsNumericOutputParameter(string paramName, object value)
        {
            return string.Equals(paramName, "p_result", StringComparison.OrdinalIgnoreCase)
                   || value is int
                   || value is long
                   || value is decimal;
        }

        private static int ConvertOracleNumber(object value)
        {
            if (value == null || value == DBNull.Value) return -1;
            if (value is OracleDecimal oracleDecimal)
                return oracleDecimal.IsNull ? -1 : oracleDecimal.ToInt32();
            if (value is decimal decimalValue) return Convert.ToInt32(decimalValue);
            if (value is int intValue) return intValue;
            if (value is long longValue) return Convert.ToInt32(longValue);

            return int.TryParse(value.ToString(), out var parsed) ? parsed : -1;
        }

        private static string ConvertOracleString(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            if (value is OracleString oracleString)
                return oracleString.IsNull ? "" : oracleString.Value;
            return value.ToString();
        }
    }

    // Enum for parameter type
    public enum OracleParamType
    {
        Input,
        Output
    }
}

