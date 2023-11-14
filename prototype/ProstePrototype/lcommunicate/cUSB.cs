using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using HidSharp;
using LibUsbDotNet;
using System.Threading;
using HidSharp.Reports;
using System.Windows.Input;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;
using System.Reflection.PortableExecutable;
using HidLibrary;
using System.Linq;

namespace lcommunicate
{
    internal class cUSB : cTransport
    {
        object _last_dev = null;

        private object Connect(HidSharp.HidDevice _dev)
        {
            HidSharp.HidStream _hconn = null;
            _dev.TryOpen(out _hconn);
            _conn = _hconn;
            return _conn;
        }
        private object Connect(HidLibrary.HidDevice _dev)
        {
            _dev.OpenDevice();//
            _conn = _dev;
            return _conn;
        }
        private object Connect(UsbDevice _dev)
        {
            DeviceStream _conn = null;
            _dev.Open();
            return _conn;
        }
        internal override object Connect(object o)
        {
            HidSharp.HidDevice d = (HidSharp.HidDevice)o;
            int _vid = d.VendorID;
            int _pid = d. ProductID;
            //
            HidLibrary.HidDevice _dev = HidLibrary.HidDevices.GetDevice(d.DevicePath);
            o = _dev;
            //////////
            //UsbDeviceFinder f = new UsbDeviceFinder(_vid, _pid);
            //UsbDevice.ForceLibUsbWinBack = true;
            //UsbDevice.ForceLegacyLibUsb = true;
            //UsbRegDeviceList lst = UsbDevice.AllWinUsbDevices;
            ////WinUsbDevice _dev = WinUsbDevice.;
            //return Connect(o/*_dev*/);
            if (o is HidSharp.HidDevice) return Connect((HidSharp.HidDevice)o);
            else if (o is HidLibrary.HidDevice) return Connect((HidLibrary.HidDevice)o);
            else if (o is UsbDevice) return Connect((UsbDevice)o);
            return null;
        }
        internal override object ConnectCached(object o, object _cache)
        {
            return Connect(o);
        }
        internal override void Close(object o)
        {
            if (o is HidStream) ((HidStream)o).Close();
            else if (o is UsbDevice)
            {
                UsbDevice _dev = (UsbDevice)o;
                if (_dev == null) return;
                if (_dev.IsOpen) _dev.Close();
            }
            else if (o is HidLibrary.HidDevice) ((HidLibrary.HidDevice)o).CloseDevice();
        }
        internal override void Close()
        {
            Close(_conn);
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
            int _reslen = _command[_command.Length - 1] + 2;
            if (_reslen > _result.Length - 1) _reslen = _result.Length - 1;
            byte[] res = new byte[_reslen];
            //for (int i = 1; i < _command[_command.Length - 1] + 3; i++) res[i - 1] = _result[i];
            Memory<byte> memcmd = new Memory<byte>(_result, 1, _reslen);
            Memory<byte> memres = new Memory<byte>(res);
            memcmd.CopyTo(memres);
            return res;
        }
        private byte[] unpack_result(byte? _len, byte[] _command, byte[] _result)
        {
            if (_len == null) return unpack_result(_command, _result);
            byte[] res = new byte[(byte)_len];
            Array.Copy(_result, 1, res, 0, (byte)_len);
            return res;
        }
        private byte? responce_len(byte[] _result)
        {
            if (_result == null || _result.Length <= 1) return null;
            if (ResponceLenByte != null && _result.Length > 1 + ResponceLenByte &&
                _result[1 + (byte)ResponceLenByte] != 0)
                return (byte?)(_result[1 + (byte)ResponceLenByte] + 1 + ResponceSysBytes);
            return ResponceDefaultLen;
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
        private byte[] SendCommand(HidStream _hid_conn, byte[] _command)
        {
            int maxLen = _hid_conn.Device.GetMaxOutputReportLength();
            List<byte[]> lcmd = split_command(_command, maxLen - 1);
            foreach (byte[] a in lcmd) _hid_conn.Write(pack_cmd(a));
            //
            Thread.Sleep(SleepAfterWriteMilliseconds);
            byte[] res = new byte[1024];
            int cnt = _hid_conn.Read(res);
            //byte[] rbuf = new byte[1024];
            //_hid_conn.ReadTimeout = SleepAfterWriteMilliseconds;
            //int cnt = 0;
            //int offs = 0;
            //while (true)
            //{
            //    if (!_hid_conn.CanRead) break;
            //    try { cnt = _hid_conn.Read(rbuf); }
            //    catch { cnt = 0; }
            //    if (cnt <= 0) break;
            //    Array.Copy(rbuf, 0, res, offs, cnt);
            //    offs += cnt;
            //}
            //res = unpack_result(_command, res);
            byte? _len = responce_len(res);
            res = unpack_result(_len, _command, res);
            //byte[] res = unpack_result(_command, _hid_conn.Read());
            return res;
        }
        private byte[] SendCommand(UsbDevice _dev, byte[] _command)
        {
            UsbEndpointReader _r = _dev.OpenEndpointReader(LibUsbDotNet.Main.ReadEndpointID.Ep01);
            UsbEndpointWriter _w = _dev.OpenEndpointWriter(WriteEndpointID.Ep01);
            int cntw = 0;
            ErrorCode ec = ErrorCode.None;
            ec = _w.Write(_command, 20, out cntw);
            if (ec != ErrorCode.None) throw new Exception(UsbDevice.LastErrorString);
            byte[] res = new byte[1024];
            while (ec == ErrorCode.None)
            {
                int cntr;
                ec = _r.Read(res, SleepAfterWriteMilliseconds, out cntr);
                if (cntr == 0) break;
            }
            return res;
        }
        private byte[] SendCommand(HidLibrary.HidDevice _dev, byte[] _command)
        {
            List<byte[]> lcmd = split_command(_command, _dev.Capabilities.OutputReportByteLength - 1/*128*/);
            foreach (byte[] a in lcmd) _dev.Write(pack_cmd(a));
            Thread.Sleep(SleepAfterWriteMilliseconds);
            MemoryStream ms = new MemoryStream();
            //if (_command[_command.Length - 1] > 128)
            //{
            //    _command[0] = _command[0];
            //}
            HidDeviceData dt = new HidDeviceData(new byte[2048], HidDeviceData.ReadStatus.NoDataRead);
            bool first = true;
            byte? _cmdlennull;
            int _cmdlen = 0;
            //HidLibrary.HidReport _r = new HidReport(1924, dt);
            //_r.ReadStatus.
            while (true)
            {
                dt = _dev.Read(0);
                ms.Write(dt.Data);
                if (first)
                {
                    _cmdlennull = responce_len(dt.Data);
                    if (_cmdlennull != null) _cmdlen = (byte)_cmdlennull;
                }
                first = false;
                _cmdlen -= (byte)(dt.Data.Length - 1);
                //int i = _dev.Capabilities.NumberInputDataIndices;
                if (_cmdlen <= 0) break;
                if (dt.Status != HidDeviceData.ReadStatus.Success || dt.Data.Length <= 0) break;
                if (_command[_command.Length - 1] <= 128 || _command[_command.Length - 1] == 255) break;
            }
            byte[] res = ms.ToArray();
            //if (res.Length > 129)
            //{
            //    res[0] = res[0];
            //}
            return unpack_result(responce_len(res), _command, res);
            return null;
        }
        internal override byte[] SendCommand(object _connection, byte[] _command)
        {
            if (_connection is HidStream) return SendCommand((HidStream)_connection, _command);
            else if (_connection is UsbDevice) return SendCommand((UsbDevice)_connection, _command);
            else if (_connection is HidLibrary.HidDevice) return SendCommand((HidLibrary.HidDevice)_connection, _command);
            return null;
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
            return SendCommand(_conn, _command);
        }
        internal override byte[] SendCommand(string _command)
        {
            //if (_command == "03511500006C")
            //{
            //    _command = _command;
            //}
            if (_command == "035117000250" || _command == "035117000350" || _command == "035800000005")
            {
                _command = _command;
            }
            byte[] cmd = new byte[_command.Length / 2];
            for (int i = 0; i < cmd.Length; i++)
                cmd[i] = Convert.ToByte(_command.Substring(i * 2, 2), 16);
            return SendCommand(cmd);
        }
    }
}
