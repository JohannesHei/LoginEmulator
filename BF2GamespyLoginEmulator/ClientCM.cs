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

        public ClientCM(TcpClient client)
        {
            this.client = client;

            Console.WriteLine(" - <GPCM> Client Connected: {0}", client.Client.RemoteEndPoint);

             // Init a new client stream class
            Stream = new ClientStream(client);
        }
    }
}
