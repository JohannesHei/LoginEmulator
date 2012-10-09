using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Gamespy
{
    class ClientBF2A
    {
        private char[] expected = {
			'\x09', '\x00', '\x00', '\x00', '\x00', '\x62', '\x61', '\x74', '\x74', 
			'\x6c', '\x65', '\x66', '\x69', '\x65', '\x6c', '\x64', '\x32', '\x00' 
		};

		private char[] send = {
			'\xfe', '\xfd', '\x09', '\x00', '\x00', '\x00', '\x00'
		};

        private ClientStream Stream;

        public ClientBF2A(TcpClient client)
        {
            // Init a new client stream class
            Stream = new ClientStream(client);

            string read = Stream.Read();
            string cmp = new String(expected);

            if (String.Compare(read, cmp) == 0)
            {
                Stream.Write(new String(send));
            }
        }
    }
}
