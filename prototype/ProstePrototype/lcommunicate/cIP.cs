using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using common;
using System.IO;
using System.Runtime.InteropServices;

namespace lcommunicate
{
    internal class cIP : cTransport
    {
        //
        private cIPParams _last_ip = null;
        internal override object Connect(object o)
        {
            cIPParams ip = (cIPParams)o;
            TcpClient c = new TcpClient(ip.address, ip.port);
            _last_ip = ip;
            //byte[] ver = SendCommand(c, _time_cmd);
            return c;
        }

        internal override void Close(object o)
        {
            ((TcpClient)o).Close();
        }

        private byte[] GetBytes(NetworkStream ns)
        {
            byte[] data = new byte[1024];
            int readed = 0;
            MemoryStream ms = new MemoryStream();
            do
            {
                readed = ns.Read(data, 0, data.Length);
                ms.Write(data, 0, readed);
            } while (ns.DataAvailable);
            return ms.GetBuffer();
        }

        internal override byte[] SendCommand(object _connection, byte[] _command)
        {
            TcpClient _conn = (TcpClient)_connection;
            if (!_conn.Connected)
                if (_last_ip != null)
                    _conn.Connect(_last_ip.address, _last_ip.port);
                else return base.SendCommand(_conn, _command);
            //
            NetworkStream netstr = _conn.GetStream();
            StreamReader r = new StreamReader(netstr);
            netstr.Write(_command, 0, _command.Length);
            byte[] res = GetBytes(netstr);
            return res;
                //Encoder. r.ReadToEnd();
            //
            return base.SendCommand(_conn, _command);
        }
    }
}
