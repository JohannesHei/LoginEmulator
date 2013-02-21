using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BF2sLoginEmu
{
    class GpcmServer
    {
        /// <summary>
        /// Our GPSP Server Listener Socket
        /// </summary>
        private TcpListener Listener;

        /// <summary>
        /// Our client connection thread
        /// </summary>
        private Thread ConnectionsThread;

        /// <summary>
        /// List of connected clients
        /// </summary>
        private List<GpcmClient> Clients = new List<GpcmClient>();

        public GpcmServer()
        {
            // Attempt to bind to port 29900
            Listener = new TcpListener(IPAddress.Any, 29900);
            Listener.Start();

            // Create a new thread to accept the connection
            Listener.BeginAcceptTcpClient(new AsyncCallback(AcceptClient), null);

            // Start a new thread for accepting clients
            ConnectionsThread = new Thread(new ThreadStart(UpdateConnections));
            ConnectionsThread.IsBackground = true;
            ConnectionsThread.Start();
        }

        /// <summary>
        /// Shutsdown the GPSP server and socket
        /// </summary>
        public void Shutdown()
        {
            // Stop updating client checks
            ConnectionsThread.Abort();

            // Disconnected all connected clients
            foreach (GpcmClient C in Clients)
                C.Dispose();

            // Unbind the port
            Listener.Stop();
        }

        /// <summary>
        /// Returns the number of connected clients
        /// </summary>
        /// <returns></returns>
        public int NumClients()
        {
            return Clients.Count;
        }

        /// <summary>
        /// Update the connected clients
        /// </summary>
        private void UpdateConnections()
        {
            // Keep looping
            while (true)
            {
                // Remove from back to front
                for (int i = Clients.Count - 1; i >= 0; i--)
                {
                    if (Clients[i].Disposed)
                    {
                        lock (Clients)
                        Clients.RemoveAt(i);
                    }
                }

                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Accepts a TcpClient
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptClient(IAsyncResult ar)
        {
            // End the operation and display the received data on  
            // the console.
            TcpClient Client = Listener.EndAcceptTcpClient(ar);
            Listener.BeginAcceptTcpClient(new AsyncCallback(AcceptClient), null);
            Clients.Add(new GpcmClient(Client));
        }
    }
}
