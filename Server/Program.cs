using MySqlConnector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static Socket listenSocket;

        // 공유 영역
        static List<Socket> clientSockets = new List<Socket>();
        //static List<Thread> threadManager = new List<Thread>();

        static object _lock = new object();


        static void AcceptThread()
        {
            while (true)
            {
                Socket clientSocket = listenSocket.Accept();

                lock (_lock)
                {
                    clientSockets.Add(clientSocket);
                }
                Console.WriteLine($"Connect client : {clientSocket.RemoteEndPoint}");

                Thread workThread = new Thread(new ParameterizedThreadStart(WorkThread));
                workThread.IsBackground = true;
                workThread.Start(clientSocket);
                //threadManager.Add(workThread);
            }
        }

        static void WorkThread(Object clientObjectSocket)
        {
            // 아무거나 받을 수 있게 파라미터를 Object로 해놓고 쓰레드 함수 내에서 Socket으로 바꿔줌
            Socket clientSocket = clientObjectSocket as Socket;

            while (true)
            {
                try
                {
                    byte[] headerBuffer = new byte[2];
                    int RecvLength = clientSocket.Receive(headerBuffer, 2, SocketFlags.None);
                    if (RecvLength > 0)
                    {
                        short packetlength = BitConverter.ToInt16(headerBuffer, 0);
                        packetlength = IPAddress.NetworkToHostOrder(packetlength);

                        byte[] dataBuffer = new byte[4096];
                        RecvLength = clientSocket.Receive(dataBuffer, packetlength, SocketFlags.None);
                        string JsonString = Encoding.UTF8.GetString(dataBuffer);
                        Console.WriteLine(JsonString);

                        string connectionString = "server=localhost;user=root;database=membership;password=sy103504";
                        MySqlConnection mySqlConnection = new MySqlConnection(connectionString);

                        JObject clientData = JObject.Parse(JsonString);

                        string code = clientData.Value<string>("code");
                        try
                        {
                            if (code.CompareTo("LogIn") == 0)
                            {
                                string userId = clientData.Value<string>("id");
                                string userPassword = clientData.Value<string>("password");

                                mySqlConnection.Open();
                                MySqlCommand mySqlCommand = new MySqlCommand();

                                mySqlCommand.Connection = mySqlConnection;
                                // 로그인
                                // user_id, user_password 에 $"{}"를 사용하면 보안에 좋지 않다!
                                mySqlCommand.CommandText = "select * from users where user_id = @user_id and user_password = @user_password";
                                mySqlCommand.Prepare();

                                mySqlCommand.Parameters.AddWithValue("@user_id", userId);
                                mySqlCommand.Parameters.AddWithValue("@user_password", userPassword);

                                MySqlDataReader dataReader = mySqlCommand.ExecuteReader();
                                if (dataReader.Read())
                                {
                                    // 로그인 성공
                                    JObject result = new JObject();
                                    result.Add("code", "loginresult");
                                    result.Add("message", "success");

                                    result.Add("name", dataReader["name"].ToString());
                                    result.Add("email", dataReader["email"].ToString());
                                    SendPacket(clientSocket, result.ToString());
                                }
                                else
                                {
                                    // 로그인 실패
                                    JObject result = new JObject();
                                    result.Add("code", "loginresult");
                                    result.Add("message", "failed");

                                    SendPacket(clientSocket, result.ToString());
                                }
                            }
                            else if (code.CompareTo("SignUp") == 0)
                            {
                                string userId = clientData.Value<string>("id");
                                string userPassword = clientData.Value<string>("password");
                                string name = clientData.Value<string>("name");
                                string email = clientData.Value<string>("email");

                                mySqlConnection.Open();
                                MySqlCommand mySqlCommand2 = new MySqlCommand();
                                mySqlCommand2.Connection = mySqlConnection;
                                mySqlCommand2.CommandText = "insert into users (user_id, user_password, name, email) values (@user_id, @user_password, @name, @email)";
                                mySqlCommand2.Prepare();
                                mySqlCommand2.Parameters.AddWithValue("@user_id",userId);
                                mySqlCommand2.Parameters.AddWithValue("@user_password", userPassword);
                                mySqlCommand2.Parameters.AddWithValue("@name", name);
                                mySqlCommand2.Parameters.AddWithValue("@email", email);
                                mySqlCommand2.ExecuteNonQuery();

                                // 가입 성공
                                JObject result = new JObject();
                                result.Add("code", "signupresult");
                                result.Add("message", "success");

                                SendPacket(clientSocket, result.ToString());
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        finally
                        {
                            mySqlConnection.Close();
                        }
                    }
                    else
                    {
                        //string message = "{ \"message\" : \" Disconnect : " + clientSocket.RemoteEndPoint + " \"}";
                        //byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
                        //ushort length = (ushort)IPAddress.HostToNetworkOrder((short)messageBuffer.Length);

                        //headerBuffer = BitConverter.GetBytes(length);

                        //byte[] packetBuffer = new byte[headerBuffer.Length + messageBuffer.Length];
                        //Buffer.BlockCopy(headerBuffer, 0, packetBuffer, 0, headerBuffer.Length);
                        //Buffer.BlockCopy(messageBuffer, 0, packetBuffer, headerBuffer.Length, messageBuffer.Length);

                        //clientSocket.Close();
                        //lock (_lock)
                        //{
                        //    clientSockets.Remove(clientSocket);

                        //    foreach (Socket sendSocket in clientSockets)
                        //    {
                        //        int SendLength = sendSocket.Send(packetBuffer, packetBuffer.Length, SocketFlags.None);
                        //    }
                        //}
                        JObject result = new JObject();
                        result.Add("code", "SendResult");
                        result.Add("message", "failed");

                        SendPacket(clientSocket, result.ToString());

                        return;
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine($"Error 낸 놈 : {e.Message} {clientSocket.RemoteEndPoint}");

                    //string message = "{ \"message\" : \" Disconnect : " + clientSocket.RemoteEndPoint + " \"}";
                    //byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
                    //ushort length = (ushort)IPAddress.HostToNetworkOrder((short)messageBuffer.Length);

                    //byte[] headerBuffer = new byte[2];

                    //headerBuffer = BitConverter.GetBytes(length);

                    //byte[] packetBuffer = new byte[headerBuffer.Length + messageBuffer.Length];
                    //Buffer.BlockCopy(headerBuffer, 0, packetBuffer, 0, headerBuffer.Length);
                    //Buffer.BlockCopy(messageBuffer, 0, packetBuffer, headerBuffer.Length, messageBuffer.Length);

                    //clientSocket.Close();
                    //lock (_lock)
                    //{
                    //    clientSockets.Remove(clientSocket);

                    //    foreach (Socket sendSocket in clientSockets)
                    //    {
                    //        int SendLength = sendSocket.Send(packetBuffer, packetBuffer.Length, SocketFlags.None);
                    //    }
                    //}

                    JObject result = new JObject();
                    result.Add("code", "SendResult");
                    result.Add("message", e.Message);

                    SendPacket(clientSocket, result.ToString());

                    return;
                }
            }
        }

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
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.54"), 4000);

            listenSocket.Bind(listenEndPoint);

            listenSocket.Listen(10);

            //Thread acceptThread = new Thread(new ThreadStart(AcceptThread));
            //acceptThread.IsBackground = true;
            //acceptThread.Start();
            //acceptThread.Join();

            // Task 버전
            Task acceptTask = new Task(AcceptThread);
            acceptTask.Start();
            acceptTask.Wait();

            // 만약 Task의 리턴값이 있다면
            //if (acceptTask.IsCompleted)
            //{
            //    // 리턴값 받는 로직
            //}



            listenSocket.Close();
        }
    }
}