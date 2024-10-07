using FreeNet.pages;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;

namespace FreeNet.classes
{

    // 0 - stopped; 1 - working; 2 - starting; 3 - stopping;
    public class Service
    {
        [DllImport("kernel32.dll")] private static extern bool TerminateProcess(IntPtr hProcess, int uExitCode);
        [DllImport("kernel32.dll")] private static extern System.UInt32 GetLastError();
        private int _PID = -1;
        private int _pStatus = -1;
        private bool _pRestart = false;
        private int _Status
        {
            get
            {
                return _pStatus;
            }

            set
            {
                if (_pStatus == value) return;
                _pStatus = value;
                Notify?.Invoke((int)value);
            }
        }
        private string _Name;
        private string _Directory;
        private string _Ports;
        private string _Arguments
        {
            get
            {
                return Config.getParams(_Name)["Arguments"].ToString();
            }
        }
        private List<Button> _Btns;
        private Label _status;
        private TextBox _tb_arg;
        private TextBox _pid;
        private TextBox _ports;
        private CheckBox _chkb_restart;

        private string Ports
        {
            set
            {
                _ports.Dispatcher.BeginInvoke(() => { _ports.Text = value; });
                _Ports = value;
            }

            get
            {
                return _Ports;
            }
        }

        public delegate void notifHandler(int status);
        public event notifHandler Notify;

        public Service(string name, string directory, List<Button> btns, Label status, TextBox tb_arg, TextBox pid, TextBox ports, CheckBox chkb_restart)
        {

            this._Name = name;
            this._Directory = directory;
            this._Btns = btns;
            this.Notify += changeStatus;
            this._status = status;
            this._tb_arg = tb_arg;
            this._pid = pid;
            this._ports = ports;
            this._chkb_restart = chkb_restart;
            chkb_restart.IsChecked = (bool)Config.getParams(_Name)["auto_restart"];

            this._Status = 0;


        }
        public int PID
        {
            get
            {
                return _PID;
            }
        }

        public int Status
        {
            get
            {
                return _Status;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public string Directory
        {
            get
            {
                return _Directory;
            }
        }

        public void PortsMonitoring()
        {
            while (_Status == 1)
            {

                Process proc = new Process();
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.Arguments = "/c netstat -aon | find \"LIST\"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;

                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();
                StreamReader sr = proc.StandardOutput;
                string out_ = sr.ReadToEnd();
                proc.WaitForExit();
                List<string> arr_out = out_.Split("\r\n".ToCharArray()).ToList();
                arr_out.RemoveAll(x => x.Equals(""));
                arr_out.RemoveAll(x => x.Contains("[::]"));

                List<string> ports = new List<string>();

                foreach (string a in arr_out)
                {
                    List<String> list = new List<String>();

                    foreach (string b in a.Split(' '))
                    {
                        if (!b.Equals(""))
                        {
                            list.Add(b);
                        }
                    }

                    if (list[4].Equals(_PID.ToString()))
                    {

                        string temp = list[1].Split(':')[1];
                        ports.Add(temp);

                    }



                }

                string parse_out = "";

                foreach (string port in ports)
                {
                    parse_out += port + " ";
                }

                if (!parse_out.Equals(Ports))
                {
                    Ports = parse_out;
                }

                Thread.Sleep(1000);
            }
            return;
        }

        private string parseArg(string arg)
        {
            string _arg = arg;
            string[][] replace = [
                ["%pwd%", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)],
                ["%pwd_no_disk%", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).Remove(0, 3)],
                ["\\", "/"]
                ];

            foreach (string[] rep in replace)
            {
                _arg = _arg.Replace(rep[0], rep[1]);
            }

            return _arg;

        }

