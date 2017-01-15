using UnityEngine;
using System.Collections.Generic;
using Juniverse.RPCLibrary;
using Juniverse.Notifications;
using Juniverse.Model;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

public enum DeviceType
{
    Attraction,
    SubAttraction,
    Terminal
}

public class DeviceManager : MonoBehaviour
{

    RPCConnection _rpcConnection = null;
    NotificationManager _notifications = new NotificationManager();
    public static DeviceManager instance = null;
    public bool DeviceConnected = false;

    public bool AttractionSessionCreated = false;
    public bool SubAttractionSessionCreated = false;
    public bool GameTerminalAttractionSessionCreated = false;
    bool StartedHosting = false;

    DeviceType _currentDeviceType;

    public delegate void SessionStarted();
    public static event SessionStarted OnSessionStarted;
    DeviceNotificationManager _deviceNotificationManager;

    string ServerIP = "127.0.0.1";
    public string SubServerIP = "127.0.0.1";

    string _attractionId;
    string _subAttractionId;
    string _terminalSubattractionId;

    GamerProfile _currentPlayer;
    public GamerProfile CurrentPlayer { get { return _currentPlayer; } }
    string _CurrentPlayerId;
    string _CurrentObjectiveId;
    string _AssignmentId;
    string _CurrentSubSessionId;

    ServerParameters SParams;

    QuestObjectiveData _CurrentObjectiveData;

    public AttractionSession CurrentSession;

    public DeviceNotificationManager DeviceNotificationManager
    {
        get { return _deviceNotificationManager; }
    }

    public DeviceType CurrentDeviceType
    {
        get
        {
            return _currentDeviceType;
        }
        set
        {
            _currentDeviceType = value;
        }
    }

    public Action<AttractionSessionStarted> OnCurrentSessionStarted;
    Action _profileUpdatedCallback;

    Juniverse.ClientLibrary.MainGame _mainGame;
    public Juniverse.ClientLibrary.MainGame MainGame
    {
        get
        {
            return _mainGame;
        }
        set
        {
            _mainGame = value;
        }
    }

    Juniverse.Model.GameData _gameData;
    public Juniverse.Model.GameData GameData
    {
        get
        {
            return _gameData;
        }
        set
        {
            _gameData = value;
        }
    }

    public string AttractionId
    {
        get
        {
            return _attractionId;
        }
        set
        {
            _attractionId = value;
        }
    }
    public string SubAttractionID
    {
        get
        {
            return _subAttractionId;
        }
        set
        {
            _subAttractionId = value;
        }
    }
    public string TerminalSubattractionId
    {
        get
        {
            return _terminalSubattractionId;
        }
        set
        {
            _terminalSubattractionId = value;
        }
    }

    void ProcessQueuedTasks()
    {
        int tickCount = Environment.TickCount;

        while (_rpcConnection.HasPendingCalls())
        {
            Debug.Log("Has pending call");
            while (Reactor.Loop.Enumerator().MoveNext())
            {
                Debug.Log("Next task");
            }

            if (Environment.TickCount - tickCount > 10000)
                break;
        }
    }

    public PlayerLevelData GetPlayerLevelData(int level)
    {
        foreach (var item in GameData.PlayerLevels)
        {
            if (item.Value.LevelNumber == level)
                return item.Value;
        }
        return null;
    }

    public string GetGameScene()
    {
        return SParams.GameScene;
    }

    public void FinishObjective()
    {
        // TODO add score here
        FinishObjective(_CurrentPlayerId, _CurrentObjectiveId, _CurrentSubSessionId, _AssignmentId, 10 , true, _CurrentObjectiveData);
    }

    public void SetCurrentSessionData(string SubSessionId, string PlayerId, string ObjectiveId, string AssignmentId, QuestObjectiveData CurrentObjectiveData)
    {
        _CurrentObjectiveData = CurrentObjectiveData;
        _CurrentSubSessionId = SubSessionId;
        _CurrentPlayerId = PlayerId;
        _CurrentObjectiveId = ObjectiveId;
        _AssignmentId = AssignmentId;

        _mainGame.GetPlayerProfileAsync(_CurrentPlayerId, (playerProfile) =>
        {
            _currentPlayer = playerProfile;
        });
    }

