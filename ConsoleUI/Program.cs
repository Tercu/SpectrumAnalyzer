using SpectrumAnalyser;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUI
{
    class ConsoleUI
    {
        static void Main(string[] args)
        {
            string path = @"D:\dev";
            //string pathToFile = @"D:\dev\440.flac";
            Logger logger = Logger.GetInstance();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task task = new Task(() => PrintLogs(token), token, TaskCreationOptions.LongRunning);
            task.Start();

            DateTime t1 = DateTime.Now;

            DirectoryManager dirManager = new DirectoryManager(path);
            dirManager.CreateFileList();

            List<Task> tasks = new List<Task>();

            foreach (var pathToFile in dirManager.AudioFileList)
            {
                tasks.Add(new Task(() => ProcessFile(pathToFile)));
            }

            tasks.ForEach(t => t.Start());
            tasks.ForEach(t => t.Wait());

            DateTime t2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"All files processed in {t2 - t1}. Program may be closed.");
            while (!logger.IsEmpty())
            {
                Thread.Sleep(100);
            }

            tokenSource.Cancel();
            task.Wait();

            _ = Console.ReadKey();
        }

        private static void ProcessFile(string pathToFile)
        {
            Logger logger = Logger.GetInstance();
            DateTime s1 = DateTime.Now;

            AudioFile audio = new AudioFile(pathToFile);
            audio.ReadFile();

            DateTime s2 = DateTime.Now;
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Done in: {s2 - s1}");
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
