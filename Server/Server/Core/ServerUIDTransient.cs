using log4net;
using System.Collections.Concurrent;
using System.Threading;

namespace Core
{
    public class ServerUIDTransient
    {
        public const long GENERATE_PIVOT_UID = 1_000L;
        public const int MAX_SERVER_ID = 9222 - 1000;

        private static readonly ConcurrentDictionary<string, ServerUIDTransient> cached = new ConcurrentDictionary<string, ServerUIDTransient>();
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ServerUIDTransientType _type;
        private long _nextValue;

        public ServerUIDTransient(long start, ServerUIDTransientType type)
        {
            _type = type;
            _nextValue = start;
        }

        public static ServerUIDTransient CachedOfDomain(int serverId, ServerUIDTransientType type)
        {
            var key = type.ToString() + ":" + serverId;
            if (cached.TryAdd(key, OfDomain(serverId, type)))
                return cached[key];

            return null;
        }

        public long Next()
        {
            long nextUId = Interlocked.Increment(ref _nextValue);
            return nextUId;
        }

        private static ServerUIDTransient OfDomain(int serverId, ServerUIDTransientType type)
        {
            if (MAX_SERVER_ID < serverId)
            {
                return null;
            }

            long start = serverId * GENERATE_PIVOT_UID;
            return new ServerUIDTransient(start, type);
        }
    }
}