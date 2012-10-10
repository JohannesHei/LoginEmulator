using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamespy.Database;

namespace Gamespy
{
    class GamespyDatabase
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
            var Rows = Driver.Query("SELECT id, name, password, email, country, session FROM users WHERE name='{0}'", Nick);
            Driver.Close();
            return (Rows == null) ? null : Rows[0];
        }

        public Dictionary<string, object> GetUser(string Email, string Password)
        {
            Driver = new DatabaseDriver();
            Driver.Connect();
            var Rows = Driver.Query("SELECT id, name, password, country, session FROM users WHERE email='{0}' AND password='{1}'", Email, Password);
            Driver.Close();
            return (Rows == null) ? null : Rows[0];
        }

        public bool UserExists(string Nick)
        {
            // Fetch the user
            Driver = new DatabaseDriver();
            Driver.Connect();
            var Rows = Driver.Query("SELECT id FROM users WHERE name='{0}'", Nick);
            Driver.Close();

            return (Rows == null) ? false : true;
        }
    }
}
