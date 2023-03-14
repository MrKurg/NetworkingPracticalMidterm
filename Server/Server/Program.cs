// TCP Server (blocking mode)
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq.Expressions;
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

    public static void StartServer()
    {
        Console.WriteLine("");
        IPAddress ip = IPAddress.Parse("127.0.0.1");

        Console.WriteLine("");
        IPEndPoint serverKE = new IPEndPoint(ip, 8889);

        Socket server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            server.Bind(serverKE);

            //function call sets the maximum length of the pending connections queue for the server socket to 10
            server.Listen(10);

            Console.WriteLine("[SYSTEM] Server has started");

            Task.Run(() => { ServerAccept(server); });

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

    static void ServerReceive(Socket newSocket, string clientNameMidterm)
    {
        while (newSocket.Connected)
        {
            byte[] buffer = new byte[1024];
            //int get = newSocket.Receive(data, 0, data.Length, SocketFlags.None);
            int get = newSocket.Receive(buffer);

            if (!newSocket.Poll(0, SelectMode.SelectWrite))
            {
                taskPrint("[SYSTEM] " + clientNameMidterm + ": " + "has left the server");
                socketDict.Remove(newSocket.RemoteEndPoint.ToString());

                newSocket.Shutdown(SocketShutdown.Both);
                newSocket.Close();
                return;
            }

            string msgReceived = Encoding.ASCII.GetString(buffer, 0, get);

            byte[] msg = Encoding.ASCII.GetBytes(("[" + clientNameMidterm + "]: " + msgReceived));

            foreach (string socketTag in socketDict.Keys)
            {
                socketDict[socketTag].Send(msg);
            }
        }
    }

    static void ServerAccept(Socket socket)
    {
        while (true)
        {
            Socket newSocket = socket.Accept();
            byte[] buffer = new byte[1024];
            int get = newSocket.Receive(buffer);
            string clientName = Encoding.ASCII.GetString(buffer, 0, get);
            socketDict.Add(newSocket.RemoteEndPoint.ToString(), newSocket);

            taskPrint("[SYSTEM] New client connected: " + clientName + " [" + newSocket.RemoteEndPoint.ToString() + "]");

            Task.Run(() => { ServerReceive(newSocket, clientName); });
        }
    }

    public static int Main(String[] args)
    {
        StartServer();
        return 0;
    }

}