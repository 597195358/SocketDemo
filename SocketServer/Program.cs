using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServer
{
    class Program
    {
        /// <summary>
        /// Socket服务端
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            new SocketDemo.MoreClientConnect().Init();
        }
    }

}
