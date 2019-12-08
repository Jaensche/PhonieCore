using System;
using System.Diagnostics;

namespace DotNetCoreTest
{
    public class Player
    {
        /*
         *  player.Play("/home/pi/example.mp3");
         */

        private Process _runningProc;

        public void Play(string file)
        {
            Stop();

            Console.WriteLine("Play file: " + file);

            _runningProc = CreateProcess("omxplayer", $"\"{file}\"");
            _runningProc.Start();
        }

        public void Stop()
        {
            if (_runningProc != null)
            {
                _runningProc.Kill(true);
            }
        }

        static Process CreateProcess(string path, string args)
        {
            var process = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo(path, args);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;

            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            return process;
        }
    }
}
