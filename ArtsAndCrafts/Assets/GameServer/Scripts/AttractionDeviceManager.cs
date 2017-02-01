using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Juniverse.RPCLibrary;
using Juniverse.Notifications;
using Juniverse.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;


namespace AttractionAttractionDeviceManager
{
    public enum LevelDifficulty
    {
        Easy,
        Medium,
        Hard
    }
    public class BaseBlob
    {
        public Dictionary<string, int> Variables;
        public int Difficulty;
        public string Level;
    }

    [Serializable]
    public class BaseCard
    {

        public string CardType;

        public float Value;
        public string ItemId;
        public string GetCardtype()
        {
            return CardType;
        }
    }

    public class BaseCustomNetworkManager : NetworkManager
    {
        protected bool _isServer;
        public bool IsServer { get { return _isServer; } }

        static BaseCustomNetworkManager _instance;
        public static BaseCustomNetworkManager Instance { get { return _instance; } }


        void Awake()
        {
            if (_instance != null)
                return;
            _instance = this;
        }


        public override void OnStartHost()
        {
            base.OnStartHost();
            _isServer = true;
        }


        public override void OnClientDisconnect(NetworkConnection conn)
        {
            if (AttractionDeviceManager.instance)
            {
                AttractionDeviceManager.instance.StopClient();
                SceneManager.LoadScene("Intro");
            }
        }
        public virtual void StartGame()
        {


        }
        public virtual void StartJuniverseServer()
        {
            StartHost();
        }
        public virtual void StartJuniverseClient()
        {
            StartClient();
        }
        public virtual void StopJuniverseServer()
        {
            StopHost();

        }
        public virtual void StopJuniverseClient()
        {
            StopClient();
        }


    }

    public class BaseServerParameters : MonoBehaviour
    {
        protected static BaseServerParameters _instance;

        public string GameScene;
        public static BaseServerParameters Instance { get { return BaseServerParameters._instance; } }


        public BaseCard[] Cards;
        public LevelDifficulty GameLevel;
        public Action<Dictionary<string, InventoryItem>> OnAbilityCardsRecievedFromServer;
        public Action<string> OnLevelRecievedFromServer;
        // Use this for initialization
        public BaseCard GetCard(string cardType)
        {
            for (int i = 0; i < Cards.Length; i++)
            {
                if (Cards[i].CardType == cardType)
                    return Cards[i];
            }
            return null;
        }

        public void ResetAbilityCards()
        {
            items.Clear();
        }
        public Dictionary<string, InventoryItem> GetAbilityCards()
        {
            return items;
        }
        public void SetAbilityCards(Dictionary<string, InventoryItem> _cards)
        {
            items = _cards;
        }

        protected Dictionary<string, InventoryItem> items = new Dictionary<string, InventoryItem>();
        public bool intialized;
        public virtual void DrawCards()
        {

        }
        protected BaseCard _CurrentCard;
        public BaseCard GetCard()
        {
            return _CurrentCard;
        }
        public void UseCard(string itemId, string type)
        {
            _CurrentCard = GetCard(type);
        }
        void OnLevelWasLoaded(int level)
        {
            if (SceneManager.GetActiveScene().name == "Card Selection")
            {
                DrawCards();

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if (AttractionDeviceManager.instance != null && AttractionDeviceManager.instance.CurrentDeviceType != DeviceType.Terminal)
            {
                DoCustomIntializeStuff();
            }
        }
        protected virtual void DoCustomIntializeStuff()
        {

        }
    }

