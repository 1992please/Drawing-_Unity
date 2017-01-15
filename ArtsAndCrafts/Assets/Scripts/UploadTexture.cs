using System.Collections;
using UnityEngine;
using System;

[Serializable]
public class ReplyMessage
{
    public string ExternalFilePath;
    public string InternalFilePath;
    public string CreationDate;
    public object Message;
}

public class UploadTexture : MonoBehaviour {
    public string screenShotURL = "http://172.16.1.27:666/api/file";
    public event Action<ReplyMessage> OnDataComepletelySent;

    // Use this for initialization
    void Start()
    {

    }

    public void UploadTexture2D(Texture2D texture)
    {
        StartCoroutine(UploadPNGRoutine(texture));
    }

    Texture2D TakeScreenShot()
    {
        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        return tex;
    }

    IEnumerator UploadPNGRoutine(Texture2D tex)
    {
        // We should only read the screen after all rendering is complete
        yield return new WaitForEndOfFrame();

        if(!tex)
        {
            tex = TakeScreenShot();
        }


        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        // Create a Web Form
        WWWForm form = new WWWForm();

        form.AddBinaryData("fileUpload", bytes, "screenShot.png", "image/png");

        var Headers = form.headers;

        Headers.Add("sessionID", "123");
        Headers.Add("attractionID", "123");
        Headers.Add("subAttractionID", "123");

        // Upload to a cgi script
        WWW w = new WWW(screenShotURL, form.data, Headers);
   
        yield return w;
        if (!string.IsNullOrEmpty(w.error))
        {
            print(w.error);
        }
        else
        {
            TakeJson(w.text);
        }
    }

    void TakeJson(string JsonString)
    {
        ReplyMessage msg = JsonUtility.FromJson<ReplyMessage>(JsonString);
        print("WWW Ok: " + msg.InternalFilePath);
        if (OnDataComepletelySent != null)
            OnDataComepletelySent(msg);
    }
}
