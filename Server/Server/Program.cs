using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Threading;
using System.Linq;

public struct Player
{
    public CancellationTokenSource playerclient;
    public short usernameID { get; set; }
    public string NickName { get; set; }
    public Socket userTCPSocket { get; set; }
    public IPEndPoint userEndPoint { get; set; }
    public float[] userPosition { get; set; }

    public Player(Socket userSocket, short userID, string userName)
    {
        //object can be used to cancel asynchronous operations related to the player
        playerclient = new CancellationTokenSource();
        //a short integer value represents the unique identifier of the player
        usernameID = userID;
        //TCP socket associated with the player
        userTCPSocket = userSocket;
        //a string value that represents player name
        NickName = userName;
        //float array of length 3 for the position
        userPosition = new float[3];
        //the remote endpoint of the connected socket
        userEndPoint = (IPEndPoint)userSocket.RemoteEndPoint;
    }

    public Player(Socket mySocket, IPEndPoint myProtocol, short myID, string myName)
    {
        playerclient = new CancellationTokenSource();
        userEndPoint = myProtocol;
        usernameID = myID;
        userTCPSocket = mySocket;
        NickName = myName;
        userPosition = new float[3];
    }

}

public class ServerConsole
{
    private static CancellationTokenSource mainclient = new CancellationTokenSource();

    private static Socket serverTCP;
    private static UdpClient serverUDP;

    public static Dictionary<short, Player> playerDList = new Dictionary<short, Player>();
    public static List<string> chatList = new List<string>();

    static Random random = new Random();

    static void PrintPlayerList()
    {
        DateTime lastTime = DateTime.Now;

        double KEtimer = 0.0;
        double EKinterval = 1.0;

        while (true)
        {
            DateTime MNTime = DateTime.Now;

            double deltaTime = (MNTime - lastTime).TotalSeconds;
            KEtimer += deltaTime;

            if (KEtimer >= EKinterval)
            {
                if (playerDList.Count > 0)
                {
                    Console.WriteLine("Connected!!!!");
                    foreach (Player players in playerDList.Values)
                    {

                        Console.WriteLine("ID: {0}, Name: {1}", players.usernameID, players.NickName);
                        Console.WriteLine("POS: {0}, {1}, {2}", players.userPosition[0], players.userPosition[1], players.userPosition[2]);

                    }
                    Console.WriteLine("............");
                }
                KEtimer -= EKinterval;
            }


            lastTime = MNTime;
        }

    }

    public static int Main(String[] args)
    {

        StartServer();
        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);

        Task.Run(() => { PrintPlayerList(); }, mainclient.Token);

        Console.WriteLine("Press 1 or close console");
        Console.ReadLine();

