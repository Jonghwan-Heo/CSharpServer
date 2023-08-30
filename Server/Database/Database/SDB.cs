using Database.Context;

namespace Database
{
    public class SDB
    {
        public static GameDBContext Game;

        public static void Init()
        {
            Game = new GameDBContext();
        }
    }
}