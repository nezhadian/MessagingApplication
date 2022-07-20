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
using System.Collections.ObjectModel;

namespace MessagingApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MyStyleWindow
    {
        MessageListener msgListener;
        List<ClientData> clients = new List<ClientData>();

        public ClientData SelectedClient => lstClients.SelectedIndex == -1 ? null : clients[lstClients.SelectedIndex];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            msgListener = new MessageListener(0);
            msgListener.OnMessageReceived += MsgListener_OnMessageReceived;
            msgListener.OnPortChanged += MsgListener_OnPortChanged; ;

            txtMessage.TextChanged += txtMessage_TextChanged;

            ChangeStatus("Ready");
        }


        private void MsgListener_OnMessageReceived(MessageData message, IPEndPoint source)
        {
            Dispatcher.Invoke(delegate
            {
                IPEndPoint client = IPEndPoint.Parse($"{source.Address}:{message.OpenedPort}");

                foreach (ClientData clientData in clients)
                {
                    if (clientData.TargerAddress.Equals(client))
                    {
                        clientData.MessageReceived(message, SelectedClient.TargerAddress.Equals(client));
                        return;
                    }
                }

                AddNewAddress(client.ToString()).MessageReceived(message, false);
                

            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedClient != null)
            {
                SelectedClient.SendMessage(new MessageData()
                {
                    Message = txtMessage.Text,
                    OpenedPort = msgListener.Port
                });
                txtMessage.Text = "";
            }

        }

        private ClientData AddNewAddress(string address)
        {
            ClientData client = new ClientData(IPEndPoint.Parse(address));
            UserListViewItem item = new UserListViewItem()
            {
                Content = client.TargerAddress,
                ProfileContent = client.ProfileImageContent
            };
            item.SetBinding(UserListViewItem.MessagesCountProperty, new Binding("NewMessagesCount") { Source = client });
            item.Tag = client;
            lstClients.Items.Add(item);
            clients.Add(client);
            return client;
            
        }


        #region temp
        private void btChangePort_Click(object sender, RoutedEventArgs e)
        {
            //if(int.TryParse(txtServerPort.Text,out int port))
            //{
            //    if(port >= 0 && port <= 0xffff)
            //    {
            //        msgListener.Port = int.Parse(txtServerPort.Text);
            //        ChangeStatus("Port Changed");
            //    }
            //    else
            //    {
            //        ChangeStatus($"Port Must Between 0 to {0xffff}");
            //    }
            //}
            //else
            //{
            //    ChangeStatus("Incorrect Port");
            //}
            
        }

        #endregion

        #region UI
        private void MsgListener_OnPortChanged(int port)
        {
            Dispatcher.Invoke(() => {
                Title = utils.GetSelfIPAddress();
                lblPort.Text = ":" + port;
            },System.Windows.Threading.DispatcherPriority.Loaded);
        }
        private void ChangeStatus(string status)
        {
            lblStatus.Content = status;
        }
        private void lblCurrentAddress_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(Title + lblPort.Text);
            ChangeStatus($"Address Copied : {Title + lblPort.Text}");
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void txtMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSend.Visibility = txtMessage.Text == "" ? Visibility.Collapsed : Visibility.Visible;
        }
        private void AddClientAddress_Clicked(object sender, RoutedEventArgs e)
        {
            if (IPEndPoint.TryParse(txtNewAddress.Text, out IPEndPoint target))
            {
                foreach (ClientData client in clients)
                {
                    if (client.TargerAddress.Equals(client))
                    {
                        ChangeStatus("Address Already Exists");
                        return;
                    }
                }
                AddNewAddress(txtNewAddress.Text);
                txtNewAddress.Text = "";
                ChangeStatus("Address Added");
            }
            else
            {
                txtNewAddress.Focus();
                ChangeStatus("Incorrect Address");
                return;
            }
        }

        #endregion

        #region resend command

        private void resendCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            txtMessage.Text = ((MessageView)lstMessages.SelectedItem).Content.ToString();

        }

        private void resendCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = lstMessages.SelectedItem != null;
        }


        #endregion

        private void lstClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedClient != null)
            {
                lstMessages.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = SelectedClient.Messages });
                SelectedClient.NewMessagesCount = 0;
            }
        }

        public ClientData GetSelectedClient()
        {
            return lstClients.SelectedIndex == -1 ? null : clients[lstClients.SelectedIndex];
        }
    }

    public class MyCommands
    {
        public static readonly RoutedUICommand Resend= new RoutedUICommand
            (
                "Resend",
                "Resend",
                typeof(MyCommands)
            );
    }
}
