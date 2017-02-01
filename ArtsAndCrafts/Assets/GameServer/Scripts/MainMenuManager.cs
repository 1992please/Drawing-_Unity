using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void OpenServerScene()
    {
        SceneManager.LoadScene("ServerScene");
    }

    public void OpenTerminalScene()
    {
        SceneManager.LoadScene("Intro");
    }
}
