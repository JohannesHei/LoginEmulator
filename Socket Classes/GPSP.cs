using System;
using System.Net;
using System.Net.Sockets;

namespace BF2GamespyLoginEmulator
{
    class GPSP
    {
        public const int Port = 29901;

        // Our listener port
        TcpListener listener;

        public GPSP()
        {
            // Suppossed to be a UDP port!
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Port);

            // Write to the console the success message
            Console.WriteLine("<GPSP> bound to port: " + Port.ToString());
        }
    }
}
