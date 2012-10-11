using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Gamespy.Database;

namespace Gamespy
{
    public sealed class Server
    {
        // private Socket BF2ASocket;
        private TcpListener GPCMListener, GPSPListener;
        private Thread BF2AThread, GPCMThread, GPSPThread, InputThread;
        bool Shutdown = false;
        private GamespyDatabase GsDB;
        private List<ClientCM> ClientsCM;
        private List<ClientSP> ClientsSP;

        public Server()
        {
            // First, Try to connect to the database!
            GsDB = new GamespyDatabase();

            // Init the socket classes here
            //BF2ASocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
            GPCMListener = new TcpListener( IPAddress.Loopback, 29900 );
            GPSPListener = new TcpListener( IPAddress.Loopback, 29901 );
        }

        public void Start()
        {
            //BF2ASocket.Bind( new IPEndPoint( IPAddress.Loopback, 27900 ) );
            GPSPListener.Start();
            GPCMListener.Start();

            Console.Write
            (
                "<gpcm.gamespy.com> successfully bound to {0}" + Environment.NewLine +
                "<gpsp.gamespy.com> successfully bound to {1}" + Environment.NewLine,
                GPCMListener.LocalEndpoint, GPSPListener.LocalEndpoint
            );

            InputThread = new Thread( InputLoop );
            //BF2AThread = new Thread( BF2ALoop );
            GPCMThread = new Thread( GPCMLoop );
            GPSPThread = new Thread( GPSPLoop );

            //BF2AThread.IsBackground = true;
            GPCMThread.IsBackground = true;
            GPSPThread.IsBackground = true;
            InputThread.IsBackground = false;

            //BF2AThread.Name = "BF2A Listner";
            GPCMThread.Name = "GPCM Listner";
            GPSPThread.Name = "GPSP Listner";
            InputThread.Name = "Input Listener";

            //BF2AThread.Start();
            GPCMThread.Start();
            GPSPThread.Start();
            InputThread.Start();

            Console.WriteLine("Ready for connections!");
            Console.Beep();
        }

        public void Stop()
        {
            Shutdown = true;
            Console.WriteLine( "Stopped." );
        }

        private void BF2ALoop()
        {
            //...
        }

        private void GPCMLoop()
        {
            ClientsCM = new List<ClientCM>();

            while (!Shutdown)
            {
                if (GPCMListener.Pending())
                {
                    TcpClient client = GPCMListener.AcceptTcpClient();
                    ClientsCM.Add(new ClientCM(client, GsDB));
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

        private void GPSPLoop()
        {
            ClientsSP = new List<ClientSP>();

            while (!Shutdown)
            {
                if (GPSPListener.Pending())
                {
                    TcpClient client = GPSPListener.AcceptTcpClient();
                    ClientsSP.Add(new ClientSP(client, GsDB));
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
                    default:
                        lock (Console.Out)
                        {
                            Console.WriteLine("Unrecognized input '{0}'", Line);
                        }
                        break;
                }
            }
        }

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

        public static void Log(string message, params object[] items)
        {
            Log(String.Format(message, items));
        }
    }
}