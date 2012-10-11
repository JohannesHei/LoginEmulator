using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Gamespy
{
    public sealed class Server
    {
        //private Socket BF2ASocket;
        private bool Shutdown = false;
        private TcpListener GPCMListener, GPSPListener;
        private Thread GPCMThread, GPSPThread, InputThread;
        public static GamespyDatabase Database;
        private List<ClientCM> ClientsCM;
        private List<ClientSP> ClientsSP;

        public Server()
        {
            // First, Try to connect to the database!
            Database = new GamespyDatabase();

            // Init the socket classes here
            //BF2ASocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
            GPCMListener = new TcpListener( IPAddress.Loopback, 29900 );
            GPSPListener = new TcpListener( IPAddress.Loopback, 29901 );
        }

        /// <summary>
        /// This method initiates the entire server, and allows the Listeners
        /// to bind to their ports
        /// </summary>
        public void Start()
        {
            Console.WriteLine("Initializing...");

            //BF2ASocket.Bind( new IPEndPoint( IPAddress.Loopback, 27900 ) );
            GPSPListener.Start();
            GPCMListener.Start();

            Console.Write(Environment.NewLine +
                " - <gpcm.gamespy.com> successfully bound to {0}" + Environment.NewLine +
                " - <gpsp.gamespy.com> successfully bound to {1}" + Environment.NewLine +
                Environment.NewLine, GPCMListener.LocalEndpoint, GPSPListener.LocalEndpoint
            );

            InputThread = new Thread( InputLoop );
            GPCMThread = new Thread( GPCMLoop );
            GPSPThread = new Thread( GPSPLoop );

            GPCMThread.IsBackground = true;
            GPSPThread.IsBackground = true;
            InputThread.IsBackground = false;

            GPCMThread.Name = "GPCM Listner";
            GPSPThread.Name = "GPSP Listner";
            InputThread.Name = "Input Listener";

            GPCMThread.Start();
            GPSPThread.Start();
            InputThread.Start();

            Console.WriteLine("Ready for connections! Type 'help' for a list of commands");
            Console.Beep();
        }

        /// <summary>
        /// Kills the server, and begins the shutdown process
        /// </summary>
        public void Stop()
        {
            Shutdown = true;
            Console.WriteLine( "Stopped." );
        }

        /// <summary>
        /// Main GPCM listner loop. Accepts new Tcp clients and keeps
        /// active sessions alive
        /// </summary>
        private void GPCMLoop()
        {
            ClientsCM = new List<ClientCM>();

            while (!Shutdown)
            {
                if (GPCMListener.Pending())
                {
                    TcpClient client = GPCMListener.AcceptTcpClient();
                    ClientsCM.Add(new ClientCM(client));
                }

                for(int i = 0; i < ClientsCM.Count; i++)
                {
                    if (ClientsCM[i].Disposed)
                    {
                        ClientsCM.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Main GPSP listner loop. Accepts new Tcp clients and keeps
        /// active sessions alive
        /// </summary>
        private void GPSPLoop()
        {
            ClientsSP = new List<ClientSP>();

            while (!Shutdown)
            {
                if (GPSPListener.Pending())
                {
                    TcpClient client = GPSPListener.AcceptTcpClient();
                    ClientsSP.Add(new ClientSP(client));
                }

                for (int i = 0; i < ClientsSP.Count; i++)
                {
                    if (ClientsSP[i].Disposed)
                    {
                        ClientsSP.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Main loop for Input checking. This loop also keeps the console
        /// open and the server running
        /// </summary>
        private void InputLoop()
        {
            while( !Shutdown )
            {
                if( !Console.KeyAvailable )
                    continue;

                string Line = Console.ReadLine();

                if( string.IsNullOrWhiteSpace( Line ) )
                    continue;

                string command = Line.ToLower().Trim();
                switch (command)
                {
                    case "stop":
                    case "quit":
                    case "exit":
                        Stop();
                        break;
                    case "connections":
                        Console.WriteLine("Total Connections: {0}", ClientsCM.Count);
                        break;
                    case "help":
                        Console.Write(Environment.NewLine +
                            "stop/quit/exit - Stops the server" + Environment.NewLine +
                            "connections    - Displays the current number of connected clients" + Environment.NewLine
                        );
                        break;
                    default:
                        lock (Console.Out)
                        {
                            Console.WriteLine("Unrecognized input '{0}'", Line);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// This method is used to store a message in the console.log file
        /// </summary>
        /// <param name="message">The message to be written to the log file</param>
        public static void Log(string message)
        {
            DateTime datet = DateTime.Now;
            String logFile = Path.Combine( Utils.AssemblyPath, "console.log");
            if (!File.Exists(logFile))
            {
                FileStream files = File.Create(logFile);
                files.Close();
            }
            try
            {
                StreamWriter sw = File.AppendText(logFile);
                sw.WriteLine(datet.ToString("MM/dd hh:mm") + "> " + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        /// <summary>
        /// This method is used to store a message in the console.log file
        /// </summary>
        /// <param name="message">The message to be written to the log file</param>
        public static void Log(string message, params object[] items)
        {
            Log(String.Format(message, items));
        }
    }
}