using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;

public enum LoginType { ResourcesManager, Player };
public enum AccessLevel { ViewOnly, FullAccess, BridgeAccess };
public class JuniNetworkManager : NetworkManager
{
    int _numOfPlayers;
    public int NumOfPlayers
    {
        get { return _numOfPlayers; }
    }
    int _totalNumOfPlayers;
    bool _gameStarted;

    public GameObject mainMenuPanel;
    public Text ConnectionIP_InputField;
    public InputField PlayersCount;
    public InputField GameTime_Minutes;
    public InputField GameTime_Seconds;

    [SerializeField]
    private string _connectionIP = "127.0.0.1";
    [SerializeField]
    private int _portNum = 2101;
    public static LoginType loginType;
    static bool _isServer;
    public static bool IsServer { get { return _isServer; } }

    static JuniNetworkManager _instance;
    public static JuniNetworkManager Instance { get { return _instance; } }


    void Awake()
    {
        if (_instance != null)
            return;
        _instance = this;
    }
    void Start()
    {
        _numOfPlayers = 0;
    }


    public override void OnStartHost()
    {
        base.OnStartHost();
        _isServer = true;
    }

    public void PlayerLogin()
    {

    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        if (DeviceManager.instance)
        {
            DeviceManager.instance.StopClient();
            SceneManager.LoadScene("TerminalMainScene");
        }
    }
    //public override void OnServerConnect(NetworkConnection conn)
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        print("OnServerAddPlayer");
        base.OnServerAddPlayer(conn, playerControllerId);


       // _totalNumOfPlayers = DeviceManager.instance != null ? DeviceManager.instance.CurrentSession.SubSessions[DeviceManager.instance.SubAttractionID].Slots.Count : ConvertToInt(PlayersCount.text);
    }
    int parseTimeInSeconds()
    {
        return (int)((DeviceManager.instance.CurrentSession.SessionTime * 60f) % 60);
    }
    int parseTimeInMinutes()
    {
        return (int)((DeviceManager.instance.CurrentSession.SessionTime * 60f) / 60f);
    }
    public void StartGame()
    {
        ServerParameters.Instance.CreateCurrentSessionObjectives();
    }

    public int ConvertToInt(string num)
    {
        int res = 0;

        for (int i = 0; i < num.Length; ++i)
        {
            res *= 10;
            res += num[i] - '0';
        }
        return res;
    }
}
