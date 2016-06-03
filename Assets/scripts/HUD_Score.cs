using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD_Score : MonoBehaviour 
{
	//GameObject sceneManager;
	//SceneManager sceneManagerScript;
	
	Text text;
    int score;

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
		score = SceneManager.score;
		text.text = "SCORE: " + score;
		
	}
}
