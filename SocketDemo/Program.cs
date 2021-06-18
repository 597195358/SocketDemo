using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo
{
    class Program
    {
        /// <summary>
        /// Socket客户端
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            OneToMore();
        }

        /// <summary>
        /// 点对点通讯（客户端对客户端）
        /// </summary>
        public static void OneToOne()
        {
            Socket clientSocket = null;
            IPHostEntry hostEntry = null;
            hostEntry = Dns.GetHostEntry("");
            int port = 8168;

            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("192.168.0.107"), port);
                Socket tempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tempSocket.Connect(ipe);
                if (tempSocket.Connected)
                {
                    clientSocket = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }

            Console.WriteLine("--------输入用户名--------");
            var FromUserName = Console.ReadLine();
            if (FromUserName.Contains(":") | FromUserName.Contains("："))
            {
                Console.WriteLine("用户名不能有系统限制符号‘:’或者中文格式的‘：’");
            }
            if (clientSocket.Connected)
            {
                //发送建立连接的命令，命令格式 “LQP_Connect:用户名”
                var sendByte = Encoding.UTF8.GetBytes($"LQP_Connect:{FromUserName.Trim()}");
                var result = clientSocket.Send(sendByte, sendByte.Length, SocketFlags.None);

                //接收消息
                Byte[] receiveByte = new Byte[256];
                var receive = clientSocket.Receive(receiveByte, receiveByte.Length, 0);
                var LQP_Connect_Status = Encoding.UTF8.GetString(receiveByte, 0, receive);//服务器返回的连接状态（0:失败；1:成功）
                if (LQP_Connect_Status == "1")
                {
                    Console.WriteLine($"--------用户【{FromUserName}】登录成功,输入你想要聊天的用户名--------");
                }
                else
                {
                    Console.WriteLine($"--------用户【{FromUserName}】登录失败，请重新登录！--------");
                }
            }

            var ToUserName = Console.ReadLine();
            if (ToUserName.Contains(":") | ToUserName.Contains("："))
            {
                Console.WriteLine("你想要聊天的用户名不能有系统限制符号‘:’或者中文格式的‘：’");
            }

            Console.WriteLine($"系统消息：你和用户{ToUserName}可以开始聊天了,祝你们沟通愉快顺畅--{DateTime.Now}");
            Console.WriteLine("");
            Console.WriteLine("---------------------------------------------------------------------------------");

            var IsReciver = true;
            while (clientSocket.Connected)
            {
                //接收消息，整个链接生命周期内，只会启动一次
                if (IsReciver)
                {
                    IsReciver = false;
                    Task.Run(() =>
                    {
                        while (clientSocket.Connected)
                        {
                            //接收消息
                            Byte[] receiveByte = new Byte[1024];
                            var receive = clientSocket.Receive(receiveByte, receiveByte.Length, 0);
                            //设置输出的文本颜色
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(Encoding.UTF8.GetString(receiveByte, 0, receive));
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    });
                }

                //发送消息
                {
                    //用户输入信息
                    var message = Console.ReadLine();
                    //关闭通讯
                    if (message == "End")
                    {
                        break;
                    }
                    //发送消息命令，命令格式 "LQP_Message:登录用户名:接受者用户名:要发送的消息内容"
                    message = $"LQP_Message:{FromUserName}:{ToUserName}:{message}";
                    var sendByte = Encoding.UTF8.GetBytes(message);
                    var result = clientSocket.Send(sendByte, sendByte.Length, SocketFlags.None);
                }
            }
            clientSocket.Close();
        }

        /// <summary>
        /// 群聊
        /// </summary>
        public static void OneToMore()
        {
            Socket clientSocket = null;
            IPHostEntry hostEntry = null;
            hostEntry = Dns.GetHostEntry("");
            int port = 8168;

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("192.168.0.107"), port);
            Socket tempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tempSocket.Connect(ipe);
            if (tempSocket.Connected)
            {
                clientSocket = tempSocket;
            }

            Console.WriteLine("--------输入用户名--------");
            var FromUserName = Console.ReadLine();
            if (FromUserName.Contains(":") | FromUserName.Contains("："))
            {
                Console.WriteLine("用户名不能有系统限制符号‘:’或者中文格式的‘：’");
            }
            if (clientSocket.Connected)
            {
                //发送建立连接的命令，命令格式 “LQP_Connect:用户名”
                var sendByte = Encoding.UTF8.GetBytes($"LQP_Connect:{FromUserName.Trim()}");
                var result = clientSocket.Send(sendByte, sendByte.Length, SocketFlags.None);

                //接收消息
                Byte[] receiveByte = new Byte[256];
                var receive = clientSocket.Receive(receiveByte, receiveByte.Length, 0);
                var LQP_Connect_Status = Encoding.UTF8.GetString(receiveByte, 0, receive);//服务器返回的连接状态（0:失败；1:成功）
                if (LQP_Connect_Status == "1")
                {
                    Console.WriteLine($"--------用户【{FromUserName}】登录成功，可以开始群聊了--------");
                }
                else
                {
                    Console.WriteLine($"--------用户【{FromUserName}】登录失败，请重新登录！--------");
                }
            }

            //var ToUserName = Console.ReadLine();
            //if (ToUserName.Contains(":") | ToUserName.Contains("："))
            //{
            //    Console.WriteLine("你想要聊天的用户名不能有系统限制符号‘:’或者中文格式的‘：’");
            //}

            //Console.WriteLine($"系统消息：你和用户{ToUserName}可以开始聊天了,祝你们沟通愉快顺畅--{DateTime.Now}");
            Console.WriteLine("");
            Console.WriteLine("******************************************************");

            var IsReciver = true;
            while (clientSocket.Connected)
            {
                //接收消息，整个链接生命周期内，只会启动一次
                if (IsReciver)
                {
                    IsReciver = false;
                    Task.Run(() =>
                    {
                        while (clientSocket.Connected)
                        {
                            //接收消息
                            Byte[] receiveByte = new Byte[1024];
                            var receive = clientSocket.Receive(receiveByte, receiveByte.Length, 0);
                            //设置输出的文本颜色
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(Encoding.UTF8.GetString(receiveByte, 0, receive));
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    });
                }

                //发送消息
                {
                    //用户输入信息
                    var message = Console.ReadLine();
                    //关闭通讯
                    if (message == "End")
                    {
                        break;
                    }
                    //发送消息命令，命令格式 "LQP_Message:登录用户名:要发送的消息内容"
                    message = $"LQP_Message:{FromUserName}:{message}";
                    var sendByte = Encoding.UTF8.GetBytes(message);
                    var result = clientSocket.Send(sendByte, sendByte.Length, SocketFlags.None);
                }
            }
            clientSocket.Close();
        }

    }
}
