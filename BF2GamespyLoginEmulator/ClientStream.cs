using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Gamespy
{
    class ClientStream
    {
        private TcpClient Client;

        public ClientStream(TcpClient client)
        {
            this.Client = client;
        }

        public string Read()
        {
            int bytesRead = 0;
            int bufferSize = Client.ReceiveBufferSize;
            byte[] buffer = new byte[bufferSize];
            NetworkStream Stream = Client.GetStream();
            StringBuilder message = new StringBuilder();

            do
            {
                bytesRead += Stream.Read(buffer, 0, bufferSize);
                message.AppendFormat("{0}", Encoding.ASCII.GetString(buffer, 0, bufferSize));
            } while (Stream.DataAvailable);

            if (bytesRead == 0)
                return "";

            return message.ToString();
        }

        public void Write(string message)
        {
            byte[] wBuffer = Encoding.ASCII.GetBytes(message);
            this.Write(wBuffer);
        }

        public void Write(byte[] bytes)
        {
            NetworkStream Stream = Client.GetStream();
            Stream.Write(bytes, 0, bytes.Length);
        }
    }
}
