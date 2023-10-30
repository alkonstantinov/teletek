using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using HidSharp;
using LibUsbDotNet;
using System.Threading;

namespace lcommunicate
{
    internal class cUSB : cTransport
    {
        HidDevice _last_dev = null;

        internal override object Connect(object o)
        {
            HidDevice _dev = (HidDevice)o;
            HidStream _hid_conn;
            _dev.TryOpen(out _hid_conn);
            _conn = _hid_conn;
            //
            return _conn;
        }
        internal override object ConnectCached(object o, object _cache)
        {
            return Connect(o);
        }
        internal override void Close(object o)
        {
            ((HidStream)o).Close();
        }
        internal override void Close()
        {
            ((HidStream)_conn).Close();
        }
        private byte[] pack_cmd(byte[] _command)
        {
            //if (_command.Length == 3 && _command[0] == 0 && _command[1] == 254 && _command[2] == 0)
            //{
            //    return _command;
            //}
            byte[] res = new byte[_command.Length + 1];
            res[0] = 0;
            _command.CopyTo(res, 1);
            return res;
        }
        private byte[] unpack_result(byte[] _command, byte[] _result)
        {
            byte[] res = new byte[_command[_command.Length - 1] + 2];
            //for (int i = 1; i < _command[_command.Length - 1] + 3; i++) res[i - 1] = _result[i];
            Memory<byte> memcmd = new Memory<byte>(_result, 1, _command[_command.Length - 1] + 2);
            Memory<byte> memres = new Memory<byte>(res);
            memcmd.CopyTo(memres);
            return res;
        }
        private List<byte[]> split_command(byte[] _command, int maxLen)
        {
            List<byte[]> res = new List<byte[]>();
            int idx = 0;
            while (_command.Length - idx > maxLen)
            {
                byte[] a = new byte[maxLen];
                Array.Copy(_command, idx, a, 0, maxLen);
                //for (int i = idx; i < idx + maxLen; i++) a[i - idx] = _command[i];
                res.Add(a);
                idx += maxLen;
            }
            if (_command.Length > idx)
            {
                byte[] a = new byte[_command.Length - idx];
                Array.Copy(_command, idx, a, 0, a.Length);
                for (int i = idx; i < _command.Length; i++) a[i - idx] = _command[i];
                res.Add(a);
            }
            return res;
        }
        internal override byte[] SendCommand(object _connection, byte[] _command)
        {
            HidStream _hid_conn = (HidStream)_connection;
            int maxLen = _hid_conn.Device.GetMaxOutputReportLength();
            List<byte[]> lcmd = split_command(_command, maxLen - 1);
            foreach (byte[] a in lcmd) _hid_conn.Write(pack_cmd(a));
            //_hid_conn.Write(pack_cmd(_command));
            Thread.Sleep(7);
            byte[] res = new byte[1024];
            int cnt = _hid_conn.Read(res);
            res = unpack_result(_command, res);
            //byte[] res = unpack_result(_command, _hid_conn.Read());
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
            HidStream _hid_conn = (HidStream)_conn;
            int maxLen = _hid_conn.Device.GetMaxOutputReportLength();
            List<byte[]> lcmd = split_command(_command, maxLen - 1);
            foreach (byte[] a in lcmd) _hid_conn.Write(pack_cmd(a));
            //_hid_conn.Write(pack_cmd(_command));
            Thread.Sleep(7);
            byte[] res = new byte[1024];
            int cnt = _hid_conn.Read(res);
            res = unpack_result(_command, res);
            //byte[] res = unpack_result(_command, _hid_conn.Read());
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
