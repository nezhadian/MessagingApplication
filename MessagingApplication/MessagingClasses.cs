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

namespace MessagingApplication
{

    class MessageDecoder
    {
        public readonly int OpenedPort;
        public readonly string Message;

        public MessageDecoder(Socket socket)
        {
            byte[] data = new byte[socket.ReceiveBufferSize];
            int count = 0;
            count = socket.Receive(data);

            string message = Encoding.UTF8.GetString(data, 0, count);
            //string[] parts = message.Split('\n');
            int splitIndex = message.IndexOf("\n\n\n");

            OpenedPort = int.Parse(message[0..splitIndex]);
            Message = message[(splitIndex+3)..];
        }
    }

    class MessageBuilder
    {
        public int OpenedPort { get; set; }
        public string Message { get; set; }

        public byte[] ReadBytes()
        {
            return Encoding.UTF8.GetBytes($"{OpenedPort}\n\n\n{Message}");
            
        }
    }

    class MessageData
    {
        public int OpenedPort { get; set; }
        public string Message { get; set; }

        public MessageData(Socket socket)
        {
            byte[] data = new byte[socket.ReceiveBufferSize];
            int count = 0;
            count = socket.Receive(data);

            DecodeXML(data[..count]);
        }
        public MessageData() { }

        public byte[] EncodeXML()
        {
            return Encoding.UTF8.GetBytes($"{OpenedPort}\n\n\n{Message}");
        }

        public void DecodeXML(byte[] data)
        {
            string message = Encoding.UTF8.GetString(data);
            int splitIndex = message.IndexOf("\n\n\n");

            OpenedPort = int.Parse(message[0..splitIndex]);
            Message = message[(splitIndex + 3)..];
        }
    }

    class MessageListener
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
        private void SetPortWithourRestarting(int port)
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

        public delegate void MessageEventHandler(MessageData message);
        public event MessageEventHandler OnMessageReceived;


        public MessageListener(int port)
        {
            Port = port;
            RestartListener();
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

            SetPortWithourRestarting(((IPEndPoint)listener.LocalEndpoint).Port);

            token.Register(() => listener.Stop());

            while (!token.IsCancellationRequested)
            {
                try
                {
                    socket = listener.AcceptSocket();
                    OnMessageReceived?.Invoke(new MessageData(socket));
                }
                catch { }
            }
        }



    }

    class MessageSender
    {
        public delegate void IPEndPointChangedEventHandler(IPEndPoint endPoint);
        public event IPEndPointChangedEventHandler OnTargetAddressChanged;

        private IPEndPoint _target;
        public IPEndPoint TargetAddress
        {
            get { return _target; }
            set
            {
                _target = value;
                OnTargetAddressChanged?.Invoke(_target);
            }
        }


        public MessageSender()
        {
            SetAddress("127.0.0.1", 1990);
        }

        public void SendMessage(MessageData message)
        {
            TcpClient client = new TcpClient();

            client.Connect(TargetAddress);
            client.Client.Send(message.EncodeXML());
        }

        public void SetAddress(string ip,int port)
        {
            TargetAddress = new IPEndPoint(IPAddress.Parse(ip), port);
        }

    }

}
