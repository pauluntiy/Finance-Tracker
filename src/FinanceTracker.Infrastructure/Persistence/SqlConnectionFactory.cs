using System.Data;
using FinanceTracker.Application.Interfaces.Persistence;
using Npgsql;

namespace FinanceTracker.Infrastructure.Persistence;

public sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}