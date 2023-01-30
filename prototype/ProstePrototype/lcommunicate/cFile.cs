using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace lcommunicate
{
    internal class cFile : cTransport
    {
        private Dictionary<string, byte[]> _commands = new Dictionary<string, byte[]>();
        internal override object Connect(object o)
        {
            try
            {
                string f = (string)o;
            string s = File.ReadAllText(f);
            string[] lines = Regex.Split(s, @"[\r\n]");
            for (int i = 0; i < lines.Length; i++)
            {
                string ln = Regex.Replace(lines[i], @"[\r\n]+$", "");
                if (ln == "")
                    continue;
                string[] line = ln.Split(':');
                byte[] barr = new byte[line[1].Length / 2];
                for (int j = 0; j < barr.Length; j++)
                    barr[j] = Convert.ToByte(line[1].Substring(j * 2, 2), 16);
                _commands.Add(line[0], barr);
            }
            return "Ok";
            }
            catch (Exception e)
            {
                throw e;
                return null;
            }
        }
        internal override byte[] SendCommand(object _connection, byte[] _command)
        {
            string scmd = "";
            for (int i = 0; i < _command.Length; i++)
                scmd += _command[i].ToString("X2");
            byte[] res = null;
            if (_commands.ContainsKey(scmd))
                res = _commands[scmd];
            else
                res = new byte[0];
            return res;
        }
        internal override byte[] SendCommand(object _connection, string _command)
        {
            byte[] res = null;
            if (_commands.ContainsKey(_command))
                res = _commands[_command];
            else
                res = new byte[0];
            return res;
        }
        internal override byte[] SendCommand(byte[] _command)
        {
            byte[] res = null;
            string scmd = "";
            for (int i = 0; i < _command.Length; i++)
                scmd += _command[i].ToString("X2");
            if (_commands.ContainsKey(scmd))
                res = _commands[scmd];
            else
                res = new byte[0];
            return res;
        }
        internal override byte[] SendCommand(string _command)
        {
            byte[] res = null;
            if (_commands.ContainsKey(_command))
                res = _commands[_command];
            else
                res = new byte[0];
            return res;
        }
    }
}
