//using System.IO;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.Networking;

public class TcpDataTransfer : NetworkBehaviour
{
    public string ServerIP = "127.0.0.1";
    public int port = 3003;

    TcpListener listen;

    [SyncVar]
    bool bRecieving;
    bool bRecievedData;
    bool bKeepRecieving;

    byte[] Data;

    public event Action OnDataComepletelySent;
    public event Action<byte[]> OnDataComepletelyRecieved;

    public static TcpDataTransfer singlton;

    private void Awake()
    {
        if (!singlton)
            singlton = this;
        else
            Debug.LogError("TcpDataTransfer has multiple instances in your scene");

    }

    public override void OnStartServer()
    {
        InitiateListener();
    }

    //public void SendFile(string FilePath)
    //{
    //    Data = File.ReadAllBytes(FilePath);
    //    Thread SendThread = new Thread(Send);
    //    SendThread.Start();
    //    //SendFile();
    //}

    public bool SendData(byte[] _Data)
    {
        if (!bRecieving)
        {
            Data = _Data;
            //Thread SendThread = new Thread(SendFile);
            //SendThread.Start();
            Send();
            return true;
        }
        return false;
    }

    void Send()
    {
        IPAddress ipAddress = IPAddress.Parse(ServerIP);
        int bufferSize = 1024;

        TcpClient client = new TcpClient();
        NetworkStream netStream;

        // Connect to server
        try
        {
            client.Connect(new IPEndPoint(ipAddress, port));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        netStream = client.GetStream();
        // Read bytes from image

        // Build the package
        byte[] dataLength = BitConverter.GetBytes(Data.Length);
        byte[] package = new byte[4 + Data.Length];
        dataLength.CopyTo(package, 0);
        Data.CopyTo(package, 4);

        // Send to server
        int bytesSent = 0;
        int bytesLeft = package.Length;

        //netStream.Write(package, 0, package.Length);
        while (bytesLeft > 0)
        {

            int nextPacketSize = (bytesLeft > bufferSize) ? bufferSize : bytesLeft;

            netStream.Write(package, bytesSent, nextPacketSize);
            bytesSent += nextPacketSize;
            bytesLeft -= nextPacketSize;
        }

        if (OnDataComepletelySent != null)
            OnDataComepletelySent();
        // Clean up
        netStream.Close();
        client.Close();
    }

    void InitiateListener()
    {
        listen = new TcpListener(IPAddress.Parse(ServerIP), port);
        bKeepRecieving = true;
        Thread RecieveThread = new Thread(RecieveDataThread);
        RecieveThread.Start();
    }

    void RecieveDataThread()
    {
        while (bKeepRecieving)
        {
            int bufferSize = 1024;
            NetworkStream netStream;
            int bytesRead = 0;
            int allBytesRead = 0;

            // Start listening
            listen.Start();

            // Accept client
            TcpClient client = listen.AcceptTcpClient();
            netStream = client.GetStream();
            bRecieving = true;

            // Read length of incoming data
            byte[] length = new byte[4];
            bytesRead = netStream.Read(length, 0, 4);
            int dataLength = BitConverter.ToInt32(length, 0);

            // Read the data
            int bytesLeft = dataLength;
            Data = new byte[dataLength];
            while (bytesLeft > 0)
            {

                int nextPacketSize = (bytesLeft > bufferSize) ? bufferSize : bytesLeft;

                bytesRead = netStream.Read(Data, allBytesRead, nextPacketSize);
                allBytesRead += bytesRead;
                bytesLeft -= bytesRead;

            }

            bRecievedData = true;

            // Save image to desktop
            // Clean up
            bRecieving = false;
            netStream.Close();
            client.Close();
        }
    }

    //public void SaveDataToDesktop(string FileSavePath)
    //{
    //    if (Data != null && !bRecieving)
    //    {
    //        File.WriteAllBytes(FileSavePath, Data);
    //    }
    //}

    public bool IsDataRecieved()
    {
        return bRecievedData;
    }

    public byte[] ReadDataAndClear()
    {
        if(IsDataRecieved())
        {
            bRecievedData = false;
            return Data;
        }
        return null;
    }
}
