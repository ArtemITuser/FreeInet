using FreeNet.classes;
using FreeNet.pages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace FreeNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("wininet.dll")]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private System.Windows.Forms.NotifyIcon notifyIcon;

        private bool hide_tooltips = false;

        public MainWindow()
        {

            InitializeComponent();
            CheckProc();
            Focus();


        }



        private void CheckProc()
        {
            if (Process.GetProcessesByName("FreeNet").Length > 1)
            {
                MessageBox.Show("FreeNet уже запущен!", "Уведомление", MessageBoxButton.OK);
                hide_tooltips = true;
                Application.Current.Shutdown();
                return;
            }

            Process[] dpi = Process.GetProcessesByName("goodbyedpi");

            foreach (Process proc in dpi)
            {
                proc.Kill();
                proc.WaitForExit();
            }
            Process[] doh = Process.GetProcessesByName("dnsproxy");

            foreach (Process proc in doh)
            {
                proc.Kill();
                proc.WaitForExit();
            }

            Process[] proxy = Process.GetProcessesByName("3proxy");

            foreach (Process proc in proxy)
            {
                proc.Kill();
                proc.WaitForExit();
            }


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            WindowToTray();
            e.Cancel = true;
        }

        public void WindowToTray()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = Properties.Resources.icon;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
            {
                new System.Windows.Forms.MenuItem("Открыть FREENET", (sender, e)=>{this.Show();notifyIcon.Visible = false; }),
                new System.Windows.Forms.MenuItem("Выйти", (sender, e)=>{
                MessageBoxResult box = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
                if (box == MessageBoxResult.No)
                {

                }
                else
                {
                        hide_tooltips = true;
                        CloseApp();
                }
                })
                });

            notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            notifyIcon.BalloonTipText = "Приложение работает в фоновом режиме";
            notifyIcon.BalloonTipTitle = "FREENET";
            if (!hide_tooltips) notifyIcon.ShowBalloonTip(3);



            
            this.Hide();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        static public void CloseApp()
        {
            List<Service> services = MainPage.Services;
            if (services != null)
            {
                foreach (Service service in services)
                {
                    service.Stop();
                }
            }

            offProxy();

            Process.GetCurrentProcess().Kill();
            Application.Current.Shutdown();
        }

        public static bool onProxy(string server)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            if (key != null)
            {
                key.SetValue("ProxyServer", server);
                key.SetValue("ProxyOverride", "<local>");
                key.SetValue("ProxyEnable", 1);
                key.Close();
                InternetSetOption(IntPtr.Zero, 95, IntPtr.Zero, 0);

                return true;
            }
            return false;
        }

        public static bool offProxy()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            if (key != null)
            {
                key.SetValue("ProxyServer", "");
                key.SetValue("ProxyEnable", 0);
                key.Close();
                InternetSetOption(IntPtr.Zero, 95, IntPtr.Zero, 0);
                return true;
            }
            return false;
        }
    }
}
