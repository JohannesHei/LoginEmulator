using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Gamespy
{
    class ClientCM
    {
        private ClientStream Stream;
        private TcpClient client;
        private int Step = 0;
        private Random rand;
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";


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

        public void SendServerChallenge()
        {
            // First we need to create a random string the length of 10 characters
            char[] buffer = new char[10];
            rand = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < 10; i++)
            {
                buffer[i] = chars[rand.Next(chars.Length)];
            }

            // Next we send the client the challenge key
            string key = buffer.ToString();
            Stream.Write( String.Format("\\lc\\1\\challenge\\{0}\\id\\1\\final\\", key) );

            // Process the login information returned by the client
            RecvLogin();
        }

        public void SendProof(string[] info)
        {

        }

        private void RecvLogin()
        {
            string message = Stream.Read();
            System.Diagnostics.Debug.Write("Challenge response: {0}", message);

            // Create an array by spliting the returned data
            string[] recv = message.Split('\\');
            string[] info;

            switch (recv.Length)
            {
                case 18:
                    if (recv[0] == "newuser")
                    {
                        HandleNewUser(recv);
                        break;
                    }

                    info = new string[3] { 
                        recv[4], // Nick
                        recv[2], // Challenge
                        recv[6]  // Response
                    };
                    SendProof(info);
                    break;

                case 24: // BF 1.5
                    // 1.5 == true
                case 22:
                    info = new string[3] { 
                        recv[4], // Nick
                        recv[2], // Challenge
                        recv[10]  // Response
                    };
                    SendProof(info);
                    break;

                case 20: // BF 1.5
                    info = new string[3] { 
                        recv[4], // Nick
                        recv[2], // Challenge
                        recv[6]  // Response
                    };
                    SendProof(info);
                    break;
            }
        }

        #endregion Steps

        #region User Methods

        private void HandleNewUser(string[] recv)
        {

        }

        #endregion
    }
}
