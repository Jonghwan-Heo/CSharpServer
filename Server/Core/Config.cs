using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Core
{
    public class Config
    {
        public static bool Debug;
        public static string GameDbHost;
        public static string GameDbName;
        public static int GameDbPort = 3306;
        public static string GameDbUserId;
        public static string GameDbUserPassword;
        public static int ServerPort = 27001;
        public static bool UseDump = true;
        //private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetApplicationRoot()
        {
            return Directory.GetCurrentDirectory();
        }

        public static void Init()
        {
            var data = new Dictionary<string, string>();
            var done = false;
            if (!done)
            {
                try
                {
                    foreach (var row in File.ReadAllLines(Path.Combine(GetApplicationRoot(), "server.properties")))
                        data[row.Split('=')[0].Trim()] = string.Join("=", row.Split('=').Skip(1).ToArray()).Trim();
                    done = true;
                }
                catch (Exception ex)
                {
                    //logger.Warn("", ex);
                }
            }

            if (data.TryGetValue("server.port", out var text))
                ServerPort = int.Parse(text);
            if (data.TryGetValue("server.debug", out text))
                Debug = bool.Parse(text);
            if (data.TryGetValue("server.use_dump", out text))
                UseDump = bool.Parse(text);

            GameDbHost = data.GetValueOrDefault("game_db.host", GameDbHost);
            GameDbName = data.GetValueOrDefault("game_db.name", GameDbName);
            GameDbUserId = data.GetValueOrDefault("game_db.user_id", GameDbUserId);
            GameDbUserPassword = data.GetValueOrDefault("game_db.user_password", GameDbUserPassword);
            GameDbPort = int.Parse(data.GetValueOrDefault("game_db.port", GameDbPort.ToString()));
        }

        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static string WorkingDir => Directory.GetCurrentDirectory();
    }
}