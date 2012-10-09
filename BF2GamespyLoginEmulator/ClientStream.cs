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
            string message = "";

            do
            {
                bytesRead += Stream.Read(buffer, 0, bufferSize);
                int Counter = 1;

                foreach (byte b in buffer)
                {
                    if (b == 0x00)
                        break;

                    ++Counter;
                }

                //Trim off the null bytes.
                Array.Resize(ref buffer, Counter);
                message = Encoding.UTF8.GetString(buffer);

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
