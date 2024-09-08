using FreeNet.pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FreeNet.classes
{
    public class Logger
    {
        private TextBox TextBox;
        public Logger(TextBox textBox)
        {
            this.TextBox = textBox;

            textBox.Clear();
            Test();
        }
        private void Test()
        {
            Write("Добро пожаловать в программу " + Properties.Resources.APP_name + " v" + Properties.Resources.version);

        }

        private void Cleaning()
        {
            if (TextBox.LineCount >= 5000)
            {
                TextBox.Clear();
            }
        }
        public void Write(string text, string tag = "")
        {
            TextBox.Dispatcher.BeginInvoke(() => { Cleaning(); });
            if (text == null || text.Equals(" ") || text.Equals("")) return;

            if (tag.Equals(""))
            {
                TextBox.Dispatcher.BeginInvoke(() => { TextBox.AppendText(text + "\n"); TextBox.ScrollToEnd(); });
            }
            else
            {
                TextBox.Dispatcher.BeginInvoke(() => { TextBox.AppendText("[" + tag + "] " + text + "\n"); TextBox.ScrollToEnd(); });
            }
        }
    }
}
