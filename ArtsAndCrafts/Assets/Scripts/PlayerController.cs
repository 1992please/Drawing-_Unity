using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    private bool bSentData;
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        TcpDataTransfer.singlton.OnDataComepletelySent += OnSent;

        if (GlobalDraw.singleton)
            GlobalDraw.singleton.OnClickSendImage += SendPhoto;
    }

    [Client]
    public void SendPhoto(Texture2D InTexture)
    {
        //TextureUploader.UploadTexture2D(InTexture);
        if(TcpDataTransfer.singlton.SendData(InTexture.EncodeToPNG()))
        {
            //CmdShowTexture(InTexture.width, InTexture.height, InTexture.format, InTexture.mipmapCount > 1);
            print("Sent");
        }
    }

    public void OnSent()
    {

    }

    [Command]
    void CmdShowTexture(int width, int height, TextureFormat format, bool mipmap)
    {
       // GlobalManager.singleton.
    }
}
