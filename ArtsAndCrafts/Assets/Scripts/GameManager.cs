using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        if(!(StartPage && LoadingPage && SignaturePage && IntroductionPage && GamePlayPageDrawing))
        {
            Debug.LogError("Add All the required UI Pages to the GameManager");
            return;
        }
        StartCoroutine(StartUp());
    }

    IEnumerator StartUp()
    {
        StartPage.SetActive(true);
        yield return new WaitForSeconds(2);
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
