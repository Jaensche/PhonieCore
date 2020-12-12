using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhonieCore
{
    public class Player
    {
        private readonly Library _library;
        private Mopidy.Client _mopidyClient;

        private const string StopFile = "STOP";
        private const string PauseFile = "PAUSE";
        private const string PlayFile = "PLAY";
        private const string SpotifyFile = "SPOTIFY";

        private int volume = 50;

        public Player()
        {
            _library = new Library();
            _mopidyClient = new Mopidy.Client();

            SetVolume(volume);
        }

        public void ProcessFolder(string uid)
        {
            string folder = _library.GetFolderForId(uid);
            var files = Directory.EnumerateFiles(folder).ToArray();

            foreach (string file in files)
            {
                Console.WriteLine(file);
            }

            if (files.Any(f => f.Contains(StopFile)))
            {
                Stop();
            }
            else if (files.Any(f => f.Contains(PlayFile)))
            {
                Play();
            }
            else if (files.Any(f => f.Contains(PauseFile)))
            {
                Pause();
            }
            else if (files.Any(f => f.Contains(SpotifyFile)))
            {
                PlaySpotify(File.ReadAllText(files.FirstOrDefault()));
            }
            else if(files.Any(f => f.EndsWith("mp3")))
            {
                Play(files);
            }
        }

        public void Play()
        {
            _mopidyClient.Play();
        }

        public void Next()
        {
            _mopidyClient.Next();
        }

        public void Previous()
        {
            _mopidyClient.Previous();
        }

        public void Seek(int sec)
        {
            _mopidyClient.Seek(sec);
        }

        private void SetVolume(int volume)
        {
            _mopidyClient.SetVolume(volume);
        }

        public void IncreaseVolume()
        {
            volume += 5;
            _mopidyClient.SetVolume(volume);
        }

        public void DecreaseVolume()
        {
            volume -= 5;
            _mopidyClient.SetVolume(volume);
        }

        private void Play(IEnumerable<string> files)
        {
            string arguments = string.Join(" ", files);
            Console.WriteLine("Play files: " + arguments);

            Stop();

            _mopidyClient.ClearTracks();
            foreach (string file in files)
            {
                _mopidyClient.AddTrack("file://" + file);
            }

            _mopidyClient.Play();                       
        }

        private void PlaySpotify(string uri)
        {
            Console.WriteLine("Play Spotify: " + uri);

            Stop();

            _mopidyClient.ClearTracks();            
             _mopidyClient.AddTrack(uri);            

            _mopidyClient.Play();
        }

        public void Stop()
        {
            Console.WriteLine("Stop");
            _mopidyClient.Stop();            
        }

        public void Pause()
        {
            Console.WriteLine("Pause");
            _mopidyClient.Pause();            
        }       
    }
}
