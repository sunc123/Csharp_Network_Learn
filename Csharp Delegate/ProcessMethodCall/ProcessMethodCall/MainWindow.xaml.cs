using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ProcessMethodCall
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void WriteTextBox(char ch);

        private WriteTextBox writeTextBox;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox1.IsChecked == true)
            {
                groupBox1.Content = "运行中...";

                textBox1.Clear();

                writeTextBox = new WriteTextBox(WriteTextBox1);

                WriTxt(writeTextBox);

                groupBox1.Content = "任务1";

                textBox3.Focus();

                textBox3.SelectAll();
            }

            if(checkBox2.IsChecked == true)
            {
                groupBox2.Content = "运行中...";

                textBox2.Clear();

                writeTextBox = new WriteTextBox(WriteTextBox2);

                WriTxt(writeTextBox);

                groupBox2.Content = "任务2";

                textBox3.Focus();

                textBox3.SelectAll();
            }
        }

        private void WriTxt(WriteTextBox wMethod)
        {
            string strdata = textBox3.Text;

            for (int i = 0; i < strdata.Length; i++)
            {
                wMethod(strdata[i]);

                DateTime now = DateTime.Now;

                while(now.AddSeconds(1) > DateTime.Now) { }
            }
        }

        private void WriteTextBox1(char ch)
        {
            textBox1.AppendText(ch + "\r");
        }

        private void WriteTextBox2(char ch)
        {
            textBox2.AppendText(ch + "\r");
        }
    }
}
