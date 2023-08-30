using log4net;
using System;
using System.Threading;

namespace Core
{
    public class DisposableTimeoutLock
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public class Locker : IDisposable
        {
            private readonly DisposableTimeoutLock _target;

            internal Locker(DisposableTimeoutLock target)
            {
                _target = target;
            }

            public void Dispose()
            {
                Monitor.Exit(_target);
            }
        }

        private readonly Locker _locker;

        public DisposableTimeoutLock()
        {
            _locker = new Locker(this);
        }

        public Locker Enter(TimeSpan? timeout = null)
        {
            if (!Monitor.TryEnter(this, timeout != null ? timeout.Value : TimeSpan.FromSeconds(10)))
            {
                logger.Error($"DisposableTimeoutLock Timeout! {Environment.StackTrace}");

                throw new TimeoutException();
            }

            return _locker;
        }
    }
}
