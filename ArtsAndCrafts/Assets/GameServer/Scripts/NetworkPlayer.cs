using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using AttractionAttractionDeviceManager;

public class NetworkPlayer : NetworkBehaviour {
    [SyncVar]
	public string CurrentPlayerId;

    void Start()
    {
        if(isLocalPlayer)
        {
            if(AttractionDeviceManager.instance != null && AttractionDeviceManager.instance.CurrentPlayer != null)
            {
                CmdSendPlayerId(AttractionDeviceManager.instance.CurrentPlayer.UserId);
            }
        }  
        else if(isServer)
        {
        }
    }

    [Command]
    public void CmdSendPlayerId(string PlayerId)
    {
        CurrentPlayerId = PlayerId;
    }
}
