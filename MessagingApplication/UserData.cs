using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MessagingApplication
{
    public class UserData : DependencyObject
    {

        public ObservableCollection<UIElement> Messages = new ObservableCollection<UIElement>();

        #region NewMessagesCount

        public int NewMessagesCount
        {
            get { return (int)GetValue(NewMessagesCountProperty); }
            set { SetValue(NewMessagesCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewMessagesCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewMessagesCountProperty =
            DependencyProperty.Register("NewMessagesCount", typeof(int), typeof(UserData), new PropertyMetadata(0));

        #endregion

        public IPEndPoint TargerAddress { private set; get; }

        public object ProfileImageContent;

        public RSACryptoServiceProvider RSAProvider;


        public UserData(IPEndPoint endPoint)
        {
            TargerAddress = endPoint;
            ProfileImageContent = new Ellipse() { Fill = Utilities.GetRandomColor() };
        }

        public void MessageReceived(string message,bool isFocused = false)
        {
            Messages.Add(new MessageView()
            {
                Content = message,
                MessageType = MessageView.MessageTypes.FromOthers

            });

            if (!isFocused)
                NewMessagesCount++;
        }

        public void ReportMessageSended(string message)
        {
            Messages.Add(new MessageView()
            {
                Content = message,
                MessageType = MessageView.MessageTypes.Sended

            });
        }

    }
    
}
