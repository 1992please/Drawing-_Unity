using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkManager))]
public class MyNetworkManagerHUD : MonoBehaviour {
    public bool bShowGUI;
    private NetworkManager Manager;
    // Use this for initialization
    private void Awake()
    {
        Manager = GetComponent<NetworkManager>();
        Manager.networkAddress = "127.0.0.1";
    }

    public void LANHost()
    {
        Manager.networkPort = 8888;
        Manager.StartHost();
    }

    public void LANClient()
    {
        Manager.networkAddress = "127.0.0.1";
        Manager.networkPort = 8888;
        Manager.StartClient();
    }

    public void LANServerOnly()
    {
        Manager.StartServer();
    }

    private void OnGUI()
    {
        if (!bShowGUI)
            return;

        int xpos = 10;
        int ypos = 40;
        int spacing = 24;

        if (!NetworkClient.active && !NetworkServer.active && Manager.matchMaker == null)
        {
            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Host(H)"))
            {
                Manager.StartHost();
            }
            ypos += spacing;

            if (GUI.Button(new Rect(xpos, ypos, 105, 20), "LAN Client(C)"))
            {
                Manager.StartClient();
            }
            Manager.networkAddress = GUI.TextField(new Rect(xpos + 100, ypos, 95, 20), Manager.networkAddress);
            ypos += spacing;

            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Server Only(S)"))
            {
                Manager.StartServer();
            }
            ypos += spacing;
        }
        else
        {

            if (NetworkServer.active)
            {
                GUI.Label(new Rect(xpos, ypos, 300, 20), "Server: port=" + Manager.networkPort);
                ypos += spacing;
            }
            if (NetworkClient.active)
            {
                GUI.Label(new Rect(xpos, ypos, 300, 20), "Client: address=" + Manager.networkAddress + " port=" + Manager.networkPort);
                ypos += spacing;
            }

            if (NetworkClient.active && !ClientScene.ready)
            {
                if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Client Ready"))
                {
                    ClientScene.Ready(Manager.client.connection);

                    if (ClientScene.localPlayers.Count == 0)
                    {
                        ClientScene.AddPlayer(0);
                    }
                }
                ypos += spacing;
            }

            if (NetworkServer.active || NetworkClient.active)
            {
                if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Stop (X)"))
                {
                    Manager.StopHost();
                }
                ypos += spacing;
            }
        }
    }

}
