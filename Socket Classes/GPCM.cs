using System;
using System.Net;
using System.Net.Sockets;

namespace BF2GamespyLoginEmulator
{
    class GPCM
    {
        public const int Port = 29900;

        // Our listener port
        TcpListener listener;

        public GPCM()
        {
            // Suppossed to be a UDP port!
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Port);

            // Write to the console the success message
            Console.WriteLine("<GPCM> bound to port: " + Port.ToString());
        }
    }
}
