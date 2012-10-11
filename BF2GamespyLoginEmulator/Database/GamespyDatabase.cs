using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            return (int)Rows[0]["pid"];
        }

        public int GeneratePID(string Nick)
        {
            Driver = new DatabaseDriver();
            Driver.Connect();

            int pid = 1;
            var Max = Driver.Query("SELECT MAX(pid) AS max FROM bf2pids", Nick);
            if (Max == null)
                pid = 500000000;
            else
                Server.Log("Max PID: {0}", Max[0]["max"].ToString());
                //pid = ((int)Max[0]["max"]) + 1;

            int result = Driver.Execute("INSERT INTO bf2pids(pid, nick) VALUES('{0}', '{1}')", pid, Nick);
            Console.WriteLine("Result: {0}, PID: {1}", result, pid);

            Driver.Close();
            return result;
        }
    }
}
