using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BF2sLoginEmu
{
    class ClientStream
    {
        /// <summary>
        /// The current clients stream
        /// </summary>
        private TcpClient Client;

        /// <summary>
        /// Clients NetworkStream
        /// </summary>
        private NetworkStream Stream;

        /// <summary>
        /// Write all data sent/recieved to the stream log?
        /// </summary>
        private bool Debugging;

        /// <summary>
        /// Log file stream
        /// </summary>
        private static StreamWriter LogFile = File.AppendText(Path.Combine(Utils.AssemblyPath, "stream.log"));

        public ClientStream(TcpClient client)
        {
            this.Client = client;
            this.Stream = client.GetStream();
            this.Debugging = Config.GetType<bool>("Debugging", "DebugStream");
        }

        /// <summary>
        /// Returns a bool based off on wether there is data available to be read
        /// </summary>
        /// <returns></returns>
        public bool HasData()
        {
            return Stream.DataAvailable;
        }

        /// <summary>
        /// Reads from the client stream. Will rest until data is recieved
        /// </summary>
        /// <returns>The completed data from the client</returns>
        public string Read()
        {
            int bytesRead = 0;
            int bufferSize = Client.ReceiveBufferSize;
            byte[] buffer = new byte[bufferSize];
            string message = "";

            do
            {
                bytesRead += Stream.Read(buffer, 0, bufferSize);
                int Counter = 0;

                foreach (byte b in buffer)
                {
                    if (b == 0x00)
                        break;

                    ++Counter;
                }

                //Trim off the null bytes.
                Array.Resize(ref buffer, Counter);
                message += Encoding.UTF8.GetString(buffer);
                if (Debugging)
                    Log("Port {0} Recieves: {1}", ((IPEndPoint)Client.Client.LocalEndPoint).Port, message);

            } while (Stream.DataAvailable);

            if (bytesRead == 0)
                return "";

            return message.ToString();
        }

        /// <summary>
        /// Writes a message to the client stream
        /// </summary>
        /// <param name="message">The complete message to be sent to the client</param>
        public void Write(string message)
        {
            if (Debugging)
                Log("Port {0} Sends: {1}", ((IPEndPoint)Client.Client.LocalEndPoint).Port, message);

            this.Write(Encoding.ASCII.GetBytes(message));
        }

        /// <summary>
        /// Writes a message to the client stream
        /// </summary>
        /// <param name="message">The complete message to be sent to the client</param>
        public void Write(string message, params object[] items)
        {
            message = String.Format(message, items);
            if (Debugging)
                Log("Port {0} Sends: {1}", ((IPEndPoint)Client.Client.LocalEndPoint).Port, message);

            this.Write(Encoding.ASCII.GetBytes(message));
        }

        /// <summary>
        /// Writes a message to the client stream
        /// </summary>
        /// <param name="bytes">An array of bytes to send to the stream</param>
        public void Write(byte[] bytes)
        {
            Stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes a message to the stream log
        /// </summary>
        /// <param name="message"></param>
        private static void Log(string message)
        {
            DateTime datet = DateTime.Now;
            
            try
            {
                LogFile.WriteLine(datet.ToString("MM/dd hh:mm") + "> " + message);
                LogFile.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        /// <summary>
        /// Writes a message to the stream log
        /// </summary>
        /// <param name="message"></param>
        private static void Log(string message, params object[] items)
        {
            Log(String.Format(message, items));
        }
    }
}
