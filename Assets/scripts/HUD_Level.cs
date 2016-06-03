using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD_Level : MonoBehaviour 
{
	//GameObject sceneManager;
	//SceneManager sceneManagerScript;
	
	Text text;

	// Use this for initialization
	void Start () 
	{
		//sceneManager = GameObject.Find("SceneManager");
		//sceneManagerScript = sceneManager.GetComponent<SceneManager>();
		
		text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
        text.text = "LEVEL: " + SceneManager.Instance.levelIndex;
		
	}
}
