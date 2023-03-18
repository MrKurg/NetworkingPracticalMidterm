// TCP Server (Blocking Mode)

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;

public class TCPServer
{
    private static byte[] buffer = new byte[512];
    private static byte[] sendBuffer = new byte[512];

    private static Socket serverTCP, serverUDP;
    private static Socket clientTCP, clientUDP;

    private static string sendMsg = "";
    private static float[] aPos;


    //Vector3 cubePos = new Vector3();

    public static void StartServer()
    {

        //Setup our server
        Console.WriteLine("Starting Server...");

        //serverTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        EndPoint remoteClient = new IPEndPoint(IPAddress.Any, 0);

        //serverTCP.Listen(10);


        try
        {
            //serverTCP.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));
            serverUDP.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8889));

            // Accept connections
            Console.WriteLine("Waiting for connections...");
            //clientTCP = serverTCP.Accept();
            //clientUDP = serverUDP.Accept();

            Console.WriteLine("Client connected!");

            //IPEndPoint clientTcpEP = (IPEndPoint)clientTCP.RemoteEndPoint;
            //IPEndPoint clientUdpEP = (IPEndPoint)clientUDP.RemoteEndPoint;

            //Console.WriteLine("Client: {0}  Port: {1}", clientTcpEP.Address, clientTcpEP.Port);
            //Console.WriteLine("Client: {0}  Port: {1}", clientUdpEP.Address, clientUdpEP.Port);

            //byte[] msg = Encoding.ASCII.GetBytes("This is my first TCP server!!! Welcome to INFR3830");

            // Sending data to connected client
            //clientTCP.Send(msg);

            // Loop
            while (true)
            {
                int recv = serverUDP.ReceiveFrom(buffer, ref remoteClient);
                aPos = new float[recv / 4];

                Buffer.BlockCopy(buffer, 0, aPos, 0, recv);
                Console.WriteLine("Received from " + remoteClient + " X:" + aPos[0] + " Y:" + aPos[1] + " Z:" + aPos[2]);

                serverUDP.SendTo(buffer, remoteClient);

                // User types a msg. Get input as a string
                Console.Write("\nEnter message: ");
                string userInput = Console.ReadLine();

                // Convert to bytes
                byte[] message = Encoding.ASCII.GetBytes(userInput);

                // Send to client
                clientTCP.Send(message);

                // Print msg from client
                byte[] clientMsgBuffer = new byte[512];
                int receiveClientMessage = clientTCP.Receive(clientMsgBuffer);
                int receiveClientMessageUdo = clientUDP.Receive(clientMsgBuffer);
                Console.WriteLine("Client message: {0}", Encoding.ASCII.GetString(clientMsgBuffer, 0, receiveClientMessage));

                // End loop
                break;
            }

            clientTCP.Shutdown(SocketShutdown.Both);
            clientUDP.Shutdown(SocketShutdown.Both);
            clientTCP.Close();
            clientUDP.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void ServerReceive(IAsyncResult result)
    {
        Socket socket = (Socket)result.AsyncState;

        int recv = socket.EndReceive(result);

        aPos = new float[recv / 4];

        Buffer.BlockCopy(buffer, 0, aPos, 0, recv);

        //cubePos.x = aPos[0];
        //cubePos.y = aPos[1];
        //cubePos.z = aPos[2];

        //Console.WriteLine("Received Position from Client: X:" + aPos[0] + " Y:" + aPos[1] + " Z:" + aPos[2]);

        //socket.BeginReceiveFrom(buffer, 0, buffer.Length, 0, new AsyncCallback(ServerReceive), socket);
    }

    public static int Main(String[] args)
    {
        StartServer();
        Console.ReadKey();
        return 0;
    }
}