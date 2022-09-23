using Aljaras.Core;
using Aljaras.MVVM.ViewModel;
using LiteDB;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace Aljaras
{
    public partial class App : Application
    {
        private readonly Forms.NotifyIcon _notifyIcon;
        // Create a new Mutex. The creating thread does not own the mutex.
        private static Mutex mut = new();
        public static readonly string? AppName = Assembly.GetExecutingAssembly().GetName().Name;
        public static readonly string AppLocation = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string dbName = AppName + ".jrsdb";
        public static readonly string dbBackupName = AppName + ".jrsbck";
        public static readonly string PCCurrentUserName = Environment.UserName;
        public static readonly string dbConnectionString = string.Concat("Filename=", PCCurrentUserName + dbName, ";Connection=shared");
        public static readonly LiteDatabase db = new(dbConnectionString);

        public App() => _notifyIcon = new Forms.NotifyIcon();

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "0C41354D-1236-4842-97F2-0EC4E8ACE4BD";
            mut = new Mutex(true, appName, out bool alreadyPresent);
            if (!alreadyPresent)
            {
                //MessageBox.Show(GlobalViewModel.Instance.AppLang.AlreadyRunning);
                //app is already running! Exiting the application
                //Current.Shutdown();
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processItems = Process.GetProcessesByName(currentProcess.ProcessName);
                if (processItems.Length > 1)
                    processItems.Where(p => p.Id != Environment.ProcessId).First().Kill();
            }

            if (File.Exists(AppLocation + AppName + ".ico"))
                _notifyIcon.Icon = new Icon(AppLocation + AppName + ".ico");
            else
            {
                Bitmap bmp = new(64, 64);
                using (Graphics g = Graphics.FromImage(bmp))
                    g.FillEllipse(Brushes.Red, 0, 0, 64, 64);
                _notifyIcon.Icon = Icon.FromHandle(bmp.GetHicon());
            }
            try
            {
                _notifyIcon.Text = GlobalViewModel.Instance.AppLang.Apptitle + Assembly.GetEntryAssembly()!.GetName().Version!.ToString();
            }
            catch (Exception)
            {
                _notifyIcon.Text = GlobalViewModel.Instance.AppLang.Apptitle;
            }
            _notifyIcon.DoubleClick += NotifyIconDoubleClick!;
            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add(GlobalViewModel.Instance.AppLang.Open, null, NotifyIconDoubleClick!);
            _notifyIcon.ContextMenuStrip.Items.Add(GlobalViewModel.Instance.AppLang.Exit, null, OnExitClicked!);
            _notifyIcon.Visible = true;
            base.OnStartup(e);
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void NotifyIconDoubleClick(object sender, EventArgs e)
        {
            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnExit(e);
        }

        /// <summary>
        /// Catch unhandled exceptions thrown on the main UI thread and allow 
        /// option for user to continue program. 
        /// The OnDispatcherUnhandledException method below for AppDomain.UnhandledException will handle all other exceptions thrown by any thread.
        /// </summary>
        void AppUI_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception == null)
            {
                Current.Shutdown();
                return;
            }
            string errorMessage = string.Format("An application error occurred. If this error occurs again there seems to be a serious bug in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)", e.Exception.Message);
            //insert code to log exception here
            if (MessageBox.Show(errorMessage, "Application User Interface Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Current.Shutdown();
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// Catch unhandled exceptions not thrown by the main UI thread.
        /// The above AppUI_DispatcherUnhandledException method for DispatcherUnhandledException will only handle exceptions thrown by the main UI thread. 
        /// Unhandled exceptions caught by this method typically terminate the runtime.
        /// </summary>
        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An application error occurred. If this error occurs again there seems to be a serious bug in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)", e.Exception.Message);
            //insert code to log exception here
            if (MessageBox.Show(errorMessage, "Application UnhandledException Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Current.Shutdown();
                }
            }
            e.Handled = true;
        }
    }
}
