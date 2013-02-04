using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Net.Sockets
{
    public static class SocketExtensions
    {
        //Check to see if the connection to the machine on the other side of the socket is still active.
        //Serves the same purpose as TcpClient.Connected and TcpListener.Connected.
        public static bool IsConnected(this Socket iSocket)
        {
            try
            {
                return !(iSocket.Poll(1, SelectMode.SelectRead) && iSocket.Available == 0);
            }
            catch
            {
                return false;
            }
        }
    }
}
