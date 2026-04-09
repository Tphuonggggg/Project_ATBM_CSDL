using System;
using System.Configuration;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace WindowsFormsApp1
{
    internal static class OracleConnectionManager
    {
        public static string GetConnectionStringFromConfig(string name = "OracleDb")
        {
            var cs = ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException($"Không tìm thấy connection string '{name}' trong App.config.");
            return cs;
        }

        public static string BuildConnectionString(string host, int port, string serviceName, string userId, string password, bool asSysdba = false)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Máy chủ (host) không được để trống.", nameof(host));
            if (port <= 0) throw new ArgumentOutOfRangeException(nameof(port), "Port phải > 0.");
            if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException("Service name không được rỗng.", nameof(serviceName));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("Tài khoản không được để trống.", nameof(userId));

            var dataSource = $"{host.Trim()}:{port}/{serviceName.Trim()}";
            var cs = $"User Id={userId.Trim()};Password={password ?? string.Empty};Data Source={dataSource};";
            if (asSysdba) cs += "DBA Privilege=SYSDBA;";
            return cs;
        }

        public static string WithSysdba(string connectionString, bool asSysdba)
        {
            if (!asSysdba) return connectionString;
            if (string.IsNullOrWhiteSpace(connectionString)) return connectionString;
            if (connectionString.IndexOf("DBA Privilege=SYSDBA", StringComparison.OrdinalIgnoreCase) >= 0)
                return connectionString;
            return connectionString.TrimEnd() + (connectionString.TrimEnd().EndsWith(";") ? "" : ";") + "DBA Privilege=SYSDBA;";
        }

        public static OracleConnection CreateConnection(string connectionString) => new OracleConnection(connectionString);

        public static async Task TestConnectionAsync(string connectionString)
        {
            await Task.Run(() =>
            {
                using (var conn = CreateConnection(connectionString))
                {
                    conn.Open();
                }
            });
        }
    }
}

