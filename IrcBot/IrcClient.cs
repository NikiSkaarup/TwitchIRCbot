using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;

namespace IrcBot
{
    class IrcClient
    {
        public System.Net.Sockets.TcpClient Sock { get; set; }
        public TextWriter w { get; set; }
        public TextReader r { get; set; }

        public IrcClient(string server, int port)
        {
            this.Sock = new System.Net.Sockets.TcpClient();
            this.Sock.Connect(server, port);
            this.r = new StreamReader(this.Sock.GetStream());
            this.w = new StreamWriter(this.Sock.GetStream());
        }

        public string Read()
        {
            return this.r.ReadLine();
        }

        public void Write(string m)
        {
            this.w.Write(m);
            this.w.Flush();
        }

        public void Write(string msg, string chan)
        {
            string m = "PRIVMSG %chan :%msg\r\n".Replace("%chan", chan).Replace("%msg", msg);
            Console.WriteLine("[o] " + m.Replace("\r\n", ""));
            this.Write(m);
        }

        public void Join(string msg)
        {
            string chan = msg.Replace("!join ", "");
            string m = "JOIN %chan \r\n";
            Console.WriteLine("[d] " + chan);
            if(chan[0] != '#')
            {
                m = m.Replace("%chan", '#' + chan);
            }
            else
            {
                m = m.Replace("%chan", chan);
            }
            Console.WriteLine("[o] " + m.Replace("\r\n", ""));
            this.Write(m);
        }

        public void Part(string msg)
        {
            string chan = msg.Replace("!part ", "");
            string m = "PART %chan \r\n";
            Console.WriteLine("[d] " + chan);
            if(chan[0] != '#')
            {
                m = m.Replace("%chan", '#' + chan);
            }
            else
            {
                m = m.Replace("%chan", chan);
            }
            Console.WriteLine("[o] " + m.Replace("\r\n", ""));
            this.Write(m);
        }
    }
}