    public class ServiceInterface
    {
        // Use this for initialization
        public ServiceInterface()
        {
        }
        // Update is called once per frame
        public void StartAttractionTerminalSession(string attractionId)
        {

            AttractionDeviceManager.instance.MainGame.StartAttractionTerminalSessionAsync(attractionId, (attraction) =>
            {
                AttractionDeviceManager.instance.AttractionSessionCreated = true;
            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
        }
        public void StartSubAttractionTerminalSession(string subAttractionId)
        {
            AttractionDeviceManager.instance.MainGame.StartSubAttractionTerminalSessionAsync(subAttractionId, (Subattraction) =>
            {

                AttractionDeviceManager.instance.SubAttractionSessionCreated = true;
                AttractionDeviceManager.instance.MainGame.SetSubAttractionTerminalSessionHostAsync(subAttractionId, AttractionDeviceManager.instance.GetIPAddress());


            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
        }
        public void StartSubAttractionGameTerminalSession(string attractionId, string subattractionId, string terminalId)
        {

            AttractionDeviceManager.instance.MainGame.StartSubAttractionGameTerminalSessionAsync(attractionId, subattractionId, terminalId, (SubattractionGameTerminal) =>
            {
                AttractionDeviceManager.instance.GameTerminalAttractionSessionCreated = true;
                AttractionDeviceManager.instance.MainGame.GetSubAttractionTerminalSessionHostAsync(SubattractionGameTerminal.SubAttractionId, (ip) =>
                {

                    AttractionDeviceManager.instance.SubServerIP = ip;
                }, (ex) => AttractionDeviceManager.instance.ShowException(ex));


            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
        }

        public void AttractionReconnectServer(string attractionId, RPCConnection rpcConnection)
        {
            if (string.IsNullOrEmpty(attractionId))
            {
                rpcConnection.RetryPendingCalls();
            }
            else
            {
                AttractionDeviceManager.instance.MainGame.AttractionSessionReconnectedAsync(attractionId, () =>
                {
                    rpcConnection.RetryPendingCalls();

                }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
            }
        }
        public void SubAttractionReconnectServer(string subAttractionId, RPCConnection rpcConnection)
        {
            if (string.IsNullOrEmpty(subAttractionId))
            {
                rpcConnection.RetryPendingCalls();
            }
            else
            {

                AttractionDeviceManager.instance.MainGame.SubAttractionSessionReconnectedAsync(subAttractionId, () =>
                {
                    rpcConnection.RetryPendingCalls();

                }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
            }
        }
        public void SubAttractionGameTerminalReconnectServer(string attractionId, string subAttractionId, string terminalId, RPCConnection rpcConnection)
        {
            if (string.IsNullOrEmpty(terminalId))
            {
                rpcConnection.RetryPendingCalls();
            }
            else
            {
                AttractionDeviceManager.instance.MainGame.SubAttractionGameTerminalSessionReconnectedAsync(attractionId, subAttractionId, terminalId, () =>
                {
                    rpcConnection.RetryPendingCalls();

                }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
            }
        }
    }

    public class DeviceNotificationManager
    {
        public delegate void SessionStarted(AttractionSessionStarted attractionSessionStarted);
        public event SessionStarted OnSessionStarted;
        public delegate void SessionEnded(AttractionSessionEnded attractionSessionEnded);
        public event SessionEnded OnSessionEnded;
        public delegate void SessionCanceled(AttractionSessionCanceled attractionSessionCanceled);
        public event SessionCanceled OnSessionCanceled;

        public delegate void PlayerSlotAdded(AttractionPlayerSlotAdded attractionPlayerSlotAdded);
        public event PlayerSlotAdded OnSlotAdded;

        public delegate void PlayerSlotRemoved(AttractionPlayerSlotRemoved attractionPlayerSlotRemoved);
        public event PlayerSlotRemoved OnPlayerSlotRemoved;

        public delegate void PlayerSlotCancelled(AttractionSlotCanceled attractionSlotCanceled);
        public event PlayerSlotCancelled OnSlotCancelled;

        public delegate void SlotPlayerAttendedd(AttractionSlotPlayerAttended attractionSlotPlayerAttended, QuestObjectiveData ObjectiveData, string PlayerAssignmentId);
        public event SlotPlayerAttendedd OnPlayerAttended;

        public delegate void PlayerCheckingWithoutReservation(AttractionPlayerCheckingWithoutReservation attractionPlayerCheckingWithoutReservation);
        public event PlayerCheckingWithoutReservation OnAttractionPlayerCheckingWithoutReservation;

        public delegate void AttractionDeviceStatusChanged(AttractionStatusChanged attractionStatusChanged);
        public event AttractionDeviceStatusChanged OnAttractionDeviceStatusChanged;

        AttractionStatus CurrentStatus = AttractionStatus.PlayersLeaving;

        public DeviceNotificationManager()
        {

        }
        public void BindNotifications()
        {
            AttractionDeviceManager.instance.MainGame.OnAttractionSessionStarted += AttractionSessionStartedNotification;
            AttractionDeviceManager.instance.MainGame.OnAttractionSessionCanceled += AttractionSessionCanceledNotification;
            AttractionDeviceManager.instance.MainGame.OnAttractionSessionEnded += AttractionSessionEndedNotification;
            AttractionDeviceManager.instance.MainGame.OnAttractionPlayerSlotAdded += AttractionPlayerSlotAddedNotification;
            AttractionDeviceManager.instance.MainGame.OnAttractionPlayerSlotRemoved += AttractionPlayerSlotRemovedNotification;
            AttractionDeviceManager.instance.MainGame.OnAttractionSlotPlayerAttended += AttractionSlotPlayerAttendedNotification;
            AttractionDeviceManager.instance.MainGame.OnAttractionPlayerCheckingWithoutReservation += AttractionPlayerCheckingWithoutReservationNotification;
            AttractionDeviceManager.instance.MainGame.OnAttractionSlotCanceled += AttractionSlotCanceledNotification;
            AttractionDeviceManager.instance.MainGame.OnAttractionStatusChanged += AttractionStatusChangedNotification;
        }
        void AttractionSessionEndedNotification(AttractionSessionEnded attractionSessionEnded)
        {
            if (OnSessionEnded != null)
                OnSessionEnded(attractionSessionEnded);
        }
        void AttractionPlayerSlotRemovedNotification(AttractionPlayerSlotRemoved attractionPlayerSlotRemoved)
        {
            if (OnPlayerSlotRemoved != null)
                OnPlayerSlotRemoved(attractionPlayerSlotRemoved);

            if (AttractionDeviceManager.instance != null && AttractionDeviceManager.instance.CurrentSession != null)
            {
                SubAttractionSession subsession;
                if (AttractionDeviceManager.instance.CurrentSession.SubSessions.TryGetValue(attractionPlayerSlotRemoved.SubSessionId, out subsession))
                {
                    subsession.Slots.RemoveAll(x => x.ReservationId == attractionPlayerSlotRemoved.ReservationId);
                    subsession.ShoppingSlots.RemoveAll(x => x.ReservationId == attractionPlayerSlotRemoved.ReservationId);
                }
            }

        }
        void AttractionStatusChangedNotification(AttractionStatusChanged attractionStatusChanged)
        {
            if (OnAttractionDeviceStatusChanged != null)
                OnAttractionDeviceStatusChanged(attractionStatusChanged);

            CurrentStatus = attractionStatusChanged.CurrentAttractionStatus;
        }
        void AttractionSlotCanceledNotification(AttractionSlotCanceled attractionSlotCanceled)
        {
            if (OnSlotCancelled != null)
                OnSlotCancelled(attractionSlotCanceled);

            if (AttractionDeviceManager.instance != null && AttractionDeviceManager.instance.CurrentSession != null)
            {
                SubAttractionSession subsession;
                if (AttractionDeviceManager.instance.CurrentSession.SubSessions.TryGetValue(attractionSlotCanceled.SubSessionId, out subsession))
                {
                    subsession.Slots.RemoveAll(x => x.ReservationId == attractionSlotCanceled.ReservationId);
                    subsession.ShoppingSlots.RemoveAll(x => x.ReservationId == attractionSlotCanceled.ReservationId);
                }
            }
        }
        void AttractionPlayerCheckingWithoutReservationNotification(AttractionPlayerCheckingWithoutReservation attractionPlayerCheckingWithoutReservation)
        {
            AttractionDeviceManager.instance.MainGame.GetAttractionSessionAsync(AttractionDeviceManager.instance.AttractionId, attractionPlayerCheckingWithoutReservation.SessionId, (Session) =>
            {
                AttractionDeviceManager.instance.CurrentSession = Session;

                Juniverse.Model.SubAttractionSessionSlot slot = AttractionDeviceManager.instance.CurrentSession.SubSessions[AttractionDeviceManager.instance.SubAttractionID].Slots.Find(x => x.PlayerId == attractionPlayerCheckingWithoutReservation.UserId);
                if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Terminal && slot.AssignedTerminalId != AttractionDeviceManager.instance.TerminalSubattractionId)
                    return;

                if (OnAttractionPlayerCheckingWithoutReservation != null)
                    OnAttractionPlayerCheckingWithoutReservation(attractionPlayerCheckingWithoutReservation);

                if (OnPlayerAttended != null)
                {

                    AttractionDeviceManager.instance.MainGame.GetCurrentPlayerObjectiveAsync(slot.PlayerId, AttractionDeviceManager.instance.SubAttractionID, slot.AssignmentId, (reply) =>
                    {
                        OnPlayerAttended(new AttractionSlotPlayerAttended { PlayerId = attractionPlayerCheckingWithoutReservation.UserId, ReservationId = slot.ReservationId, SessionId = attractionPlayerCheckingWithoutReservation.SessionId, SubSessionId = AttractionDeviceManager.instance.SubAttractionID }, reply, slot.AssignmentId);
                    }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
                }
            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));


        }
        void AttractionSessionCanceledNotification(AttractionSessionCanceled attractionSessionCanceled)
        {
            if (OnSessionCanceled != null)
                OnSessionCanceled(attractionSessionCanceled);
        }
        void AttractionPlayerSlotAddedNotification(AttractionPlayerSlotAdded attractionPlayerSlotAdded)
        {
            if (OnSlotAdded != null)
                OnSlotAdded(attractionPlayerSlotAdded);
        }
        void AttractionSlotPlayerAttendedNotification(AttractionSlotPlayerAttended attractionSlotPlayerAttended)
        {
            Juniverse.Model.SubAttractionSessionSlot slot = AttractionDeviceManager.instance.CurrentSession.SubSessions[attractionSlotPlayerAttended.SubSessionId].Slots.Find(x => x.ReservationId == attractionSlotPlayerAttended.ReservationId);
            if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Terminal && slot.AssignedTerminalId != AttractionDeviceManager.instance.TerminalSubattractionId)
                return;

            if (OnPlayerAttended != null)
            {

                AttractionDeviceManager.instance.MainGame.GetCurrentPlayerObjectiveAsync(slot.PlayerId, AttractionDeviceManager.instance.SubAttractionID, slot.AssignmentId, (reply) =>
                {
                    OnPlayerAttended(attractionSlotPlayerAttended, reply, slot.AssignmentId);
                }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
            }

        }
        void AttractionSessionStartedNotification(AttractionSessionStarted attractionSessionStarted)
        {

            if (attractionSessionStarted.AttractionId != AttractionDeviceManager.instance.AttractionId)
                return;

            AttractionDeviceManager.instance.MainGame.GetAttractionSessionAsync(AttractionDeviceManager.instance.AttractionId, attractionSessionStarted.SessionId, (Session) =>
            {

                AttractionDeviceManager.instance.CurrentSession = Session;
                AttractionDeviceManager.instance.UpdateAttractionSessionStarted(attractionSessionStarted);

                if (OnSessionStarted != null)
                    OnSessionStarted(attractionSessionStarted);


            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
        }
    }

    public enum DeviceType
    {
        Attraction,
        SubAttraction,
        Terminal
    }
    public class AttractionDeviceManager : MonoBehaviour
    {

        RPCConnection _rpcConnection = null;
        NotificationManager _notifications = new NotificationManager();
        public static AttractionDeviceManager instance = null;
        public bool DeviceConnected = false;

        public bool AttractionSessionCreated = false;
        public bool SubAttractionSessionCreated = false;
        public bool GameTerminalAttractionSessionCreated = false;

        DeviceType _currentDeviceType;

        DeviceNotificationManager _deviceNotificationManager;

        string ServerIP = "127.0.0.1";


        string _attractionId;
        string _subAttractionId;
        string _terminalSubattractionId;

        GamerProfile _currentPlayer;
        public GamerProfile CurrentPlayer { get { return _currentPlayer; } }
        string _CurrentPlayerId;
        string _CurrentObjectiveId;
        string _AssignmentId;
        string _CurrentSubSessionId;

        QuestObjectiveData _CurrentObjectiveData;
        ServiceInterface Interface;


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
        public void FinishObjective(int Score)
        {
            AttractionDeviceManager.instance.FinishObjective(_CurrentPlayerId, _CurrentObjectiveId, _CurrentSubSessionId, _AssignmentId, Score, true, _CurrentObjectiveData);
        }
        public void FinishObjective(bool Passed, int Score)
        {
            AttractionDeviceManager.instance.FinishObjective(_CurrentPlayerId, _CurrentObjectiveId, _CurrentSubSessionId, _AssignmentId, Score, Passed, _CurrentObjectiveData);
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
            AttractionDeviceManager.instance.MainGame.GetPlayerAbilityCardsForSubAttractionAsync(PlayerId, _subAttractionId, callback);
        }
        public void GetPlayerProfile(string PlayerId, Action<GamerProfile> callback)
        {
            AttractionDeviceManager.instance.MainGame.GetPlayerProfileAsync(PlayerId, callback);
        }
        public void GetPlayerAbillityCards(Action<Dictionary<string, InventoryItem>> callback)
        {
            AttractionDeviceManager.instance.MainGame.GetPlayerAbilityCardsForSubAttractionAsync(_CurrentPlayerId, _subAttractionId, callback);
        }
        public QuestObjectiveData GetCurrentObjectiveData()
        {
            return _CurrentObjectiveData;
        }
        public void ConsumeItem(string PlayerId, string ItemId)
        {
            AttractionDeviceManager.instance.MainGame.ConsumeItemAsync(PlayerId, ItemId);
        }
        public void ConsumeItem(string ItemId)
        {
            AttractionDeviceManager.instance.MainGame.ConsumeItemAsync(_CurrentPlayerId, ItemId);
        }

        public void UpdateAttractionSessionStarted(AttractionSessionStarted attractionSessionStarted)
        {
            AttractionDeviceManager.instance.CurrentSession.SessionStatus = AttractionSessionStatus.Started;
        }

        public double GetCurrentSessionTimeInSeconds()
        {
            return (AttractionDeviceManager.instance.CurrentSession.ExpectedFinishTime - AttractionDeviceManager.instance.CurrentSession.ExpectedStartTime).TotalSeconds;
        }
        void OnDestroy()
        {
            if (_rpcConnection != null)
            {

                Debug.Log("Ending session");
                if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Attraction)
                {
                    AttractionDeviceManager.instance.MainGame.EndAttractionTerminalSessionAsync(_attractionId, () =>
                    {


                        Debug.Log("Done session");

                        //   _rpcConnection.Close();
                    }, (ex) =>
                    {
                        AttractionDeviceManager.instance.ShowException(ex);

                        Debug.Log("Error session");

                        // _rpcConnection.Close();
                    });
                }
                if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.SubAttraction)
                {
                    AttractionDeviceManager.instance.MainGame.EndSubAttractionTerminalSessionAsync(_subAttractionId, () =>
                    {


                        Debug.Log("Done session");

                        //   _rpcConnection.Close();
                    }, (ex) =>
                    {
                        AttractionDeviceManager.instance.ShowException(ex);

                        Debug.Log("Error session");

                        // _rpcConnection.Close();
                    });
                }

                if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Terminal)
                {
                    AttractionDeviceManager.instance.MainGame.EndSubAttractionGameTerminalSessionAsync(_attractionId, _subAttractionId, _terminalSubattractionId, () =>
                    {


                        Debug.Log("Done session");

                        //   _rpcConnection.Close();
                    }, (ex) =>
                    {
                        AttractionDeviceManager.instance.ShowException(ex);

                        Debug.Log("Error session");

                        // _rpcConnection.Close();
                    });
                }

                Debug.Log("Processing.");

                ProcessQueuedTasks();
            }
            AttractionDeviceManager.instance.DeviceNotificationManager.OnSessionEnded -= DeviceNotificationManager_OnSessionEnded;
            AttractionDeviceManager.instance.DeviceNotificationManager.OnSessionCanceled -= DeviceNotificationManager_OnSessionCanceled;
            AttractionDeviceManager.instance.DeviceNotificationManager.OnSessionStarted -= DeviceNotificationManager_OnSessionStarted;
            AttractionDeviceManager.instance.DeviceNotificationManager.OnAttractionDeviceStatusChanged -= DeviceNotificationManager_OnAttractionDeviceStatusChanged;
            AttractionDeviceManager.instance.DeviceNotificationManager.OnAttractionPlayerCheckingWithoutReservation -= DeviceNotificationManager_OnAttractionPlayerCheckingWithoutReservation;
            if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Terminal)
                AttractionDeviceManager.instance.DeviceNotificationManager.OnPlayerAttended -= DeviceNotificationManager_OnPlayerAttended;
        }
        public void GetCurruntSessionFromServer(string attractionId, Action<AttractionSession> callBack)
        {
            AttractionDeviceManager.instance.MainGame.GetFirstStartedOrNotStartedAttractionSessionAsync(attractionId, "", (reply) =>
            {
                AttractionDeviceManager.instance.CurrentSession = reply;
                callBack(reply);
            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));

        }
        public string SubServerIP = "";
        void rpcConnection_OnConnect(RPCConnection RpcConnection)
        {
            Debug.Log("Connected...");
            AttractionDeviceManager.instance.DeviceConnected = true;
            MainGame = new Juniverse.ClientLibrary.MainGame(RpcConnection);
            MainGame.OnLoggedOut += LoggedOutNoTification;

            DeviceNotificationManager.BindNotifications();
            Debug.Log("Fetching game data...");
            AttractionDeviceManager.instance.MainGame.GetGameDataAsync((gameData) =>
            {
                Debug.Log("Fetching game data. Done!");
                GameData = gameData;
                if (GameData.SubAttractions.ContainsKey(_subAttractionId))
                {
                    MultiPlayerGame = GameData.SubAttractions[_subAttractionId].MultiplayerGame;
                }

                if (ARGame)
                {

                    if (CurrentARSession != null)
                    {

                        CacheCurrentSession(CurrentARSession.AttractionSessionId);
                        AttractionDeviceManager.instance.SetCurrentSessionData(CurrentARSession.SubSessionId, CurrentARSession.PlayerId, CurrentARSession.ObjectiveId, CurrentARSession.AssignmentId, AttractionDeviceManager.instance.GameData.Quests[CurrentARSession.QuestId].Objectives.Find(x => x.Id == CurrentARSession.ObjectiveId));
                    }

                    if (CurrentAutoTerminalSession != null)
                    {

                        CacheCurrentSession(CurrentAutoTerminalSession.AttractionSessionId);
                        AttractionDeviceManager.instance.SetCurrentSessionData(CurrentAutoTerminalSession.SubSessionId, CurrentAutoTerminalSession.PlayerId, CurrentAutoTerminalSession.ObjectiveId, CurrentAutoTerminalSession.AssignmentId, AttractionDeviceManager.instance.GameData.Quests[CurrentAutoTerminalSession.QuestId].Objectives.Find(x => x.Id == CurrentAutoTerminalSession.ObjectiveId));
                    }

                }



            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));

            Debug.Log("rpcConnection_OnConnect");
            StartTerminalSession();


        }

        public void CacheCurrentSession(string SessionId)
        {

            AttractionDeviceManager.instance.MainGame.GetAttractionSessionAsync(AttractionDeviceManager.instance.AttractionId, SessionId, (Session) =>
            {
                AttractionDeviceManager.instance.CurrentSession = Session;


                if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Terminal && ARGame && GameData != null && AttractionDeviceManager.instance.MultiPlayerGame)
                {

                    StartClient();
                }
            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));

        }

        bool MultiPlayerGame = false;
        public string GetIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
        void rpcConnection_OnError(RPCConnection connection, Exception ex)
        {
            AttractionDeviceManager.instance.ShowException(ex);
        }
        void rpcConnection_OnReconnect(RPCConnection rpcConnection)
        {
            Debug.LogWarning("Reconnecting...");
            if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Attraction)
                Interface.AttractionReconnectServer(_attractionId, rpcConnection);
            else if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.SubAttraction)
                Interface.SubAttractionReconnectServer(_subAttractionId, rpcConnection);
            else if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Terminal)
                Interface.SubAttractionGameTerminalReconnectServer(_attractionId, _subAttractionId, _terminalSubattractionId, rpcConnection);
        }

        public void ShowException(Exception ex)
        {
            Debug.LogException(ex);
        }

        void StartConnectionToServer()
        {
            ConnectToServer(ServerIP);
            AttractionDeviceManager.instance.DeviceNotificationManager.OnSessionEnded += DeviceNotificationManager_OnSessionEnded;
            AttractionDeviceManager.instance.DeviceNotificationManager.OnSessionCanceled += DeviceNotificationManager_OnSessionCanceled;
            AttractionDeviceManager.instance.DeviceNotificationManager.OnAttractionDeviceStatusChanged += DeviceNotificationManager_OnAttractionDeviceStatusChanged;

            AttractionDeviceManager.instance.DeviceNotificationManager.OnSessionStarted += DeviceNotificationManager_OnSessionStarted;
            AttractionDeviceManager.instance.DeviceNotificationManager.OnAttractionPlayerCheckingWithoutReservation += DeviceNotificationManager_OnAttractionPlayerCheckingWithoutReservation;
            if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Terminal)
                AttractionDeviceManager.instance.DeviceNotificationManager.OnPlayerAttended += DeviceNotificationManager_OnPlayerAttended;


        }

        void DeviceNotificationManager_OnAttractionPlayerCheckingWithoutReservation(AttractionPlayerCheckingWithoutReservation attractionPlayerCheckingWithoutReservation)
        {
            if (OnCurrentSessionStarted != null)
                OnCurrentSessionStarted(new AttractionSessionStarted { SessionId = attractionPlayerCheckingWithoutReservation.SessionId });
        }
        public void GetPlayerIdFromValidationCode(string Code, Action<string> callBack)
        {
            AttractionDeviceManager.instance.MainGame.GetPlayerIdWithValidationCodeAsync(Code, (reply) =>
            {
                callBack(reply);
            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
        }

        public string GetCurrentPlayerIdOnTerminal()
        {
            if (string.IsNullOrEmpty(_terminalSubattractionId))
                return string.Empty;

            foreach (var session in AttractionDeviceManager.instance.CurrentSession.SubSessions)
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
            AttractionDeviceManager.instance.SetCurrentSessionData(AttractionDeviceManager.instance.CurrentSession.SubSessions[attractionSlotPlayerAttended.SubSessionId].SessionId, attractionSlotPlayerAttended.PlayerId, ObjectiveData.Id, PlayerAssignmentId, ObjectiveData);

            if (OnCurrentSessionPlayerAttended != null)
                OnCurrentSessionPlayerAttended(attractionSlotPlayerAttended);
        }
        public Action<AttractionSessionStarted> OnCurrentSessionStarted;
        public Action<AttractionStatusChanged> OnCurrentSessionStatusChanged;
        public Action<AttractionSlotPlayerAttended> OnCurrentSessionPlayerAttended;
        public Action<AttractionSessionEnded> OnCurrentSessionEnded;
        public Action<AttractionSessionCanceled> OnCurrentSessionCanceled;
        void DeviceNotificationManager_OnSessionStarted(AttractionSessionStarted attractionSessionStarted)
        {
            if (AttractionDeviceManager.instance.CurrentDeviceType != DeviceType.Terminal)
            {
                if (MultiPlayerGame)
                    StartHost();
            }
            else
            {
                AttractionDeviceManager.instance.ResetCurrentSessionData();
                SParams.intialized = false;
                SParams.ResetAbilityCards();
            }

            if (OnCurrentSessionStarted != null)
                OnCurrentSessionStarted(attractionSessionStarted);
        }
        BaseServerParameters SParams;
        void Awake()
        {
            if (instance != null)
                return;
            instance = this;

            DontDestroyOnLoad(transform.gameObject);

            _deviceNotificationManager = new DeviceNotificationManager();
            Interface = new ServiceInterface();

        }
        bool ARGame = false;
        public PlayerARSession CurrentARSession;

        public PlayerEnterAutoTerminalSession CurrentAutoTerminalSession;


        public void Init(string AttractionId, string SubAttractionId, string TerminalSubattractionId, string gameScene, string IP, bool IsARGame = false)
        {
            _attractionId = AttractionId;
            _subAttractionId = SubAttractionId;
            _terminalSubattractionId = TerminalSubattractionId;
            SParams = GetComponent<BaseServerParameters>();
            SParams.GameScene = gameScene;

            ARGame = IsARGame;

            if (_attractionId != "" && _subAttractionId == "" && _terminalSubattractionId == "")
            {
                AttractionDeviceManager.instance.CurrentDeviceType = DeviceType.Attraction;

            }
            else if (_attractionId != "" && _subAttractionId != "" && _terminalSubattractionId == "")
            {
                AttractionDeviceManager.instance.CurrentDeviceType = DeviceType.SubAttraction;

            }
            else if (_attractionId != "" && _subAttractionId != "" && _terminalSubattractionId != "")
            {
                AttractionDeviceManager.instance.CurrentDeviceType = DeviceType.Terminal;
            }
            ServerIP = IP;
            StartConnectionToServer();
        }
        bool StartedHosting = false;
        void DeviceNotificationManager_OnSessionEnded(AttractionSessionEnded attractionSessionEnded)
        {
            if (AttractionDeviceManager.instance.CurrentSession.SessionId == attractionSessionEnded.SessionId)
                SessionEnded();

            if (OnCurrentSessionEnded != null)
                OnCurrentSessionEnded(attractionSessionEnded);
        }

        void DeviceNotificationManager_OnSessionCanceled(AttractionSessionCanceled attractionSessionCanceled)
        {
            if (AttractionDeviceManager.instance.CurrentSession.SessionId == attractionSessionCanceled.SessionId)
                SessionEnded();

            if (OnCurrentSessionCanceled != null)
                OnCurrentSessionCanceled(attractionSessionCanceled);
        }
        void DeviceNotificationManager_OnAttractionDeviceStatusChanged(AttractionStatusChanged attractionStatusChanged)
        {
            if (OnCurrentSessionStatusChanged != null)
                OnCurrentSessionStatusChanged(attractionStatusChanged);
        }

        void SessionEnded()
        {
            if (ARGame)
            {
                if (BaseCustomNetworkManager.Instance != null)
                {
                    StopClient();
                    Destroy(BaseCustomNetworkManager.Instance.gameObject);
                }
                SceneManager.LoadScene("Profile");

            }
            else
            {
                if (AttractionDeviceManager.instance.CurrentDeviceType == DeviceType.Terminal)
                    SceneManager.LoadScene("Intro");
                else
                {
                    if (MultiPlayerGame)
                        StopHost();
                }
            }

        }
        void StartHost()
        {
            if (StartedHosting)
                return;

            BaseCustomNetworkManager.Instance.networkPort = GameData.Configurations.ARGamePort;
            BaseCustomNetworkManager.Instance.StartJuniverseServer();
            StartedHosting = true;
        }

        void StopHost()
        {
            if (!StartedHosting)
                return;

            BaseCustomNetworkManager.Instance.StopJuniverseServer();
            StartedHosting = false;
        }

        public void StartClient()
        {
            if (MultiPlayerGame)
            {
                StartCoroutine(DoStartClientWork());
            }
            else
            {
                SceneManager.LoadScene(GetGameScene());
            }



        }
        IEnumerator DoStartClientWork()
        {
            if (SubServerIP == "")
            {
                yield return new WaitForSeconds(2);
            }

            if (!StartedHosting)
            {

                BaseCustomNetworkManager.Instance.networkAddress = SubServerIP;
                BaseCustomNetworkManager.Instance.networkPort = GameData.Configurations.ARGamePort;
                BaseCustomNetworkManager.Instance.StartJuniverseClient();
                StartedHosting = true;
            }
        }

        public void StopClient()
        {
            if (MultiPlayerGame)
            {
                if (!StartedHosting)
                    return;

                BaseCustomNetworkManager.Instance.StopJuniverseClient();
                StartedHosting = false;
            }
        }

        public void ValidatePassCode(string CurrentPlayerId, string code, Action<bool> callBack)
        {
            AttractionDeviceManager.instance.MainGame.ValidateGameTerminalPlayerCodeAsync(CurrentPlayerId, code, AttractionDeviceManager.instance.CurrentSession.AttractionId, AttractionDeviceManager.instance.CurrentSession.SessionId, (reply) =>
            {
                callBack(reply);
            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
        }

        public void ValidatePassCode(string code, Action<bool> callBack)
        {
            AttractionDeviceManager.instance.MainGame.ValidateGameTerminalPlayerCodeAsync(_CurrentPlayerId, code, AttractionDeviceManager.instance.CurrentSession.AttractionId, AttractionDeviceManager.instance.CurrentSession.SessionId, (reply) =>
            {
                callBack(reply);
            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
        }

        public void GiveItemToPlayer(string CurrentPlayerId, string ItemId)
        {
            AttractionDeviceManager.instance.MainGame.GiveItemAsync(CurrentPlayerId, ItemId);
        }

        public void FinishObjective(string CurrentPlayerId, string ObjectiveId, string SubSessionId, string AssignmenetId, int Score, bool Passed, QuestObjectiveData ObjectiveData)
        {
            if (ObjectiveData.ObjectiveScoreNeededToPass)
                AttractionDeviceManager.instance.MainGame.FinishObjectiveAsync(CurrentPlayerId, AssignmenetId, ObjectiveId, SubSessionId, Score, Passed && Score >= ObjectiveData.ObjectivePassScore);
            else
                AttractionDeviceManager.instance.MainGame.FinishObjectiveAsync(CurrentPlayerId, AssignmenetId, ObjectiveId, SubSessionId, Score, Passed);
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
                AttractionDeviceManager.instance.CurrentDeviceType = DeviceType.Attraction;
                AttractionDeviceManager.instance.StartAttractionTerminalSession(_attractionId);
            }
            else if (_attractionId != "" && _subAttractionId != "" && _terminalSubattractionId == "")
            {
                AttractionDeviceManager.instance.CurrentDeviceType = DeviceType.SubAttraction;
                AttractionDeviceManager.instance.StartSubAttractionTerminalSession(_subAttractionId);
            }
            else if (_attractionId != "" && _subAttractionId != "" && _terminalSubattractionId != "")
            {
                AttractionDeviceManager.instance.CurrentDeviceType = DeviceType.Terminal;
                AttractionDeviceManager.instance.StartSubAttractionGameTerminalSession(_attractionId, _subAttractionId, _terminalSubattractionId);
            }
        }

        public void StartAttractionTerminalSession(string terminalId)
        {
            if (AttractionDeviceManager.instance.DeviceConnected && !AttractionDeviceManager.instance.AttractionSessionCreated)
            {
                Interface.StartAttractionTerminalSession(terminalId);

            }
        }
        public void StartSubAttractionTerminalSession(string subTerminalId)
        {

            if (AttractionDeviceManager.instance.DeviceConnected && !AttractionDeviceManager.instance.SubAttractionSessionCreated)
            {
                Interface.StartSubAttractionTerminalSession(subTerminalId);

            }
        }
        public void StartSubAttractionGameTerminalSession(string attractionId, string subAttractionId, string subGameTerminalId)
        {
            if (AttractionDeviceManager.instance.DeviceConnected && !AttractionDeviceManager.instance.GameTerminalAttractionSessionCreated)
            {
                Interface.StartSubAttractionGameTerminalSession(attractionId, subAttractionId, subGameTerminalId);

            }
        }
        public void GetPlayerNickName(string playerId, Action<string> callBack)
        {
            AttractionDeviceManager.instance.MainGame.GetNicknameAsync(playerId, (Playername) =>
            {
                callBack(Playername);
            }, (ex) => AttractionDeviceManager.instance.ShowException(ex));
        }

        public void GivePlayerXP(int xp)
        {
            _mainGame.GiveXPAsync(_CurrentPlayerId, xp, _attractionId, _subAttractionId, _terminalSubattractionId);
        }

        Action _profileUpdatedCallback;
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
}