    public void ResetCurrentSessionData()
    {
        _CurrentObjectiveData = null;
        _CurrentSubSessionId = string.Empty;
        _CurrentPlayerId = string.Empty;
        _CurrentObjectiveId = string.Empty;
        _AssignmentId = string.Empty;
    }

    public void GetPlayerAbillityCards(string PlayerId, Action<Dictionary<string, InventoryItem>> callback)
    {
        MainGame.GetPlayerAbilityCardsForSubAttractionAsync(PlayerId, _subAttractionId, callback);
    }

    public void GetPlayerProfile(string PlayerId, Action<GamerProfile> callback)
    {
        MainGame.GetPlayerProfileAsync(PlayerId, callback);
    }

    public void GetPlayerAbillityCards(Action<Dictionary<string, InventoryItem>> callback)
    {
        MainGame.GetPlayerAbilityCardsForSubAttractionAsync(_CurrentPlayerId, _subAttractionId, callback);
    }

    public QuestObjectiveData GetCurrentObjectiveData()
    {
        return _CurrentObjectiveData;
    }

    public void ConsumeItem(string PlayerId, string ItemId)
    {
        MainGame.ConsumeItemAsync(PlayerId, ItemId);
    }

    public void ConsumeItem(string ItemId)
    {
        MainGame.ConsumeItemAsync(_CurrentPlayerId, ItemId);
    }

    public void UpdateAttractionSessionStarted(AttractionSessionStarted attractionSessionStarted)
    {
        CurrentSession.SessionStatus = AttractionSessionStatus.Started;
    }

    public double GetCurrentSessionTimeInSeconds()
    {
        return (CurrentSession.ExpectedFinishTime - CurrentSession.ExpectedStartTime).TotalSeconds;
    }

    void OnDestroy()
    {
        if (_rpcConnection != null)
        {

            Debug.Log("Ending session");
            if (CurrentDeviceType == DeviceType.Attraction)
            {
                MainGame.EndAttractionTerminalSessionAsync(_attractionId, () =>
                {


                    Debug.Log("Done session");

                    //   _rpcConnection.Close();
                }, (ex) =>
                {
                    Debug.LogException(ex);

                    Debug.Log("Error session");

                    // _rpcConnection.Close();
                });
            }
            if (CurrentDeviceType == DeviceType.SubAttraction)
            {
                MainGame.EndSubAttractionTerminalSessionAsync(_subAttractionId, () =>
                {


                    Debug.Log("Done session");

                    //   _rpcConnection.Close();
                }, (ex) =>
                {
                    Debug.LogException(ex);

                    Debug.Log("Error session");

                    // _rpcConnection.Close();
                });
            }

            if (CurrentDeviceType == DeviceType.Terminal)
            {
                MainGame.EndSubAttractionGameTerminalSessionAsync(_attractionId, _subAttractionId, _terminalSubattractionId, () =>
                {


                    Debug.Log("Done session");

                    //   _rpcConnection.Close();
                }, (ex) =>
                {
                    Debug.LogException(ex);

                    Debug.Log("Error session");

                    // _rpcConnection.Close();
                });
            }

            Debug.Log("Processing.");

            ProcessQueuedTasks();
        }
        DeviceNotificationManager.OnSessionEnded -= DeviceNotificationManager_OnSessionEnded;
        DeviceNotificationManager.OnSessionCanceled -= DeviceNotificationManager_OnSessionCanceled;
        DeviceNotificationManager.OnSessionStarted -= DeviceNotificationManager_OnSessionStarted;
        DeviceNotificationManager.OnAttractionPlayerCheckingWithoutReservation -= DeviceNotificationManager_OnAttractionPlayerCheckingWithoutReservation;
        if (CurrentDeviceType == DeviceType.Terminal)
            DeviceNotificationManager.OnPlayerAttended -= DeviceNotificationManager_OnPlayerAttended;
    }

    public void GetCurruntSessionFromServer(string attractionId, Action<AttractionSession> callBack)
    {
        MainGame.GetFirstStartedOrNotStartedAttractionSessionAsync(attractionId, "", (reply) =>
        {
            CurrentSession = reply;
            callBack(reply);
        }, (ex) => Debug.LogException(ex));

    }

