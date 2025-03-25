
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{

    class Program
    {
        static void SendPacket(Socket toSocket, string message)
        {
            byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
            ushort length = (ushort)IPAddress.HostToNetworkOrder((short)messageBuffer.Length);

            byte[] headerBuffer = BitConverter.GetBytes(length);

            byte[] packetBuffer = new byte[headerBuffer.Length + messageBuffer.Length];
            Buffer.BlockCopy(headerBuffer, 0, packetBuffer, 0, headerBuffer.Length);
            Buffer.BlockCopy(messageBuffer, 0, packetBuffer, headerBuffer.Length, messageBuffer.Length);

            int SendLength = toSocket.Send(packetBuffer, packetBuffer.Length, SocketFlags.None);
        }

        static void RecvPacket(Socket fromSocket, out string jsonString)
        {
            byte[] lengthBuffer = new byte[2];

            int RecvLength = fromSocket.Receive(lengthBuffer, 2, SocketFlags.None);
            ushort length = BitConverter.ToUInt16(lengthBuffer, 0);
            // 호스트 바이트 순서가 cpu, os에 따라 달라서 네트워크 바이트 순서로 바꿈
            length = (ushort)IPAddress.NetworkToHostOrder((short)length);

            byte[] recvBuffer = new byte[4096];
            RecvLength = fromSocket.Receive(recvBuffer, length, SocketFlags.None);

            jsonString = Encoding.UTF8.GetString(recvBuffer);
        }

        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.22"), 4000);
            IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.54"), 4000);

            clientSocket.Connect(listenEndPoint);

            //JObject result = new JObject();
            //result.Add("code", "LogIn");
            //result.Add("id", "minsuh");
            //result.Add("password", "1234");
            //SendPacket(clientSocket, result.ToString());

            JObject result = new JObject();
            result.Add("code", "SignUp");
            result.Add("id", "cc");
            result.Add("password", "1234");
            result.Add("name", "cc");
            result.Add("email", "cc@naver.com");
            SendPacket(clientSocket, result.ToString());

            string jsonString;
            RecvPacket(clientSocket, out jsonString);

            Console.WriteLine(jsonString);

            clientSocket.Close();
        }
    }
}
