using SpectrumAnalyser;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUI
{
    class ConsoleUI
    {
        static void Main(string[] args)
        {
            Logger logger = Logger.GetInstance();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task task = new Task(() => PrintLogs(token), token, TaskCreationOptions.LongRunning);
            task.Start();

            string path = @"D:\dev";

            DateTime t1 = DateTime.Now;

            DirectoryManager dirManager = new DirectoryManager(path);
            dirManager.CreateFileList();

            foreach (var pathToFile in dirManager.AudioFileList)
            {

                DateTime s1 = DateTime.Now;

                AudioFile audio = new AudioFile(pathToFile);
                audio.ReadFile();

                DateTime s2 = DateTime.Now;
                Console.WriteLine($"Done in: {s2 - s1}");
            }

            DateTime t2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"All files processed in {t2 - t1}. Program may be closed.");
            while (!logger.IsEmpty())
            {
                Thread.Sleep(100);
            }
            tokenSource.Cancel();

            Console.ReadKey();
        }
        private static void PrintLogs(CancellationToken token)
        {
            Logger logger = Logger.GetInstance();
            while (!token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
                while (logger.LogMessages.Count > 0)
                {
                    Console.WriteLine(logger.GetLogMessage().ToString());
                }
                Thread.Sleep(100);
            }
        }
    }
}
