using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Juniverse.Model;
using UnityEngine.SceneManagement;
using AttractionAttractionDeviceManager;

public class TermainalValidationManager : MonoBehaviour
{

    public Text CodeField;
    public Text PlayerName;
	// Use this for initialization
    void Start()
    {
        if (AttractionDeviceManager.instance != null)
            AttractionDeviceManager.instance.OnCurrentSessionStarted += DeviceNotificationManager_OnSessionStarted;

    }
    void OnDestroy()
    {
        AttractionDeviceManager.instance.DeviceNotificationManager.OnSessionStarted -= DeviceNotificationManager_OnSessionStarted;
    }
    void DeviceNotificationManager_OnSessionStarted(AttractionSessionStarted attractionSessionStarted)
    {
        string PlayerId = AttractionDeviceManager.instance.GetCurrentPlayerIdOnTerminal();
        AttractionDeviceManager.instance.GetPlayerProfile(PlayerId, (profile) =>
            {
                PlayerName.text = profile.FullName;
            });
    }
    public void ValidateCode()
    {
        AttractionDeviceManager.instance.ValidatePassCode( CodeField.text, (reply) =>
        {
            if (reply)
            {
                SceneManager.LoadScene("GamePlayScene");
            }
            else
            {
                Debug.Log("Error Wrong Passcode");
            }
        });
    }
}
