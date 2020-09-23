using System.IO;

namespace Common.Shared.Helpers
{
    public static class DirectoryHelper
    {
        public static void EnsureCreated(string directoryPath)
        {
            if (Directory.Exists(directoryPath)) return;

            Directory.CreateDirectory(directoryPath);
        }

        public static void EnsureDeleted(string directoryPath, bool recursive = false)
        {
            if (!Directory.Exists(directoryPath)) return;

            Directory.Delete(directoryPath, recursive);
        }
    }
}