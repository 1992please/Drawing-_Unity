using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AttractionAttractionDeviceManager;
using Juniverse.Model;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject StartPage;
    [SerializeField]
    GameObject LoadingPage;
    [SerializeField]
    GameObject SignaturePage;
    [SerializeField]
    GameObject IntroductionPage;
    [SerializeField]
    GameObject GamePlayPageDrawing;


    bool bStart;
    private void Awake()
    {
        bStart = false;
    }

    private void Start()
    {
        if(!(StartPage && LoadingPage && SignaturePage && IntroductionPage && GamePlayPageDrawing))
        {
            Debug.LogError("Add All the required UI Pages to the GameManager");
            return;
        }

        if (AttractionDeviceManager.instance != null)
        {
            AttractionDeviceManager.instance.OnCurrentSessionStarted += DeviceNotificationManager_OnSessionStarted;
            AttractionDeviceManager.instance.OnCurrentSessionStatusChanged += DeviceNotificationManager_OnSessionStatusChanged;
        }

        StartPage.SetActive(true);
        //
    }

    void DeviceNotificationManager_OnSessionStarted(AttractionSessionStarted x)
    {
    }
    void DeviceNotificationManager_OnSessionStatusChanged(AttractionStatusChanged x)
    {
        switch(x.CurrentAttractionStatus)
        {
            case AttractionStatus.InSession:
                print("we are in Session now");
                bStart = true;
                break;
        }
    }

    private void Update()
    {
        if(bStart)
        {
            bStart = false;
            StartCoroutine("StartUp");
        }
    }

    IEnumerator StartUp()
    {
        StartPage.SetActive(false);
        LoadingPage.SetActive(true);
        yield return new WaitForSeconds(LoadingPage.GetComponent<LoadingPage>().LoadingTime);
        SignaturePage.SetActive(true);
        LoadingPage.SetActive(false);
    }

    public void OnClickLogin()
    {
        IntroductionPage.SetActive(true);
        SignaturePage.SetActive(false);
    }
}
