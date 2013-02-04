using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BF2sLoginEmu
{
    class GpspServer
    {
        /// <summary>
        /// Our GPSP Server Listener Socket
        /// </summary>
        private TcpListener Listener;

        /// <summary>
        /// Our client connection thread
        /// </summary>
        private Thread ListenThread;

        /// <summary>
        /// List of connected clients
        /// </summary>
        private List<GpspClient> Clients = new List<GpspClient>();

        public GpspServer()
        {
            // Attempt to bind to port 29900
            Listener = new TcpListener(IPAddress.Any, 29901);
            Listener.Start();

            // Start a new thread for accepting clients
            ListenThread = new Thread(new ThreadStart(ListenForClients));
            ListenThread.IsBackground = true;
            ListenThread.Start();
        }

        /// <summary>
        /// Shutsdown the GPSP server and socket
        /// </summary>
        public void Shutdown()
        {
            // Stop Listening for new clients
            ListenThread.Abort();

            // Disconnected all connected clients
            foreach (GpspClient C in Clients)
                C.Dispose();

            // Unbind the port
            Listener.Stop();
        }

        /// <summary>
        /// Listens for pending connections
        /// </summary>
        private void ListenForClients()
        {
            // Keep looping
            while (true)
            {
                // While we have connections pending, its a good idea to
                // proccess each one now...
                while (Listener.Pending())
                {
                    // Create a new thread to accept the connection
                    TcpClient Client = Listener.AcceptTcpClient();
                    Clients.Add(new GpspClient(Client));
                }


                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].Disposed)
                        Clients.RemoveAt(i);
                }

                Thread.Sleep(100);
            }
        }
    }
}
