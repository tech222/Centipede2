using UnityEngine;
using System.Collections;

public class enemyEggScript : MonoBehaviour {

	float delayTillBaby = 7f;								// timer delay 
	float timer = 0f;
	Vector3 startlocation;

	public GameObject enemySnake;

	// Use this for initialization
	void Start () 
	{
		startlocation = transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime;
		if (timer > delayTillBaby)
		{
			GameObject newEnemy = (GameObject) Instantiate (enemySnake, startlocation, Quaternion.identity);				// instantiate enemy
			newEnemy.name = "enemySnake_" + SceneManager.Instance.getEnemyCount();
			EnemyNavScript enemyScript = newEnemy.GetComponentInChildren<EnemyNavScript>();
			enemyScript.initSegments = 2;														// start baby with 2 segments
			Destroy (gameObject);																// destroy egg after delay
		}
	
	}

}
