namespace PhonieCore
{
    public class Radio
    {
        private readonly Player _player;

        public Radio()
        {
            _player = new Player();
            Rfid rfid = new Rfid(HandleNewCardDetected, HandleCardDetected);
        }

        private void HandleCardDetected(string uid)
        {
        }

        private void HandleNewCardDetected(string uid)
        {
            _player.ProcessFolder(uid);
        }
    }
}
