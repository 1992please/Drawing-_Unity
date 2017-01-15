using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GlobalManager : NetworkBehaviour {
    [SerializeField]
    private GameObject[] ClientObjects;
    [SerializeField]
    private GameObject[] ServerObjects;
    [SerializeField]
    private RawImage testImage;

    Texture2D OutputTex;

    public static GlobalManager singleton;

    private void Awake()
    {
        if (!singleton)
            singleton = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        print("Im server");
        foreach (GameObject obj in ServerObjects)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in ClientObjects)
        {
            obj.SetActive(false);
        }
    }

    private void Update()
    {
        if(TcpDataTransfer.singlton.IsDataRecieved())
        {
            OutputTex = new Texture2D(2, 2);
            OutputTex.LoadImage(TcpDataTransfer.singlton.ReadDataAndClear());
            print(OutputTex.height);
            testImage.texture = OutputTex;
        }
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        print("Im client");
        foreach (GameObject obj in ServerObjects)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in ClientObjects)
        {
            obj.SetActive(true);
        }
    }

    public void SetTexture(int width, int height, TextureFormat format, bool mipmap)
    {
        OutputTex = new Texture2D(width, height, format, mipmap);
    }
}
