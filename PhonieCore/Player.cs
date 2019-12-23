using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PhonieCore
{
    public class Player
    {
        private Process _runningProc;

        public void PlayFolder(string folder)
        {
            Stop();

            string arguments = string.Join(" ", Directory.EnumerateFiles(folder));

            Console.WriteLine("Play files: " + arguments);
            _runningProc = CreateProcess("mpg123", arguments);
            _runningProc.Start();
        }

        public void Stop()
        {
            _runningProc?.Kill(true);
        }

        private static Process CreateProcess(string fileName, string args)
        {
            var process = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo(fileName, args);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = false;
            startInfo.RedirectStandardError = false;
            startInfo.RedirectStandardInput = false;

            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            return process;
        }
    }
}
