using SpectrumAnalyser;
using System;
using System.IO;

namespace ConsoleUI
{
    class ConsoleUI
    {
        static void Main(string[] args)
        {
            string path = @"D:\dev";

            DateTime t1 = DateTime.Now;

            DirectoryManager dirManager = new DirectoryManager(path);
            dirManager.CreateFileList();

            foreach (var pathToFile in dirManager.AudioFileList)
            {
                Console.WriteLine($"Processing file: {Path.GetFileName(pathToFile)}");
                DateTime s1 = DateTime.Now;

                AudioFile audio = new AudioFile(pathToFile);
                audio.ReadFile();

                DateTime s2 = DateTime.Now;
                Console.WriteLine($"Done in: {s2 - s1}");
            }

            DateTime t2 = DateTime.Now;
            Console.WriteLine($"All files processed in {t2 - t1}. Program may be closed.");

            //foreach (string s in dirManager.AudioFileList)
            //{
            //    Console.WriteLine(s);
            //}
            Console.ReadKey();
        }
        //dirManager.PrintFileList();
    }
}
