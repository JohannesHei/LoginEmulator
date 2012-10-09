using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Gamespy
{
    public sealed class Server
    {
        // private Socket BF2ASocket;
        private TcpListener GPCMListener, GPSPListener;
        private Thread BF2AThread, GPCMThread, GPSPThread, InputThread;
        bool Shutdown = false;

        public Server()
        {
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
            List<ClientCM> Clients = new List<ClientCM>();

            while (!Shutdown)
            {
                if (GPCMListener.Pending())
                {
                    TcpClient client = GPCMListener.AcceptTcpClient();
                    Clients.Add(new ClientCM(client));
                }

                for(int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].Disposed)
                    {
                        Clients.RemoveAt(i);
                        Console.WriteLine("Successfully Removed Client");
                    }
                }
            }
        }

        private void GPSPLoop()
        {
            List<ClientSP> Clients = new List<ClientSP>();

            while (!Shutdown)
            {
                if (GPSPListener.Pending())
                {
                    TcpClient client = GPSPListener.AcceptTcpClient();
                    Clients.Add(new ClientSP(client));
                }

                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].Disposed)
                    {
                        Clients.RemoveAt(i);
                        Console.WriteLine("Successfully Removed Client");
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

                //Basic input logic.
                if( Line.ToLower().Trim() == "q" || Line.ToLower().Trim() == "quit" || Line.ToLower().Trim() == "x" || Line.ToLower().Trim() == "exit" )
                    Stop();
                else
                {
                    lock( Console.Out )
                    {
                        Console.WriteLine( "Unrecognized input '{0}'", Line );
                    }
                }
            }
        }

        private void InitClientCM(object client)
        {
            ClientCM clientCM = new ClientCM((TcpClient)client);
        }
    }
}