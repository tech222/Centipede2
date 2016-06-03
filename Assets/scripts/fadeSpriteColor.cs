using UnityEngine;
using System.Collections;

public class fadeSpriteColor : MonoBehaviour {

	SpriteRenderer spColor;
	public Color col1;
	public Color col2;
	float fg;

	void Awake ()
	{
		spColor = GetComponent<SpriteRenderer>();
	}

	void OnDisable ()
	{
		StopCoroutine("ChangeColor");
		fg = 0;
	}

	public void StartColor ()
	{
		StartCoroutine ("ChangeColor");
		//StopCoroutine ("ChangeColor");
		//fg = 0;
	}
	
	IEnumerator ChangeColor ()
	{
		while (spColor.color != col2)
		{
			spColor.color = Color.Lerp(col1, col2, fg);
			fg += Time.deltaTime;
			print (gameObject.name + ": color = " + spColor.color + ", fg = " + fg);
			yield return null;
		}
		fg = 0;
	}
}
