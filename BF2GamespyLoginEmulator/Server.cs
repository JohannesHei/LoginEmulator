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

            // Start our listners
            GPSPListener.Start();
            GPCMListener.Start();

            Console.Write(Environment.NewLine +
                " - <gpcm.gamespy.com> successfully bound to {0}" + Environment.NewLine +
                " - <gpsp.gamespy.com> successfully bound to {1}" + Environment.NewLine +
                Environment.NewLine, GPCMListener.LocalEndpoint, GPSPListener.LocalEndpoint
            );

            // Init our main threads
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

            Console.WriteLine("Ready for connections! Type 'help' for a list of commands" + Environment.NewLine);
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

                Thread.Sleep(100);
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

                Thread.Sleep(100);
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
                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(100);
                    continue;
                }


                string Line = Console.ReadLine();

                if( string.IsNullOrWhiteSpace( Line ) )
                    continue;

                // Define some base vars
                Dictionary<string, object> user = new Dictionary<string,object>();

                string command = Line.Trim();
                string[] parts = command.Split(' ');
                switch (parts[0])
                {
                    case "stop":
                    case "quit":
                    case "exit":
                        Stop();
                        break;
                    case "connections":
                        Console.WriteLine(" - Total Connections: {0}" + Environment.NewLine, ClientsCM.Count);
                        break;
                    case "accounts":
                        Console.WriteLine(" - Total Accounts: {0}" + Environment.NewLine, Database.GetNumAccounts());
                        break;
                    case "fetch":
                        // Prevent an out of range exception
                        if (parts.Length < 2)
                        {
                            Console.WriteLine(" - Incorrect command format. Please type 'help' to see list of available commands.");
                            Console.WriteLine("");
                            continue;
                        }

                        // Make sure we have a nick
                        if(String.IsNullOrEmpty(parts[1]))
                        {
                            Console.WriteLine(" - No account named provided. Please make sure you are providing an account name, and not a space");
                            Console.WriteLine("");
                            continue;
                        }

                        // Fetch user account info
                        user = Database.GetUser(parts[1]);
                        if(user == null)
                        {
                            Console.WriteLine(" - Account '{0}' does not exist in the gamespy database.", parts[1]);
                            continue;
                        }

                        // Get BF2 PID
                        Console.Write(
                            " - Account ID: " + user["id"].ToString() + Environment.NewLine +
                            " - Email: " + user["email"].ToString() + Environment.NewLine +
                            " - Country: " + user["country"].ToString() + Environment.NewLine
                            + Environment.NewLine
                        );
                        break;
                    case "create":
                        // Prevent an out of range exception
                        if (parts.Length < 4)
                        {
                            Console.WriteLine(" - Incorrect command format. Please type 'help' to see list of available commands.");
                            Console.WriteLine("");
                            continue;
                        }

                        // Make sure our strings are not empty!
                        if (String.IsNullOrEmpty(parts[1]) || String.IsNullOrEmpty(parts[2]) || String.IsNullOrEmpty(parts[3]))
                        {
                            Console.WriteLine(" - Account name, password, or email was not provided. Please try again with the correct format.");
                            Console.WriteLine("");
                            continue;
                        }

                        // Make sure the account exists!
                        if (Database.UserExists(parts[1]))
                        {
                            Console.WriteLine(" - Account '{0}' already exists in the gamespy database.", parts[1]);
                            continue;
                        }

                        bool r = Database.CreateUser(parts[1], parts[2], parts[3], "00");
                        string m = (r == true) ? " - Account created successfully" : " - Error creating account!";
                        Console.WriteLine(m + Environment.NewLine);
                        break;
                    case "delete":
                        // Prevent an out of range exception
                        if (parts.Length < 2)
                        {
                            Console.WriteLine(" - Incorrect command format. Please type 'help' to see list of available commands.");
                            Console.WriteLine("");
                            continue;
                        }

                        // Make sure our strings are not empty!
                        if (String.IsNullOrEmpty(parts[1]))
                        {
                            Console.WriteLine(" - Account name was not provided. Please try again with the correct format.");
                            Console.WriteLine("");
                            continue;
                        }

                        // Make sure the account exists!
                        if (!Database.UserExists(parts[1]))
                        {
                            Console.WriteLine(" - Account '{0}' doesnt exist in the gamespy database.", parts[1]);
                            Console.WriteLine("");
                            continue;
                        }

                        // Do a confimration
                        Console.WriteLine(" - Are you sure you want to delete account '{0}'? <y/n>", parts[1]);
                        string v = Console.ReadLine().ToLower();

                        // If no, stop here
                        if (v == "n" || v == "no")
                        {
                            Console.WriteLine(" - Command cancelled." + Environment.NewLine);
                            continue;
                        }

                        // Process any command other then no
                        if (v == "y" || v == "yes")
                        {
                            string output = "";
                            if (Database.DeleteUser(parts[1]) == 1)
                                output = " - Account deleted successfully";
                            else
                                output = " - Failed to remove account from database.";

                            Console.WriteLine(output + Environment.NewLine);
                        }
                        else
                            Console.WriteLine(" - Incorrect repsonse. Aborting command" + Environment.NewLine);

                        break;
                    case "setpid":
                        // Prevent an out of range exception
                        if (parts.Length < 3)
                        {
                            Console.WriteLine(" - Incorrect command format. Please type 'help' to see list of available commands.");
                            Console.WriteLine("");
                            continue;
                        }

                        // Make sure our strings are not empty!
                        if (String.IsNullOrEmpty(parts[1]) || String.IsNullOrEmpty(parts[2]))
                        {
                            Console.WriteLine(" - Account name or PID not provided. Please try again with the correct format.");
                            Console.WriteLine("");
                            continue;
                        }

                        // Make sure the account exists!
                        user = Database.GetUser(parts[1]);
                        if (user == null)
                        {
                            Console.WriteLine(" - Account '{0}' does not exist in the gamespy database.", parts[1]);
                            continue;
                        }

                        // Try to make a PID out of parts 2
                        int newpid;
                        if (!Int32.TryParse(parts[2], out newpid))
                        {
                            Console.WriteLine(" - Player ID must be an numeric only!");
                            Console.WriteLine("");
                            continue;
                        }

                        // try and set the PID
                        int result = Database.SetPID(parts[1], newpid);
                        string message = "";
                        switch (result)
                        {
                            case 1:
                                message = "New PID is set!";
                                break;
                            case 0:
                                message = "Error setting PID";
                                break;
                            case -1:
                                message = String.Format("Account '{0}' does not exist in the gamespy database.", parts[1]);
                                break;
                            case -2:
                                message = String.Format("PID {0} is already in use.", newpid);
                                break;
                        }
                        Console.WriteLine(" - " + message);
                        Console.WriteLine("");
                        break;
                    case "help":
                        Console.Write(Environment.NewLine +
                            "stop/quit/exit          - Stops the server" + Environment.NewLine +
                            "connections             - Displays the current number of connected clients" + Environment.NewLine +
                            "accounts                - Displays the current number accounts in the DB." + Environment.NewLine +
                            "create {nick} {password} {email}  - Create a new Gamespy account." + Environment.NewLine +
                            "delete {nick}           - Deletes a user account. BF2 PID will not be removed." + Environment.NewLine +
                            "fetch {nick}            - Displays the account information" + Environment.NewLine +
                            "setpid {nick} {newpid}  - Sets the BF2 Player ID of the givin account name" + Environment.NewLine
                            + Environment.NewLine
                        );
                        break;
                    default:
                        lock (Console.Out)
                        {
                            Console.WriteLine("Unrecognized input '{0}'", Line);
                        }
                        break;
                }

                Thread.Sleep(100);
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