using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Gamespy
{
    class ClientCM
    {
        private ClientStream Stream;
        private TcpClient client;
        private int Step = 0;
        private Random rand;
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";


        public ClientCM(TcpClient client)
        {
            this.client = client;

            Console.WriteLine(" - <GPCM> Client Connected: {0}", client.Client.RemoteEndPoint);

             // Init a new client stream class
            Stream = new ClientStream(client);

            switch (Step)
            {
                case 0:
                    SendServerChallenge();
                    Step++;
                    break;
                case 1:
                    break;
            }
        }

        public void SendServerChallenge()
        {
            char[] buffer = new char[10];
            rand = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < 10; i++)
            {
                buffer[i] = chars[rand.Next(chars.Length)];
            }

            string key = buffer.ToString();

            Stream.Write( String.Format("\\lc\\1\\challenge\\{0}\\id\\1\\final\\", key) );

            Console.WriteLine(Stream.Read());
        }
    }
}
