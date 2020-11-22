using Spectrogram;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUI
{
    class ConsoleUI
    {
        private static readonly Logger logger = Logger.GetInstance();
        static void Main(string[] args)
        {
            List<string> path = new List<string> { @"D:\dev\test\s1", @"D:\dev\test\s2" };
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task loggerTask = new Task(() => PrintLogs(token), token, TaskCreationOptions.LongRunning);
            loggerTask.Start();

            DateTime t1 = DateTime.Now;

            Spectrum spectrum = new Spectrum(path);

            //spectrum.RunSingleThread();
            spectrum.RunMultiThreadTaskLimit();

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
