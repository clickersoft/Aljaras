using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;

namespace Aljaras.Core
{
    public class StartUpManager
    {
        static readonly RegistryKey? CurrentUserKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        static readonly RegistryKey? LocalMachineKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        static readonly RegistryKey? WOW6432NodeKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        static readonly string AppLocationWithEXEExtension = !string.IsNullOrEmpty(App.AppName) ? Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe") : string.Empty;

        public static void AddApplicationToCurrentUserStartup()
        {
            if (CurrentUserKey != null && !string.IsNullOrEmpty(App.AppName))
                CurrentUserKey.SetValue(App.AppName, AppLocationWithEXEExtension);
        }

        public static void RemoveApplicationFromCurrentUserStartup()
        {
            if (CurrentUserKey != null && !string.IsNullOrEmpty(App.AppName))
                CurrentUserKey.DeleteValue(App.AppName, false);
        }

        public static bool CheckCurrentUserKey()
        {
            if (CurrentUserKey != null)
                return true;
            return false;
        }

        public static void AddApplicationToAllUsersStartup()
        {
            if (LocalMachineKey != null && !string.IsNullOrEmpty(App.AppName))
                LocalMachineKey.SetValue(App.AppName, AppLocationWithEXEExtension);
        }

        public static void RemoveApplicationFromAllUsersStartup()
        {
            if (LocalMachineKey != null && !string.IsNullOrEmpty(App.AppName))
                LocalMachineKey.DeleteValue(App.AppName, false);
        }

        public static bool CheckAllUsersKey()
        {
            if (LocalMachineKey != null)
                return true;
            return false;
        }

        public static void AddApplicationToWOW6432Startup()
        {
            if (Environment.Is64BitOperatingSystem && !string.IsNullOrEmpty(App.AppName))
                if (WOW6432NodeKey != null && !string.IsNullOrEmpty(App.AppName))
                    WOW6432NodeKey.SetValue(App.AppName, AppLocationWithEXEExtension);
        }

        public static void RemoveApplicationFromWOW6432Startup()
        {
            if (Environment.Is64BitOperatingSystem && !string.IsNullOrEmpty(App.AppName))
                if (WOW6432NodeKey != null && !string.IsNullOrEmpty(App.AppName))
                    WOW6432NodeKey.DeleteValue(App.AppName, false);
        }

        public static bool CheckWOW6432Key()
        {
            if (WOW6432NodeKey != null)
                return true;
            return false;
        }

        public static bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        public static void RelaunchAsAdministrator()
        {
            ProcessStartInfo proc = new()
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe"),
                Verb = "runas"
            };
            try
            {
                Process.Start(proc);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("This program must be run as an administrator! \n\n" + ex.ToString());
                Environment.Exit(0);
            }
        }
    }
}
