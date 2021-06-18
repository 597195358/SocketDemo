using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketDemo
{
    /// <summary>
    /// 客户端和客户端通讯的代码（群聊）
    /// </summary>
    public class MoreClientConnect
    {
        //定义静态用户字典
        private static Dictionary<string, Socket> UserDic = new Dictionary<string, Socket>();
        //离线消息字典


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

                        foreach (var item in UserDic)
                        {
                            //当前登录用户回复连接成功
                            if (item.Key == userName)
                            {
                                var sendByte = Encoding.UTF8.GetBytes("1");
                                var result = socket.Send(sendByte, sendByte.Length, SocketFlags.None);
                            }
                            //群里其他已登录用户回复有新的人连接上线提示
                            else
                            {
                                if (item.Value.Connected)
                                {
                                    var sendByte = Encoding.UTF8.GetBytes($"用户【{userName}】已上线");
                                    var result = item.Value.Send(sendByte, sendByte.Length, SocketFlags.None);
                                }
                            }
                        }
                    }
                    //发送消息命令
                    if (recevieString.Contains("LQP_Message"))
                    {
                        //协议解析
                        var messageArray = recevieString.Split(':');
                        var FromUserName = messageArray[1];//发送方
                        var message = messageArray[2];//消息内容

                        foreach (var item in UserDic)
                        {
                            if (item.Key != userName)
                            {
                                if (item.Value.Connected)
                                {
                                    var sendByte = Encoding.UTF8.GetBytes($"{FromUserName}:{message}");
                                    var result = item.Value.Send(sendByte, sendByte.Length, SocketFlags.None);
                                }
                                else
                                {
                                    UserDic.Remove(item.Key);
                                    item.Value.Close();
                                    Console.WriteLine($"用户{item.Key}：{((IPEndPoint)item.Value.RemoteEndPoint).Address}:{((IPEndPoint)item.Value.RemoteEndPoint).Port} 连接已断开！");
                                }
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
                //下线通知
                foreach (var item in UserDic)
                {
                    if (item.Value.Connected)
                    {
                        var sendByte = Encoding.UTF8.GetBytes($"用户【{userName}】已下线");
                        var result = item.Value.Send(sendByte, sendByte.Length, SocketFlags.None);
                    }
                }
            }
            catch (Exception ex)
            {
                //关闭连接
                Console.WriteLine($"客户端：{((IPEndPoint)socket.RemoteEndPoint).Address}:{((IPEndPoint)socket.RemoteEndPoint).Port} 已下线！");
                if (!string.IsNullOrEmpty(userName))
                {
                    UserDic.Remove(userName);
                }
                socket.Close();
                //下线通知
                foreach (var item in UserDic)
                {
                    if (item.Value.Connected)
                    {
                        var sendByte = Encoding.UTF8.GetBytes($"用户【{userName}】已下线");
                        var result = item.Value.Send(sendByte, sendByte.Length, SocketFlags.None);
                    }
                }
            }
        }

    }

}