        return 0;
    }

    static void StartServer()
    {
        IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        IPEndPoint serverTCPECK = new IPEndPoint(serverIP, 8888);
        IPEndPoint serverUDPECK = new IPEndPoint(serverIP, 8889);

        serverTCP = new Socket(serverIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        serverUDP = new UdpClient(serverUDPECK);

        try
        {
            serverTCP.Bind(serverTCPECK);
            serverTCP.Listen(10);


            serverUDP.BeginReceive(ServerUDPReceiveCallBack, null);

            Task.Run(() => { TCPconnect(); }, mainclient.Token);
        }
        catch (Exception X)
        {

            throw;
        }
    }

    /// <summary>
    /// TCP Accept thread, recurrsive, keep accepting
    /// </summary>
    static void TCPconnect()
    {
        Console.WriteLine("TCP Connecting");
        try
        {
            Socket acceptedClientSocket = serverTCP.Accept();
            Task.Run(() => { PlayerSetup(acceptedClientSocket); }, mainclient.Token);

        }
        catch (Exception X)
        {

        }

        TCPconnect();
    }

    /// <summary>
    /// Receives first TCP packet with header0. perfrom player login. Not recurrsive
    /// </summary>
    /// <param name="printSocket"> accepted socket from the player</param>
    /// method that sets up a new player in a game. It receives a TCP socket as a parameter and reads data from it to get the player's name. 
    /// It then generates a unique ID for the player and adds it to a dictionary that maps player IDs to player objects.
    static void PlayerSetup(Socket printSocket)
    {
        try
        {
            byte[] recvBuffer = new byte[1024];
            int recv = printSocket.Receive(recvBuffer);

            short DYheader = GetHeader(recvBuffer, 0);

            if (DYheader == 0)
            {
                string myname = Encoding.ASCII.GetString(GetContent(recvBuffer.Take(recv).ToArray(), 2));
                Console.WriteLine("Pname length: " + myname.Length + " VS: " + recv);
                short printid = (short)random.Next(1000, 13000);

                playerDList.Add(printid, new Player(printSocket, printid, myname));

                Console.WriteLine("Player Created: {0}: {1}", printid, myname);

                if (playerDList.ContainsKey(printid))
                {
                    string playerList = printid.ToString() + myname;

                    foreach (Player player in playerDList.Values)
                    {
                        if (player.usernameID != printid)
                        {
                            playerList += "#" + player.usernameID.ToString() + player.NickName;
                        }

                    }
                    //created by concatenating the ID and name of each player in playerDList using a number delimiter. This string is then converted to a byte array using ASCII encoding.
                    byte[] allPlayer = Encoding.ASCII.GetBytes(playerList);

                    printSocket.Send(AddHeader(allPlayer, 0));

                    foreach (Player player in playerDList.Values)
                    {
                        if (player.usernameID != printid)
                        {

                            player.userTCPSocket.Send(AddHeader(AddHeader(Encoding.ASCII.GetBytes(myname), printid), 9));
                        }
                    }
                    //begin listening for TCP data from the new player. If an exception is thrown, the method logs the error and throws it again.
                    PlayerTCPReceive(printid);
                }
            }
        }
        catch (Exception Y)
        {
            Console.WriteLine(Y.ToString());
            throw;
        }
    }

    /// <summary>
    /// TCP receive from specific player; Receive content handling
    /// </summary>
    /// <param name="pName"></param>
    static void PlayerTCPReceive(short LMID)
    {
        try
        {
            if (playerDList.ContainsKey(LMID))
            {

                byte[] krecvBuffer = new byte[1024];
                int recv = playerDList[LMID].userTCPSocket.Receive(krecvBuffer);

                short[] pheaderBuffer = new short[2];
                Buffer.BlockCopy(krecvBuffer, 0, pheaderBuffer, 0, 4);


                switch (pheaderBuffer[0])
                {
                    // Chat
                    case 1:

                        string content = Encoding.ASCII.GetString(krecvBuffer, 4, recv - 4);
                        //string msgPlayerName = "["+playerDList[pID].playerName.ToString();
                        //string msgTime = "] <"+DateTime.Now.ToString("MM/dd hh:mm:ss tt")+"> ";

                        string chatting = $"[<{DateTime.Now.ToString("MM/dd hh:mm:ss tt")}> {playerDList[LMID].NickName}]: {content}";

                        Console.WriteLine(chatting);

                        chatList.Add(chatting);

                        byte[] message = AddHeader(Encoding.ASCII.GetBytes(chatting), 1);

                        Console.WriteLine("String: {0}, Bytes[]: {1}", chatting.Length, message.Length);

                        foreach (Player player in playerDList.Values)
                        {
                            player.userTCPSocket.Send(message);
                        }

                        break;


                    default:
                        break;
                }
            }
        }
        catch (Exception ER)
        {
            Console.WriteLine(ER.ToString());
            playerDList[LMID].playerclient.Cancel();
            playerDList.Remove(LMID);
            throw;
        }

        PlayerTCPReceive(LMID);

    }


    static void ServerUDPReceiveCallBack(IAsyncResult result)
    {
        //UdpClient udpClient = (UdpClient)result.AsyncState;
        IPEndPoint clientprotocol = new IPEndPoint(IPAddress.Any, 0);
        byte[] recvBuffer = serverUDP.EndReceive(result, ref clientprotocol);


        switch (GetHeader(recvBuffer, 0))
        {
            case 0:
                short kid = GetHeader(recvBuffer, 2);

                if (playerDList.ContainsKey(kid))
                {
                    Player setupPlayer = new Player(playerDList[kid].userTCPSocket, clientprotocol, playerDList[kid].usernameID, playerDList[kid].NickName);

                    playerDList[kid] = setupPlayer;

                    Console.WriteLine("SETUP!: " + playerDList[kid].userEndPoint.Address + " " + playerDList[kid].userEndPoint.Port);

                }
                break;

            case 1:
                short myplayerid = GetHeader(recvBuffer, 2);
                if (playerDList.ContainsKey(myplayerid))
                {
                    Buffer.BlockCopy(GetContent(recvBuffer, 4), 0, playerDList[myplayerid].userPosition, 0, 12);

                    //Console.WriteLine("GET Position: {0}: {1}, {2}, {3}", playerid,
                    //    playerDList[playerid].playerPosition[0],
                    //   playerDList[playerid].playerPosition[1],
                    // playerDList[playerid].playerPosition[2]);

                    foreach (Player twoplayer in playerDList.Values)
                    {
                        if (twoplayer.usernameID != myplayerid)
                        {
                            if (twoplayer.userEndPoint != null)
                            {
                                serverUDP.Send(recvBuffer, recvBuffer.Length, twoplayer.userEndPoint);
                            }
                            else
                            {
                                Console.WriteLine(twoplayer.NickName + "'s endpoint is null");
                            }

                        }
                    }

                }
                break;

            default:
                break;
        }
        serverUDP.BeginReceive(ServerUDPReceiveCallBack, null);
    }


    /*
    static void ServerUDPReceive()
    {
        byte[] recvBuffer = new byte[1024];
        IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
        recvBuffer = serverUDP.Receive(ref clientEP);

        

        switch(GetHeader(recvBuffer))
        {
            case 0:
                short pid = GetID(recvBuffer);

                if(playerDList.ContainsKey(pid))
                {
                    byte[] ip = Encoding.ASCII.GetBytes(clientEP.Address.ToString());
                    int[] port = { clientEP.Port };
                    Buffer.BlockCopy(ip, 0, playerDList[pid].pEPiP, 0, ip.Length);
                    Buffer.BlockCopy(port, 0, playerDList[pid].pEPpo, 0, 4);

                    Console.WriteLine("SETUP!: " + Encoding.ASCII.GetString(playerDList[pid].pEPiP) + " " + playerDList[pid].pEPpo[0]);

                    Buffer.BlockCopy(GetContent(recvBuffer), 0, playerDList[pid].playerPosition, 0, 12);

                    short[] header = { 0, -1 };

                    byte[] allTrans = new byte[2 + playerDList.Count * 14];
                    Buffer.BlockCopy(header, 0, allTrans, 0, 2);

                    int ind = 0;
                    foreach(Player player in playerDList.Values)
                    {
                        header[1] = player.playerID;
                        Buffer.BlockCopy(header, 2, allTrans, ind * 14, 2);
                        Buffer.BlockCopy(player.playerPosition, 0, allTrans, ind * 14 + 2, 12);
                        ind++;
                    }


                    foreach(Player player in playerDList.Values)
                    {
                        if(player.playerID != pid)
                        {
                            serverUDP.Send(allTrans, new IPEndPoint(IPAddress.Parse(Encoding.ASCII.GetString(player.pEPiP)), player.pEPpo[0]));
                        }
                        
                    }

                    //Console.WriteLine(allTrans.Length);

                    //Console.WriteLine(clientEP.Address.ToString() + " " + clientEP.Port.ToString());
                    
                    //playerDList[pid].playerTCPSocket.Send(allTrans);

                }
                break;

            default:
                break;
        }
        ServerUDPReceive();
    }

    */
    //It creates a new array myheader with a length of one, copies two bytes from bytes starting at the offset specified by myoffset into myheader, and returns the first element of myheader as a short.
    static short GetHeader(byte[] bytes, int myoffset)
    {
        short[] myheader = new short[1];
        Buffer.BlockCopy(bytes, myoffset, myheader, 0, 2);
        return myheader[0];
    }
    //takes in a byte array bytes and a short value ourheader as parameters, and returns a new byte array.
    static byte[] AddHeader(byte[] bytes, short ourheader)
    {
        byte[] Abuffer = new byte[bytes.Length + 2];
        short[] myBuffer = { ourheader };
        Buffer.BlockCopy(myBuffer, 0, Abuffer, 0, 2);
        Buffer.BlockCopy(bytes, 0, Abuffer, 2, bytes.Length);
        return Abuffer;
    }
    //takes in a byte array buffer and an integer offset 
    static byte[] GetContent(byte[] buffer, int ouroffset)
    {
        byte[] KreturnBy = new byte[buffer.Length - ouroffset];
        Buffer.BlockCopy(buffer, ouroffset, KreturnBy, 0, KreturnBy.Length);
        return KreturnBy;
    }

    //an event handler that is triggered when the user presses Ctrl+C to cancel the application
    static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Quitting...");
        serverTCP.Close();
        serverUDP.Close();
        mainclient.Cancel();
    }
}