using Newtonsoft.Json.Linq;
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

    internal static class Utils
    {
        public static string LimitCharacters(string text, int length)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // If text in shorter or equal to length, just return it
            if (text.Length <= length)
            {
                return text;
            }

            // Text is longer, so try to find out where to cut
            char[] delimiters = new char[] { ' ', '.', ',', ':', ';' };
            int index = text.LastIndexOfAny(delimiters, length - 3);

            if (index > (length / 2))
            {
                return text.Substring(0, index) + "...";
            }
            else
            {
                return text.Substring(0, length - 3) + "...";
            }
        }

        public static string MakeTranslation(string keyword)
        {
            JObject translationsJSON = JObject.Parse(Properties.Settings.Default.translations);
            if (translationsJSON is null) return "Error attaching translations object";
            if (translationsJSON[keyword] != null)
            {
                var t = translationsJSON[keyword];
                if (t[Properties.Settings.Default.Language] != null)
                {
                    return t[Properties.Settings.Default.Language].ToString();
                }
                else
                {
                    return "Not found translation language";
                }
            }
            else
            {
                return $"Not found translation key";
            }
        }
    }
}
