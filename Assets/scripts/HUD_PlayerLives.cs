using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD_PlayerLives : MonoBehaviour 
{
	//GameObject sceneManager;
	//SceneManager sceneManagerScript;
	
	Text text;
    int lives;

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
        lives = SceneManager.Instance.livesCount;
		text.text = "LIVES: " + lives + "\nTIME: " + (int)Time.timeSinceLevelLoad + "\nENEMIES: " + SceneManager.Instance.getEnemyCount();
		//text.text = "Time: " + (int)Time.timeSinceLevelLoad;
	}
}
