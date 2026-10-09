using System.Data;

namespace FinanceTracker.Application.Interfaces.Persistence;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}