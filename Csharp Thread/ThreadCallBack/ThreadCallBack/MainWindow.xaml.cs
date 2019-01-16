using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ThreadCallBack
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void WriteTextBox(char ch);
        private WriteTextBox writeTextBox;

        private delegate void WriteTextBox1Callback(char ch);
        private WriteTextBox1Callback writeTextBox1Callback;

        private delegate void WriteTextBox2Callback(char ch);
        private WriteTextBox2Callback writeTextBox2Callback;


        public MainWindow()
        {
            InitializeComponent();

            writeTextBox1Callback = new WriteTextBox1Callback(WriteText1);

            writeTextBox2Callback = new WriteTextBox2Callback(WriteText2);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string str = textBox3.Text;

            if(checkBox1.IsChecked == true)
            {
                Thread tsk1 = new Thread(
                    new ParameterizedThreadStart(DoTask1));

                tsk1.Start(str);
            }

            if(checkBox2.IsChecked == true)
            {
                Thread tsk2 = new Thread(
                    new ParameterizedThreadStart(DoTask2));

                tsk2.Start(str);
            }
        }
        
        private void DoTask1(object str)
        {
            writeTextBox = new WriteTextBox(WriteTextBox1);

            RunDelegate(writeTextBox, str.ToString());
        }

        private void DoTask2(object str)
        {
            writeTextBox = new WriteTextBox(WriteTextBox2);

            RunDelegate(writeTextBox, str.ToString());
        }

        private void WriteTextBox1(char ch)
        {
            //textBox1.Invoke(writeTextBox1Callback, ch);

            writeTextBox1Callback.Invoke(ch);
        }

        private void WriteTextBox2(char ch)
        {
            writeTextBox2Callback.Invoke(ch);
        }

        private void RunDelegate(WriteTextBox writeTextBox, string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                writeTextBox(str[i]);

                DateTime now = DateTime.Now;

                while(now.AddSeconds(1) > DateTime.Now) { }
            }
        }

        private void WriteText1(char ch)
        {
            textBox1.AppendText(ch + "");
        }

        private void WriteText2(char ch)
        {
            textBox2.AppendText(ch + "");
        }
    }
}
