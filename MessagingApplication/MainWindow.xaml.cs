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
using System.Net;

namespace MessagingApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MyStyleWindow
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
            msgListener.OnPortChanged += MsgListener_OnPortChanged; ;

            ChangeStatus("Ready");
        }

        private void MsgListener_OnPortChanged(int port)
        {
            Dispatcher.Invoke(() => {
                lblCurrentAddress.Content = $"{utils.GetSelfIPAddress()}:{port}";
            },System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void MsgListener_OnMessageReceived(MessageData message, IPEndPoint source)
        {
            Dispatcher.Invoke(delegate
            {
                lstMessages.Items.Add(new MessageView()
                {
                    Content = message.Message,
                    MessageType = MessageView.MessageTypes.FromOthers

                });

                string sourceAddress = $"{source.Address}:{message.OpenedPort}";

                //if (txtTargetAddress.Text != sourceAddress && 
                //        MessageBoxResult.Yes == MessageBox.Show("Do You want to set target address to sender address", "Info", MessageBoxButton.YesNo))
                {
                    txtTargetAddress.Text = sourceAddress;
                }
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }


        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if(IPEndPoint.TryParse(txtTargetAddress.Text,out IPEndPoint target))
                msgSender.TargetAddress = target;
            else
            {
                txtTargetAddress.Focus();
                ChangeStatus("Incorrect Address");
                return;
            }

            try
            {
                msgSender.SendMessage(new MessageData()
                {
                    OpenedPort = msgListener.Port,
                    Message = txtMessage.Text
                });

                lstMessages.Items.Add(new MessageView()
                {
                    Content = txtMessage.Text,
                    MessageType = MessageView.MessageTypes.Sended

                });

                txtMessage.Text = "";
                ChangeStatus("Message Sent");

            }
            catch (System.Net.Sockets.SocketException ex)
            {
                MessageBox.Show(ex.Message);
                ChangeStatus($"Message Error: {ex.Message}");
            }
            finally
            {
                txtMessage.Focus();
            }

            
        }

        private void btChangePort_Click(object sender, RoutedEventArgs e)
        {
            if(int.TryParse(txtServerPort.Text,out int port))
            {
                if(port >= 0 && port <= 0xffff)
                {
                    msgListener.Port = int.Parse(txtServerPort.Text);
                    ChangeStatus("Port Changed");
                }
                else
                {
                    ChangeStatus($"Port Must Between 0 to {0xffff}");
                }
            }
            else
            {
                ChangeStatus("Incorrect Port");
            }
            
        }


        private void ChangeStatus(string status)
        {
            lblStatus.Content = status;
        }

        private void lblCurrentAddress_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(lblCurrentAddress.Content.ToString());
            ChangeStatus("Address Copied");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void lstMessages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMessages.SelectedItem == null)
                return;

            txtMessage.Text = ((MessageView)lstMessages.SelectedItem).Content.ToString();
        }
    }
}
