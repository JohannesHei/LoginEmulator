using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Gamespy
{
    class ClientSP
    {
        private ClientStream Stream;
        private TcpClient client;

        public ClientSP(TcpClient client)
        {
            this.client = client;

            Console.WriteLine(" - <GPSP> Client Connected: {0}", client.Client.RemoteEndPoint);

             // Init a new client stream class
            Stream = new ClientStream(client);
        }
    }
}
