using System;
using System.Collections.Generic;
using System.Text;
using common;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;

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
            conn.BaudRate = cp.rate;//96000;// 115200;
            //conn.Handshake = Handshake.XOnXOff;
            //conn.DtrEnable = false;
            //conn.RtsEnable = true;
            try
            {
                conn.Open();
                return conn;
            }
            catch
            {
                return null;
            }
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
                _last_port = null;
            }
        }
        //private byte[] GetBytes(NetworkStream ns)
        //{
        //    return null;
        //}

        internal override byte[] SendCommand(object _connection, byte[] _command)
        {
            _command = _packer.Pack(_command);
            SerialPort conn = (SerialPort)_connection;
            conn.Write(_command, 0, _command.Length);
            byte[] res = new byte[conn.BytesToRead];
            conn.Read(res, 0, res.Length);
            if (res.Length == 0) return null;
            return _packer.UnPack(res);
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
            _command = _packer.Pack(_command);
            if (_last_port == null) return null;
            _last_port.Write(_command, 0, _command.Length);
            _last_port.WriteLine("");
            Thread.Sleep(10);
            byte[] res = new byte[_last_port.BytesToRead];
            _last_port.Read(res, 0, res.Length);
            if (res.Length == 0) return null;
            return _packer.UnPack(res);
        }
        internal override byte[] SendCommand(string _command)
        {
            List<byte[]> lstRes = new List<byte[]>();
            int len = 0;
            string[] cmds = _command.Split('\n');
            foreach (string scmd in cmds)
            {
                byte[] cmd = new byte[scmd.Length / 2];
                for (int i = 0; i < cmd.Length; i++)
                    cmd[i] = Convert.ToByte(scmd.Substring(i * 2, 2), 16);
                byte[] bres = SendCommand(cmd);
                if (bres != null && bres.Length > 0)
                {
                    len+=bres.Length;
                    lstRes.Add(bres);
                }
            }
            if (len > 0)
            {
                byte[] res = new byte[len];
                int offs = 0;
                foreach (byte[] a in lstRes)
                {
                    a.CopyTo(res, offs);
                    offs += a.Length;
                }
                return res;
            }
            return null;
        }
    }
}
