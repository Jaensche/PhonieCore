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
            Console.WriteLine("SetCurrentDirectoryStar");
            foreach (string directory in Directory.EnumerateDirectories(MediaDirectory))
            {
                if (directory.EndsWith(Marker))
                {
                    Directory.Move(directory, directory.Replace(Marker, string.Empty).Trim());
                }
            }

            string newDirectoryName = MediaDirectory + id + Marker;
            Console.WriteLine("New directory name: " + newDirectoryName);
            if (Directory.Exists(MediaDirectory + id))
            {
                Console.WriteLine("Move");
                Directory.Move(MediaDirectory + id, newDirectoryName);
            }
            else
            {
                Console.WriteLine("Create");
                var dir = Directory.CreateDirectory(newDirectoryName);
                Console.WriteLine("Created");
                Bash.Exec("sudo chmod 777 " + dir.FullName);
                Console.WriteLine("Chmodded");
            }

            return newDirectoryName;
        }
    }
}
