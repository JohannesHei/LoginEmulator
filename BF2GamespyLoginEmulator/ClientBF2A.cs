using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Gamespy
{
    class ClientBF2A
    {
        private byte[] expected = {
			0x09, 0x00, 0x00, 0x00, 0x00, 0x62, 0x61, 0x74, 0x74, 
			0x6c, 0x65, 0x66, 0x69, 0x65, 0x6c, 0x64, 0x32, 0x00  
		};

		private byte[] send = {
			0xfe, 0xfd, 0x09, 0x00, 0x00, 0x00, 0x00 
		};

        private NetworkStream Stream;

        public ClientBF2A(Socket socket, UdpClient client)
        {
            // Init a new client stream class
            this.Stream = new NetworkStream(socket);

            while (socket.IsConnected())
            {
                Update();
            }

            socket.Close();
            socket.Dispose();
        }

        public void Update()
        {
            if (Stream.DataAvailable)
            {
                int bytesRead = 0;
                int bufferSize = 4096;
                byte[] buffer = new byte[bufferSize];

                do
                {
                    bytesRead += Stream.Read(buffer, 0, bufferSize);
                    int Counter = 0;

                    foreach (byte b in buffer)
                    {
                        if (b == 0x00)
                            break;

                        ++Counter;
                    }

                    //Trim off the null bytes.
                    Array.Resize(ref buffer, Counter);
                } while (Stream.DataAvailable);

                Console.WriteLine(Encoding.ASCII.GetString(buffer));

                if (buffer.SequenceEqual(expected))
                    Stream.Write(send, 0, 7);
            }
        }
    }
}
