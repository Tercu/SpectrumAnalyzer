using SpectrumAnalyser;
using System;

namespace ConsoleUI
{
    class ConsoleUI
    {
        static void Main(string[] args)
        {
            string path = @"D:\dev";
            DateTime s1 = DateTime.Now;

            //string path = @"D:\Lossless Poprawki\MUZYKA";

            DirectoryManager dirManager = new DirectoryManager(path);
            dirManager.CreateFileList();

            DateTime s2 = DateTime.Now;
            Console.WriteLine(s2 - s1);

            foreach (string s in dirManager.AudioFileList)
            {
                Console.WriteLine(s);
            }
        }
        //dirManager.PrintFileList();
    }
}
