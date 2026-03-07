using System.Data.Common;
using StackExchange.Profiling.Data;

namespace Playarr.Core.Datastore;

public static class ProfiledImplementations
{
    public class NpgSqlConnection : ProfiledDbConnection
    {
        public NpgSqlConnection(DbConnection connection, IDbProfiler profiler)
            : base(connection, profiler)
        {
        }
    }
}
