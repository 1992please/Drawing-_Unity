using UnityEngine;
using System.Collections;
using Juniverse.RPCLibrary;
using Juniverse.Notifications;
using Juniverse.Model;
using UnityEngine.UI;
using System;
using System.Collections.Generic;


public class ServiceInterface : MonoBehaviour
{
    public static ServiceInterface instance = null;
    // Use this for initialization
    void Start()
    {
    }
    void Update()
    {
    }
    void Awake()
    {
        if (instance == null)
            instance = this;
    }
    // Update is called once per frame
    public void StartAttractionTerminalSession(string attractionId)
    {

        DeviceManager.instance.MainGame.StartAttractionTerminalSessionAsync(attractionId, (attraction) =>
        {
            DeviceManager.instance.AttractionSessionCreated = true;
        }, (ex) => Debug.LogException(ex));
    }
    public void StartSubAttractionTerminalSession(string subAttractionId)
    {

        DeviceManager.instance.MainGame.StartSubAttractionTerminalSessionAsync(subAttractionId, (Subattraction) =>
        {
            DeviceManager.instance.SubAttractionSessionCreated = true;
            DeviceManager.instance.MainGame.SetSubAttractionTerminalSessionHostAsync(subAttractionId, DeviceManager.instance.GetIPAddress());

        }, (ex) => Debug.LogException(ex));
    }
    public void StartSubAttractionGameTerminalSession(string attractionId, string subattractionId, string terminalId)
    {

        DeviceManager.instance.MainGame.StartSubAttractionGameTerminalSessionAsync(attractionId, subattractionId, terminalId, (SubattractionGameTerminal) =>
        {
            DeviceManager.instance.GameTerminalAttractionSessionCreated = true;
            DeviceManager.instance.MainGame.GetSubAttractionTerminalSessionHostAsync(SubattractionGameTerminal.SubAttractionId, (ip) =>
            {
                DeviceManager.instance.SubServerIP = ip;
            }, (ex) => Debug.LogException(ex));
        }, (ex) => Debug.LogException(ex));
    }

    public void AttractionReconnectServer(string attractionId, RPCConnection rpcConnection)
    {
        if (string.IsNullOrEmpty(attractionId))
        {
            print("Session Reconnected!!");
            rpcConnection.RetryPendingCalls();
        }
        else
        {
            DeviceManager.instance.MainGame.AttractionSessionReconnectedAsync(attractionId, () =>
            {
                print("Session Reconnected!!");
                rpcConnection.RetryPendingCalls();

            }, (ex) => Debug.LogException(ex));
        }
    }
    public void SubAttractionReconnectServer(string subAttractionId, RPCConnection rpcConnection)
    {
        if (string.IsNullOrEmpty(subAttractionId))
        {
            print("Session Reconnected!!");
            rpcConnection.RetryPendingCalls();
        }
        else
        {

            DeviceManager.instance.MainGame.SubAttractionSessionReconnectedAsync(subAttractionId, () =>
            {
                print("Session Reconnected!!");
                rpcConnection.RetryPendingCalls();

            }, (ex) => Debug.LogException(ex));
        }
    }
    public void SubAttractionGameTerminalReconnectServer(string attractionId, string subAttractionId, string terminalId, RPCConnection rpcConnection)
    {
        if (string.IsNullOrEmpty(terminalId))
        {
            print("Session Reconnected!!");
            rpcConnection.RetryPendingCalls();
        }
        else
        {
            DeviceManager.instance.MainGame.SubAttractionGameTerminalSessionReconnectedAsync(attractionId, subAttractionId, terminalId, () =>
            {
                print("Session Reconnected!!");
                rpcConnection.RetryPendingCalls();

            }, (ex) => Debug.LogException(ex));
        }
    }
}
