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

        protected ConsoleUI() { }

        static void Main(string[] args)
        {
            List<string> path = new List<string>(args);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var loggerTask = PrintLogs(token);

            DateTime t1 = DateTime.Now;

            Spectrum spectrum = new Spectrum(path);
            spectrum.RunSingleThread();

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
        private static async Task PrintLogs(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                while (logger.LogMessages.Count > 0)
                {
                    Console.WriteLine(logger.GetLogMessage().ToString());
                }
                await Task.Delay(50, CancellationToken.None);
            }
        }
    }
}
