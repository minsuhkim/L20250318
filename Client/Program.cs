
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
    class Program
    {
        static void Main(string[] args)
        {
            string jsonString = "{\"message\" : \"안녕하세요\"}";
            byte[] message = Encoding.UTF8.GetBytes(jsonString);
            ushort length = (ushort)message.Length;

            byte[] lengthBuffer = new byte[2];
            lengthBuffer = BitConverter.GetBytes(length);

            byte[] buffer = new byte[2 + length];

            Buffer.BlockCopy(lengthBuffer, 0, buffer, 0, 2);
            Buffer.BlockCopy(message, 0, buffer, 2, length);

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Loopback, 4000);

            clientSocket.Connect(listenEndPoint);

            int SendLength = clientSocket.Send(buffer, buffer.Length, SocketFlags.None);

            int RecvLength = clientSocket.Receive(lengthBuffer, 2, SocketFlags.None);
            length = BitConverter.ToUInt16(lengthBuffer, 0);

            byte[] recvBuffer = new byte[4096];
            RecvLength = clientSocket.Receive(recvBuffer, length, SocketFlags.None);

            string JsonString = Encoding.UTF8.GetString(recvBuffer);
            Console.WriteLine(JsonString);

            clientSocket.Close();
        }
    }
}
