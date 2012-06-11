using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using axScript3;

namespace Sockets
{
    public class SocketStuff
    {
        [ExportAx("tcp_socket", "Creates and returns a new TCP Socket")]
        public static AxTCPSocket CreateSocket(String IP, double Port)
        {
            return new AxTCPSocket(IP, (int)Port);
        }
    }
}
