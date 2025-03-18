
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
            string jsonString = "{\"message\" : \"이건 클라이언트에서 서버로 보내는 패킷.\"}";
            byte[] message = Encoding.UTF8.GetBytes(jsonString);
            // 정수형 숫자를 버퍼에 넣을 때는 항상 바이트 오더를 조정해줘야함!
            ushort length = (ushort)message.Length;
            //length = (ushort)IPAddress.HostToNetworkOrder((short)length);

            byte[] lengthBuffer = new byte[2];
            lengthBuffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)length));

            byte[] buffer = new byte[2 + length];

            Buffer.BlockCopy(lengthBuffer, 0, buffer, 0, 2);
            Buffer.BlockCopy(message, 0, buffer, 2, length);

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Loopback, 4000);

            clientSocket.Connect(listenEndPoint);

            int SendLength = clientSocket.Send(buffer, buffer.Length, SocketFlags.None);

            int RecvLength = clientSocket.Receive(lengthBuffer, 2, SocketFlags.None);
            length = BitConverter.ToUInt16(lengthBuffer, 0);
            // 호스트 바이트 순서가 cpu, os에 따라 달라서 네트워크 바이트 순서로 바꿈
            length = (ushort)IPAddress.NetworkToHostOrder((short)length);

            byte[] recvBuffer = new byte[4096];
            RecvLength = clientSocket.Receive(recvBuffer, length, SocketFlags.None);

            string JsonString = Encoding.UTF8.GetString(recvBuffer);
            Console.WriteLine(JsonString);

            clientSocket.Close();
        }
    }
}
