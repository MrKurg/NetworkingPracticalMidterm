//// TCP Server (blocking mode)
//using System;
//using System.Text;
//using System.Net;
//using System.Net.Sockets;
//using System.Linq.Expressions;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//public class TCPServer
//{
//    private static Dictionary<string, Socket> socketDict = new Dictionary<string, Socket>();

//    private static bool isPrinting = false;
//    private static string printContent = "";

//    private static void taskPrint(string POKE)
//    {
//        printContent = POKE;
//        isPrinting = true;
//    }

//    public static void StartServer()
//    {
//        Console.WriteLine("");
//        IPAddress ip = IPAddress.Parse("127.0.0.1");

//        Console.WriteLine("");
//        IPEndPoint serverKE = new IPEndPoint(ip, 8889);

//        Socket server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

//        try
//        {
//            server.Bind(serverKE);

//            //function call sets the maximum length of the pending connections queue for the server socket to 10
//            server.Listen(10);

//            Console.WriteLine("[SYSTEM] Server has started");

//            Task.Run(() => { ServerAccept(server); });

//            while (true)
//            {
//                if (isPrinting)
//                {
//                    Console.WriteLine(printContent);
//                    isPrinting = false;
//                }
//            }
//        }
//        catch (Exception JMK)
//        {
//            Console.WriteLine(JMK.ToString());
//        }
//    }

//    static void ServerReceive(Socket newSocket, string clientNameMidterm)
//    {
//        while (newSocket.Connected)
//        {
//            byte[] buffer = new byte[1024];
//            //int get = newSocket.Receive(data, 0, data.Length, SocketFlags.None);
//            int get = newSocket.Receive(buffer);

//            if (!newSocket.Poll(0, SelectMode.SelectWrite))
//            {
//                taskPrint("[SYSTEM] " + clientNameMidterm + ": " + "has left the server");
//                socketDict.Remove(newSocket.RemoteEndPoint.ToString());

//                newSocket.Shutdown(SocketShutdown.Both);
//                newSocket.Close();
//                return;
//            }

//            string msgReceived = Encoding.ASCII.GetString(buffer, 0, get);

//            byte[] msg = Encoding.ASCII.GetBytes(("[" + clientNameMidterm + "]: " + msgReceived));

//            foreach (string socketTag in socketDict.Keys)
//            {
//                socketDict[socketTag].Send(msg);
//            }
//        }
//    }

//    static void ServerAccept(Socket socket)
//    {
//        while (true)
//        {
//            Socket newSocket = socket.Accept();
//            byte[] buffer = new byte[1024];
//            int get = newSocket.Receive(buffer);
//            string clientName = Encoding.ASCII.GetString(buffer, 0, get);
//            socketDict.Add(newSocket.RemoteEndPoint.ToString(), newSocket);

//            taskPrint("[SYSTEM] New client connected: " + clientName + " [" + newSocket.RemoteEndPoint.ToString() + "]");

//            Task.Run(() => { ServerReceive(newSocket, clientName); });
//        }
//    }

//    public static int Main(String[] args)
//    {
//        StartServer();
//        return 0;
//    }

//}

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TCPServer
{
    private static Dictionary<string, Socket> socketDict = new Dictionary<string, Socket>();

    private static bool isPrinting = false;
    private static string printContent = "";

    private static void taskPrint(string POKE)
    {
        printContent = POKE;
        isPrinting = true;
    }

    static void TcpServerReceive(Socket socket, string clientName)
    {
        while (socket.Connected)
        {
            byte[] buffer = new byte[1024];
            int get = socket.Receive(buffer);

            if (!socket.Poll(0, SelectMode.SelectWrite))
            {
                taskPrint($"[TCP] {clientName}: has left the server");
                socketDict.Remove(socket.RemoteEndPoint.ToString());

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return;
            }

            string msgReceived = Encoding.ASCII.GetString(buffer, 0, get);

            byte[] msg = Encoding.ASCII.GetBytes($"[TCP] [{clientName}]: {msgReceived}");

            foreach (string socketTag in socketDict.Keys)
            {
                if (socketDict[socketTag].ProtocolType == ProtocolType.Tcp)
                {
                    socketDict[socketTag].Send(msg);
                }
            }
        }
    }

    static void TcpServerAccept(Socket socket)
    {
        while (true)
        {
            Socket newSocket = socket.Accept();
            byte[] buffer = new byte[1024];
            int get = newSocket.Receive(buffer);
            string clientName = Encoding.ASCII.GetString(buffer, 0, get);
            socketDict.Add(newSocket.RemoteEndPoint.ToString(), newSocket);

            taskPrint($"[TCP] New client connected: {clientName} [{newSocket.RemoteEndPoint.ToString()}]");

            Task.Run(() => { TcpServerReceive(newSocket, clientName); });
        }
    }

    public static void StartTCPServer()
    {
        Console.WriteLine("");
        IPAddress ip = IPAddress.Parse("127.0.0.1");

        Console.WriteLine("");
        IPEndPoint serverKE = new IPEndPoint(ip, 8889);

        Socket server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            server.Bind(serverKE);

            server.Listen(10);

            Console.WriteLine("[SYSTEM] TCP Server has started");

            Task.Run(() => { TcpServerAccept(server); });

            while (true)
            {
                if (isPrinting)
                {
                    Console.WriteLine(printContent);
                    isPrinting = false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    static void UdpServerReceive(Socket socket)
    {
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            byte[] buffer = new byte[1024];

            int get = socket.ReceiveFrom(buffer, ref remoteEndPoint);

            string msgReceived = Encoding.ASCII.GetString(buffer, 0, get);

            byte[] msg = Encoding.ASCII.GetBytes($"[UDP] {remoteEndPoint}: {msgReceived}");

            foreach (string socketTag in socketDict.Keys)
            {
                if (socketDict[socketTag].ProtocolType == ProtocolType.Udp)
                {
                    socketDict[socketTag].SendTo(msg, socketDict[socketTag].RemoteEndPoint);
                }
            }
        }
    }

    public static void StartUDPServer()
    {
        Console.WriteLine("");
        IPAddress ip = IPAddress.Parse("127.0.0.1");

        Console.WriteLine("");
        IPEndPoint serverKE = new IPEndPoint(ip, 8888);

        Socket server = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            server.Bind(serverKE);

            Console.WriteLine("[SYSTEM] UDP Server has started");

            Task.Run(() => { UdpServerReceive(server); });

            while (true)
            {
                if (isPrinting)
                {
                    Console.WriteLine(printContent);
                    isPrinting = false;
                }
            }
        }
        catch (Exception JMK)
        {
            Console.WriteLine(JMK.ToString());
        }
    }

    static void UdpServerReceive(IAsyncResult result)
    {
        Socket socket = (Socket)result.AsyncState;
        while (true)
        {
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[1024];

            int get = socket.ReceiveFrom(buffer, ref remoteEndPoint);

            string msgReceived = Encoding.ASCII.GetString(buffer, 0, get);
            byte[] msg = Encoding.ASCII.GetBytes($"[UDP] {remoteEndPoint}: {msgReceived}");

            foreach (string socketTag in socketDict.Keys)
            {
                if (socketDict[socketTag].ProtocolType == ProtocolType.Udp)
                {
                    socketDict[socketTag].SendTo(msg, socketDict[socketTag].RemoteEndPoint);
                }
            }
        }
    }
}
