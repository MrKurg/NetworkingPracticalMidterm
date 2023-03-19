using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TCPServer
{
    private static byte[] buffer = new byte[512];
    private static Socket serverTCP, serverUDP;
    private static Socket clientTCP, clientUDP;
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

        // Accept a TCP connection
        clientTCP = serverTCP.Accept();

        Console.WriteLine("TCP client connected!");

        // Receive data from UDP client
        EndPoint remoteClient = new IPEndPoint(IPAddress.Any, 0);
        int recv = serverUDP.ReceiveFrom(buffer, ref remoteClient);

        // Extract position data from the received buffer
        aPos = new float[recv / 4];
        Buffer.BlockCopy(buffer, 0, aPos, 0, recv);

        Console.WriteLine("Received from " + remoteClient + " X:" + aPos[0] + " Y:" + aPos[1] + " Z:" + aPos[2]);

        // Send response to the UDP client
        serverUDP.SendTo(buffer, remoteClient);

        // Loop to receive and send messages to the TCP client
        while (true)
        {
            // Receive data from the TCP client
            byte[] clientMsgBuffer = new byte[512];
            int receiveClientMessage = clientTCP.Receive(clientMsgBuffer);

            Console.WriteLine("Client message: {0}", Encoding.ASCII.GetString(clientMsgBuffer, 0, receiveClientMessage));

            // Get input from the console as a string
            Console.Write("\nEnter message: ");
            string userInput = Console.ReadLine();

            // Convert the input string to bytes and send to the TCP client
            byte[] message = Encoding.ASCII.GetBytes(userInput);
            clientTCP.Send(message);

            // End loop
            break;
        }

        // Close the sockets and clean up
        clientTCP.Shutdown(SocketShutdown.Both);
        clientTCP.Close();
        serverTCP.Close();
        serverUDP.Close();
    }

    public static void Main(string[] args)
    {
        StartServer();
        Console.ReadKey();
    }
}