    void rpcConnection_OnConnect(RPCConnection RpcConnection)
    {
        Debug.Log("Connected...");
        DeviceConnected = true;
        MainGame = new Juniverse.ClientLibrary.MainGame(RpcConnection);
        MainGame.OnLoggedOut += LoggedOutNoTification;

        DeviceNotificationManager.BindNotifications();
        Debug.Log("Fetching game data...");
        MainGame.GetGameDataAsync((gameData) =>
        {
            Debug.Log("Fetching game data. Done!");
            GameData = gameData;

        }, (ex) => Debug.LogException(ex));

        StartTerminalSession();
    }

    public string GetIPAddress()
    {

        // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface network in networkInterfaces)
        {
            // Read the IP configuration for each network 
            IPInterfaceProperties properties = network.GetIPProperties();

            // Each network interface may have multiple IP addresses 
            foreach (IPAddressInformation address in properties.UnicastAddresses)
            {
                // We're only interested in IPv4 addresses for now 
                if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                // Ignore loopback addresses (e.g., 127.0.0.1) 
                if (IPAddress.IsLoopback(address.Address))
                    continue;

                return address.Address.ToString();
            }
        }
        return string.Empty;
    }

    void rpcConnection_OnError(RPCConnection connection, Exception ex)
    {
        Debug.LogException(ex);
    }

    void rpcConnection_OnReconnect(RPCConnection rpcConnection)
    {
        Debug.LogWarning("Reconnecting...");
        if (CurrentDeviceType == DeviceType.Attraction)
            ServiceInterface.instance.AttractionReconnectServer(_attractionId, rpcConnection);
        else if (CurrentDeviceType == DeviceType.SubAttraction)
            ServiceInterface.instance.SubAttractionReconnectServer(_subAttractionId, rpcConnection);
        else if (CurrentDeviceType == DeviceType.Terminal)
            ServiceInterface.instance.SubAttractionGameTerminalReconnectServer(_attractionId, _subAttractionId, _terminalSubattractionId, rpcConnection);
    }

    void StartConnectionToServer()
    {
        ConnectToServer(ServerIP);
        DeviceNotificationManager.OnSessionEnded += DeviceNotificationManager_OnSessionEnded;
        DeviceNotificationManager.OnSessionCanceled += DeviceNotificationManager_OnSessionCanceled;

        DeviceNotificationManager.OnSessionStarted += DeviceNotificationManager_OnSessionStarted;
        DeviceNotificationManager.OnAttractionPlayerCheckingWithoutReservation += DeviceNotificationManager_OnAttractionPlayerCheckingWithoutReservation;
        if (CurrentDeviceType == DeviceType.Terminal)
            DeviceNotificationManager.OnPlayerAttended += DeviceNotificationManager_OnPlayerAttended;


    }

    void DeviceNotificationManager_OnAttractionPlayerCheckingWithoutReservation(AttractionPlayerCheckingWithoutReservation attractionPlayerCheckingWithoutReservation)
    {
        if (OnCurrentSessionStarted != null)
            OnCurrentSessionStarted(new AttractionSessionStarted { SessionId = attractionPlayerCheckingWithoutReservation.SessionId });
    }

    public string GetCurrentPlayerIdOnTerminal()
    {
        if (string.IsNullOrEmpty(_terminalSubattractionId))
            return string.Empty;

        foreach (var session in CurrentSession.SubSessions)
        {
            SubAttractionSessionSlot slot = session.Value.Slots.FirstOrDefault(x => x.AssignedTerminalId == _terminalSubattractionId);
            if (slot != null)
            {
                return slot.PlayerId;
            }

            slot = session.Value.ShoppingSlots.FirstOrDefault(x => x.AssignedTerminalId == _terminalSubattractionId);
            if (slot != null)
            {
                return slot.PlayerId;
            }
        }
        return string.Empty;
    }

    void LoggedOutNoTification(LoggedOut loggedOut)
    {
        DeviceConnected = false;
        AttractionSessionCreated = false;
        SubAttractionSessionCreated = false;
        GameTerminalAttractionSessionCreated = false;
    }

    void DeviceNotificationManager_OnPlayerAttended(Juniverse.Model.AttractionSlotPlayerAttended attractionSlotPlayerAttended, Juniverse.Model.QuestObjectiveData ObjectiveData, string PlayerAssignmentId)
    {
        SetCurrentSessionData(CurrentSession.SubSessions[attractionSlotPlayerAttended.SubSessionId].SessionId, attractionSlotPlayerAttended.PlayerId, ObjectiveData.Id, PlayerAssignmentId, ObjectiveData);
    }

