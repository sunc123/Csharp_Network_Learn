using System;
using System.Collections.Generic;
using System.IO;
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

namespace SyncTCPChatClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isExit = false;
        TcpClient client;
        BinaryReader br;
        BinaryWriter bw;

        public MainWindow()
        {
            InitializeComponent();
            Random r = new Random((int)DateTime.Now.Ticks);
            nameBox.Text = "user" + r.Next(100, 999);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            loginButton.IsEnabled = false;
            try
            {
                client = new TcpClient(Dns.GetHostName(), 51888);
                AddTalkMessage("连接成功");
            }
            catch(Exception a)
            {
                AddTalkMessage(a.Message);
                AddTalkMessage("连接失败");
                loginButton.IsEnabled = true;
                return;
            }

            NetworkStream netWorkStream = client.GetStream();

            br = new BinaryReader(netWorkStream);
            bw = new BinaryWriter(netWorkStream);
            SendMessage("Login," + nameBox.Text);

            Thread thread = new Thread(new ThreadStart(ReceiveData))
            {
                IsBackground = true
            };
            thread.Start();
        }

        void ReceiveData()
        {
            string receiveString = null;

            while(isExit == false)
            {
                try
                {
                    receiveString = br.ReadString();
                }
                catch
                {
                    if (isExit == false)
                    {
                        MessageBox.Show("与服务器失去联系");
                    }
                    return;
                }

                string[] splitString = receiveString.Split(',');
                string command = splitString[0].ToLower();
                switch (command)
                {
                    case "login":
                        AddOnline(splitString[1]);
                        break;
                    case "logout":
                        RemoveUserName(splitString[1]);
                        break;
                    case "talk":
                        AddTalkMessage(string.Format("[{0}]说: {1}",
                            splitString[1], 
                            receiveString.Substring(splitString[0].Length + 
                            splitString[1].Length + 2)));
                        break;
                    default:
                        AddTalkMessage("什么意思啊：" + receiveString);
                        break;
                }
            }
            
            //Application.Exit();
        }

        void SendMessage(string message)
        {
            try
            {
                bw.Write(message);
                bw.Flush();
            }
            catch
            {
                AddTalkMessage("发送失败");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if(userBox.SelectedIndex != -1)
            {
                SendMessage("Talk," + userBox.SelectedItem + ","
                    + sendBox.Text);
                sendBox.Clear();
            }
            else
            {
                MessageBox.Show("请先选择一个对话者");
            }
        }

        delegate void MessageDeleage(string message);

        void AddTalkMessage(string message)
        {
            if (!messageBox.Dispatcher.CheckAccess())
            {
                MessageDeleage d = new MessageDeleage(AddTalkMessage);
                messageBox.Dispatcher.Invoke(d, new object[] { message });
            }
            else
            {
                messageBox.AppendText(message + Environment.NewLine);
                messageBox.ScrollToEnd();
            }
        }

        delegate void AddOnlineDelegate(string message);

        void AddOnline(string userName)
        {
            if (!userBox.Dispatcher.CheckAccess())
            {
                AddOnlineDelegate d = AddOnline;
                userBox.Dispatcher.Invoke(d, new object[] { userName });
            }
            else
            {
                userBox.Items.Add(userName);
                userBox.SelectedIndex = userBox.Items.Count - 1;
                //userBox.ClearSelected();
            }
        }

        delegate void RemoveUserNameDelegate(string userName);

        void RemoveUserName(string userName)
        {
            if (!userBox.Dispatcher.CheckAccess())
            {
                RemoveUserNameDelegate d = RemoveUserName;
                userBox.Dispatcher.Invoke(d, userName);
            }
            else
            {
                userBox.Items.Remove(userName);
                userBox.SelectedIndex = userBox.Items.Count - 1;
                //userBox.ClearSelected();
            }
        }
    }
}
