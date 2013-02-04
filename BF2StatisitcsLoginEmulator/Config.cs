using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using System.Net;
using BF2sLoginEmu.Database;

namespace BF2sLoginEmu
{
    public static class Config
    {
        //Tapping into the Win32 API to deal with INI files.
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder Value, int Length, string FilePath);

        private static readonly string IniLocation = Path.Combine(Utils.AssemblyPath, "Config.ini");

        static Config()
        {
            // Check if it exists, if not, create the default one.
            if (!File.Exists(IniLocation))
            {
                Console.WriteLine("File doesnt exist!" + Utils.AssemblyPath);
                string IniString = Utils.GetResourceString("BF2sLoginEmu.Config.ini");
                File.WriteAllText(IniLocation, IniString, Encoding.UTF8);
            }
        }

        public static T GetType<T>(string Section, string Key) where T : IConvertible
        {
            StringBuilder Value = new StringBuilder(1024);
            Config.GetPrivateProfileString(Section, Key, "", Value, 1024, IniLocation);

            return (T)Convert.ChangeType(Value.ToString(), typeof(T), CultureInfo.InvariantCulture);
        }

        public static DatabaseEngine GetDatabaseEngine()
        {
            string Name = Config.GetType<string>("Database", "Engine");
            Type EnumType = typeof(Database.DatabaseEngine);
            return ((Database.DatabaseEngine)Enum.Parse(EnumType, Name, true));
        }

        public static void SetValue(string Section, string Key, object Value)
        {
            Config.WritePrivateProfileString(Section, Key, Value.ToString(), IniLocation);
        }
    }
}
