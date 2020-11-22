using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Spectrogram
{
    public class Spectrum
    {
        private readonly List<Task> taskList = new List<Task>();
        private readonly Logger logger = Logger.GetInstance();
        private readonly DirectoryManager dirManager;
        private List<AudioProcessor> audioProcessorList;
        public void RunSingleThread()
        {
            foreach (var audioFile in dirManager.AudioFileList)
            {
                RunAudioProcessor(audioFile);
            }
        }

        private static void RunAudioProcessor(string audioFile)
        {
            AudioProcessor audioProcessor = new AudioProcessor(new AudioFile(audioFile));
            audioProcessor.ProcessFile();
        }

        public void RunMultiThread()
        {
            InitTasks();
            StartTasks();
            WaitForTasks();
        }
        public void RunMultiThreadTaskLimit(int limit = 8)
        {

            foreach (var fileName in dirManager.AudioFileList)
            {
                if (taskList.Count < limit)
                {
                    AudioProcessor audioProcessor = new AudioProcessor(new AudioFile(fileName));
                    var t = Task.Run(() => { audioProcessor.ProcessFile(); });

                    taskList.Add(t);
                }
                else
                {
                    Task.WaitAny(taskList.ToArray());
                }

                for (int i = 0; i < taskList.Count; ++i)
                {
                    if (taskList[i].IsCompleted)
                    {
                        taskList[i].Dispose();
                        taskList.RemoveAt(i);
                    }
                }
            }
            Task.WaitAll(taskList.ToArray());
        }

        public Spectrum(List<string> pathToDirectory)
        {
            List<string> ext = new List<string> { ".flac", ".mp3" };
            dirManager = new DirectoryManager(new System.IO.Abstractions.FileSystem(), pathToDirectory, ext);
            dirManager.CreateFileList();
            audioProcessorList = new List<AudioProcessor>();
        }
        private void InitTasks()
        {
            foreach (var audioProcessor in audioProcessorList)
            {
                taskList.Add(new Task(() => audioProcessor.ProcessFile(), TaskCreationOptions.LongRunning));
            }
        }
        private void StartTasks()
        {
            taskList.ForEach(t => t.Start());
        }
        private void WaitForTasks()
        {
            taskList.ForEach(t => t.Wait());
        }

    }
}
