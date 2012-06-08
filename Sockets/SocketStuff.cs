using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using axScript3;

namespace Sockets
{
    public class SocketStuff
    {
        [ExportAsAxFunction("tcp_socket")]
        public static AxTCPSocket CreateSocket(String IP, double Port)
        {
            return new AxTCPSocket(IP, (int)Port);
        }
    }
}
