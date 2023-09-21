using System;
using System.Collections.Generic;
using System.Text;

namespace lcommunicate
{
    internal class cPackerNatron : cPacker
    {
        internal override byte[] Pack(byte[] cmd)
        {
            if (cmd == null || cmd.Length == 0 || cmd[0] == 0xac) return cmd;
            byte len = (byte)(cmd.Length + 4);
            byte[] res = new byte[len];
            res[0] = 0xac;
            res[1] = cmd[0];
            res[2] = len;
            for (int i = 1; i < cmd.Length; i++)
                res[2 + i] = cmd[i];
            byte[] buf = new byte[len - 2];
            for (int i = 0; i < buf.Length; i++) buf[i] = res[i];
            UInt16 crc = crc16M(buf);
            res[res.Length - 2] = (byte)(crc >> 8);
            res[res.Length - 1] = (byte)(crc & 0x00ff);
            return res;
        }
        internal override string Pack(string cmd)
        {
            byte[] buf = new byte[cmd.Length / 2];
            for (int i = 0; i < buf.Length; i++) buf[i] = Convert.ToByte(cmd.Substring(i * 2, 2), 16);
            byte[] pack = Pack(buf);
            string res = "";
            for (int i = 0; i < pack.Length; i++) res += pack[i].ToString("X2");
            return res;
        }
        internal override byte[] UnPack(byte[] res)
        {
            int offs = -1;
            if (res.Length >= 3) offs = res[2]; else return res;
            if (res.Length <= offs) return res;
            if (res[offs] != 0xac) return res;
            byte[] unpack = new byte[res.Length - offs];
            for (int i = offs; i < res.Length; i++)
                unpack[i - offs] = res[i];
            return unpack;
        }
    }
}
