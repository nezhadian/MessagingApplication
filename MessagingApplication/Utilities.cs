using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace MessagingApplication
{
    class Utilities
    {
        public static string GetSelfIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }

            return "127.0.0.1";
        }


        public static SolidColorBrush GetRandomColor()
        {
            Random r = new Random();
            return new SolidColorBrush(Color.FromRgb((byte)r.Next(1, 255),
                     (byte)r.Next(1, 255), (byte)r.Next(1, 255)));
        }

        public static byte[] ObjectToData(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            MemoryStream stream = new MemoryStream();

            serializer.Serialize(stream, obj);

            return stream.ToArray();
        }

        public static T ReadObject<T>(ref byte[] data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream(data);

            try
            {
                return (T)serializer.Deserialize(stream);
            }
            catch
            {
                return default(T);
            }
        }
    }
}
