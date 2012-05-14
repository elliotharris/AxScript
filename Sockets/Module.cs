using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using axScript3;

namespace Sockets
{
    public class Module : AxSharpModule
    {
        public void Load(AxInterpreter instance)
        {
            instance.RegisterFunction("tcp_socket", GetFunc("CreateSocket"));
        }

        static NetFunction GetFunc(string f)
        {
            return new NetFunction(typeof(Module).GetMethod(f));
        }

        public static AxTCPSocket CreateSocket(String IP, double Port)
        {
            return new AxTCPSocket(IP, (int)Port);
        }
    }
}
