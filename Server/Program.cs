
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

        static void Main(string[] args)
        {
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);

            listenSocket.Bind(listenEndPoint);
            listenSocket.Listen(10);

            Socket clientSocket = listenSocket.Accept();

            // 패킷 길이 받기(header)
            byte[] headerBuffer = new byte[2];
            int RecvLength = clientSocket.Receive(headerBuffer, 2, SocketFlags.None);
            ushort packetLength = BitConverter.ToUInt16(headerBuffer, 0);
            packetLength = (ushort)IPAddress.NetworkToHostOrder((short)packetLength);

            // 실제 패킷(header 길이 만큼)
            byte[] dataBuffer = new byte[4096];
            RecvLength = clientSocket.Receive(dataBuffer, packetLength, SocketFlags.None);

            string JsonString = Encoding.UTF8.GetString(dataBuffer);
            
            Console.WriteLine(JsonString);

            string message = "{\"message\" : \"서버가 잘 받음...\"}";
            byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
            ushort length = (ushort)IPAddress.HostToNetworkOrder((short)messageBuffer.Length);

            headerBuffer = BitConverter.GetBytes(length);

            byte[] packetBuffer = new byte[headerBuffer.Length + messageBuffer.Length];

            Buffer.BlockCopy(headerBuffer, 0, packetBuffer, 0, headerBuffer.Length);
            Buffer.BlockCopy(messageBuffer, 0,packetBuffer, headerBuffer.Length, messageBuffer.Length);

            int sendLength = clientSocket.Send(packetBuffer, packetBuffer.Length, SocketFlags.None);

            clientSocket.Close();

            listenSocket.Close();
        }
    }
}
