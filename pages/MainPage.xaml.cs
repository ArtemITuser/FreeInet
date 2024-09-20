using FreeNet.classes;
using FreeNet.Properties;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
using static System.Net.WebRequestMethods;

namespace FreeNet.pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public static Page Instance { get; private set; }
        private List<Service> _services;
        private List<object[]> _pid_for_check = new List<object[]>();

        static public Logger Log
        {
            get; private set;
        }


        public static List<Service> Services { get; private set; }

        private bool faq_show = false;
        private bool app_work = true;
        private bool log_show = false;


        public MainPage()
        {
            InitializeComponent();
            Instance = this;
            Application.Current.Exit += (s, e) => { app_work = false; };
            Task.Run(() => getIps());
            setLogger();
            setService();

            MainPage.Log.Write("");
        }

        private void setService()
        {
            List<Button> buttons_dpi = new List<Button>();
            List<Button> buttons_doh = new List<Button>();
            List<Button> buttons_proxy = new List<Button>();

            buttons_dpi.Add(btn_dpi_Start);
            buttons_dpi.Add(btn_dpi_Stop);
            buttons_dpi.Add(btn_dpi_Restart);

            buttons_doh.Add(btn_doh_Start);
            buttons_doh.Add(btn_doh_Stop);
            buttons_doh.Add(btn_doh_Restart);

            buttons_proxy.Add(btn_proxy_Start);
            buttons_proxy.Add(btn_proxy_Stop);
            buttons_proxy.Add(btn_proxy_Restart);

            _services =
            [
                new Service("GoodByeDPI", "./programs/GoodbyeDPI/x86/goodbyedpi.exe", buttons_dpi, dpi_status, dpi_arg, dpi_pid),
                new Service("DOH", "./programs/DoH/windows-386/dnsproxy.exe", buttons_doh, doh_status, doh_arg, doh_pid, doh_ports),
                new Service("Proxy", "./programs/3proxy-0.9.3/bin/3proxy.exe", buttons_proxy, proxy_status, proxy_arg, proxy_pid, proxy_ports),
            ];
            Services = _services;

        }

        public void setLogger()
        {
            Log = new Logger();
        }

        private void getIps()
        {
            List<string> ips_prev = [];

            while (app_work)
            {
                bool upd = false;

                List<string> ips = [];

                ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection objMOC = objMC.GetInstances();

                foreach (ManagementObject objMO in objMOC)
                {
                    if ((bool)objMO["IPEnabled"])
                    {

                        if (((String)objMO["Caption"]).Contains("Microsoft Wi-Fi Direct Virtual Adapter"))
                        {
                            ips.Add(((String[])objMO["IPAddress"])[0] + " - HOT SPOT (WIFI)");
                        }
                        else
                        {
                            ips.Add(((String[])objMO["IPAddress"])[0]);
                        }
                    }
                }

                if (ips.Count != ips_prev.Count) upd = true;

                foreach (string ip in ips_prev)
                {
                    if (!ips.Contains(ip)) upd = true;
                }

                if (upd)
                {
                    ips_prev = ips;
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        IpList.Items.Clear();
                        foreach (string ip in ips)
                        {
                            IpList.Items.Add(ip);
                        }
                        upd = false;

                    });
                }
                Thread.Sleep(1000);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Service dpi = _services.Find(x => x.Name.Equals("GoodByeDPI"));
            dpi.Start();


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Service dpi = _services.Find(x => x.Name.Equals("GoodByeDPI"));
            dpi.Stop();
        }

        private void btn_dpi_Restart_Click(object sender, RoutedEventArgs e)
        {
            Service service = _services.Find(x => x.Name.Equals("GoodByeDPI"));
            service.Restart();
        }

        async private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Settings form = new Settings(Config.getParam("GoodByeDPI"));
            form.ShowDialog();
            if (form.save)
            {
                await Config.setCoifig("GoodByeDPI", form.arg);
            }

        }

        async private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Settings form = new Settings(Config.getParam("DOH"));
            form.ShowDialog();
            if (form.save)
            {
                await Config.setCoifig("DOH", form.arg);
            }
        }

       async private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Settings form = new Settings(Config.getParam("Proxy"));
            form.ShowDialog();
            if (form.save)
            {
                await Config.setCoifig("Proxy", form.arg);
            }
        }

        private void btn_doh_Start_Click(object sender, RoutedEventArgs e)
        {
            Service service = _services.Find(x => x.Name.Equals("DOH"));
            Button btn = (Button)sender;
            service.Start();
        }

        private void btn_doh_Stop_Click(object sender, RoutedEventArgs e)
        {
            Service service = _services.Find(x => x.Name.Equals("DOH"));
            Button btn = (Button)sender;
            service.Stop();
        }

        private void btn_doh_Restart_Click(object sender, RoutedEventArgs e)
        {
            Service service = _services.Find(x => x.Name.Equals("DOH"));
            Button btn = (Button)sender;
            service.Restart();
        }

        private void btn_proxy_Start_Click(object sender, RoutedEventArgs e)
        {
            Service service = _services.Find(x => x.Name.Equals("Proxy"));
            Button btn = (Button)sender;
            service.Start();
        }

        private void btn_proxy_Stop_Click(object sender, RoutedEventArgs e)
        {
            Service service = _services.Find(x => x.Name.Equals("Proxy"));
            Button btn = (Button)sender;
            service.Stop();
        }

        private void btn_proxy_Restart_Click(object sender, RoutedEventArgs e)
        {
            Service service = _services.Find(x => x.Name.Equals("Proxy"));
            Button btn = (Button)sender;
            service.Restart();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Config.createDefaultConfig();
            MessageBox.Show("Конфигурация сброшена", "Уведомление", MessageBoxButton.OK);
        }

        private async void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            btn.IsEnabled = false;
            string url = "https://x.com";
            MainPage.Log.Write("Отправка запроса к " + url);
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseProxy = true;
            handler.Proxy = null;

            HttpClient httpClient = new HttpClient(handler);

            httpClient.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                HttpResponseMessage res = await httpClient.GetAsync(url);
                MainPage.Log.Write("Ответ: " + res.StatusCode.ToString());
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    MessageBox.Show("Сайт доступен!", "Уведомление", MessageBoxButton.OK);
                    MainPage.Log.Write("Обход работает!");
                }
                else
                {
                    MessageBox.Show("Сайт недоступен!", "Уведомление", MessageBoxButton.OK);
                    MainPage.Log.Write("Обход не работает. Проверьте правильность установленных параметров.");
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("Сайт недоступен!", "Уведомление", MessageBoxButton.OK);
                MainPage.Log.Write("Обход не работает. Проверьте правильность установленных параметров.");
                MainPage.Log.Write(err.Message);
            }

            btn.IsEnabled = true;


        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (MainWindow.onProxy("http://localhost:" + port_proxy_conn.Text))
            {
                MessageBox.Show("Прокси установлен", "Уведомление", MessageBoxButton.OK);
                Log.Write("Прокси в системе настроено");
            }


        }

        private void port_proxy_conn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (MainWindow.offProxy())
            {
                MessageBox.Show("Прокси сброшен", "Уведомление", MessageBoxButton.OK);
                Log.Write("Настройки прокси в системе сброшены");
            }
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            if (!faq_show)
            {
                Button btn = sender as Button;
                btn.IsEnabled = false;
                faq faq = new faq();
                faq.Closed += new EventHandler((object sender, EventArgs e) => { faq_show = false; btn.IsEnabled = true; });
                faq.Show();
                faq_show = true;
            }
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseApp();
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            if (!log_show)
            {
                Button btn = sender as Button;
                btn.IsEnabled = false;
                LogBox win = new LogBox();
                win.Closed += new EventHandler((object sender, EventArgs a) => { log_show = false; btn.IsEnabled = true; });
                win.Show();
                win.Focus();
                log_show = true;
            }

        }
    }
}
