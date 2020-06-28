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
        private const string SpotifyFile = "SPOTIFY";

        public Player()
        {
            _library = new Library();
            _mopidyClient = new Mopidy.Client();
        }

        public void ProcessFolder(string uid)
        {
            string folder = _library.GetFolderForId(uid);
            var files = Directory.EnumerateFiles(folder).ToArray();

            foreach (string file in files)
            {
                Console.WriteLine(file);
            }

            if(files.Any(f => f.Contains(StopFile)))
                Stop();
            else if (files.Any(f => f.Contains(PauseFile)))
            {
                Pause();
            }
            else if (files.Any(f => f.Contains(SpotifyFile)))
            {
                PlaySpotify(File.ReadAllText(files.FirstOrDefault()));
            }
            else
            {
                Play(files);
            }
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

        private void Stop()
        {
            Console.WriteLine("Stop");
            _mopidyClient.Stop();            
        }

        private void Pause()
        {
            Console.WriteLine("Pause");
            _mopidyClient.Pause();            
        }       
    }
}
