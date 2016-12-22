using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Zopa.ServiceDiagnostics.Health.Checks
{
    public class SqlHealthCheck : IAmAHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _connectionName;

        public SqlHealthCheck(string connectionString, string connectionName)
        {
            _connectionString = connectionString;
            _connectionName = connectionName;
        }

        public string Name => $"Sql connection check for {_connectionName}";

        public async Task ExecuteAsync(Guid correlationId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var command = new SqlCommand("select 1", conn);

                await command.ExecuteScalarAsync();
            }
        }
    }
}
