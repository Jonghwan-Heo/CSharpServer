using Core;
using Database.Mappers;

namespace Database.Context
{
    public class GameDBContext
    {
        private readonly GamePlayerMapper _player;

        public GamePlayerMapper Player => _player;

        public GameDBContext()
        {
            var executor = new DBSqlExecutor(Config.GameDbHost, Config.GameDbName, Config.GameDbUserId, Config.GameDbUserPassword, Config.GameDbPort);
            _player = new GamePlayerMapper(executor);
        }
    }
}
