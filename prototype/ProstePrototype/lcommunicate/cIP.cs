using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using common;

namespace lcommunicate
{
    internal class cIP : cTransport
    {
        internal override object Connect(object o)
        {
            cIPParams ip = (cIPParams)o;
            //TcpClient c = new TcpClient(ip.address, ip.port);
            return null;
        }
    }
}
