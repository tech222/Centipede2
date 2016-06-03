using UnityEngine;
using System.Collections;

public class BoomScript : MonoBehaviour 
{
	public float radius = 5.0f;
	public int segments = 4;
	public float length = 1.0f;
	
	LineRenderer line;
	int starPoints;
	
	// Use this for initialization
	void Start () 
	{
		line = GetComponent<LineRenderer>();

		StartCoroutine (MakeStar());
	}
	
	
	
	IEnumerator MakeStar()
	{
		starPoints = 2*segments + 1;
		line.SetVertexCount (starPoints);
		line.useWorldSpace = false;
		
		
		float x;
		float y;
		float z = 0f;
		
		float angle = 0f;
		
		int i = 0;
		while (i <= starPoints-1)
		{
			angle += (360f / (starPoints-1));
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * (radius);
			y = Mathf.Cos (Mathf.Deg2Rad * angle) * (radius);
			
			line.SetPosition (i, new Vector3(x,y,z) );
			Debug.Log ("angle = " + angle);
			Debug.Log ("i = " + i);
			
			i++;
			if (i > starPoints-1)
				break;
			
			angle += (360f / (starPoints-1));
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * (radius + length);
			y = Mathf.Cos (Mathf.Deg2Rad * angle) * (radius + length);
			
			line.SetPosition (i, new Vector3(x,y,z) );
			Debug.Log ("angle = " + angle);
			Debug.Log ("i = " + i);
			
			i++;
			if (i > starPoints-1)
				break;
		}
		yield return null;
		StartCoroutine (MakeStar());
	}
	
	void CreatePoints () 
	{
		line.SetVertexCount (segments + 1);
		line.useWorldSpace = false;
		
		float x;
		float y;
		float z = 0f;
		
		float angle = 0f;
		for (int i = 0; i < (segments + 1); i++)
		{
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
			y = Mathf.Cos (Mathf.Deg2Rad * angle) * radius;
			
			line.SetPosition (i, new Vector3(x,y,z) );
			
			angle += (360f / segments);
		}
		return;
	}
}
