
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        public class Message
        {
            public Message(string inMessage)
            {
                message = inMessage;
            }

            public string message;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Server");

            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);

            listenSocket.Bind(listenEndPoint);

            listenSocket.Listen(10);

            bool isRunning = true;

            while (isRunning)
            {
                Socket clientSocket = listenSocket.Accept();

                byte[] buffer = new byte[1024];
                int receiveLength = clientSocket.Receive(buffer);
                if (receiveLength <= 0)
                {
                    isRunning = false;
                }
                string receiveMessage = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"receive : {receiveMessage}");
                Message message = JsonConvert.DeserializeObject<Message>(receiveMessage);

                if (message.message.CompareTo("안녕하세요") == 0)
                {
                    message.message = "반가워요";
                }
                else
                {
                    message.message = "";
                }

                buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                int sendLength = clientSocket.Send(buffer);
                if (sendLength <= 0)
                {
                    isRunning = false;
                }

                clientSocket.Close();
            }

            listenSocket.Close();
        }
    }
}
