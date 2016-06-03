using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD_EnemyLives : MonoBehaviour 
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
        //lives = SceneManager.Instance.livesCount;
		text.text = SceneManager.Instance.getEnemyCount().ToString();
		//text.text = "Time: " + (int)Time.timeSinceLevelLoad;
	}
}
