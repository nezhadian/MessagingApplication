using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MessagingApplication
{
    partial class MyStyle : ResourceDictionary
    {

        Window window;

        public MyStyle()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            window.DragMove();
        }


        private void Close_Clicked(object sender, RoutedEventArgs e)
        {
            window.Close();
            if (App.Current.MainWindow == window)
                Environment.Exit(0);
        }

        private void Maxmize_Clicked(object sender, RoutedEventArgs e)
        {
            window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void Minimize_Button(object sender, RoutedEventArgs e)
        {
            window.WindowState = WindowState.Minimized;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            window = sender as Window;
        }

    }

    #region Main Controls

    public class MyStyleWindow : Window
    {
        public object TitleBarContent
        {
            get { return (object)GetValue(TitleBarContentProperty); }
            set { SetValue(TitleBarContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleBarContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleBarContentProperty =
            DependencyProperty.Register("TitleBarContent", typeof(object), typeof(MyStyleWindow), new PropertyMetadata("Title Bar Content"));





        public double TitleBarHeight
        {
            get { return (double)GetValue(TitleBarHeightProperty); }
            set { SetValue(TitleBarHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleBarHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleBarHeightProperty =
            DependencyProperty.Register("TitleBarHeight", typeof(double), typeof(MyStyleWindow), new PropertyMetadata(30.0));



    }

    public class TextboxWithPlaceHolder : TextBox
    {
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Placeholder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(TextboxWithPlaceHolder), new PropertyMetadata(""));    


    }

    public class IconTextButton : Button
    {


        public string IconName
        {
            get { return (string)GetValue(IconNameProperty); }
            set { SetValue(IconNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconNameProperty =
            DependencyProperty.Register("IconName", typeof(string), typeof(IconTextButton), new PropertyMetadata(""));


    }

    #endregion

    #region Converters

    public class HalfHeightToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new CornerRadius((double)value / 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    #endregion

    #region other controls

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

    #endregion

    public class UserListViewItem : ListViewItem
    {

        public string MessagesCount
        {
            get { return (string)GetValue(MessagesCountProperty); }
            set { SetValue(MessagesCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MessagesCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessagesCountProperty =
            DependencyProperty.Register("MessagesCount", typeof(string), typeof(UserListViewItem), new PropertyMetadata(""));





        public object ProfileContent
        {
            get { return (object)GetValue(ProfileContentProperty); }
            set { SetValue(ProfileContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProfileContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProfileContentProperty =
            DependencyProperty.Register("ProfileContent", typeof(object), typeof(UserListViewItem), new PropertyMetadata(""));


        static UserListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UserListViewItem), new FrameworkPropertyMetadata(typeof(UserListViewItem)));
        }
    }



}
