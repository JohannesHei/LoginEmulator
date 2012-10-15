using System;
using System.Collections.Generic;
using Gamespy.Database;

namespace Gamespy
{
    public class GamespyDatabase
    {
        private DatabaseDriver Driver;

        public GamespyDatabase()
        {
            Driver = new DatabaseDriver();
            Driver.Connect();
            Driver.Close();
        }

        ~GamespyDatabase()
        {
            Driver.Close();
        }

        public Dictionary<string, object> GetUser(string Nick)
        {
            // Fetch the user
            Driver = new DatabaseDriver();
            Driver.Connect();
            var Rows = Driver.Query("SELECT id, name, password, email, country, session FROM accounts WHERE name='{0}'", Nick);
            Driver.Close();
            return (Rows == null) ? null : Rows[0];
        }

        public Dictionary<string, object> GetUser(string Email, string Password)
        {
            Driver = new DatabaseDriver();
            Driver.Connect();
            var Rows = Driver.Query("SELECT id, name, password, country, session FROM accounts WHERE email='{0}' AND password='{1}'", Email, Password);
            Driver.Close();
            return (Rows == null) ? null : Rows[0];
        }

        public bool UserExists(string Nick)
        {
            // Fetch the user
            Driver = new DatabaseDriver();
            Driver.Connect();
            var Rows = Driver.Query("SELECT id FROM accounts WHERE name='{0}'", Nick);
            Driver.Close();

            return (Rows == null) ? false : true;
        }

        public bool CreateUser(string Nick, string Pass, string Email, string Country)
        {
            Driver = new DatabaseDriver();
            Driver.Connect();
            int Rows = Driver.Execute("INSERT INTO accounts(name, password, email, country) VALUES('{0}', '{1}', '{2}', '{3}')", 
                Nick, Pass, Email, Country
            );

            // Check for error
            if (Rows == 0) return false;
            Driver.Close();

            return (GeneratePID(Nick) != 0) ? true : false;
        }

        public void UpdateUser(string Nick, string CC)
        {
            Driver = new DatabaseDriver();
            Driver.Connect();
            Driver.Execute("UPDATE accounts SET country='{0}' WHERE name='{1}'", Nick, CC);
            Driver.Close();
        }

        public int GetPID(string Nick)
        {
            Driver = new DatabaseDriver();
            Driver.Connect();
            var Rows = Driver.Query("SELECT pid FROM bf2pids WHERE nick='{0}'", Nick);

            // If we have no result, we need to create a new Player :)
            if (Rows == null)
            {
                GeneratePID(Nick);
                Rows = Driver.Query("SELECT pid FROM bf2pids WHERE nick='{0}'", Nick);
                if (Rows == null) 
                { 
                    Driver.Close(); 
                    return 0; 
                }
            }

            Driver.Close();

            int pid;
            Int32.TryParse(Rows[0]["pid"].ToString(), out pid);
            return pid;
        }

        public int SetPID(string Nick, int Pid)
        {
            Driver = new DatabaseDriver();
            Driver.Connect();

            var Rows = Driver.Query("SELECT pid FROM bf2pids WHERE nick='{0}'", Nick);
            var PidExists = Driver.Query("SELECT nick FROM bf2pids WHERE pid='{0}'", Pid);

            // If no user exists, return code -1
            if (Rows == null)
                return -1;

            // If PID is false, the PID is not taken
            if (PidExists == null)
            {
                int Success = Driver.Execute("UPDATE bf2pids SET pid='{0}' WHERE nick='{1}'", Pid, Nick);
                Driver.Close();
                return (Success == 1) ? 1 : 0;
            }

            Driver.Close();
            return -2; // PID exists already
        }

        public int GeneratePID(string Nick)
        {
            Driver = new DatabaseDriver();
            Driver.Connect();

            int pid = 1;
            var Max = Driver.Query("SELECT MAX(pid) AS max FROM bf2pids", Nick);
            try
            {
                pid = ((int)Max[0]["max"]) + 1;
                if (pid < 500000000)
                    pid = 500000000;
            }
            catch
            {
                pid = 500000000;
            }

            int result = Driver.Execute("INSERT INTO bf2pids(pid, nick) VALUES('{0}', '{1}')", pid, Nick);

            Driver.Close();
            return(result != 0) ? pid : 0;
        }
    }
}
