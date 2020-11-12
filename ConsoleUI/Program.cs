using SpectrumAnalyser;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUI
{
    class ConsoleUI
    {
        private static readonly Logger logger = Logger.GetInstance();
        static void Main(string[] args)
        {
            string path = @"D:\dev\test";
            //string pathToFile = @"D:\dev\440.flac";
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task loggerTask = new Task(() => PrintLogs(token), token, TaskCreationOptions.LongRunning);
            loggerTask.Start();

            DateTime t1 = DateTime.Now;

            Spectrum spectrum = new Spectrum(path);

            //spectrum.RunSingleThread();
            spectrum.RunMultiThread();

            DateTime t2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"All files processed in {t2 - t1}. Program may be closed.");
            while (!logger.IsEmpty())
            {
                Thread.Sleep(100);
            }

            tokenSource.Cancel();
            loggerTask.Wait();

            _ = Console.ReadKey();
        }
        private static void PrintLogs(CancellationToken token)
        {
            Logger logger = Logger.GetInstance();
            while (!token.IsCancellationRequested)
            {
                while (logger.LogMessages.Count > 0)
                {
                    Console.WriteLine(logger.GetLogMessage().ToString());
                }
            }
        }
    }
}
