using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using AttractionAttractionDeviceManager;
public enum LoginType { ResourcesManager, Player };
public enum AccessLevel { ViewOnly, FullAccess, BridgeAccess };
public class CustomNetworkManager : AttractionAttractionDeviceManager.BaseCustomNetworkManager
{
    public override void OnClientDisconnect(NetworkConnection conn)
    {
    }
    public override void StartJuniverseServer()
    {
    }
    public override void StartJuniverseClient()
    {
    }
    public override void StopJuniverseServer()
    { 
    }
    public override void StopJuniverseClient()
    {
    }

}
