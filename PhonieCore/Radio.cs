using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhonieCore
{
    public class Radio
    {
        private Player _player;
        private Library _library;

        public Radio()
        {
            _player = new Player();
            _library = new Library();
            Rfid rfid = new Rfid(HandleNewCardDetected, HandleCardDetected);
        }

        private void HandleCardDetected(string uid)
        {
        }

        private void HandleNewCardDetected(string uid)
        {
            string folder = _library.GetFolderForId(uid);

            string fileToPlay = Directory.EnumerateFiles(folder).FirstOrDefault();
            if (fileToPlay != null)
            {
                _player.Play(fileToPlay);
            }

            Console.WriteLine(folder);
        }
    }
}
