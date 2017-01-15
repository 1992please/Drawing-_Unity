using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Juniverse.Model;
using UnityEngine.SceneManagement;
using System;

public enum ResourcesType { Power, Air, Food, Water, Gravity, None }

public enum GameEventType
{
    CellBroken, EnergySocketBroken, FacilityNonBuildable,
    DecrementResourceGeneration, FacilityNonDestructable,
    IncreaseResourceConsumption
}

[Serializable]
public class ServerResourceObject
{
    public ResourcesType Type;
    public bool IsDisabled;
    public float MinCapacity;
    public float InitialValue;

    [Header("Auto Consumption")]
    public bool IsAutoConsumable;
    public float ConsumptionRate;
    public float MinConsumptionRate;
    public float MaxConsumptionRate;
    public float ConsumptionTimeInterval;
}

[Serializable]
public class ServerResourceObjective
{
    public ResourcesType Type;
    public float Delay;
    public float Duration;
    public float Amount;
    public float Score;
}

[Serializable]
public class ServerEventData
{
    public GameEventType EventType;
    public float Delay;
    [Range(0, 100)]
    public float Chance;
    [Tooltip("Percentage of total game time")]
    [Range(0.0f, 1.0f)]
    public float EventFiringTimeLimit;
    public float CoolDownTime;
    [Tooltip("Only effective in Cell Broken, Energy Socket Broken")]
    public int Count;
    [Tooltip("Not effective in Cell Broken, Energy Socket Broken")]
    public ResourcesType ResourceType;
    [Tooltip("Only effective in Decrement Resource Generation")]
    [Range(-1, -0.1f)]
    public float GenerationRate;
    [Tooltip("Only effective in Increase Resource Consumption")]
    [Range(0.1f, 1)]
    public float ConsumptionRate;
}

[Serializable]
public class PresetFacility
{
    public ResourcesType Type;
    public int Count;
}



public enum CardType
{
    None,
    Shield,
    AsteroidSlicer,
    FuelIncrease
}

[System.Serializable]
public class Card
{

    public string CardType;

    public float Value;
    public string ItemId;
    public string GetCardtype()
    {
        return CardType;
    }
}

public class Blob
{
    public Dictionary<string, int> Variables;
    public int Difficulty;
    public string Level;
    public ServerResourceObjective[] Objectives;
    public PresetFacility[] PresetFacilities;

}

public class ObjectiveLevel
{
    public string PlayerId;
    public int Difficulty;
    public ServerResourceObjective[] Objectives;
}
public enum LevelDifficulty
{
    Easy,
    Medium,
    Hard
}
public class ServerParameters : MonoBehaviour
{
    static ServerParameters _instance;
    public static ServerParameters Instance { get { return ServerParameters._instance; } }

    [SerializeField]
    List<ServerResourceObject> _resources;
    [SerializeField]
    List<ServerResourceObjective> _objectives;
    [SerializeField]
    List<ServerEventData> _events;
    [SerializeField]
    List<PresetFacility> _presetFacilities;
    [SerializeField]
    int _initialEnergyBallsCount;
    [SerializeField]
    int _maxEnergyBallsCount;


    public bool[] MyDoneObjectives;
    public ServerResourceObjective[] MyObjectives;
    public List<ObjectiveLevel> AllPlayersObjectives = new List<ObjectiveLevel>();
    public Card[] Cards;
    public LevelDifficulty GameLevel;
    public Action<Dictionary<string, InventoryItem>> OnAbilityCardsRecievedFromServer;
    public Action<string> OnLevelRecievedFromServer;
    // Use this for initialization
    public Card GetCard(string cardType)
    {
        for (int i = 0; i < Cards.Length; i++)
        {
            if (Cards[i].CardType == cardType)
                return Cards[i];
        }
        return null;
    }

