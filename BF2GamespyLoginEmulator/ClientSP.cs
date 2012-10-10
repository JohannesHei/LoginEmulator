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
    class ClientSP : IDisposable
    {
        public bool Disposed { get; protected set; }
        private ClientStream Stream;
        private TcpClient Client;
        private Thread iThread;
        private GamespyDatabase GsDB;
        private bool BF_15 = false;
        private Dictionary<string, object> ClientData = null;

        public ClientSP(TcpClient client, GamespyDatabase gsDB)
        {
            // Set disposed to false!
            this.Disposed = false;

            // Set the client variable
            this.Client = client;

            // Init a database connection
            this.GsDB = gsDB;

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

        //This becomes private void Dispose( bool Disposing ) on sealed classes.
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

        public void Dispose()
        {
            //Clean up everything.
            this.Dispose(true);
        }

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
                        int L = recv.Length;
                        if (L == 15)
                            BF_15 = true;

                        // TODO Validate that the account exists!
                        ClientData = GsDB.GetUser(GetParameterValue(recv, "email"), GetParameterValue(recv, "pass"));
                        if (ClientData == null)
                        {
                            Stream.Write("\\nr\\0\\ndone\\\\final\\");
                            break;
                        }
                        SendGPSP();
                        break;
                    case "check":
                        SendCheck(recv);
                        break;
                }
            }
        }

        private void SendGPSP()
        {
            Stream.Write("\\nr\\1\\nick\\{0}\\uniquenick\\{1}\\ndone\\\\final\\", 
                (string)ClientData["name"], (string)ClientData["name"]
            );
        }

        private void SendCheck(string[] recv)
        {
            if (ClientData == null)
            {
                ClientData = GsDB.GetUser(GetParameterValue(recv, "nick"));
                if (ClientData == null)
                {
                    Stream.Write("\\cur\\0\\pid\\0\\final\\");
                    return;
                }
            }

            // Calculate a proper pid
            int pid;
            if ((int)ClientData["id"] < 1000000)
                pid = (((int)ClientData["id"]) + 500000000);
            else
                pid = (int)ClientData["id"];

            ClientData["id"] = pid.ToString();

            Stream.Write("\\cur\\0\\pid\\{0}\\final\\", ClientData["id"]);
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
