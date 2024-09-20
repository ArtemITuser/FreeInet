using FreeNet.classes;
using FreeNet.pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace FreeNet
{
    /// <summary>
    /// Interaction logic for LogBox.xaml
    /// </summary>
    public partial class LogBox : Window
    {
        private Logger log = MainPage.Log;
        private bool stop = false;
        public LogBox()
        {
            InitializeComponent();
            initLogging();
        }

        private void initLogging()
        {
            

            readTextFrom();

            log.newText += addTextTo;
            Closing += (sender, e) => { log.newText -= addTextTo; };
        }

        private void readTextFrom()
        {
            OutBox.Clear();
            foreach (string s in log.all_text)
            {
                OutBox.Dispatcher.BeginInvoke(() => { OutBox.AppendText(s + '\n'); OutBox.ScrollToEnd(); });
            }
        }
        private void Cleaning()
        {
            OutBox.Dispatcher.BeginInvoke(() =>
            {
                if (OutBox.LineCount >= 1023)
                {
                    OutBox.Clear();
                }
            });
        }
        private void addTextTo(string text)
        {
            if (!stop)
            {
                Cleaning();
                OutBox.Dispatcher.BeginInvoke(() => { OutBox.AppendText(text + '\n'); OutBox.ScrollToEnd(); });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            log.Cleaning(true);
            OutBox.Clear(); 
            OutBox.ScrollToEnd();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            stop = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            readTextFrom();
            stop = false;
        }
    }
}