    public void FinishObjective(ServerResourceObjective obj)
    {
        for (int i = 0; i < MyObjectives.Length; i++)
        {
            if (MyObjectives[i].Amount == obj.Amount && MyObjectives[i].Delay == obj.Delay && MyObjectives[i].Duration == obj.Duration && MyObjectives[i].Score == obj.Score && MyObjectives[i].Type == obj.Type)
            {
                MyDoneObjectives[i] = true;
            }
        }
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
    public void SetDebugBlob(string _blob)
    {
        blob = JsonConvert.DeserializeObject<Blob>(_blob);
    }

    public void StartGame()
    {
    }
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    Blob blob;
    Dictionary<string, InventoryItem> items = new Dictionary<string, InventoryItem>();
    public bool intialized;
    public void DrawCards()
    {
        if (intialized)
            return;
        _CurrentCard = null;
        if (DeviceManager.instance != null)
        {
            //Get Ability Cards that can be used in current Attraction/SubAttraction/Terminal 
            DeviceManager.instance.GetPlayerAbillityCards((res) =>
            {
                print("items got: " + res.Count);
                items = new Dictionary<string, InventoryItem>();
                foreach (var item in res)
                {
                    bool found = false;
                    foreach (var resitem in items)
                    {
                        if (resitem.Value.ItemId == item.Value.ItemId)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        items.Add(item.Key, item.Value);
                    }

                }
                OnAbilityCardsRecievedFromServer(items);
                print("items got: " + items.Count);
            });
            blob = JsonConvert.DeserializeObject<Blob>(DeviceManager.instance.GetCurrentObjectiveData().AttractionDataBlob);
        }
        else
        {
            OnAbilityCardsRecievedFromServer(items);
        }
        if (blob.PresetFacilities != null)
        {
            _presetFacilities = new List<PresetFacility>(blob.PresetFacilities);
        }
        GameLevel = (LevelDifficulty)blob.Difficulty;
        OnLevelRecievedFromServer(blob.Level);
        intialized = true;
    }


    public Blob GetBlob()
    {
        return blob;
    }
    public Card GetCard()
    {
        return _CurrentCard;
    }
    Card _CurrentCard;
    public string GameScene;
    public string AdditiveSceneToLoad;
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


        else if (DeviceManager.instance != null && DeviceManager.instance.CurrentDeviceType != DeviceType.Terminal)
        {
            AllPlayersObjectives.Clear();
            return;
        }
    }
    public List<ServerResourceObjective> GetObjectives()
    {
        return _objectives;
    }
    public void SetObjectives(List<ServerResourceObjective> Objectives)
    {
        _objectives = Objectives;
        // if (SceneManager.GetActiveScene().name == GameplayUIManager.Scene_Gameplay)
        {
            // TODO look at this two lines
            //ResourcesManager.Instance.Initialize(_resources, _objectives);
            //EventsManager.Instance.Initialize(_events);
        }
    }
    public void CreateCurrentSessionObjectives()
    {
        if ((DeviceManager.instance != null && DeviceManager.instance.CurrentDeviceType != DeviceType.SubAttraction) || AllPlayersObjectives.Count == 0)
            return;

        AllPlayersObjectives.Sort((x, y) => x.Difficulty.CompareTo(y.Difficulty));

        int Capacity = DeviceManager.instance.GameData.SubAttractions[DeviceManager.instance.SubAttractionID].Capacity;

        List<ServerResourceObjective> TempObjectives = new List<ServerResourceObjective>();
        List<ObjectiveLevel> TempPlayersObjectives = new List<ObjectiveLevel>();

        int index = 0;
        while (TempObjectives.Count < Capacity && index < AllPlayersObjectives[0].Objectives.Length)
        {
            foreach (var item in AllPlayersObjectives)
            {
                if (TempPlayersObjectives.Exists(x => x.PlayerId == item.PlayerId))
                {
                    foreach (var tempitem in TempPlayersObjectives)
                    {
                        if (tempitem.PlayerId == item.PlayerId)
                        {
                            ServerResourceObjective[] NewT = new ServerResourceObjective[tempitem.Objectives.Length + 1];
                            for (int i = 0; i < tempitem.Objectives.Length; i++)
                            {
                                NewT[i] = tempitem.Objectives[i];
                            }
                            NewT[NewT.Length - 1] = item.Objectives[index];

                            tempitem.Objectives = NewT;
                            break;
                        }
                    }
                }
                else
                {
                    ServerResourceObjective[] NewT = new ServerResourceObjective[1];
                    NewT[0] = item.Objectives[index];
                    TempPlayersObjectives.Add(new ObjectiveLevel { Difficulty = item.Difficulty, PlayerId = item.PlayerId, Objectives = NewT });
                }
                TempObjectives.Add(item.Objectives[index]);

            }
            index++;
        }

        AllPlayersObjectives = TempPlayersObjectives;

        _objectives = TempObjectives;
    }


}
