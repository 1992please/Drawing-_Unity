using UnityEngine;
using UnityEngine.UI;
public class LoadingPage : MonoBehaviour {
    public Image Loader;
    public float LoadingTime;
    // Use this for initialization
    private float Counter;
	void OnEnable () {
        Counter = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if(Counter <= LoadingTime)
        {
            Loader.fillAmount = Counter / LoadingTime;
            Counter += Time.deltaTime;
        }
    }
}