        public bool Start()
        {
            MainPage.Log.Write("Init Start ", _Name);

            Task.Run(() =>
            {
                _Status = 2;
                Process proc = new Process();
                proc.StartInfo.FileName = _Directory;
                proc.StartInfo.Arguments = parseArg(_Arguments);
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;

                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;
                proc.EnableRaisingEvents = true;

                proc.Exited += (new EventHandler((object sender, EventArgs e) => { Stop(true, proc.ExitCode); }));

                proc.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("CP866");
                proc.StartInfo.StandardErrorEncoding = Encoding.GetEncoding("CP866");



                proc.OutputDataReceived += (s, e) => { MainPage.Log.Write(e.Data, _Name); };
                proc.ErrorDataReceived += (s, e) => { MainPage.Log.Write(e.Data, _Name); };


                try
                {
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                }
                catch (Exception e)
                {
                    _Status = 0;
                    MainPage.Log.Write(e.Message, _Name);
                }




                _PID = proc.Id;
                int pid_main = Process.GetCurrentProcess().Id;

                if (_pRestart) _pRestart = false;
                _Status = 1;
                MainPage.Log.Write("Started with PID:" + _PID, _Name);
                if (_ports != null)
                {
                    Task.Run(() => PortsMonitoring());
                }
            });




            return true;
        }

        public bool Stop(bool proc = false, int exitCode = 0)
        {
            Task.Run(() =>
            {
                if (proc)
                {
                    _Status = 0;
                    MainPage.Log.Write("Stopped\tCode: " + exitCode, _Name);
                    if (exitCode != -33)
                    {
                        _chkb_restart.Dispatcher.BeginInvoke(() =>
                        {
                            if ((bool)_chkb_restart.IsChecked)
                            {
                                Start();
                            }
                        });
                    }
                    return;
                }
                _Status = 3;
                KillProcess(_PID);
            });

            return true;
        }

        private static void KillProcess(int pid)
        {

            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited)
                {
                    if (!TerminateProcess(proc.Handle, -33))
                    {
                        var err = GetLastError();
                        throw new Exception("Error code: " + err);
                    };
                }

            }
            catch (ArgumentException)
            {

            }
        }

        public bool Restart()
        {
            _pRestart = true;
            changeStatus(-1);

            Task.Run(() =>
            {
                Stop();

                while (true)
                {
                    if (_Status == 0)
                    {
                        Start();
                        break;
                    }

                    Thread.Sleep(500);
                }
            });


            return true;
        }

        private void changeStatus(int status)
        {
            foreach (Button btn in _Btns)
            {
                btn.Dispatcher.BeginInvoke(() => { btn.IsEnabled = false; });
            }
            _tb_arg.Dispatcher.BeginInvoke(() => { _tb_arg.Clear(); });
            _pid.Dispatcher.BeginInvoke(() => { _pid.Clear(); });
            if (_ports != null) _ports.Dispatcher.BeginInvoke(() => { _ports.Clear(); });

            if (_pRestart)
            {
                _status.Dispatcher.BeginInvoke(() => { _status.Content = "Перезапуск"; });
            }
            else if (status == 0)
            {
                _status.Dispatcher.BeginInvoke(() => { _status.Content = "Остановлен"; _status.Foreground = Brushes.Red; });

                foreach (Button btn in _Btns)
                {
                    btn.Dispatcher.BeginInvoke(() =>
                    {
                        if (!btn.Tag.Equals("stop") && !btn.Tag.Equals("restart"))
                        {
                            btn.IsEnabled = true;
                        }
                    });
                }
            }
            else if (status == 1)
            {
                _status.Dispatcher.BeginInvoke(() => { _status.Content = "Запущен"; _status.Foreground = Brushes.Green; });
                _tb_arg.Dispatcher.BeginInvoke(() => { _tb_arg.Text = parseArg(_Arguments); });
                _pid.Dispatcher.BeginInvoke(() => { _pid.Text = _PID.ToString(); });

                foreach (Button btn in _Btns)
                {
                    btn.Dispatcher.BeginInvoke(() =>
                    {
                        if (!btn.Tag.Equals("start"))
                        {
                            btn.IsEnabled = true;
                        }
                    });
                }
            }
            else if (status == 2)
            {
                _status.Dispatcher.BeginInvoke(() => { _status.Content = "Запускается"; });
            }
            else if (status == 3)
            {
                _status.Dispatcher.BeginInvoke(() => { _status.Content = "Останавливается"; });
            }
        }

    }
}
