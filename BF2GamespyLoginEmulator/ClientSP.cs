using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Gamespy.Database;

namespace Gamespy
{
    public class ClientSP : IDisposable
    {
        public bool Disposed { get; protected set; }
        private ClientStream Stream;
        private TcpClient Client;
        private Thread iThread;
        private Dictionary<string, object> ClientData = null;

        public ClientSP(TcpClient client)
        {
            // Set disposed to false!
            this.Disposed = false;

            // Set the client variable
            this.Client = client;

            // Init a new client stream class
            Stream = new ClientStream(client);

            iThread = new Thread(new ThreadStart(Start));
            iThread.IsBackground = true;
            iThread.Start();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ClientSP()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// The Classes' disposing method. This method is used by the server to
        /// cleanup after the client disconnects, as well as when to unload the class
        /// </summary>
        /// <param name="Disposing"></param>
        protected virtual void Dispose(bool Disposing)
        {
            //If we've already been disposed, don't call again.
            if (this.Disposed)
                return;

            if (Disposing)
            {
                //Dispose of all managed resources here, for example a Windows.Forms.Control, Component or other object on the Framework.
            }

            //Done.
            this.Disposed = true;
        }

        /// <summary>
        /// Dispose method to be called by the server
        /// </summary>
        public void Dispose()
        {
            //Clean up everything.
            this.Dispose(true);
        }

        /// <summary>
        /// Starts the GPSP.gamespy.com listner for this client
        /// </summary>
        public void Start()
        {
            Server.Log("[GPSP] Client Connected: {0}", Client.Client.RemoteEndPoint);

            while (Client.Client.IsConnected())
            {
                Update();
            }

            Server.Log("[GPSP] Client Disconnected: {0}", Client.Client.RemoteEndPoint);
            Dispose();
        }

        /// <summary>
        /// Main Listener loop
        /// </summary>
        public void Update()
        {
            if (Stream.HasData())
            {
                // TODO: process the 'getprofile' (returned at this point) data
                string message = Stream.Read();
                string[] recv = message.Split('\\');

                switch (recv[1])
                {
                    case "nicks":
                        SendGPSP(recv);
                        break;
                    case "check":
                        SendCheck(recv);
                        break;
                }
            }
        }

        /// <summary>
        /// This method is requested by the client whenever an accounts existance needs validated
        /// </summary>
        /// <param name="recv"></param>
        private void SendGPSP(string[] recv)
        {
            ClientData = Server.Database.GetUser(GetParameterValue(recv, "email"), GetParameterValue(recv, "pass"));
            if (ClientData == null)
            {
                Stream.Write("\\nr\\0\\ndone\\\\final\\");
                return;
            }

            Stream.Write("\\nr\\1\\nick\\{0}\\uniquenick\\{1}\\ndone\\\\final\\", 
                (string)ClientData["name"], (string)ClientData["name"]
            );
        }

        /// <summary>
        /// This is the primary method for fetching an accounts BF2 PID
        /// </summary>
        /// <param name="recv"></param>
        private void SendCheck(string[] recv)
        {
            int pid = Server.Database.GetPID(GetParameterValue(recv, "nick"));
            if (pid == 0)
            {
                Stream.Write("\\cur\\0\\pid\\0\\final\\");
                return;
            }

            Stream.Write("\\cur\\0\\pid\\{0}\\final\\", pid);
        }

        /// <summary>
        /// A simple method of getting the value of the passed parameter key,
        /// from the returned array of data from the client
        /// </summary>
        /// <param name="parts">The array of data from the client</param>
        /// <param name="parameter">The parameter</param>
        /// <returns>The value of the paramenter key</returns>
        private string GetParameterValue(string[] parts, string parameter)
        {
            bool next = false;
            foreach (string part in parts)
            {
                if (next)
                    return part;
                else if (part == parameter)
                    next = true;
            }
            return "";
        }
    }
}
