
using Newtonsoft.Json;
using System;
using System.IO;
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
            string imagePath = "./image.webp";

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
                    continue;
                }
                string receiveMessage = Encoding.UTF8.GetString(buffer, 0, receiveLength);

                Console.WriteLine($"받은 메시지 : {receiveMessage}");

                Message message = JsonConvert.DeserializeObject<Message>(receiveMessage);
                Message responseMessage;

                if(message.message == "이미지 요청")
                {
                    Console.WriteLine("클라이언트가 이미지 요청!");

                    // 먼저 JSON 메시지를 전송
                    responseMessage = new Message("이미지 전송 시작");
                    string jsonResponse = JsonConvert.SerializeObject(responseMessage);
                    buffer = Encoding.UTF8.GetBytes(jsonResponse);
                    clientSocket.Send(buffer);

                    // 그 후 이미지 바이너리 전송
                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                    clientSocket.Send(imageBytes);

                    Console.WriteLine("이미지 전송 완료");
                }
                else if (message.message.CompareTo("안녕하세요") == 0)
                {
                    responseMessage = new Message("반가워요");
                    string jsonResponse = JsonConvert.SerializeObject(responseMessage);
                    buffer = Encoding.UTF8.GetBytes(jsonResponse);
                    clientSocket.Send(buffer);
                }
                else
                {
                    isRunning = false;
                    continue;
                }

                clientSocket.Close();
            }

            listenSocket.Close();
        }
    }
}
