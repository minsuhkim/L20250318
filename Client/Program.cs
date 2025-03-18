
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Message
    {
        public Message(string inMessage)
        {
            message = inMessage;
        }

        public string message;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client");

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);

            serverSocket.Connect(clientEndPoint);

            byte[] buffer = new byte[1024];

            Message message = new Message("안녕하세요");
            string jsonData = JsonConvert.SerializeObject(message);

            buffer = Encoding.UTF8.GetBytes(jsonData);
            int sendLength = serverSocket.Send(buffer);

            byte[] buffer2 = new byte[1024];
            int receiveLength = serverSocket.Receive(buffer2);
            Console.WriteLine($"receive: {Encoding.UTF8.GetString(buffer2)}");

            serverSocket.Close();
        }
    }
}
