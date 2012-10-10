using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Gamespy
{
    public static class Utils
    {
        public static readonly string AssemblyPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

        public static string GetResourceString( string ResourceName )
        {
            string Result = "";

            using( Stream ResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream( ResourceName ) )
            using( StreamReader Reader = new StreamReader( ResourceStream ) )
            {
                Result = Reader.ReadToEnd();
            }

            return Result;
        }
    }
}