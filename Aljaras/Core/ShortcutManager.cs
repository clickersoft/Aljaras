using System;
using System.IO;
using System.Reflection;
using ShellLink; // Swapped from IWshRuntimeLibrary

namespace Aljaras.Core
{
    public class ShortcutManager
    {
        /// <summary>
        /// Creates or deletes a desktop shortcut.
        /// </summary>
        /// <param name="create">True to create a shortcut or False to remove the shortcut.</param>
        public static void CreateDesktopShortcut(bool create)
        {
            try
            {
                var desktopPathName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), GlobalVariables.AppName + ".lnk");
                CreateShortcut(desktopPathName, Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe"), create);
                return;
            }
            catch
            {
                // Consider logging the exception here if needed
            }
            return;
        }

        /// <summary>
        /// Creates or removes a shortcut at the specified pathname.
        /// Sets the icon of the shortcut to the exe icon.
        /// </summary>
        /// <param name="shortcutPathName">The path where the shortcut is to be created or removed from including the (.lnk) extension.</param>
        /// <param name="shortcutTarget">The URL or exe to execute.</param>
        /// <param name="create">True to create a shortcut or False to remove the shortcut.</param>
        /// <param name="arguments">The optional arguments.</param>
        private static void CreateShortcut(string shortcutPathName, string shortcutTarget, bool create, string arguments = "")
        {
            if (create)
            {
                Shortcut shortcut;

                // Handle Web URL vs Executable conditions
                if (shortcutTarget.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    string iconPath = File.Exists("MyIcon.ico") ? "MyIcon.ico" : "";

                    // CreateShortcut overload: (targetPath, arguments, iconPath, iconIndex)
                    shortcut = Shortcut.CreateShortcut(shortcutTarget, arguments, iconPath, 0);
                }
                else
                {
                    // Not a URL: Retrieve the parent directory
                    string workDir = Directory.GetParent(shortcutTarget)?.FullName ?? "";

                    // CreateShortcut overload: (targetPath, arguments, workingDir, iconPath, iconIndex)
                    shortcut = Shortcut.CreateShortcut(shortcutTarget, arguments, workDir, shortcutTarget, 0);
                }

                // Save the shortcut structure directly to the file system
                shortcut.WriteToFile(shortcutPathName);
            }
            else
            {
                // Delete the shortcut safely
                if (File.Exists(shortcutPathName))
                {
                    File.SetAttributes(shortcutPathName, FileAttributes.Normal);
                    File.Delete(shortcutPathName);
                }
            }
        }
    }
}
