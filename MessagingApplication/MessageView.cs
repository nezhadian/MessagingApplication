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
    public class MessageView : ListViewItem
    {
        public enum MessageTypes
        {
            FromOthers, Sended
        }

        public MessageTypes MessageType
        {
            get { return (MessageTypes)GetValue(MessageTypeProperty); }
            set { SetValue(MessageTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MessageType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageTypeProperty =
            DependencyProperty.Register("MessageType", typeof(MessageTypes), typeof(MessageView), new PropertyMetadata(MessageTypes.Sended));


        static MessageView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageView), new FrameworkPropertyMetadata(typeof(MessageView)));
        }
    }
}
