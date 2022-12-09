using System;

namespace common
{
    public enum eCommuncationType { IP = 1, COM = 2, USB = 3 };

    public class cIPParams
    {
        public string address;
        public int port;

        public cIPParams(string _ip, int _port) { address = _ip; port = _port; }
    }
    public class classes
    {
    }
}