    void DeviceNotificationManager_OnSessionStarted(AttractionSessionStarted attractionSessionStarted)
    {
        if (CurrentDeviceType != DeviceType.Terminal)
        {
            StartHost();
        }
        else
        {
            ResetCurrentSessionData();
            SParams.intialized = false;
            SParams.ResetAbilityCards();
        }

        if (OnCurrentSessionStarted != null)
            OnCurrentSessionStarted(attractionSessionStarted);
    }

    void Awake()
    {
        if (instance != null)
            return;
        instance = this;

        DontDestroyOnLoad(transform.gameObject);

        _deviceNotificationManager = GetComponent<DeviceNotificationManager>();

    }

    public void Init(string AttractionId, string SubAttractionId, string TerminalSubattractionId, string gameScene, string IP)
    {
        _attractionId = AttractionId;
        _subAttractionId = SubAttractionId;
        _terminalSubattractionId = TerminalSubattractionId;
        SParams = GetComponent<ServerParameters>();
        SParams.GameScene = gameScene;

        if (_attractionId != "" && _subAttractionId == "" && _terminalSubattractionId == "")
        {
            CurrentDeviceType = DeviceType.Attraction;

        }
        else if (_attractionId != "" && _subAttractionId != "" && _terminalSubattractionId == "")
        {
            CurrentDeviceType = DeviceType.SubAttraction;

        }
        else if (_attractionId != "" && _subAttractionId != "" && _terminalSubattractionId != "")
        {
            CurrentDeviceType = DeviceType.Terminal;
        }
        ServerIP = IP;
        StartConnectionToServer();
    }

    void DeviceNotificationManager_OnSessionEnded(AttractionSessionEnded attractionSessionEnded)
    {
        if (CurrentSession.SessionId == attractionSessionEnded.SessionId)
            SessionEnded();
    }

    void DeviceNotificationManager_OnSessionCanceled(AttractionSessionCanceled attractionSessionCanceled)
    {
        if (CurrentSession.SessionId == attractionSessionCanceled.SessionId)
            SessionEnded();
    }

    void SessionEnded()
    {
        if (CurrentDeviceType == DeviceType.Terminal)
            SceneManager.LoadScene("Intro");
        else
        {
            StopHost();
        }
    }

    void StartHost()
    {
        if (StartedHosting)
            return;

        JuniNetworkManager.Instance.networkPort = GameData.Configurations.ARGamePort;
        JuniNetworkManager.Instance.StartHost();
        StartedHosting = true;
    }

    void StopHost()
    {
        if (!StartedHosting)
            return;

        JuniNetworkManager.Instance.StopHost();
        StartedHosting = false;
    }

    public void StartClient()
    {
        if (StartedHosting)
            return;

        JuniNetworkManager.Instance.networkAddress = SubServerIP;
        JuniNetworkManager.Instance.networkPort = GameData.Configurations.ARGamePort;
        JuniNetworkManager.Instance.StartClient();
        StartedHosting = true;

        if (ServerParameters.Instance != null && ServerParameters.Instance.GetCard() != null)
            ConsumeItem(ServerParameters.Instance.GetCard().ItemId);
    }

    public void StopClient()
    {
        if (!StartedHosting)
            return;

        JuniNetworkManager.Instance.StopClient();
        StartedHosting = false;
    }

    public void ValidatePassCode(string CurrentPlayerId, string code, Action<bool> callBack)
    {
        MainGame.ValidateGameTerminalPlayerCodeAsync(CurrentPlayerId, code, CurrentSession.AttractionId, CurrentSession.SessionId, (reply) =>
        {
            callBack(reply);
        }, (ex) => Debug.LogException(ex));
    }

    public void ValidatePassCode(string code, Action<bool> callBack)
    {
        MainGame.ValidateGameTerminalPlayerCodeAsync(_CurrentPlayerId, code, CurrentSession.AttractionId, CurrentSession.SessionId, (reply) =>
        {
            callBack(reply);
        }, (ex) => Debug.LogException(ex));
    }

    public void GiveItemToPlayer(string CurrentPlayerId, string ItemId)
    {
        MainGame.GiveItemAsync(CurrentPlayerId, ItemId);
    }

