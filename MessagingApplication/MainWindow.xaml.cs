using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MessagingApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MyStyleWindow
    {
        PacketReceiver msgListener;
        List<UserData> users = new List<UserData>();
        RSACryptoServiceProvider SelfRSA;


        public UserData SelectedUser => lstUsers.SelectedIndex == -1 ? null : users[lstUsers.SelectedIndex];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            msgListener = new PacketReceiver(0);
            msgListener.OnPacketReceived += MsgListener_OnMessageReceived;
            msgListener.OnPortChanged += MsgListener_OnPortChanged; ;
            SelfRSA = new RSACryptoServiceProvider();

            txtMessage.TextChanged += txtMessage_TextChanged;

            ChangeStatus("Ready");
        }


        private void MsgListener_OnMessageReceived(byte[] data, IPEndPoint source)
        {
            Packet packet = PacketReceiver.ReadObject<Packet>(ref data);

            if (packet != null)
            {
                IPEndPoint actualSource = IPEndPoint.Parse($"{source.Address}:{packet.OpenedPort}");
                UserData findedUser = FindUser(actualSource) ?? AddNewUser(actualSource);

                switch (packet.PacketType)
                {
                    case PacketTypes.Message:
                        findedUser.MessageReceived(Encoding.Unicode.GetString(SelfRSA.Decrypt((byte[])packet.Content,false)),SelectedUser == findedUser);
                        break;
                    case PacketTypes.Handshake:
                        findedUser.RSAProvider = new RSACryptoServiceProvider();
                        findedUser.RSAProvider.FromXmlString((string)packet.Content);
                        PacketSender.SendObject(new Packet()
                        {
                            PacketType = PacketTypes.Handshake,
                            OpenedPort = msgListener.Port,
                            Content = SelfRSA.ToXmlString(false)

                        }, actualSource);
                        break;
                }
            }


        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedUser != null)
            {
                if (SelectedUser.RSAProvider != null)
                {
                    PacketSender.SendObject(new Packet()
                    {
                        PacketType = PacketTypes.Message,
                        OpenedPort = msgListener.Port,
                        Content = SelectedUser.RSAProvider.Encrypt(Encoding.Unicode.GetBytes(txtMessage.Text), false)

                    }, SelectedUser.TargerAddress); 

                    SelectedUser.ReportMessageSended(txtMessage.Text);

                    txtMessage.Text = "";
                }
                else
                {
                    PacketSender.SendObject(new Packet()
                    {
                        PacketType = PacketTypes.Handshake,
                        OpenedPort = msgListener.Port,
                        Content = SelfRSA.ToXmlString(false)

                    }, SelectedUser.TargerAddress);
                }

            }

        }

        private UserData AddNewUser(IPEndPoint address)
        {
            UserData client = new UserData(address);
            UserListViewItem item = new UserListViewItem()
            {
                Content = client.TargerAddress,
                ProfileContent = client.ProfileImageContent
            };
            item.SetBinding(UserListViewItem.MessagesCountProperty, new Binding("NewMessagesCount") { Source = client });
            item.Tag = client;
            lstUsers.Items.Add(item);
            users.Add(client);
            return client;

        }
        public UserData FindUser(IPEndPoint address)
        {
            foreach (UserData user in users)
            {
                if (user.TargerAddress.Equals(address))
                {
                    return user;
                }
            }

            return null;
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
            Dispatcher.Invoke(() =>
            {
                Title = Utilities.GetSelfIPAddress();
                lblPort.Text = ":" + port;
            }, System.Windows.Threading.DispatcherPriority.Loaded);
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

                if (FindUser(target) != null)
                {
                    ChangeStatus("User Already Exists");

                }
                else
                {
                    AddNewUser(target);
                    txtNewAddress.Text = "";
                    ChangeStatus("User Added");

                }
            }
            else
            {
                txtNewAddress.Focus();
                ChangeStatus("Incorrect User Address");
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
            if (SelectedUser != null)
            {
                lstMessages.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = SelectedUser.Messages });
                SelectedUser.NewMessagesCount = 0;
            }
        }

    }

    public class MyCommands
    {
        public static readonly RoutedUICommand Resend = new RoutedUICommand
            (
                "Resend",
                "Resend",
                typeof(MyCommands)
            );
    }
}
