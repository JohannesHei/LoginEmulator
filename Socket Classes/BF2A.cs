using System;
using System.Net;
using System.Net.Sockets;

namespace BF2GamespyLoginEmulator
{
    class BF2A
    {
        // Our binding port
        public const int Port = 27900;

        // Socket
        Socket listener;

        public BF2A()
        {
            // Create our end point for binding
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);

            // Init the socket
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the listener
            listener.Bind(localEndPoint);

            // Write to the console the success message
            Console.WriteLine("<BF2Available> bound to port: " + Port.ToString());
        }
    }
}