    public void FinishObjective(string CurrentPlayerId, string ObjectiveId, string SubSessionId, string AssignmenetId, int Score, bool Passed, QuestObjectiveData ObjectiveData)
    {
        if (ObjectiveData.ObjectiveScoreNeededToPass)
            MainGame.FinishObjectiveAsync(CurrentPlayerId, AssignmenetId, ObjectiveId, SubSessionId, Score, Passed && Score >= ObjectiveData.ObjectivePassScore);
        else
            MainGame.FinishObjectiveAsync(CurrentPlayerId, AssignmenetId, ObjectiveId, SubSessionId, Score, Passed);
    }

    void Update()
    {
        StartCoroutine(Reactor.Loop.Enumerator());
    }

    public void ConnectToServer(string serverIP)
    {

        if (string.IsNullOrEmpty(serverIP))
            serverIP = "127.0.0.1";

        _rpcConnection = new RPCConnection(serverIP, 2101);
        _rpcConnection.AddCallableObject(_notifications);
        _rpcConnection.OnConnect += rpcConnection_OnConnect;
        _rpcConnection.OnReconnect += rpcConnection_OnReconnect;
        _rpcConnection.OnError += rpcConnection_OnError;
        _rpcConnection.OnEnd += _rpcConnection_OnEnd;
        _rpcConnection.OnRetry += _rpcConnection_OnRetry;
        _rpcConnection.OnLog += _rpcConnection_OnLog;
        _rpcConnection.Connect();
    }

    void _rpcConnection_OnLog(RPCConnection arg1, string arg2)
    {
        Debug.Log(arg2);
    }

    void _rpcConnection_OnRetry(RPCConnection obj)
    {
        Debug.LogWarning("Retrying connection");
    }

    void _rpcConnection_OnEnd(RPCConnection obj)
    {
        Debug.LogWarning("Connection end");
    }

    public void StartTerminalSession()
    {
        if (_attractionId != "" && _subAttractionId == "" && _terminalSubattractionId == "")
        {
            CurrentDeviceType = DeviceType.Attraction;
            StartAttractionTerminalSession(_attractionId);
        }
        else if (_attractionId != "" && _subAttractionId != "" && _terminalSubattractionId == "")
        {
            CurrentDeviceType = DeviceType.SubAttraction;
            StartSubAttractionTerminalSession(_subAttractionId);
        }
        else if (_attractionId != "" && _subAttractionId != "" && _terminalSubattractionId != "")
        {
            CurrentDeviceType = DeviceType.Terminal;
            StartSubAttractionGameTerminalSession(_attractionId, _subAttractionId, _terminalSubattractionId);
        }
    }

    public void StartAttractionTerminalSession(string terminalId)
    {
        if (DeviceConnected && !AttractionSessionCreated)
        {
            ServiceInterface.instance.StartAttractionTerminalSession(terminalId);

        }
    }

    public void StartSubAttractionTerminalSession(string subTerminalId)
    {
        if (DeviceConnected && !SubAttractionSessionCreated)
        {
            ServiceInterface.instance.StartSubAttractionTerminalSession(subTerminalId);

        }
    }

    public void StartSubAttractionGameTerminalSession(string attractionId, string subAttractionId, string subGameTerminalId)
    {
        if (DeviceConnected && !GameTerminalAttractionSessionCreated)
        {
            ServiceInterface.instance.StartSubAttractionGameTerminalSession(attractionId, subAttractionId, subGameTerminalId);

        }
    }

    public void GetPlayerNickName(string playerId, Action<string> callBack)
    {
        MainGame.GetNicknameAsync(playerId, (Playername) =>
        {
            callBack(Playername);
        }, (ex) => Debug.LogException(ex));
    }

    public void GivePlayerXP(int xp)
    {
        _mainGame.GiveXPAsync(_CurrentPlayerId, xp, _attractionId, _subAttractionId, _terminalSubattractionId);
    }

    public void UpdatePlayerProfile(Action callback)
    {
        _profileUpdatedCallback = callback;
        _mainGame.GetPlayerProfileAsync(_CurrentPlayerId, (profile) =>
        {
            _currentPlayer = profile;
            if (_profileUpdatedCallback != null)
                _profileUpdatedCallback();
        });

        _profileUpdatedCallback = null;
    }

    public void SubmitPlayerSkillPoints(Dictionary<SkillTreeBranch, SkillData> sp)
    {
        foreach (SkillData skill in sp.Values)
        {
            _mainGame.GiveSkillPointsAsync(_CurrentPlayerId, skill.SkillPoints, skill.Type);
        }
    }

}