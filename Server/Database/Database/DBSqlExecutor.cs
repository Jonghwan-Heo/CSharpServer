using Dapper;
using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Database
{
    public class DBSqlExecutor
    {
        //private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Action<IDbConnection> _closeConnection;
        private Func<IDbConnection> _getConnection;

        public DBSqlExecutor(string host, string dbName, string dbUserId, string dbPassword, int dbPort)
        {
            _getConnection = delegate
            {
                return new MySqlConnection($"Server={host};Database={dbName};Uid={dbUserId};Pwd={dbPassword};SslMode=none;Convert Zero Datetime=true;Port={dbPort}");
            };

            _closeConnection = delegate (IDbConnection db)
            {
                if (db.Database != null)
                    db.Dispose();
            };

            SimpleCRUD.SetDialect(SimpleCRUD.Dialect.MySQL);

            WithSessionAsync(x =>
            {
                return x.QueryAsync<int>("SELECT 1");
            }).Wait();

            //logger.Info($"{GetType().Name} is running...");
        }

        public async Task<T> WithSessionAsync<T>(Func<IDbConnection, Task<T>> work)
        {
            var db = _getConnection();

            try
            {
                return await work(db);
            }
            finally
            {
                _closeConnection?.Invoke(db);
            }
        }
    }
}