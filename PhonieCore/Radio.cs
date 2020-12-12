using System;
using Unosquare.WiringPi;

namespace PhonieCore
{
    public class Radio
    {
        private readonly Player _player;

        public Radio()
        {
            Unosquare.RaspberryIO.Pi.Init<BootstrapWiringPi>();

            _player = new Player();
            //var bla = new KeyListener(HandleKeyPressed);
            Console.WriteLine("Player");
            var rfid = new Rfid(HandleNewCardDetected, HandleCardDetected);

        }

        private void HandleCardDetected(string uid)
        {            
        }

        private void HandleNewCardDetected(string uid)
        {
            Console.WriteLine($"New card: " + uid);
            _player.ProcessFolder(uid);
        }

        private void HandleKeyPressed(char key)
        {
            Console.WriteLine(key);

            switch (key)
            {
                case 'a':
                    _player.Pause();
                    break;
                case 's':
                    _player.Stop();
                    break;
                case 'q':
                    _player.IncreaseVolume();
                    break;
                case 'w':
                    _player.DecreaseVolume();
                    break;
            }
        }
    }
}
