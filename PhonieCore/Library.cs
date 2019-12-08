using System.IO;

namespace PhonieCore
{
    public class Library
    {
        private const string BaseDirectory = "/home/pi/";
        private const string Marker = "@";

        public string GetFolderForId(string id)
        {
            return SetCurrentDirectoryStar(id);
        }

        private string SetCurrentDirectoryStar(string id)
        {
            foreach(string directory in Directory.EnumerateDirectories(BaseDirectory))
            {
                if (directory.EndsWith(Marker))
                {
                    Directory.Move(directory, directory.Replace(Marker, string.Empty).Trim());
                }
            }

            string newDirectoryName = BaseDirectory + id + Marker;
            if (Directory.Exists(BaseDirectory + id))
            {
                Directory.Move(BaseDirectory + id, newDirectoryName);
            }
            else
            {
                Directory.CreateDirectory(newDirectoryName);
            }

            return newDirectoryName;
        }
    }
}
