using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class PixelDensityCamera : MonoBehaviour 
{
	public float pixelsToUnits = 100;

	// Update is called once per frame
	void Update () 
	{
		GetComponent<Camera>().orthographicSize = Screen.height / pixelsToUnits / 2;
	
	}
}
