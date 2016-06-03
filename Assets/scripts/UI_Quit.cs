using UnityEngine;
using System.Collections;

public class UI_Quit : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void OnApplicationQuit() 
	{
		//PlayerPrefs.Save();
		Application.Quit();
		Debug.Log ("quit game");
	}
}
