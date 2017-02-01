using UnityEngine;
using System.Collections;
using AttractionAttractionDeviceManager;

public class ServerManager : MonoBehaviour {

    public string ServerIP = "127.0.0.1";


    public string _attractionId = "";
    public string _subAttractionId = "";
    public string _terminalSubattractionId = "";
    public string GameScene = "";

    public GameObject ServerConnection;
    public GameObject NetworkManager;
	// Use this for initialization
	void Start () {
	    
	}
    void Awake()
    {
        AttractionDeviceManager Manager = GameObject.FindObjectOfType<AttractionDeviceManager>();
        if (Manager == null)
        {
            GameObject net = null ;
            GameObject serverConnection;
			if(NetworkManager != null)
                net = Instantiate(NetworkManager);

            serverConnection = Instantiate(ServerConnection);
            serverConnection.GetComponent<AttractionDeviceManager>().Init(_attractionId, _subAttractionId, _terminalSubattractionId, GameScene,ServerIP);
            if(net)
            {
                net.transform.parent = serverConnection.transform;
            }

        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
