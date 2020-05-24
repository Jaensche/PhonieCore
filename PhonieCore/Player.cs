using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PhonieCore
{
    public class Player
    {
        private Process _runningProc;
        private readonly Library _library;

        private const string StopFile = "STOP";
        private const string PauseFile = "PAUSE";

        public Player()
        {
            _library = new Library();
        }

        public void ProcessFolder(string uid)
        {
            string folder = _library.GetFolderForId(uid);
            var files = Directory.EnumerateFiles(folder).ToArray();

            if(files.Any(f => f.Contains(StopFile)))
                Stop();
            else if (files.Any(f => f.Contains(PauseFile)))
            {
                Pause();
            }
            else
            {
                Play(files);
            }
        }

        private void Play(IEnumerable<string> files)
        {
            Stop();
            string arguments = string.Join(" ", files);
            Console.WriteLine("Play files: " + arguments);
            _runningProc = CreateProcess("mpg123", arguments);
            _runningProc.Start();
        }

        private void Stop()
        {
            _runningProc?.Kill(true);
            Console.WriteLine("Stop");
        }

        private void Pause()
        {
            _runningProc?.StandardInput.Write("s");
            _runningProc?.StandardInput.Flush();
            Console.WriteLine("Pause");
        }

        private static Process CreateProcess(string fileName, string args)
        {
            var process = new Process();

            var startInfo = new ProcessStartInfo(fileName, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = true
            };

            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            return process;
        }
    }
}
