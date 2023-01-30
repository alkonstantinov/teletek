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
            try
            {
                TcpClient c = new TcpClient(ip.address, ip.port);
                _last_ip = ip;
                c.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
                c.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 2);
                c.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
                c.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                //byte[] ver = SendCommand(c, _time_cmd);
                _conn = c;
                return c;
            }
            catch { return null; }
        }

        internal override void Close(object o)
        {
            ((TcpClient)o).Close();
        }
        internal override void Close()
        {
            ((TcpClient)_conn).Close();
        }

        private byte[] GetBytes(NetworkStream ns)
        {
            byte[] data = new byte[1024];
            int readed = 0;
            int allreaded = 0;
            MemoryStream ms = new MemoryStream();
            do
            {
                readed = ns.Read(data, 0, data.Length);
                allreaded += readed;
                ms.Write(data, 0, readed);
            } while (ns.DataAvailable);
            byte[] res = new byte[allreaded];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(res, 0, (int)ms.Length);
            //byte[] buff = ms.GetBuffer();
            return res;
        }

        internal override byte[] SendCommand(object _connection, byte[] _command)
        {
            TcpClient _conn = (TcpClient)_connection;
            if (!_conn.Connected)
                if (_last_ip != null)
                {
                    _conn.Connect(_last_ip.address, _last_ip.port);
                }
                else return base.SendCommand(_conn, _command);
            //
            NetworkStream netstr = _conn.GetStream();
            StreamReader r = new StreamReader(netstr);
            netstr.Write(_command, 0, _command.Length);
            byte[] res = GetBytes(netstr);
            return res;
        }
        internal override byte[] SendCommand(object _connection, string _command)
        {
            byte[] cmd = new byte[_command.Length / 2];
            for (int i = 0; i < cmd.Length; i++)
                cmd[i] = Convert.ToByte(_command.Substring(i * 2, 2), 16);
            return SendCommand(_connection, cmd);
        }
        internal override byte[] SendCommand(byte[] _command)
        {
            TcpClient _tcpconn = (TcpClient)_conn;
            if (!_tcpconn.Connected)
                if (_last_ip != null)
                    _tcpconn.Connect(_last_ip.address, _last_ip.port);
                else return base.SendCommand(_command);
            //
            NetworkStream netstr = _tcpconn.GetStream();
            StreamReader r = new StreamReader(netstr);
            netstr.Write(_command, 0, _command.Length);
            byte[] res = GetBytes(netstr);
            return res;
        }
        internal override byte[] SendCommand(string _command)
        {
            byte[] cmd = new byte[_command.Length / 2];
            for (int i = 0; i < cmd.Length; i++)
                cmd[i] = Convert.ToByte(_command.Substring(i * 2, 2), 16);
            return SendCommand(cmd);
        }
    }
}
