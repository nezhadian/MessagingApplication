using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

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


        #region CanChat

        public bool CanChat
        {
            get { return (bool)GetValue(CanChatProperty); }
            set { SetValue(CanChatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanChat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanChatProperty =
            DependencyProperty.Register("CanChat", typeof(bool), typeof(UserData), new PropertyMetadata(false));

        #endregion



        public IPEndPoint TargerAddress { private set; get; }

        public object ProfileImageContent;

        public RSACryptoServiceProvider RSAProvider;
        RSACryptoServiceProvider SelfRSA;

        public AESData AesProvider;
        AESData SelfAes;


        public UserData(IPEndPoint endPoint)
        {
            TargerAddress = endPoint;
            ProfileImageContent = new Ellipse() { Fill = Utilities.GetRandomColor() };
            SelfRSA = new RSACryptoServiceProvider();
            SelfAes = new AESData();
        }

        #region RSA

        public void SendRSAPublicKey(int openedPort)
        {
            try
            {
                PacketSender.SendObject(new Packet()
                {
                    PacketType = PacketTypes.Handshake,
                    OpenedPort = openedPort,
                    Content = SelfRSA.ToXmlString(false)

                }, TargerAddress);
            }catch(Exception ex)
            {
                MessageBox.Show($"unable to send rsa public key to target \r\b error message: {ex.Message}");
            }
            
        }
        public void ReadRSAPublicKey(string key)
        {
            RSAProvider = new RSACryptoServiceProvider();
            RSAProvider.FromXmlString(key);
            CanChat = true;
        }

        #endregion

        public void SendAESKey(int openedPort)
        {
            PacketSender.SendObject(new Packet()
            {
                PacketType = PacketTypes.AES,
                OpenedPort = openedPort,
                Content = RSAProvider.Encrypt(SelfAes.ToXmlString(), false)
                //Content = SelfAes.ToXmlString()

            }, TargerAddress);
        }
        public void ReadAESKey(byte[] data)
        {
            byte[] decData = SelfRSA.Decrypt(data, false);
            AesProvider = new AESData();
            AesProvider.FromXmlString(decData);
        }


        public void DecodeMessage(byte[] data, bool isFocused = false)
        {
            Messages.Add(new MessageView()
            {
                Content = AesProvider.Decrypt(data),
                MessageType = MessageView.MessageTypes.FromOthers

            });

            if (!isFocused)
                NewMessagesCount++;
        }

        public void SendMessage(string message,int openedPort)
        {
            PacketSender.SendObject(new Packet()
            {
                PacketType = PacketTypes.Message,
                OpenedPort = openedPort,
                Content = SelfAes.Encrypt(message)

            }, TargerAddress);

            Messages.Add(new MessageView()
            {
                Content = message,
                MessageType = MessageView.MessageTypes.Sended

            });
        }



    }

    public class AESData
    {
        public byte[] Key;
        public byte[] IV;

        public AESData()
        {
            using(Aes aes = Aes.Create())
            {
                Key = aes.Key;
                IV = aes.IV;
            }
        }

        public byte[] Encrypt(string plainText)
        {
            using(AesManaged aes = new AesManaged())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using(CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        return ms.ToArray();
                    }
                }
            }
            

        }

        public string Decrypt(byte[] data)
        {
            using (AesManaged aes = new AesManaged())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                            return sr.ReadToEnd();
                    }
                }
            }
        }

        public byte[] ToXmlString()
        {
            return Key.Concat(IV).ToArray();
        }

        public void FromXmlString(byte[] value)
        {
            Key = value[0..32];
            IV = value[32..48];
        }
    }
    
}
