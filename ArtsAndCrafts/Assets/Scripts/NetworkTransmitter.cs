using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

[NetworkSettings(channel = 2, sendInterval = 0.01f)]
public class NetworkTransmitter : NetworkBehaviour
{
    struct TransmissionData
    {
        public int CurrentDataIndex;
        public byte[] data;

        public TransmissionData(byte[] _data)
        {
            CurrentDataIndex = 0;
            data = _data;
        }
    }

    private static readonly string LOG_PREFIX = "[" + typeof(NetworkTransmitter).Name + "]: ";
    private static int defaultBufferSize = 1024; //max ethernet MTU is ~1400

    //maps the transmission id to the data being received.
    Dictionary<int, TransmissionData> ServerTransmissionData = new Dictionary<int, TransmissionData>();

    //list of transmissions currently going on. a transmission id is used to uniquely identify to which transmission a received byte[] belongs to.
    List<int> ClientTransmissionIds = new List<int>();

    //callbacks which are invoked on the respective events. int = transmissionId. byte[] = data sent or received.
    public event Action<int, byte[]> OnDataComepletelySent;
    public event Action<int, byte[]> OnDataFragmentSent;
    public event Action<int, byte[]> OnDataFragmentReceived;
    public event Action<int, byte[]> OnDataCompletelyReceived;

    public void SendDataToServer(int transmissionId, byte[] data)
    {
        if (!isLocalPlayer)
            return;
        Debug.Assert(!ClientTransmissionIds.Contains(transmissionId));

        StartCoroutine(SendBytesToServerRoutine(transmissionId, data));
    }

    [Client]
    private IEnumerator SendBytesToServerRoutine(int transmissionId, byte[] data)
    {
        Debug.Log(LOG_PREFIX + "SendBytesToClients processId=" + transmissionId + " | datasize=" + data.Length);

        //tell Server that he is going to receive some data and tell him how much it will be.
        CmdPrepareToReceiveData(transmissionId, data.Length);
        yield return null;

        // Begin transmission of data. send chunks of 'bufferSize' until completely transmitted
        ClientTransmissionIds.Add(transmissionId);
        TransmissionData DataToTransmit = new TransmissionData(data);
        int BufferSize = defaultBufferSize;
        while (DataToTransmit.CurrentDataIndex < DataToTransmit.data.Length - 1)
        {
            // determine the remaining amount of bytes, still need to be sent.
            int remaining = DataToTransmit.data.Length - DataToTransmit.CurrentDataIndex;
            // in case last chunk
            if (remaining < BufferSize)
                BufferSize = remaining;

            // prepare the chunks of data which will be sent in this iteration
            byte[] buffer = new byte[BufferSize];
            Array.Copy(DataToTransmit.data, DataToTransmit.CurrentDataIndex, buffer, 0, BufferSize);

            // send the chunk of data which will be sent in this iteration
            CmdReceiveBytes(transmissionId, buffer);
            DataToTransmit.CurrentDataIndex += BufferSize;

            yield return null;

            if (OnDataFragmentSent != null)
                OnDataFragmentSent(transmissionId, buffer);
        }

        // TransmissionComplete
        ClientTransmissionIds.Remove(transmissionId);
        if (OnDataComepletelySent != null)
            OnDataComepletelySent(transmissionId, DataToTransmit.data);
    }

    [Command]
    private void CmdPrepareToReceiveData(int transmissionId, int expectedSize)
    {
        if (ServerTransmissionData.ContainsKey(transmissionId))
            return;

        //prepare data array which will be filled chunk by chunk by the received data
        TransmissionData receivingData = new TransmissionData(new byte[expectedSize]);
        ServerTransmissionData.Add(transmissionId, receivingData);
    }

    [Command(channel = 2)]
    private void CmdReceiveBytes(int transmissionId, byte[] recBuffer)
    {
        //already completely received or not prepared?
        if (!ServerTransmissionData.ContainsKey(transmissionId))
            return;

        //copy received data into prepared array and remember current dataposition
        TransmissionData dataToReceive = ServerTransmissionData[transmissionId];
        System.Array.Copy(recBuffer, 0, dataToReceive.data, dataToReceive.CurrentDataIndex, recBuffer.Length);
        dataToReceive.CurrentDataIndex += recBuffer.Length;
        ServerTransmissionData[transmissionId] = dataToReceive;

        if (null != OnDataFragmentReceived)
            OnDataFragmentReceived(transmissionId, recBuffer);

        if (dataToReceive.CurrentDataIndex < dataToReceive.data.Length - 1)
            //current data not completely received
            return;

        //current data completely received
        Debug.Log(LOG_PREFIX + "Completely Received Data at transmissionId=" + transmissionId);
        ServerTransmissionData.Remove(transmissionId);

        if (null != OnDataCompletelyReceived)
            OnDataCompletelyReceived(transmissionId, dataToReceive.data);
    }
}
