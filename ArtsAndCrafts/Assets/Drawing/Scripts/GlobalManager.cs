using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GlobalManager : MonoBehaviour {
    [SerializeField]
    private RawImage testImage;

    Texture2D OutputTex;

    public static GlobalManager singleton;

    private void Awake()
    {
        if (!singleton)
            singleton = this;
    }

    private void Update()
    {
        if(TcpDataTransfer.singlton.IsDataRecieved())
        {
            ObjectToTransfer obj = TcpDataTransfer.singlton.ReadDataAndClear();
            OutputTex = obj.GetTexture();
            print(obj.PlayerID);
            testImage.texture = OutputTex;
        }
    }
}
