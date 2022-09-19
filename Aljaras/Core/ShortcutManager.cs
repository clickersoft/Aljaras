using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Reflection;
using File = System.IO.File;

namespace Aljaras.Core
{
    public class ShortcutManager
    {
        /// <summary>
        /// Creates or deletes a desktop shortcut.
        /// </summary>
        /// <param name="appname">The appname.</param>
        /// <param name="appPathFull">The full app Path or URL.</param>
        /// <param name="create">True to create a shortcut or False to remove the shortcut.</param>
        /// <returns>The .lnk full file name.</returns>
        public static void CreateDesktopShortcut(bool create)
        {
            try
            {
                var desktopPathName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), App.AppName + ".lnk");
                CreateShortcut(desktopPathName, Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe"), create);
                return;
            }
            catch 
            {
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
                WshShell myShell = new WshShell();
                IWshShortcut? myShortcut = myShell.CreateShortcut(shortcutPathName) as IWshShortcut;
                myShortcut!.TargetPath = shortcutTarget;

                if (shortcutTarget.StartsWith("http"))
                {
                    if (File.Exists("MyIcon.ico"))
                    {
                        myShortcut.IconLocation = "MyIcon.ico";
                    }
                }
                else
                {
                    // Not an URL, but an .exe or other file
                    myShortcut.IconLocation = shortcutTarget + ",0";
                    myShortcut.WorkingDirectory = Directory.GetParent(shortcutTarget)!.ToString();
                }

                myShortcut.Arguments = arguments;
                myShortcut.Save();
            }
            else
            {
                // Delete the shortcut
                if (File.Exists(shortcutPathName))
                {
                    File.SetAttributes(shortcutPathName, FileAttributes.Normal);
                    File.Delete(shortcutPathName);
                }
            }
        }
    }
}
