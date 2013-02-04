using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BF2sLoginEmu
{
    public class GpspClient : IDisposable
    {
        public bool Disposed { get; protected set; }
        private ClientStream Stream;
        private TcpClient Client;
        private Thread ClientThread;
        private Dictionary<string, object> ClientData = null;

        public GpspClient(TcpClient client)
        {
            // Set disposed to false!
            this.Disposed = false;

            // Set the client variable
            this.Client = client;

            // Init a new client stream class
            Stream = new ClientStream(client);

            ClientThread = new Thread(new ThreadStart(Start));
            ClientThread.IsBackground = true;
            ClientThread.Start();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~GpspClient()
        {
            this.Dispose();
        }

        /// <summary>
        /// Dispose method to be called by the server
        /// </summary>
        public void Dispose()
        {
            if (Client.Client.Connected)
                Client.Close();

            this.Disposed = true;
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
                Thread.Sleep(200);
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
            // Try to get user data from database
            try
            {
                ClientData = Server.Database.GetUser(GetParameterValue(recv, "email"), GetParameterValue(recv, "pass"));
            }
            catch 
            {
                Dispose();
                return;
            }

            if (ClientData == null)
            {
                Stream.Write("\\nr\\{0}\\ndone\\\\final\\");
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
            int pid = 0;
            try
            {
                pid = Server.Database.GetPID(GetParameterValue(recv, "nick"));
            }
            catch
            {
                Dispose();
                return;
            }

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
