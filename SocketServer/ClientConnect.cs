using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketDemo
{
    /// <summary>
    /// 客户端和客户端通讯的代码(单聊)
    /// </summary>
    public class ClientConnect
    {
        //定义静态用户字典
        private static Dictionary<string, Socket> UserDic = new Dictionary<string, Socket>();

        /// <summary>
        /// 服务器开启监听连接请求
        /// </summary>
        public void Init()
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.107"), 8168);
            server.Bind(endPoint);
            server.Listen(0);
            Console.WriteLine($"---------服务器开始监听---------");
            while (true)
            {
                var socket = server.Accept();//为连接到服务器的请求生成一个新的Socket
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
            string userName = null;
            var socket = (Socket)socketObject;

            try
            {
                while (socket.Connected)
                {
                    //接收来自客户端发送的消息
                    Byte[] receiveByte = new Byte[1024];
                    var receive = socket.Receive(receiveByte, receiveByte.Length, 0);
                    var recevieString = Encoding.UTF8.GetString(receiveByte, 0, receive);
                    Console.WriteLine($"From:{((IPEndPoint)socket.RemoteEndPoint).Address}:{((IPEndPoint)socket.RemoteEndPoint).Port}--{recevieString}");

                    //解析协议内容
                    //连接命令
                    if (recevieString.Contains("LQP_Connect"))
                    {
                        //解析登录的用户名
                        userName = recevieString.Split(':')[1];
                        //判断连接是否存在，存在则删除旧的换成新的
                        if (UserDic.ContainsKey(userName))
                        {
                            UserDic.Remove(userName);
                        }
                        UserDic.Add(userName, socket);
                        //回复连接成功
                        var sendByte = Encoding.UTF8.GetBytes("1");
                        var result = socket.Send(sendByte, sendByte.Length, SocketFlags.None);
                        //continue;
                    }
                    //发送消息命令
                    if (recevieString.Contains("LQP_Message"))
                    {
                        //协议解析
                        var messageArray = recevieString.Split(':');
                        var FromUserName = messageArray[1];//发送方
                        var ToUserName = messageArray[2];//接受方
                        var message = messageArray[3];//消息内容

                        if (UserDic.ContainsKey(ToUserName))
                        {
                            //接收方的socket连接
                            var ToUser_Socket = UserDic[ToUserName];
                            if (ToUser_Socket.Connected)
                            {
                                //服务器给客户端发送消息
                                var sendByte = Encoding.UTF8.GetBytes($"{FromUserName}:{message}");
                                var result = ToUser_Socket.Send(sendByte, sendByte.Length, SocketFlags.None);
                            }
                            else
                            {
                                UserDic.Remove(ToUserName);
                                ToUser_Socket.Close();
                                Console.WriteLine($"客户端：{((IPEndPoint)ToUser_Socket.RemoteEndPoint).Address}:{((IPEndPoint)ToUser_Socket.RemoteEndPoint).Port} 连接已断开！");
                            }
                        }
                        else
                        {
                            //发送方的socket连接
                            var FromUser_Socket = UserDic[FromUserName];
                            if (FromUser_Socket.Connected)
                            {
                                //服务器给客户端发送消息
                                var sendByte = Encoding.UTF8.GetBytes($"系统消息：用户{ToUserName}还没有登录");
                                var result = FromUser_Socket.Send(sendByte, sendByte.Length, SocketFlags.None);
                            }
                            else
                            {
                                Console.WriteLine($"客户端：{((IPEndPoint)FromUser_Socket.RemoteEndPoint).Address}:{((IPEndPoint)FromUser_Socket.RemoteEndPoint).Port} 连接已断开！");
                                FromUser_Socket.Close();
                            }
                        }
                    }
                }

                //关闭连接
                Console.WriteLine($"客户端：{((IPEndPoint)socket.RemoteEndPoint).Address}:{((IPEndPoint)socket.RemoteEndPoint).Port} 连接已断开！");
                if (!string.IsNullOrEmpty(userName))
                {
                    UserDic.Remove(userName);
                }
                socket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"客户端：{((IPEndPoint)socket.RemoteEndPoint).Address}:{((IPEndPoint)socket.RemoteEndPoint).Port} 已下线！");
                if (!string.IsNullOrEmpty(userName))
                {
                    UserDic.Remove(userName);
                }
                socket.Close();
            }
        }
    }

}
