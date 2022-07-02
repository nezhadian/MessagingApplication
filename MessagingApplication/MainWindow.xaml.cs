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

namespace MessagingApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MessageSender msgSender = new MessageSender();
        MessageListener msgListener;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            msgListener = new MessageListener(0);
            msgListener.OnMessageReceived += MsgListener_OnMessageReceived;
            msgListener.OnPortChanged += (p) => Dispatcher.Invoke(() => lblCurrentPort.Content = $"Current Port : {p}");

            lblStatus.Content = "Ready";
        }

        private void MsgListener_OnMessageReceived(MessageData message)
        {
            Dispatcher.Invoke((Action)delegate
            {
                lstMessages.Items.Add(new ListBoxItem()
                {
                    Content = message.Message,
                    HorizontalContentAlignment = HorizontalAlignment.Left

                });

                txtPort.Text = message.OpenedPort + "";
            },System.Windows.Threading.DispatcherPriority.Render);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtMessage.Text))
            {
                txtMessage.Focus();
                return;
            }

            bool success = true;
            try
            {

                msgSender.SetAddress(txtIpAddress.Text, int.Parse(txtPort.Text));
                msgSender.SendMessage(new MessageData()
                {
                    OpenedPort = msgListener.Port,
                    Message = txtMessage.Text
                });

            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Error",MessageBoxButton.OK);
                success = false;
            }


            lstMessages.Items.Add(new ListBoxItem()
            {
                Content = txtMessage.Text,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Foreground = new SolidColorBrush(success ? Colors.Black : Colors.Red)

            });
            txtMessage.Text = "";
            txtMessage.Focus();
        }

        private void btChangePort_Click(object sender, RoutedEventArgs e)
        {
            msgListener.Port = int.Parse(txtServerPort.Text);
        }
    }
}
