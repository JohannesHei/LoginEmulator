using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Common;
using System.Data.SQLite;
using MySql.Data;
using MySql.Data.Common;
using MySql.Data.MySqlClient;

namespace Gamespy.Database
{
    public class DatabaseDriver
    {
        public DatabaseEngine DatabaseEngine { get; protected set; }

        protected DbConnection Connection = null;
        protected DbCommand Command = null;
        private bool IsNewDatabase = false; //Only applies to SQLite databases, used to determine whether or not
                                            //the specified file already existed prior to attempting the connection.

        public DatabaseDriver()
        {
            this.DatabaseEngine = Config.GetDatabaseEngine();

            DbConnectionStringBuilder Builder;

            if( this.DatabaseEngine == Database.DatabaseEngine.Sqlite )
            {
                Builder = new SQLiteConnectionStringBuilder();
                string FullPath = Path.Combine( Utils.AssemblyPath, Config.GetType<string>( "Database", "Database" ) + ".sqlite3" );
                IsNewDatabase = !File.Exists( FullPath ) || new FileInfo( FullPath ).Length == 0;

                Builder.Add( "Data Source", FullPath );

                Connection = new SQLiteConnection( Builder.ConnectionString );
            }
            else if( this.DatabaseEngine == Database.DatabaseEngine.Mysql )
            {
                Builder = new MySqlConnectionStringBuilder();

                Builder.Add( "Server", Config.GetType<string>( "Database", "Hostname" ) );
                Builder.Add( "Port", Config.GetType<int>( "Database", "Port" ) );
                Builder.Add( "User ID", Config.GetType<string>( "Database", "Username" ) );
                Builder.Add( "Password", Config.GetType<string>( "Database", "Password" ) );
                Builder.Add( "Database", Config.GetType<string>( "Database", "Database" ) );

                Connection = new MySqlConnection( Builder.ConnectionString );
            }
        }

        public void Connect()
        {
            Connection.Open();

            if( IsNewDatabase )
            {
                string SQL = Utils.GetResourceString( "Gamespy.SQL.SQLite.CreateTables.sql" );

                this.CreateCommand( SQL );
                Command.ExecuteNonQuery();
                Command.Dispose();
            }
        }

        public void Close()
        {
            Connection.Close();
            Connection.Dispose();
            Command.Dispose();
        }

        public List<Dictionary<string, object>> Query( string Sql )
        {
            this.CreateCommand( Sql );
            DbDataReader Reader = Command.ExecuteReader();

            if( !Reader.HasRows )
                return null;

            List<Dictionary<string, object>> Rows = new List<Dictionary<string, object>>();

            while( Reader.Read() )
            {
                Dictionary<string, object> Row = new Dictionary<string, object>( Reader.FieldCount );

                for( int i = 0; i < Reader.FieldCount; ++i )
                    Row.Add( Reader.GetName( i ), Reader.GetValue( i ) );

                Rows.Add( Row );
            }

            Reader.Dispose();
            Command.Dispose();

            return Rows;
        }

        public List<Dictionary<string, object>> Query( string SqlFormat, params object[] Items )
        {
            string Formatted = string.Format( SqlFormat, Items );
            return this.Query( Formatted );
        }

        public int Execute( string Sql )
        {
            this.CreateCommand( Sql );
            int Result = Command.ExecuteNonQuery();
            Command.Dispose();

            return Result;
        }

        public int Execute( string SqlFormat, params object[] Items )
        {
            string Formatted = string.Format( SqlFormat, Items );
            return this.Execute( Formatted );
        }

        protected void CreateCommand( string QueryString )
        {
            if( DatabaseEngine == Database.DatabaseEngine.Sqlite )
                Command = new SQLiteCommand( QueryString, Connection as SQLiteConnection );
            else if( DatabaseEngine == Database.DatabaseEngine.Mysql )
                Command = new MySqlCommand( QueryString, Connection as MySqlConnection );
        }
    }
}