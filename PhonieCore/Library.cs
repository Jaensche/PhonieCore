using System;
using System.IO;

namespace PhonieCore
{
    public class Library
    {
        private const string MediaDirectory = "/media/";
        private const string Marker = "@";

        public string GetFolderForId(string id)
        {
            return SetCurrentDirectoryStar(id);
        }

        private string SetCurrentDirectoryStar(string id)
        {
            foreach (string directory in Directory.EnumerateDirectories(MediaDirectory))
            {
                if (directory.EndsWith(Marker))
                {
                    Directory.Move(directory, directory.Replace(Marker, string.Empty).Trim());
                }
            }

            string newDirectoryName = MediaDirectory + id + Marker;
            if (Directory.Exists(MediaDirectory + id))
            {
                Directory.Move(MediaDirectory + id, newDirectoryName);
            }
            else
            { 
                var dir = Directory.CreateDirectory(newDirectoryName);
                Bash.Exec("sudo chmod 777 " + dir.FullName);
                Console.WriteLine("sudo chmod 777 " + dir.FullName);
            }

            return newDirectoryName;
        }
    }
}
