using System;
using System.Collections.Generic;
using System.Text;
using common;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace lcommunicate
{
    internal class cCOM : cTransport
    {
        internal static string[] GetCOMPorts()
        {
            string[] res = null;
            res = System.IO.Ports.SerialPort.GetPortNames();
            return res;
        }
        internal static bool TryOpen(string port)
        {
            try
            {
                SerialPort _port = new SerialPort(port);
                _port.ReadTimeout = 10;
                _port.Open();
                _port.WriteLine("123");
                int cnt = _port.ReadByte();
                _port.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        //
        private SerialPort _last_port = null;
        internal override object Connect(object o)
        {
            cCOMParams cp = (cCOMParams)o;
            SerialPort conn = new SerialPort(cp.COMName);
            _last_port = conn;
            conn.Open();
            return conn;
        }

        internal override void Close(object o)
        {
            SerialPort conn = (SerialPort)o;
            conn.Close();
            conn.Dispose();
        }
        internal override void Close()
        {
            if (_last_port != null)
            {
                _last_port.Close();
                _last_port.Dispose();
            }
        }
        //private byte[] GetBytes(NetworkStream ns)
        //{
        //    return null;
        //}

        internal override byte[] SendCommand(object _connection, byte[] _command)
        {
            SerialPort conn = (SerialPort)_connection;
            conn.Write(_command, 0, _command.Length);
            conn.WriteLine("");
            //byte[] buff = new byte[2048];
            byte[] res = new byte[conn.BytesToRead];
            //string buf = conn.ReadExisting();
            conn.Read(res, 0, res.Length);
            if (res.Length == 0) return null;
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
            if (_last_port == null) return null;
            _last_port.Write(_command, 0, _command.Length);
            byte[] res = new byte[_last_port.BytesToRead];
            _last_port.Read(res, 0, res.Length);
            if (res.Length == 0) return null;
            return null;
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
