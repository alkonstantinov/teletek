using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ProstePrototype
{
    internal static class UpdateLoop
    {
        private static bool _update_exists = false;
        private static object _lock = new object();
        internal static bool UpdateExists
        {
            get
            {
                bool res = false;
                Monitor.Enter(_lock);
                res = _update_exists;
                Monitor.Exit(_lock);
                return res;
            }
        }
        public static void Loop()
        {
            while (true)
            {
                Monitor.Enter(_lock);
                CheckUpdate();
                Monitor.Exit(_lock);
                Thread.Sleep(1000*60);
            }
        }

        public static void CheckUpdate() {
            _update_exists = lupd.cUpd.Check4Updates(common.settings.updpath);
        }
    }
}
