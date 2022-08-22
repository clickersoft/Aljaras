using Aljaras.MVVM.ViewModel;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using Forms = System.Windows.Forms;

namespace Aljaras
{
    public partial class App : Application
    {
        private readonly Forms.NotifyIcon _notifyIcon;
        private static Mutex _mutex = null;

        public App()
        {
            _notifyIcon = new Forms.NotifyIcon();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "0C41354D-1236-4842-97F2-0EC4E8ACE4BD";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show(GlobalViewModel.Instance.AppLang.AlreadyRunning);
                //app is already running! Exiting the application
                Current.Shutdown();
            }

            if (File.Exists("SchoolBell.ico"))
                _notifyIcon.Icon = new Icon("SchoolBell.ico");
            else
            {
                Bitmap bmp = new(64, 64);
                using (Graphics g = Graphics.FromImage(bmp))
                    g.FillEllipse(Brushes.Red, 0, 0, 64, 64);
                _notifyIcon.Icon = Icon.FromHandle(bmp.GetHicon());
            }
            try
            {
                _notifyIcon.Text = GlobalViewModel.Instance.AppLang.Apptitle + System.Reflection.Assembly.GetEntryAssembly()!.GetName().Version!.ToString(); ;
            }
            catch (Exception)
            {
                _notifyIcon.Text = GlobalViewModel.Instance.AppLang.Apptitle;
            }
            _notifyIcon.DoubleClick += NotifyIconDoubleClick;
            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Open", null, NotifyIconDoubleClick);
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, OnExitClicked);
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
    }
}
