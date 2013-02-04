using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SQLite;
using MySql.Data;
using MySql.Data.Common;
using MySql.Data.MySqlClient;

namespace BF2sLoginEmu.Database
{
    public class DatabaseDriver
    {
        /// <summary>
        /// Current DB Engine
        /// </summary>
        public DatabaseEngine DatabaseEngine { get; protected set; }

        /// <summary>
        /// The database connection
        /// </summary>
        protected DbConnection Connection = null;

        /// <summary>
        /// Current command running against the database
        /// </summary>
        protected DbCommand Command = null;

        /// <summary>
        /// Only applies to SQLite databases, used to determine whether or not
        /// the specified file already existed prior to attempting the connection.
        /// </summary>
        private bool IsNewDatabase = false;

        public DatabaseDriver()
        {
            this.DatabaseEngine = Config.GetDatabaseEngine();

            DbConnectionStringBuilder Builder;

            if (this.DatabaseEngine == DatabaseEngine.Sqlite)
            {
                Builder = new SQLiteConnectionStringBuilder();
                string FullPath = Path.Combine(Utils.AssemblyPath, Config.GetType<string>("Database", "Database") + ".sqlite3");
                IsNewDatabase = !File.Exists(FullPath) || new FileInfo(FullPath).Length == 0;

                Builder.Add("Data Source", FullPath);

                Connection = new SQLiteConnection(Builder.ConnectionString);
            }
            else if (this.DatabaseEngine == DatabaseEngine.Mysql)
            {
                Builder = new MySqlConnectionStringBuilder();

                Builder.Add("Server", Config.GetType<string>("Database", "Hostname"));
                Builder.Add("Port", Config.GetType<int>("Database", "Port"));
                Builder.Add("User ID", Config.GetType<string>("Database", "Username"));
                Builder.Add("Password", Config.GetType<string>("Database", "Password"));
                Builder.Add("Database", Config.GetType<string>("Database", "Database"));

                Connection = new MySqlConnection(Builder.ConnectionString);
            }
            else
            {
                throw new Exception("Invalid Database type.");
            }
        }

        public void Connect()
        {
            Connection.Open();
            
            // Create the SqlDatabase file if it doesnt exist
            if (IsNewDatabase)
            {
                string SQL = Utils.GetResourceString("BF2sLoginEmu.SQL.SQLite.CreateTables.sql");

                this.CreateCommand(SQL);
                Command.ExecuteNonQuery();
                Command.Dispose();
            }
        }

        public void Close()
        {
            try
            {
                Connection.Close();
                Connection.Dispose();
            }
            catch { }
        }

        public List<Dictionary<string, object>> Query(string Sql)
        {
            this.CreateCommand(Sql);
            DbDataReader Reader = Command.ExecuteReader();

            if (!Reader.HasRows)
                return null;

            List<Dictionary<string, object>> Rows = new List<Dictionary<string, object>>();

            while (Reader.Read())
            {
                Dictionary<string, object> Row = new Dictionary<string, object>(Reader.FieldCount);

                for (int i = 0; i < Reader.FieldCount; ++i)
                    Row.Add(Reader.GetName(i), Reader.GetValue(i));

                Rows.Add(Row);
            }

            Reader.Close();
            Reader.Dispose();
            Command.Dispose();

            return Rows;
        }

        public List<Dictionary<string, object>> Query(string SqlFormat, params object[] Items)
        {
            string Formatted = string.Format(SqlFormat, Items);
            return this.Query(Formatted);
        }

        public int Execute(string Sql)
        {
            this.CreateCommand(Sql);
            int Result = Command.ExecuteNonQuery();
            Command.Dispose();

            return Result;
        }

        public int Execute(string SqlFormat, params object[] Items)
        {
            string Formatted = string.Format(SqlFormat, Items);
            return this.Execute(Formatted);
        }

        protected void CreateCommand(string QueryString)
        {
            if (DatabaseEngine == Database.DatabaseEngine.Sqlite)
                Command = new SQLiteCommand(QueryString, Connection as SQLiteConnection);
            else if (DatabaseEngine == Database.DatabaseEngine.Mysql)
                Command = new MySqlCommand(QueryString, Connection as MySqlConnection);
        }
    }
}
