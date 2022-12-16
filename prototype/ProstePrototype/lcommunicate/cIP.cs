using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using common;
using System.IO;

namespace lcommunicate
{
    internal class cIP : cTransport
    {
        private byte[] _ver_cmd = new byte[] { 0x07, 0x51, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private byte[] _cmd = new byte[] { 0x03, 0x51, 0x0F, 0x00, 0x00, 0x89 };
        private byte[] _time_cmd = new byte[] { 0x00, 0x23, 0xFF };
        //
        //
        private cIPParams _last_ip = null;
        internal override object Connect(object o)
        {
            cIPParams ip = (cIPParams)o;
            TcpClient c = new TcpClient(ip.address, ip.port);
            _last_ip = ip;
            byte[] ver = SendCommand(c, _time_cmd);
            return c;
        }

        internal override void Close(object o)
        {
            ((TcpClient)o).Close();
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
            string res = r.ReadToEnd();
            //
            return base.SendCommand(_conn, _command);
        }
    }
}
