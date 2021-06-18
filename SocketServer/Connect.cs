using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketDemo
{
    /// <summary>
    /// 客户端和用户通讯的代码
    /// </summary>
    public class Connect
    {
        //定义静态用户字典
        //private static Dictionary<string, List<Socket>> UserDic = new Dictionary<string, List<Socket>>();

        public void Init()
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.107"), 8168);
            server.Bind(endPoint);
            server.Listen(0);
            Console.WriteLine($"服务器开始监听");
            while (true)
            {
                var socket = server.Accept();
                if (socket.Connected)
                {
                    Console.WriteLine($"客户端：{((IPEndPoint)socket.RemoteEndPoint).Address}:{((IPEndPoint)socket.RemoteEndPoint).Port} 已上线");
                    Thread threadListen = new Thread(new ParameterizedThreadStart(StartListen));
                    threadListen.IsBackground = true;
                    threadListen.Start(socket);
                }
            }
        }

        public void StartListen(Object socketObject)
        {
            var socket = (Socket)socketObject;
            try
            {
                while (true)
                {
                    //接收消息
                    Byte[] receiveByte = new Byte[1024];
                    var receive = socket.Receive(receiveByte, receiveByte.Length, 0);
                    Console.WriteLine($"From:{((IPEndPoint)socket.RemoteEndPoint).Address}:{((IPEndPoint)socket.RemoteEndPoint).Port}--{Encoding.UTF8.GetString(receiveByte, 0, receive)}");

                    //发送消息
                    var text = Console.ReadLine();
                    var sendByte = Encoding.UTF8.GetBytes($"From:{((IPEndPoint)socket.LocalEndPoint).Address}:{((IPEndPoint)socket.LocalEndPoint).Port}--{text}");
                    var result = socket.Send(sendByte, sendByte.Length, SocketFlags.None);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"客户端：{((IPEndPoint)socket.RemoteEndPoint).Address}:{((IPEndPoint)socket.RemoteEndPoint).Port} 已下线");
                socket.Close();
                //throw;
            }

        }
    }

}
