using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TCPServer
{
    private static byte[] buffer = new byte[512];
    private static Socket serverTCP, serverUDP;
    private static Socket[] clientsTCP = new Socket[2];
    private static Socket clientUDP;
    private static string sendMsg = "";
    private static float[] aPos;

    public static void StartServer()
    {
        // Setup our server
        Console.WriteLine("Starting Server...");

        serverTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Bind the sockets to the desired IP and port
        serverTCP.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));
        serverUDP.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8889));

        // Listen for incoming connections
        serverTCP.Listen(10);

        Console.WriteLine("Waiting for connections...");

        // Accept two TCP connections
        clientsTCP[0] = serverTCP.Accept();
        Console.WriteLine("TCP client 1 connected!");
        clientsTCP[1] = serverTCP.Accept();
        Console.WriteLine("TCP client 2 connected!");

        // Receive data from UDP client
        EndPoint remoteClient = new IPEndPoint(IPAddress.Any, 0);
        int recv = serverUDP.ReceiveFrom(buffer, ref remoteClient);

        // Extract position data from the received buffer
        aPos = new float[recv / 4];
        Buffer.BlockCopy(buffer, 0, aPos, 0, recv);

        Console.WriteLine("Received from " + remoteClient + " X:" + aPos[0] + " Y:" + aPos[1] + " Z:" + aPos[2]);

        // Send response to the UDP client
        serverUDP.SendTo(buffer, remoteClient);

        // Loop to receive and send messages to the TCP clients
        while (true)
        {
            // Receive data from the TCP clients
            byte[] client1MsgBuffer = new byte[512];
            int receiveClient1Message = clientsTCP[0].Receive(client1MsgBuffer);
            byte[] client2MsgBuffer = new byte[512];
            int receiveClient2Message = clientsTCP[1].Receive(client2MsgBuffer);

            Console.WriteLine("Client 1 message: {0}", Encoding.ASCII.GetString(client1MsgBuffer, 0, receiveClient1Message));
            Console.WriteLine("Client 2 message: {0}", Encoding.ASCII.GetString(client2MsgBuffer, 0, receiveClient2Message));

            // Get input from the console as a string
            Console.Write("\nEnter message: ");
            string userInput = Console.ReadLine();

            // Convert the input string to bytes and send to the TCP clients
            byte[] message = Encoding.ASCII.GetBytes(userInput);
            clientsTCP[0].Send(message);
            clientsTCP[1].Send(message);

            // End loop
            break;
        }

        // Close the sockets and clean up
        clientsTCP[0].Shutdown(SocketShutdown.Both);
        clientsTCP[0].Close();
        clientsTCP[1].Shutdown(SocketShutdown.Both);
        clientsTCP[1].Close();
        serverTCP.Close();
        serverUDP.Close();
    }

    public static void Main(string[] args)
    {
        StartServer();
        Console.ReadKey();
    }
}
