using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Media;
using System.Security.Cryptography;

namespace MessagingApplication
{
    class PacketReceiver
    {
        #region Port

        public delegate void IntPropertyChangedEventHandler(int number);
        public event IntPropertyChangedEventHandler OnPortChanged;

        int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                if (CheckPort(value))
                {
                    _port = value;
                    RestartListener();
                    OnPortChanged?.Invoke(_port);
                }
                else
                {
                    throw new Exception($"Port Must Between 0 to {0xFFFF}");
                }
                
            }
        }
        private void SetPortWithoutRestarting(int port)
        {
            if (CheckPort(port))
            {
                _port = port;
                OnPortChanged?.Invoke(_port);
            }
        }

        private bool CheckPort(int port)
        {
            return port >= 0 && port <= 0xffff;
        }

        #endregion

        CancellationTokenSource listenerSource;

        public delegate void DataPacketEventHandler(byte[] data,IPEndPoint source);
        public event DataPacketEventHandler OnPacketReceived;



        public PacketReceiver(int port)
        {
            Port = port;
        }

        public void RestartListener()
        {
            listenerSource?.Cancel();
            listenerSource = new CancellationTokenSource();
            Thread thread = new Thread(Listen);
            thread.Start(listenerSource.Token);
        }

        void Listen(object obj)
        {
            CancellationToken token = (CancellationToken)obj;

            TcpListener listener = new TcpListener(IPAddress.Any, Port);

            try{listener.Start();}
            catch { 
                MessageBox.Show($"Can`t Start Server On Port {Port}"); 
                return;
            }

            Socket socket;

            SetPortWithoutRestarting(((IPEndPoint)listener.LocalEndpoint).Port);

            token.Register(() => listener.Stop());

            while (!token.IsCancellationRequested)
            {
                try
                {
                    socket = listener.AcceptSocket();
                    byte[] data = ReadBytes(socket);
                    Thread th = new Thread(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() => OnPacketReceived?.Invoke(data, (IPEndPoint)socket.RemoteEndPoint));
                    });
                    th.Start();
                    
                }
                catch { }
            }
        }

        private byte[] ReadBytes(Socket socket)
        {
            byte[] data = new byte[socket.ReceiveBufferSize];
            int count = socket.Receive(data);

            return data[..count];
        }


    }

    class PacketSender
    {

        public static void SendObject(object obj,IPEndPoint target)
        {
            SendBytes(Utilities.ObjectToData(obj), target);
        }

        public static void SendString(string value, IPEndPoint target)
        {
            SendBytes(Encoding.Unicode.GetBytes(value), target);

        }

        public static void SendBytes(byte[] data, IPEndPoint target)
        {
            TcpClient client = new TcpClient();

            client.Connect(target);
            client.Client.Send(data);
        }

        

    }

    public enum PacketTypes
    {
        Message,Handshake,AES
    }
    public class Packet
    {
        public PacketTypes PacketType;

        public int OpenedPort;

        public object Content;

    }
}
