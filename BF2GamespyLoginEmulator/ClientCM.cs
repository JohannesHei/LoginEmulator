﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Gamespy
{
    /// <summary>
    /// Gamespy Client Manager
    /// This class is used to proccess the client login process,
    /// create new user accounts, and fetch profile information
    /// <remarks>gpcm.gamespy.com</remarks>
    /// </summary>
    class ClientCM
    {
        #region Variables

        private ClientStream Stream;
        private TcpClient client;
        private Random rand;
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private char[] backslash = { '\\' };
        private string[] BtoH = { 
                "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0a", "0b", "0c", "0d", "0e", "0f",
                "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1a", "1b", "1c", "1d", "1e", "1f",
                "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2a", "2b", "2c", "2d", "2e", "2f",
                "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3a", "3b", "3c", "3d", "3e", "3f",
                "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4a", "4b", "4c", "4d", "4e", "4f",
                "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5a", "5b", "5c", "5d", "5e", "5f",
                "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6a", "6b", "6c", "6d", "6e", "6f",
                "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7a", "7b", "7c", "7d", "7e", "7f",
                "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8a", "8b", "8c", "8d", "8e", "8f",
                "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9a", "9b", "9c", "9d", "9e", "9f",
                "a0", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9", "aa", "ab", "ac", "ad", "ae", "af",
                "b0", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b9", "ba", "bb", "bc", "bd", "be", "bf",
                "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9", "ca", "cb", "cc", "cd", "ce", "cf",
                "d0", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "d9", "da", "db", "dc", "dd", "de", "df",
                "e0", "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "e9", "ea", "eb", "ec", "ed", "ee", "ef",
                "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "f9", "fa", "fb", "fc", "fd", "fe", "ff" 
            };
        private char[] alphanumeric = { 
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
                'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
                'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };
        private ushort[] CrcTable = {
                0x0000,0xC0C1,0xC181,0x0140,0xC301,0x03C0,0x0280,0xC241,
                0xC601,0x06C0,0x0780,0xC741,0x0500,0xC5C1,0xC481,0x0440,
                0xCC01,0x0CC0,0x0D80,0xCD41,0x0F00,0xCFC1,0xCE81,0x0E40,
                0x0A00,0xCAC1,0xCB81,0x0B40,0xC901,0x09C0,0x0880,0xC841,
                0xD801,0x18C0,0x1980,0xD941,0x1B00,0xDBC1,0xDA81,0x1A40,
                0x1E00,0xDEC1,0xDF81,0x1F40,0xDD01,0x1DC0,0x1C80,0xDC41,
                0x1400,0xD4C1,0xD581,0x1540,0xD701,0x17C0,0x1680,0xD641,
                0xD201,0x12C0,0x1380,0xD341,0x1100,0xD1C1,0xD081,0x1040,
                0xF001,0x30C0,0x3180,0xF141,0x3300,0xF3C1,0xF281,0x3240,
                0x3600,0xF6C1,0xF781,0x3740,0xF501,0x35C0,0x3480,0xF441,
                0x3C00,0xFCC1,0xFD81,0x3D40,0xFF01,0x3FC0,0x3E80,0xFE41,
                0xFA01,0x3AC0,0x3B80,0xFB41,0x3900,0xF9C1,0xF881,0x3840,
                0x2800,0xE8C1,0xE981,0x2940,0xEB01,0x2BC0,0x2A80,0xEA41,
                0xEE01,0x2EC0,0x2F80,0xEF41,0x2D00,0xEDC1,0xEC81,0x2C40,
                0xE401,0x24C0,0x2580,0xE541,0x2700,0xE7C1,0xE681,0x2640,
                0x2200,0xE2C1,0xE381,0x2340,0xE101,0x21C0,0x2080,0xE041,
                0xA001,0x60C0,0x6180,0xA141,0x6300,0xA3C1,0xA281,0x6240,
                0x6600,0xA6C1,0xA781,0x6740,0xA501,0x65C0,0x6480,0xA441,
                0x6C00,0xACC1,0xAD81,0x6D40,0xAF01,0x6FC0,0x6E80,0xAE41,
                0xAA01,0x6AC0,0x6B80,0xAB41,0x6900,0xA9C1,0xA881,0x6840,
                0x7800,0xB8C1,0xB981,0x7940,0xBB01,0x7BC0,0x7A80,0xBA41,
                0xBE01,0x7EC0,0x7F80,0xBF41,0x7D00,0xBDC1,0xBC81,0x7C40,
                0xB401,0x74C0,0x7580,0xB541,0x7700,0xB7C1,0xB681,0x7640,
                0x7200,0xB2C1,0xB381,0x7340,0xB101,0x71C0,0x7080,0xB041,
                0x5000,0x90C1,0x9181,0x5140,0x9301,0x53C0,0x5280,0x9241,
                0x9601,0x56C0,0x5780,0x9741,0x5500,0x95C1,0x9481,0x5440,
                0x9C01,0x5CC0,0x5D80,0x9D41,0x5F00,0x9FC1,0x9E81,0x5E40,
                0x5A00,0x9AC1,0x9B81,0x5B40,0x9901,0x59C0,0x5880,0x9841,
                0x8801,0x48C0,0x4980,0x8941,0x4B00,0x8BC1,0x8A81,0x4A40,
                0x4E00,0x8EC1,0x8F81,0x4F40,0x8D01,0x4DC0,0x4C80,0x8C41,
                0x4400,0x84C1,0x8581,0x4540,0x8701,0x47C0,0x4680,0x8641,
                0x8201,0x42C0,0x4380,0x8341,0x4100,0x81C1,0x8081,0x4040
            };

        private string clientNick;
        private string clientChallengeKey;
        private string serverChallengeKey;
        private string clientResponse;

        #endregion Variables


        public ClientCM(TcpClient client)
        {
            this.client = client;

            Console.WriteLine(" - <GPCM> Client Connected: {0}", client.Client.RemoteEndPoint);

             // Init a new client stream class
            Stream = new ClientStream(client);

            // Start off by sending the client a challenge
            SendServerChallenge();
        }

        #region Steps

        /// <summary>
        ///  This method starts off by sending a random string 10 characters
        ///  in length, known as the Server challenge key. This is used by 
        ///  the client to return a client challenge key, which is used
        ///  to validate login information later.
        ///  </summary>
        public void SendServerChallenge()
        {
            // First we need to create a random string the length of 10 characters
            char[] buffer = new char[10];
            rand = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < 10; i++)
                buffer[i] = chars[rand.Next(chars.Length)];

            // Next we send the client the challenge key
            serverChallengeKey = buffer.ToString();
            Stream.Write( String.Format("\\lc\\1\\challenge\\{0}\\id\\1\\final\\", serverChallengeKey) );

            // Process the login information returned by the client
            RecvLogin();
        }

        /// <summary>
        /// This method takes the returned server challenge info
        /// and uses it to determine what we have to return next
        /// </summary>
        private void RecvLogin()
        {
            // Read the stream to get the client login data
            string message = Stream.Read();

            // Create an array by spliting the returned data
            string[] recv = message.Split(backslash);

            // If the first string is 'newuser', then we are creating
            // a new account, otherwise its a login proccess
            if (recv[0] == "newuser")
            {
                HandleNewUser(recv);
            }
            else
            {
                clientNick = getParameterValue(recv, "uniquenick");
                clientChallengeKey = getParameterValue(recv, "challenge");
                clientResponse = getParameterValue(recv, "response");
                SendProof();
            }
        }

        /// <summary>
        /// This method verifies the login information sent by
        /// the client, and returns encrypted data for the client
        /// to verify as well
        /// </summary>
        public void SendProof()
        {
            // Get password and user data from database
            // Use the GenerateRepsonseValue method to compare with the "response" value
            //  ^ this validates the given password
            // Use the GenerateResponseValue method to create the proof string
            // Generate a session ID
            string value = GenerateResponseValue(clientNick, "test", clientChallengeKey, serverChallengeKey);
            if (clientResponse == value)
            {
                // Password is correct
                string proof = GenerateResponseValue(clientNick, "test", serverChallengeKey, clientChallengeKey);
                string data = String.Format(
                    "\\lc\\2\\sesskey\\{0}\\proof\\{1}\\userid\\{2}\\profileid\\{3}\\uniquenick\\{4}\\lt\\{5}__\\id\\1\\final\\",
                    GenerateSession(), proof, "101249154", "101249154", clientNick, GenerateRandomString(22)
                );
                Stream.Write(data);

                // TODO: process the 'getprofile' (returned at this point) data
                Console.WriteLine(Stream.Read());
            }
            else
            {
                // Password is incorrect with database value
                Stream.Write("\\error\\\\err\\260\\fatal\\\\errmsg\\The password provided is incorrect.\\id\\1\\final\\");
            }
        }

        #endregion Steps

        #region User Methods

        private void HandleNewUser(string[] recv)
        {
            // ...
        }

        #endregion



        #region Misc Methods

        /// <summary>
        /// A simple method of getting the value of the passed parameter key,
        /// from the returned array of data from the client
        /// </summary>
        /// <param name="parts">The array of data from the client</param>
        /// <param name="parameter">The parameter</param>
        /// <returns>The value of the paramenter key</returns>
        private string getParameterValue(string[] parts, string parameter)
        {
            bool next = false;
            foreach (string part in parts)
            {
                if (next) 
                    return part;
                else if (part == parameter) 
                    next = true;
            }
            return "";
        } 

        /// <summary>
        /// Generates an encrypted reponse to return to the client, which verifies
        /// the clients account information, and login info
        /// </summary>
        /// <param name="nickname">The account username / InGame player nick</param>
        /// <param name="password">The account password</param>
        /// <param name="challenge1">First challenge key</param>
        /// <param name="challenge2">Second challenge key</param>
        /// <returns>Encrypted account info / Login verification</returns>
        private string GenerateResponseValue(string nickname, string password, string challenge1, string challenge2)
        {
            // Generate a MD5 password hash, and convert it to Gamespy's specific hasing method
            MD5 createMD5 = MD5.Create();
            byte[] passwordHash = createMD5.ComputeHash(Encoding.ASCII.GetBytes(password));
            string pwmd5 = "";
            foreach (byte b in passwordHash)
                pwmd5 += BtoH[b];

            // Generate our string to be hashed
            string hash = pwmd5;
            for (int i = 0; i < 48; i++)
                hash += " ";
            hash += nickname + challenge1 + challenge2 + pwmd5;

            // Create our final hash, and then convert that hash to a string
            byte[] finalHash = createMD5.ComputeHash(Encoding.ASCII.GetBytes(hash));
            StringBuilder result = new StringBuilder();
            foreach (byte b in finalHash)
            {
                result.AppendFormat("{0}", BtoH[b]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Generates a session ID for the account name. Just
        /// 1 more verification data string for the client
        /// </summary>
        /// <returns></returns>
        public ushort GenerateSession()
        {
            int Length = clientNick.Length;
            int Session = 0;

            for (int i = 0; i < Length; ++i)
                Session = CrcTable[(clientNick[i] ^ Session) & 0xFF] ^ (Session >> 8);

            return (ushort)Session;
        }

        /// <summary>
        /// Generates a random alpha-numeric string
        /// </summary>
        /// <param name="length">The lenght of the string to be generated</param>
        /// <returns></returns>
        private string GenerateRandomString(int length)
        {
            string s = "";

            while (length > 0)
            {
                --length;
                s += alphanumeric[rand.Next(62)];
            }
            return s;
        }

        #endregion
    }
}
