using System;
using System.IO;
using System.Linq;

namespace DotNetCoreTest
{
    public class Radio
    {
        private Player sound;
        private Library library;

        public Radio()
        {
            sound = new Player();
            library = new Library();
            RfidPoller poller = new RfidPoller(HandleNewCardDetected, HandleCardDetected);
        }

        private void HandleCardDetected(string uid)
        {

        }

        private void HandleNewCardDetected(string uid)
        {
            string folder = library.GetFolderForId(uid);

            string fileToPlay = Directory.EnumerateFiles(folder).FirstOrDefault();
            if (fileToPlay != null)
            {
                sound.Play(fileToPlay);
            }

            Console.WriteLine(folder);
        }
    }
}
