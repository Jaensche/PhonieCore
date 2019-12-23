using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PhonieCore
{
    public class Player
    {
        private Process _runningProc;
        private readonly Library _library;

        public Player()
        {
            _library = new Library();
        }

        public void PlayFolder(string uid)
        {
            Stop();

            string folder = _library.GetFolderForId(uid);

            string arguments = string.Join(" ", Directory.EnumerateFiles(folder));

            Console.WriteLine("Play files: " + arguments);
            _runningProc = CreateProcess("mpg123", arguments);
            _runningProc.Start();
        }

        private void Stop()
        {
            _runningProc?.Kill(true);
        }

        private static Process CreateProcess(string fileName, string args)
        {
            var process = new Process();

            var startInfo = new ProcessStartInfo(fileName, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false
            };

            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            return process;
        }
    }
}
