using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MessagingApplication
{
    public class ClientData : DependencyObject
    {
        private MessageSender msgSender;

        public ObservableCollection<UIElement> Messages = new ObservableCollection<UIElement>();

        #region NewMessagesCount

        public int NewMessagesCount
        {
            get { return (int)GetValue(NewMessagesCountProperty); }
            set { SetValue(NewMessagesCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewMessagesCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewMessagesCountProperty =
            DependencyProperty.Register("NewMessagesCount", typeof(int), typeof(ClientData), new PropertyMetadata(0));

        #endregion

        public IPEndPoint TargerAddress => msgSender.TargetAddress;

        public object ProfileImageContent;




        public ClientData(IPEndPoint endPoint)
        {
            msgSender = new MessageSender();
            msgSender.TargetAddress = endPoint;
            ProfileImageContent = new Ellipse() { Fill = utils.GetRandomColor() };
        }

        public void MessageReceived(MessageData data,bool isFocused = false)
        {
            Messages.Add(new MessageView()
            {
                Content = data.Message,
                MessageType = MessageView.MessageTypes.FromOthers

            });

            if (!isFocused)
                NewMessagesCount++;
        }

        public void SendMessage(MessageData data)
        {
            try
            {
                msgSender.SendMessage(data);

                Messages.Add(new MessageView()
                {
                    Content = data.Message,
                    MessageType = MessageView.MessageTypes.Sended

                });

            }
            catch (System.Net.Sockets.SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
