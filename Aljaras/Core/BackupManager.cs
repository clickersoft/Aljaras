using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Aljaras.Core
{
    /// <summary>
    /// Creates self-contained <c>.jrsbck</c> backups (the per-user audio library
    /// plus the LiteDB file, same format the Settings screen exports) under a
    /// <c>Backups</c> folder next to the exe. Never throws to the caller.
    /// </summary>
    public static class BackupManager
    {
        public static string BackupDirectory => Path.Combine(GlobalVariables.AppLocation, "Backups");

        public static bool CreateBackup()
        {
            try
            {
                Directory.CreateDirectory(BackupDirectory);
                string userAudioLibrary = Path.Combine(GlobalVariables.AppLocation, "Audio", GlobalVariables.PCCurrentUserName);
                string dbFile = string.Concat(GlobalVariables.AppLocation, GlobalVariables.PCCurrentUserName, GlobalVariables.AppName, ".jrsdb");
                if (!File.Exists(dbFile))
                    return false;

                string destination = Path.Combine(BackupDirectory,
                    $"{GlobalVariables.PCCurrentUserName}{GlobalVariables.AppName}-{DateTime.Now:yyyyMMdd-HHmmss}.jrsbck");

                if (Directory.Exists(userAudioLibrary))
                {
                    ZipFile.CreateFromDirectory(userAudioLibrary, destination, CompressionLevel.Fastest, true);
                    using ZipArchive zip = ZipFile.Open(destination, ZipArchiveMode.Update);
                    zip.CreateEntryFromFile(dbFile, Path.GetFileName(dbFile), CompressionLevel.Fastest);
                }
                else
                {
                    using ZipArchive zip = ZipFile.Open(destination, ZipArchiveMode.Create);
                    zip.CreateEntryFromFile(dbFile, Path.GetFileName(dbFile), CompressionLevel.Fastest);
                }

                Prune();
                Logger.Info($"Auto-backup created: {destination}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Auto-backup failed", ex);
                return false;
            }
        }

        /// <summary>Keep only the newest <paramref name="keep"/> backups.</summary>
        private static void Prune(int keep = 14)
        {
            try
            {
                foreach (FileInfo old in new DirectoryInfo(BackupDirectory)
                             .GetFiles("*.jrsbck")
                             .OrderByDescending(f => f.CreationTime)
                             .Skip(keep))
                    old.Delete();
            }
            catch
            {
                // Pruning is best-effort.
            }
        }
    }
}
