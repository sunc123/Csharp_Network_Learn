using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SyncTCPChatServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<User> users = new List<User>();

        IPAddress localAddress;

        private const int port = 51888;
        private TcpListener listener;

        bool isNormalExit = false;

        public MainWindow()
        {
            InitializeComponent();
            IPAddress[] addIP = Dns.GetHostAddresses
                (Dns.GetHostName());

            localAddress = addIP[0];

            //localAddress = IPAddress.Parse("127.0.0.1");

            StopButton.IsEnabled = false;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            listener = new TcpListener(localAddress, port);
            listener.Start();
            AddItemToListBox(string.Format("开始在{0}:{1}监听客户连接",
                localAddress.ToString(), port));

            Thread thread = new Thread(ListenClientConnect);
            thread.Start();
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        void ListenClientConnect()
        {
            TcpClient newClient = null;
            while (true)
            {
                try
                {
                    newClient = listener.AcceptTcpClient();
                }
                catch(Exception a)
                {
                    AddItemToListBox(a.Message);
                    break;
                }
                
                User user = new User(newClient);
                Thread thread = new Thread(ReceiveData);
                thread.Start(user);
                users.Add(user);
                AddItemToListBox(string.Format("[{0}]进入",
                    newClient.Client.RemoteEndPoint));
                AddItemToListBox(string.Format("当前连接用户数：{0}",
                    users.Count));
            }
        }

        void ReceiveData(object userState)
        {
            User user = (User)userState;
            TcpClient client = user.client;
            while (isNormalExit == false)
            {
                string receiveString = null;
                try
                {
                    receiveString = user.br.ReadString();
                }
                catch
                {
                    if (isNormalExit == false)
                    {
                        AddItemToListBox(string.Format
                            ("与[{0}]失去联系，已终止接收该用户信息",
                            client.Client.RemoteEndPoint));
                        RemoveUser(user);
                    }
                    break;
                }

                AddItemToListBox(string.Format("来自[{0}]: {1}",
                    user.client.Client.RemoteEndPoint, receiveString));

                string[] splitString = receiveString.Split(',');
                switch (splitString[0])
                {
                    case "Login":
                        user.userName = splitString[1];
                        SendToAllClient(user, receiveString);
                        break;
                    case "Logout":
                        SendToAllClient(user, receiveString);
                        RemoveUser(user);
                        return;
                    case "Talk":
                        string talkString = receiveString.Substring
                            (splitString[0].Length + splitString[1].Length + 2);
                        AddItemToListBox(string.Format("{0}对{1}说： {2}",
                            user.userName, splitString[1], talkString));
                        SendToClient(user, "talk," + user.userName + "," + talkString);
                        foreach (var target in users)
                        {
                            if(target.userName == splitString[1] &&
                                user.userName != splitString[1])
                            {
                                SendToClient(target, "talk," + user.userName + "," + talkString);
                                break;
                            }
                        }
                        break;
                    default:
                        AddItemToListBox("什么意思啊: " + receiveString);
                        break;
                }

            }
        }

        void SendToClient(User user, string message)
        {
            try
            {
                user.bw.Write(message);
                user.bw.Flush();
                AddItemToListBox(string.Format("向[{0}]发送: {1}",
                    user.userName, message));
            }
            catch
            {
                AddItemToListBox(string.Format("向[{0}]发送消息失败",
                    user.userName));
            }
        }

        void SendToAllClient(User user, string message)
        {
            string command = message.Split(',')[0].ToLower();
            if(command == "login")
            {
                for (int i = 0; i < users.Count; i++)
                {
                    SendToClient(users[i], message);

                    if(users[i].userName != user.userName)
                    {
                        SendToClient(user, "login," + users[i].userName);
                    }
                }
            }else if(command == "logout")
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].userName != user.userName)
                    {
                        SendToClient(users[i], message);
                    }
                }
            }
        }

        void RemoveUser(User user)
        {
            users.Remove(user);
            user.Close();
            AddItemToListBox(string.Format("当前连接用户数: {0}",
                users.Count));
        }

        private delegate void AddItemToListBoxDelegate(string str);

        void AddItemToListBox(string str)
        {
            if (!listBox.Dispatcher.CheckAccess())
            {
                AddItemToListBoxDelegate d = AddItemToListBox;
                listBox.Dispatcher.Invoke(d, str);
            }
            else
            {
                listBox.Items.Add(str);
                listBox.SelectedIndex = listBox.Items.Count - 1;

                //listBox.ClearSelected();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemToListBox("开始停止服务，并依次使用户退出");
            isNormalExit = true;
            for (int i = users.Count - 1; i >= 0; i--)
            {
                RemoveUser(users[i]);

                listener.Stop();
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listener != null)
            {
                StopButton_Click(sender, new RoutedEventArgs());
            }
        }
    }
}
