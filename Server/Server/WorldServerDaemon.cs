using Core;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    internal class WorldServerDaemon
    {
        private static readonly ILog logger = LogManager.GetLogger("", typeof(WorldServerDaemon));

        private static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (e.Exception is ThreadInterruptedException
                || e.Exception is WebSocketException
                || e.Exception is ObjectDisposedException
                || e.Exception is InvalidOperationException
                )
                return;

            logger.Warn("1", e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Warn("2", e.ExceptionObject as Exception);
        }

        private static void InitLogger()
        {
            var config = @"<log4net>
            <appender name=""Console"" type=""log4net.Appender.ConsoleAppender"">
                <layout type=""log4net.Layout.PatternLayout"" >
                   <conversionpattern value = ""%d [%t] %-5p - %m%n"" />
                </layout>
            </appender>

            <appender name=""LogFileAppender"" type=""log4net.Appender.RollingFileAppender"">
                <param name=""File"" type=""log4net.Util.PatternString"" value=""logs/logfile.log"" />
                <param name=""AppendToFile"" value=""false"" />
                <rollingStyle value=""Date"" />
                <maxSizeRollBackups value=""10"" />
                <maximumFileSize value=""10MB"" />
                <staticLogFileName value=""false"" /> 
                <preserveLogFileNameExtension value=""true"" />
                <datePattern value=""_yyyy-MM-dd"" />
                <layout type=""log4net.Layout.PatternLayout"">
                    <param name=""ConversionPattern"" value=""%-5p%d{yyyy-MM-dd hh:mm:ss} – %m%n"" />
                </layout>
            </appender>

            <root>
                <level value=""ALL"" />
                    <appender-ref ref=""Console"" />
                    <appender-ref ref=""LogFileAppender"" />
                </root>
            </log4net>";

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(config));
            if (LogManager.GetAllRepositories().Count() == 0)
                XmlConfigurator.Configure(LogManager.CreateRepository(""), ms);
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                InitLogger();

                ThreadPool.GetMaxThreads(out var worker, out _);
                ThreadPool.GetMinThreads(out var minWorker, out var minIo);
                ThreadPool.SetMaxThreads(worker, 5000);
                ThreadPool.GetAvailableThreads(out worker, out var io);
                logger.Info("Thread pool threads available at startup: ");
                logger.Info($"Worker threads - MIN : {minWorker}, MAX : {worker}");
                logger.Info($"Asynchronous I/O threads - MIN : {minIo}, MAX : {io}");

                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                WorldServer.INSTANCE.Start(Config.ServerPort, false);

                while (true)
                {
                    try
                    {
                        Thread.Sleep(10);
                    } 
                    catch (ThreadInterruptedException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("", ex);
                    }
                }

                WorldServer.INSTANCE.Stop();
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            e.Exception.Handle(finalEX =>
            {
                if (finalEX is DotNetty.Transport.Channels.ClosedChannelException
                    || finalEX is DotNetty.Transport.Libuv.Native.OperationException)
                    return true;

                logger.Warn("4", finalEX);
                return true;
            });
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Warn(e.ExceptionObject.ToString());
        }
    }
}