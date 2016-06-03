	using UnityEngine;
using System.Collections;

public class playerHome2 : MonoBehaviour {

	GameObject playerBabyGate;
	GameObject movingHead;
	GameObject[] headList = new GameObject[100];
	BabyHeadScript babyScript;

	Vector3 startMovePos;
	Vector3 startGatePos;
	Vector3 initHeadPos;

	GameObject playerHomePrefab;
	SceneManager sceneManagerScript;
	
    HeadScript headScript;

    bool flag = false;
	bool isHead = false;
	int headListindex = 0;
    
    float xx;

	// Use this for initialization
	void Awake () 
    {

    }
	
	void OnTriggerEnter2D(Collider2D coll)
	{
		//print (gameObject.name + "home hit" + coll.gameObject.name);
		if (coll.gameObject.tag == "baby")
		{
            babyScript = coll.gameObject.GetComponent<BabyHeadScript>();
			babyScript.SwitchTarget();
		}

		if (coll.gameObject.tag == "head")
		{
			headScript = coll.GetComponent<HeadScript>();
			if (headScript.getGoingHome())
			{
                headScript.SwitchTarget();
			}	
		}
	}
}
		