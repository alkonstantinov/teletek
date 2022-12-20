using System;
using System.Threading;

namespace common
{
    public enum eCommuncationType { IP = 1, COM = 2, USB = 3 };

    public class cIPParams
    {
        public string address;
        public int port;

        public cIPParams(string _ip, int _port) { address = _ip; port = _port; }
    }

    /// <summary>
    /// Съдържа XML-конфигурации(template, read, write)
    /// </summary>
    public class cXmlConfigs
    {
        private object _cs_config = new object();

        private string _config_path = null;
        /// <summary>
        /// файл, съдържащ основна конфигурация на устройство
        /// </summary>
        public string ConfigPath {
            get
            {
                string f = null;
                Monitor.Enter(_cs_config);
                f = _config_path;
                Monitor.Exit(_cs_config);
                return f;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _config_path = value;
                Monitor.Exit(_cs_config);
            }
        }

        private object _config = null;
        /// <summary>
        /// Основна конфигурация(XmlDocument)
        /// </summary>
        public object Config
        {
            get
            {
                object cfg = null;
                Monitor.Enter(_cs_config);
                cfg = _config;
                Monitor.Exit(_cs_config);
                return cfg;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _config = value;
                Monitor.Exit(_cs_config);
            }
        }

        private string _read_path = null;
        /// <summary>
        /// файл, съдържащ read-конфигурация на устройство
        /// </summary>
        public string ReadConfigPath
        {
            get
            {
                string f = null;
                Monitor.Enter(_cs_config);
                f = _read_path;
                Monitor.Exit(_cs_config);
                return f;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _read_path = value;
                Monitor.Exit(_cs_config);

            }
        }

        private object _read_config = null;
        /// <summary>
        /// read-конфигурация(XmlDocument)
        /// </summary>
        public object ReadConfig
        {
            get
            {
                object cfg = null;
                Monitor.Enter(_cs_config);
                cfg = _read_config;
                Monitor.Exit(_cs_config);
                return cfg;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _read_config = value;
                Monitor.Exit(_cs_config);

            }
        }

        private string _write_path = null;
        /// <summary>
        /// файл, съдържащ write-конфигурация на устройство
        /// </summary>
        public string WriteConfigPath
        {
            get
            {
                string f = null;
                Monitor.Enter(_cs_config);
                f = _write_path;
                Monitor.Exit(_cs_config);
                return f;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _write_path = value;
                Monitor.Exit(_cs_config);
            }
        }

        private object _write_config = null;
        /// <summary>
        /// write-конфигурация(XmlDocument)
        /// </summary>
        public object WriteConfig
        {
            get
            {
                object cfg = null;
                Monitor.Enter(_cs_config);
                cfg = _write_config;
                Monitor.Exit(_cs_config);
                return cfg;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _write_config = value;
                Monitor.Exit(_cs_config);

            }
        }
    }
    public class classes
    {
    }
}
