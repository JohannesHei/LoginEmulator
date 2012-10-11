// ------------------------------------------
// Gamespy Login emulator for battlefield 2
//
// Created By: Plexis Development Team
// Author: Steven Wilson
// Author: Tony Hudgins
// Based on the work by Luigi Auriemma http://aluigi.altervista.org
// ------------------------------------------
using System;

namespace Gamespy
{
    class Program
    {
        public const string Version = "1.0";

        static void Main(string[] args)
        {
            // Set window title
            Console.Title = "Battlefield 2 Gamespy Login Emulator ";
            Console.WriteLine("Battlefield 2 Gamespy Login Emulator v" + Version);
            Console.WriteLine("Based on the work by Luigi Auriemma http://aluigi.altervista.org");
            Console.WriteLine("");

            // Run the server
            Server iServer;

            try
            {
                iServer = new Server();
                iServer.Start();
            }
            catch( System.Net.Sockets.SocketException e )
            {
                Console.WriteLine( e.ToString() );
            }
            catch( System.Threading.ThreadInterruptedException e )
            {
                Console.WriteLine( e.ToString() );
            }
            catch( Exception e )
            {
                Console.WriteLine( e.ToString() );
            }
        }
    } 
}