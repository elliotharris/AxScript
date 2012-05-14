﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Sockets
{
    public class AxTCPSocket
    {
        public TcpClient Client;
        public StreamReader Reader;
        public StreamWriter Writer;

        public AxTCPSocket(String IP, int Port)
        {
            Client = new TcpClient(IP, Port);
            Reader = new StreamReader(Client.GetStream());
            Writer = new StreamWriter(Client.GetStream());
        }

        public void WriteLine(String Format, params object[] args)
        {
            Writer.WriteLine(Format, args);
        }

        public void Flush()
        {
            Writer.Flush();
        }

        public void Close()
        {
            Client.Close();
        }
    }
}
