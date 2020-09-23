using System.IO;

namespace Common.Shared.Helpers
{
    public static class FileHelper
    {
        public static void EnsureDeleted(string filePath)
        {
            if (!File.Exists(filePath)) return;

            File.Delete(filePath);
        }
    }
}