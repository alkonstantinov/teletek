using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ljson
{
    public class cEclipse
    {
        public static string Convert(string json)
        {
            JObject o = JObject.Parse(json);
            JObject o1 = new JObject();
            //
            return o.ToString();
        }
    }
}
