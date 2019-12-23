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

            _player.PlayFolder(folder);
        }
    }
}
