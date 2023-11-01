using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace common
{
    internal class dClasses
    {
    }

    public static class debugSettings
    {
        private static object _ds_ = new object();
        private static JObject _dSettings = null;
        private static JObject dSettings
        {
            get
            {
                JObject _res = null;
                Monitor.Enter(_ds_);
                if (_dSettings == null && File.Exists("debugSettings.json"))
                {
                    string s = File.ReadAllText("debugSettings.json");
                    _dSettings = JObject.Parse(s);
                }
                _res = _dSettings;
                Monitor.Exit(_ds_);
                return _res;
            }
        }

        public static bool readLog
        {
            get
            {
                if (dSettings == null) return false;
                return Convert.ToBoolean(dSettings["readLog"].ToString());
            }
        }

        public static bool showReadWriteProgressSigns
        {
            get
            {
                if (dSettings == null) return false;
                return Convert.ToBoolean(dSettings["showReasWriteProgressSigns"].ToString());
            }
        }
    }
}
