using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BF2sLoginEmu
{
    class Program
    {
        public static readonly string Version = "1.1";

        static void Main(string[] args)
        {
            // Setup the console
            Console.Title = "Bf2Statistics Gamespy Login Emulator v" + Version;
            Console.WriteLine("Battlefield 2 Gamespy Login Emulator v" + Version);
            Console.WriteLine("Based on the work by Luigi Auriemma http://aluigi.altervista.org");
            Console.WriteLine("");

            // Start the server
            Server S = new Server();

            Console.WriteLine(Environment.NewLine + "Press any key to close window...");
            Console.ReadLine();
        }
    }
}
