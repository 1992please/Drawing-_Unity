using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Juniverse.Model;
using UnityEngine.SceneManagement;
using System;
using AttractionAttractionDeviceManager;





public class Blob : AttractionAttractionDeviceManager.BaseBlob
{

}

public class ObjectiveLevel
{

}

public class ServerParameters : BaseServerParameters
{
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

    public void SetDebugBlob(string _blob)
    {
        blob = JsonConvert.DeserializeObject<Blob>(_blob);
    }
   
    void Start()
    {
        if (AttractionDeviceManager.instance != null)
            AttractionDeviceManager.instance.OnCurrentSessionStatusChanged += DeviceNotificationManager_OnCurrentSessionStatusChanged;
    }
    AttractionStatus CurrentStatus = AttractionStatus.PlayersLeaving;
    void DeviceNotificationManager_OnCurrentSessionStatusChanged(AttractionStatusChanged NewStatus)
    {
       
    }
    Blob blob;
    public override void DrawCards()
    {
        if (intialized)
            return;
        _CurrentCard = null;
        if(AttractionDeviceManager.instance != null)
        {
            //Get Ability Cards that can be used in current Attraction/SubAttraction/Terminal 
            AttractionDeviceManager.instance.GetPlayerAbillityCards((res) =>
            {
                print("items got: " + res.Count);
                items = new Dictionary<string, InventoryItem>();
                foreach (var item in res)
                {
                    bool found = false;
                    foreach (var resitem in items)
                    {
		                if(resitem.Value.ItemId == item.Value.ItemId)
                        {
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                    {
                        items.Add(item.Key,item.Value);
                    }
                    
                }
                OnAbilityCardsRecievedFromServer(items);
                print("items got: " + items.Count);
            });
            blob = JsonConvert.DeserializeObject<Blob>(AttractionDeviceManager.instance.GetCurrentObjectiveData().AttractionDataBlob);
        }
        else
        {
            OnAbilityCardsRecievedFromServer(items);
        }
        GameLevel = (LevelDifficulty)blob.Difficulty;
        OnLevelRecievedFromServer(blob.Level);
        intialized = true;
    }
    public Blob GetBlob()
    {
        return blob;
    }


}
