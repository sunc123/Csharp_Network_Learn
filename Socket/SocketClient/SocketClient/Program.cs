﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketClient
{
    class Program
    {
        private static byte[] result = new byte[1024];

        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");

            Socket clientSocket =
                new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                clientSocket.Connect(new IPEndPoint(ip, 8889));
                Console.WriteLine("连接服务器成功");
            }
            catch
            {
                Console.WriteLine("连接服务器失败，请按回车键退出");
                return;
            }

            int receiveLength = clientSocket.Receive(result);

            Console.WriteLine("接受服务器消息：{0}",
                Encoding.ASCII.GetString(result, 0, receiveLength));

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Thread.Sleep(1000);
                    string sendMessage = "client send Message Hello " +
                        DateTime.Now;
                    clientSocket.Send(Encoding.ASCII.GetBytes(sendMessage));
                    Console.WriteLine("向服务器发送消息: {0}", sendMessage);
                }
                catch
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    break;
                }
            }

            Console.WriteLine("发送完毕，按回车键退出");
            Console.ReadKey();
        }
    }
}
