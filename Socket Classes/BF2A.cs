using System;
using System.Net;
using System.Net.Sockets;

namespace BF2GamespyLoginEmulator
{
    class BF2A
    {
        // Our binding port
        public const int Port = 27900;

        public BF2A()
        {
            // Suppossed to be a UDP port!

            // Write to the console the success message
            Console.WriteLine("<BF2Available> bound to port: " + Port.ToString());
        }
    }
}
