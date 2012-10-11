using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

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

        public static string EncodeGamespyPassword( string Password )
        {
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.UseShellExecute = false;
            Info.CreateNoWindow = true;
            Info.RedirectStandardOutput = true;
            Info.Arguments = string.Format( "e \"{0}\"", Password );
            Info.FileName = Path.Combine( Utils.AssemblyPath, "gspassenc.exe" );

            Process gsProcess = Process.Start( Info );
            string EncodedPassword = gsProcess.StandardOutput.ReadToEnd();

            return EncodedPassword;
        }

        public static string DecodeGamespyPassword( string Password )
        {
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.UseShellExecute = false;
            Info.CreateNoWindow = true;
            Info.RedirectStandardOutput = true;
            Info.Arguments = string.Format( "d \"{0}\"", Password );
            Info.FileName = Path.Combine( Utils.AssemblyPath, "gspassenc.exe" );

            Process gsProcess = Process.Start( Info );
            string DecodedPassword = gsProcess.StandardOutput.ReadToEnd();

            return DecodedPassword;
        }
    }
}