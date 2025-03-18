
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
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

            Message message = new Message("이미지 요청");
            string jsonData = JsonConvert.SerializeObject(message);

            buffer = Encoding.UTF8.GetBytes(jsonData);
            int sendLength = serverSocket.Send(buffer);


            byte[] buffer2 = new byte[1024];
            int receiveLength = serverSocket.Receive(buffer2);
            string jsonResponse = Encoding.UTF8.GetString(buffer2, 0, receiveLength);

            Message responseMessage = JsonConvert.DeserializeObject<Message>(jsonResponse);
            Console.WriteLine($"서버 응답: {responseMessage}");

            if(responseMessage.message == "이미지 전송 시작")
            {
                string saveImagePath = "./received_image.webp";
                using (FileStream fs = new FileStream(saveImagePath, FileMode.Create, FileAccess.Write))
                {
                    buffer = new byte[8192]; // 8KB 버퍼
                    int bytesRead;

                    while ((bytesRead = serverSocket.Receive(buffer)) > 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                        if (bytesRead < buffer.Length) // 더 이상 받을 데이터가 없으면 종료
                            break;
                    }
                }

                Console.WriteLine($"이미지 저장 완료: {saveImagePath}");
            }

            serverSocket.Close();
        }
    }
}
