using UnityEngine;
using System.Collections;

public class UI_Start : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void OnApplicationStart() 
	{
		//PlayerPrefs.Save();
		Application.LoadLevel(1);
		Debug.Log ("start game");
	}
}
