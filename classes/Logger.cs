using FreeNet.pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace FreeNet.classes
{
    public class Logger
    {
        public delegate void notifHandler(string text);
        public event notifHandler newText;


        private static string[] _buf = new string[1024];
        private static int _buf_len = 0;

        public string[] all_text
        {
          get
            {
                string[] texts = new string[_buf_len];
                for (int i = 0; i < _buf_len; i++)
                {
                    texts[i] = _buf[i];
                }
                return texts;
            }
        }
        
        public Logger()
        {
            Test();
        }
        private void Test()
        {
            Write("Добро пожаловать в программу " + Properties.Resources.APP_name + " v" + Properties.Resources.version);

        }

        public void Cleaning(bool forcibly = false)
        {
            if (_buf_len >= 1023 || forcibly)
            {
                _buf_len = 0;
                _buf = new string[1024];
            }
        }

        //public void Write(string text, string tag = "")
        //{
        //    TextBox.Dispatcher.BeginInvoke(() => { Cleaning(); });
        //    if (text == null || text.Equals(" ") || text.Equals("")) return;

        //    if (tag.Equals(""))
        //    {
        //        TextBox.Dispatcher.BeginInvoke(() => { TextBox.AppendText(text + "\n"); TextBox.ScrollToEnd(); });
        //    }
        //    else
        //    {
        //        TextBox.Dispatcher.BeginInvoke(() => { TextBox.AppendText("[" + tag + "] " + text + "\n"); TextBox.ScrollToEnd(); });
        //    }
        //}

        public void Write(string text, string tag = "")
        {
            Cleaning();
            if (text == null || text.Equals(" ") || text.Equals("")) return;
            if (tag.Equals(""))
            {
                _buf[_buf_len] = "[" + DateTime.Now + "] " + text;
                newText?.Invoke(_buf[_buf_len]);
                Console.WriteLine(_buf[_buf_len]);
                _buf_len++;
            }
            else
            {
                _buf[_buf_len] = "[" + DateTime.Now + "]["+tag+"] "+text;
                newText?.Invoke(_buf[_buf_len]);
                Console.WriteLine(_buf[_buf_len]);
                _buf_len++;
            }

        }
    }
}
