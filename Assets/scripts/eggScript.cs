using UnityEngine;
using System.Collections;

public class eggScript : MonoBehaviour {

	float delayTillBaby = 7f;								// timer delay 
	float timer = 0f;
	Vector3 startlocation;

	public GameObject BabySnake;


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
			Instantiate (BabySnake, startlocation, Quaternion.identity);						// instantiate baby
			Destroy (gameObject);																// destroy egg after delay
		}
	}
}
