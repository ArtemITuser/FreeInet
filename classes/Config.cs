using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

using Newtonsoft.Json.Linq;

namespace FreeNet.classes
{
    static internal class Config
    {

        static public bool createDefaultConfig()
        {
            string json = """
                {
                  "GoodByeDPI": {
                    "Arguments": "-p -m -a -f 2 -k 2 -e 1 --max-payload 2500 --min-ttl 128 --auto-ttl 1-4-128 --reverse-frag --native-frag"
                  },
                  "DOH": {
                    "Arguments": "-u https://1.1.1.1/dns-query  --hosts-files=\"%pwd_no_disk%/programs/DoH/windows-386/hosts\""
                  },
                  "Proxy": {
                    "Arguments": "%pwd%/programs/3proxy-0.9.3/bin/cfg.cfg"
                  }
                }
                """;

            File.WriteAllText("config.json", json);


            return true;
        }

        static public bool writeConfig(JObject JSON)
        {
            string json = JsonConvert.SerializeObject(JSON, Formatting.Indented);
            try
            {
                File.WriteAllText("config.json", json);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
                return false;
            }

            return true;
        }

        static private JObject getConfig()
        {
        Start:
            string text = "";

            try
            {
                if (File.Exists("config.json"))
                {
                    text = File.ReadAllText("config.json");
                }
                else
                {
                    createDefaultConfig();
                    goto Start;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace, "Ошибка чтения конф. файла", MessageBoxButton.OK);
                return null;
            }

            JObject json = JObject.Parse(text);

            return json;
        }

        static public bool setCoifig(string name, string arg)
        {
            JObject conf = getConfig();

            if (conf.Count == 0)
            {
                return false;
            }

            conf[name]["Arguments"] = arg;

            writeConfig(conf);

            return true;
        }

        static public string getParam(string name)
        {
            JObject conf = getConfig();
            return conf[name]["Arguments"].ToString();
        }

    }
}
