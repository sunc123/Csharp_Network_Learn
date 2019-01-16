using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServer
{
    class Program
    {
        private static byte[] result = new byte[1024];

        private static int myport = 8889;

        private static Socket serverSocket;

        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");

            serverSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(new IPEndPoint(ip, myport));
            serverSocket.Listen(10);

            Console.WriteLine("启动监听{0}成功", 
                serverSocket.LocalEndPoint.ToString());

            Thread thread = new Thread(ListenClientConnect);
            thread.Start();
            Console.ReadKey();
        }

        private static void ListenClientConnect()
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();

                clientSocket.Send(Encoding.ASCII.
                    GetBytes("Server say Hello"));

                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
        }

        private static void ReceiveMessage(object client)
        {
            Socket socket = (Socket)client;

            while (true)
            {
                try
                {
                    int receiveNumber = socket.Receive(result);

                    Console.WriteLine("接收客户端 {0} 消息 {1}",
                        socket.RemoteEndPoint.ToString(),
                        Encoding.ASCII.GetString(result, 0, receiveNumber));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    break;
                }
            }
        }
    }
}
