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
        public static bool Debug = true;
        public static int ServerId = 0;
        public static int ServerPort = 27001;
        
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                    logger.Warn("", ex);
                }
            }
            if (data.TryGetValue("debug", out string debug))
                Debug = bool.Parse(debug);
            if (data.TryGetValue("server.id", out string text))
                ServerId = int.Parse(text);
            if (data.TryGetValue("server.port", out text))
                ServerPort = int.Parse(text);

            logger.Info("Server ID = " + ServerId);
            logger.Info("Server Port = " + ServerPort);
            logger.Info("Build Date = " + File.GetLastWriteTime(typeof(Config).Assembly.Location));
        }

        public static bool IsLinux
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            }
        }

        public static string WorkingDir
        {
            get
            {
                return Directory.GetCurrentDirectory();
            }
        }
    }
}
