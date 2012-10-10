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
        }

        public Dictionary<string, object> GetUser(string Nick)
        {
            // Fetch the user
            var Rows = Driver.Query("SELECT id, name, password, email, country, session FROM users WHERE name='{0}'", Nick);
            return (Rows == null) ? null : Rows[0];
        }

        public Dictionary<string, object> GetUser(string Email, string Password)
        {
            var Rows = Driver.Query("SELECT id, name, password, country, session FROM users WHERE email='{0}' AND password='{1}'", Email, Password);
            return (Rows == null) ? null : Rows[0];
        }


    }
}
