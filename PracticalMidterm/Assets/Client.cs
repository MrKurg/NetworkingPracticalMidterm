using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using TMPro;

public class Client : MonoBehaviour
{
    public GameObject myCube;
    private static byte[] myPos;
    private static IPEndPoint remoteEP;
    private static Socket clientSocket;

    private float[] aPos;

    //public Toggle detectMovement;
    //public Toggle packetIntervals;
    //public TMP_InputField packetInput;
    private float timer;

    private static Socket clientTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private static Socket clientUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private static byte[] buffer = new byte[512];

    //LECTURE 06
    private static byte[] bpos;
    private static float[] pos;

    public static void StartClient()
    {
        remoteEP = new IPEndPoint(IPAddress.Any, 8889);

        //clientTCP.Connect(IPAddress.Parse("127.0.0.1"), 8888);
        clientUDP.Connect(IPAddress.Parse("127.0.0.1"), 8889);
        Debug.Log("Connected to server!");


    }

    private void SendPosition()
    {
        aPos = new float[] { myCube.transform.position.x, myCube.transform.position.y, myCube.transform.position.z };
        myPos = new byte[aPos.Length * 4];
        Buffer.BlockCopy(aPos, 0, myPos, 0, myPos.Length);

        clientSocket.SendTo(myPos, remoteEP);

        Debug.Log("Position Sent.");
    }

    // Start is called before the first frame update
    void Start()
    {
        myCube = GameObject.Find("Cube");
        StartClient();
    }

    // Update is called once per frame
    void Update()
    {
    //    if (detectMovement.isOn)
    //    {
    //        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
    //        {
    //            if (packetIntervals.isOn)
    //            {
    //                if (timer <= 0)
    //                {
    //                    Debug.Log("Move Detected. Send position: " + myCube.transform.position);
    //                    SendPosition();
    //                    timer += float.Parse(packetInput.text);
    //                }
    //                else
    //                {
    //                    timer -= Time.deltaTime;
    //                }
    //            }
    //            else
    //            {
    //                Debug.Log("Movement detected. New position: " + myCube.transform.position);
    //                SendPosition();
    //            }


    //        }
    //    }
    //    else
    //    {
    //        if (packetIntervals.isOn)
    //        {
    //            if (timer <= 0)
    //            {
    //                Debug.Log("Send position: " + myCube.transform.position);
    //                SendPosition();
    //                timer += float.Parse(packetInput.text);
    //            }
    //            else
    //            {
    //                timer -= Time.deltaTime;
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("Send position: " + myCube.transform.position);
    //            SendPosition();
    //        }
    //    }
    }
}
