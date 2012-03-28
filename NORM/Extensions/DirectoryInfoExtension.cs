using System;
using System.IO;

namespace NORM.Extensions
{
    /// <summary>
    /// Расширения для Directory
    /// </summary>
    public static class DirectoryInfoExtension
    {
        public static void Copy(this DirectoryInfo source, DirectoryInfo target)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            foreach (var fileInfo in source.GetFiles())
            {
                fileInfo.CopyTo(Path.Combine(target.ToString(), fileInfo.Name), true);
            }

            foreach (var directoryInfo in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(directoryInfo.Name);
                Copy(directoryInfo, nextTargetSubDir);
            }
        }
    }
}
