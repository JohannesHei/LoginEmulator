using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BF2sLoginEmu.Database;

namespace BF2sLoginEmu
{
    public class Server
    {
        protected static bool isRunning = true;

        private GpcmServer CmServer;

        private GpspServer SpServer;

        public static GamespyDatabase Database;

        private static StreamWriter LogFile = File.AppendText( Path.Combine(Utils.AssemblyPath, "server.log") );

        public Server()
        {
            Console.WriteLine("Initializing..." + Environment.NewLine);

            // Start the DB Connection
            try
            {
                Database = new GamespyDatabase();
            }
            catch
            {
                // The exception message will already be on the console
                return;
            }

            // Bind gpcm server on port 29900
            try
            {
                Console.WriteLine("<GPCM> Binding to port 29900");
                CmServer = new GpcmServer();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error binding to port 29900! " + ex.Message);
                Console.WriteLine("Press any key to close");
                Console.ReadKey();
                return;
            }

            // Bind gpsp server on port 29901
            try
            {
                Console.WriteLine("<GPSP> Binding to port 29901");
                SpServer = new GpspServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error binding to port 29901! " + ex.Message);
                Console.WriteLine("Press any key to close");
                Console.ReadKey();
                return;
            }

            // Let the client know we are ready for connections
            Console.Write(
                Environment.NewLine + 
                "Ready for connections! Type 'help' for a list of commands" + 
                Environment.NewLine + Environment.NewLine
            );
            Console.Beep();

            // Handle input
            while (isRunning) 
            {
                CheckInput();
            }

            CmServer.Shutdown();
            SpServer.Shutdown();

            Console.WriteLine("Server shutdown Successfully");
        }

        public void CheckInput()
        {
            // Get user input
            Console.Write("cmd > ");
            string Line = Console.ReadLine();

            // Make sure input is not empty
            if (string.IsNullOrWhiteSpace(Line))
                return;

            // Define some base vars
            Dictionary<string, object> user = new Dictionary<string, object>();

            string command = Line.Trim();
            string[] parts = command.Split(' ');
            try
            {
                switch (parts[0])
                {
                    case "stop":
                    case "quit":
                    case "exit":
                        isRunning = false;
                        break;
                    case "connections":
                        Console.WriteLine(" - Total Connections: {0}" + Environment.NewLine, CmServer.NumClients());
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
                            return;
                        }

                        // Make sure we have a nick
                        if (String.IsNullOrEmpty(parts[1]))
                        {
                            Console.WriteLine(" - No account named provided. Please make sure you are providing an account name, and not a space");
                            Console.WriteLine("");
                            return;
                        }

                        // Fetch user account info
                        user = Database.GetUser(parts[1]);
                        if (user == null)
                        {
                            Console.WriteLine(
                                " - Account '{0}' does not exist in the gamespy database." + Environment.NewLine,
                                parts[1]);
                            return;
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
                            return;
                        }

                        // Make sure our strings are not empty!
                        if (String.IsNullOrEmpty(parts[1]) || String.IsNullOrEmpty(parts[2]) || String.IsNullOrEmpty(parts[3]))
                        {
                            Console.WriteLine(" - Account name, password, or email was not provided. Please try again with the correct format.");
                            Console.WriteLine("");
                            return;
                        }

                        // Make sure the account exists!
                        if (Database.UserExists(parts[1]))
                        {
                            Console.WriteLine(" - Account '{0}' already exists in the gamespy database.", parts[1]);
                            return;
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
                            return;
                        }

                        // Make sure our strings are not empty!
                        if (String.IsNullOrEmpty(parts[1]))
                        {
                            Console.WriteLine(" - Account name was not provided. Please try again with the correct format.");
                            Console.WriteLine("");
                            return;
                        }

                        // Make sure the account exists!
                        if (!Database.UserExists(parts[1]))
                        {
                            Console.WriteLine(" - Account '{0}' doesnt exist in the gamespy database.", parts[1]);
                            Console.WriteLine("");
                            return;
                        }

                        // Do a confimration
                        Console.Write(" - Are you sure you want to delete account '{0}'? <y/n>: ", parts[1]);
                        string v = Console.ReadLine().ToLower();

                        // If no, stop here
                        if (v == "n" || v == "no")
                        {
                            Console.WriteLine(" - Command cancelled." + Environment.NewLine);
                            return;
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
                            return;
                        }

                        // Make sure our strings are not empty!
                        if (String.IsNullOrEmpty(parts[1]) || String.IsNullOrEmpty(parts[2]))
                        {
                            Console.WriteLine(" - Account name or PID not provided. Please try again with the correct format.");
                            Console.WriteLine("");
                            return;
                        }

                        // Make sure the account exists!
                        user = Database.GetUser(parts[1]);
                        if (user == null)
                        {
                            Console.WriteLine(" - Account '{0}' does not exist in the gamespy database.", parts[1]);
                            return;
                        }

                        // Try to make a PID out of parts 2
                        int newpid;
                        if (!Int32.TryParse(parts[2], out newpid))
                        {
                            Console.WriteLine(" - Player ID must be an numeric only!");
                            Console.WriteLine("");
                            return;
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
                            "delete {nick}           - Deletes a user account." + Environment.NewLine +
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
            }
            catch {}

            Thread.Sleep(100);
        }

        public static void Shutdown()
        {
            isRunning = false;
        }

        /// <summary>
        /// This method is used to store a message in the console.log file
        /// </summary>
        /// <param name="message">The message to be written to the log file</param>
        public static void Log(string message)
        {
            DateTime datet = DateTime.Now;
            String logFile = Path.Combine(Utils.AssemblyPath, "server.log");
            
            try
            {
                LogFile.WriteLine(datet.ToString("MM/dd hh:mm") + "> " + message);
                LogFile.Flush();
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
