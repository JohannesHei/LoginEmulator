﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using System.Net;

namespace Gamespy
{
    public static class Config
    {
        //Tapping into the Win32 API to deal with INI files.
        [DllImport( "kernel32" )]
        private static extern long WritePrivateProfileString( string Section, string Key, string Value, string FilePath );

        [DllImport( "kernel32" )]
        private static extern int GetPrivateProfileString( string Section, string Key, string Default, StringBuilder Value, int Length, string FilePath );

        private static readonly string IniLocation;

        static Config()
        {
            Assembly Asm = Assembly.GetExecutingAssembly();

            //Build the path to our config file.
            string StartupPath = Asm.Location;
            IniLocation = Path.Combine( Path.GetDirectoryName( StartupPath ), "Config.ini" );

            //Check if it exists, if not, create the default one.
            if( !File.Exists( IniLocation ) )
            {
                Stream ResStream = Asm.GetManifestResourceStream( "Gamespy.Config.ini" );
                FileStream ConfigStream = File.Open( IniLocation, FileMode.Create, FileAccess.Write, FileShare.None );

                ResStream.CopyTo( ConfigStream );
                ResStream.Close();
                ConfigStream.Close();
            }
        }

        public static T Get<T>( string Section, string Key ) where T : IConvertible
        {
            StringBuilder Value = new StringBuilder( 1024 );
            Config.GetPrivateProfileString( Section, Key, "", Value, 1024, IniLocation );

            return (T)Convert.ChangeType( Value.ToString(), typeof( T ), CultureInfo.InvariantCulture );
        }

        public static void SetValue( string Section, string Key, object Value )
        {
            Config.WritePrivateProfileString( Section, Key, Value.ToString(), IniLocation );
        }
    }
}