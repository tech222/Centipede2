using UnityEngine;
using System.Collections;

public class frogMoveScript : MonoBehaviour {
	
	Vector3 currLocation  = new Vector3 ();
	Vector3 newLocation  = new Vector3 ();	

	Quaternion currAngle = new Quaternion ();
	Quaternion newAngle =  new Quaternion ();

	int rndJump;
	int rndDirection;
	int jumpDirection;
	int jumpLength;
	int frogMove = 0;											// favored direction for frog to attack egg
	Vector3 displacement;										// displacement between frog and egg
	float magnitude;											// magnitude of displacement

	// screen bounds
	float xMin = -21f;
	float xMax = 21f;
	float yMin = -18f;
	float yMax = 18f;

	public float speed = 1f;
	public float delay = 0.5f;									// timer delay (higher = slower)
	
	bool delayTrigger = true;									// timer delay trigger
	Vector3[] hopSpots = new Vector3[12];						// potential landing locations for frog
	Vector3 eggLoc = new Vector3();								// potential egg location for frog to eat
	int hopIndex = 0;
	bool eggCheck = false;
	bool isOkay = false;
	bool moving = false;
	Quaternion angle;
    float frogOffTimer = 0f;
    GameObject frogEater;

	frogColliderScript[] frogCollScriptE = new frogColliderScript[3];			// frogColliderScript within frogCollider gameObject
	frogColliderScript[] frogCollScriptN = new frogColliderScript[3];
	frogColliderScript[] frogCollScriptW = new frogColliderScript[3];
	frogColliderScript[] frogCollScriptS = new frogColliderScript[3];

	GameObject[] collEast = new GameObject[3];
	GameObject[] collNorth = new GameObject[3];
	GameObject[] collWest = new GameObject[3];
	GameObject[] collSouth = new GameObject[3];
	
	public GameObject frogHitGO;								// frog after being hit gameobject
	public GameObject frogSplosion;								// frog explosion particle effect gameobject


	// Use this for initialization
	void Awake() {

		newLocation = transform.position;						//new position
		newAngle = transform.rotation;							//new roatation

		currLocation = transform.position;						//current position
		currAngle = transform.rotation;							//current rotation

		//frogColl = Instantiate(frogCollider, transform.position + Vector3(2f,0f,0f), transform.rotation) as GameObject;
		//frogColl.transform.parent = this.transform;
		int bb = 0;
		for (int aa = 0; aa < 3; aa++)
		{
			collSouth[bb] = gameObject.transform.GetChild(aa).gameObject; 
			//collSouth[aa].transform.position = transform.position + new Vector3(0,-2*(1+aa),0);
			frogCollScriptS[bb] = collSouth[aa].GetComponent<frogColliderScript>();
			
			collEast[bb] = gameObject.transform.GetChild(aa + 3).gameObject; 
			//collEast[aa].transform.position = transform.position + new Vector3(2*(1+aa),0,0);
			frogCollScriptE[bb] = collEast[aa].GetComponent<frogColliderScript>();

			collNorth[bb] = gameObject.transform.GetChild(aa + 6).gameObject; 
			//collNorth[aa].transform.position = transform.position + new Vector3(0,2*(1+aa),0);
			frogCollScriptN[bb] = collNorth[aa].GetComponent<frogColliderScript>();

			collWest[bb] = gameObject.transform.GetChild(aa + 9).gameObject; 
			//collWest[aa].transform.position = transform.position + new Vector3(-2*(1+aa),0,0);
			frogCollScriptW[bb] = collWest[aa].GetComponent<frogColliderScript>();
			
			bb++;
			//Debug.Log ("collider east #" + aa + ", vector3 = " + collEast[aa].transform.position);
		}
        StartCoroutine (newFrog ());
	}

	void CheckColliders()
	{
		eggCheck = false;
		hopIndex = 0;
		for (int aa=0; aa < 3; aa++)
		{	
			if (frogCollScriptE[aa].getWallCheck() == false)								// check for wall hit
			{
				if (InsideBorder(collEast[aa].transform))									// check if inside playing field
				{
					if (frogCollScriptE[aa].getEggCheck())	{								// check if egg at location
						eggCheck = true;
						eggLoc = collEast[aa].transform.position;
					}
					hopSpots[hopIndex] =  collEast[aa].transform.position;					// if okay add to hopSpots
					hopIndex++;
				}
			}

			if (frogCollScriptN[aa].getWallCheck() == false)
			{
				if (InsideBorder(collNorth[aa].transform))
				{
					if (frogCollScriptN[aa].getEggCheck()){
						eggCheck = true;
						eggLoc = collNorth[aa].transform.position;
					}
					hopSpots[hopIndex] =  collNorth[aa].transform.position;
					hopIndex++;
				}
			}

			if (frogCollScriptW[aa].getWallCheck() == false)
			{
				if (InsideBorder(collWest[aa].transform))
				{
					if (frogCollScriptW[aa].getEggCheck())	{
						eggCheck = true;
						eggLoc = collWest[aa].transform.position;
					}
					hopSpots[hopIndex] =  collWest[aa].transform.position;
					hopIndex++;
				}
			}

			if (frogCollScriptS[aa].getWallCheck() == false)
			{
				if (InsideBorder(collSouth[aa].transform))
				{
					if (frogCollScriptS[aa].getEggCheck())	{
						eggCheck = true;
						eggLoc = collSouth[aa].transform.position;
					}
					hopSpots[hopIndex] =  collSouth[aa].transform.position;
					hopIndex++;
				}
			}
		}
	}

	bool InsideBorder(Transform loc)
	{
		Vector3 location = loc.TransformPoint(0f,0f,0f);							// transform location coord to world coord
		//Debug.Log (loc.name + " world location = " + location);
		if (location.x >= xMax || location.x <= xMin || location.y >= yMax || location.y <= yMin)
			isOkay = false;
		else 
			isOkay = true;
		
		return (isOkay);
	}

	// Update is called once per frame
	void Update () 
	{
		if ((delayTrigger == true) & (GetComponent<Renderer>().enabled))
		{
			currLocation = transform.position;
			if (!moving)
			{
				
				CheckColliders();
				rndJump = Random.Range(0, hopIndex);
				
				if (eggCheck) 
				{
					newLocation = eggLoc;
				}
				else
				{
					newLocation = hopSpots[rndJump];
				}
			}
			Vector3 offset = newLocation - transform.position;
			//print ("frog offset = " + offset);
			
			if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
			{
				if (offset.x > 0)
                    transform.rotation = Quaternion.Euler(0,0,-90);  //new Quaternion(0, 0, -0.7071068f, 0.7071068f);
				else if (offset.x < 0)
                    transform.rotation = Quaternion.Euler(0,0,90); //Quaternion.LookRotation(-transform.right, transform.up); //new Quaternion(0, 0, 0.7071068f, 0.7071068f);
			}
			else
			{
				if (offset.y > 0) 
                    transform.rotation = Quaternion.Euler(0,0,0); //Quaternion.LookRotation(transform.up, transform.up); //new Quaternion(0,0,0f,1f);
				else if (offset.y < 0)
                    transform.rotation = Quaternion.Euler(0,0,180); //Quaternion.LookRotation(-transform.up, transform.up); //new Quaternion(0,0,1f,0f);
			}
			
		
			if (offset.magnitude > 0f) 
			{
				StartCoroutine(playClip());
			}
			StartCoroutine (MoveFrog());


			for (int aa=0; aa < 3; aa++)	{								// reset wall & egg checks on collider empties
				frogCollScriptE[aa].setWallCheck(false);
				frogCollScriptE[aa].setEggCheck(false);

				frogCollScriptN[aa].setWallCheck(false);
				frogCollScriptN[aa].setEggCheck(false);

				frogCollScriptW[aa].setWallCheck(false);
				frogCollScriptW[aa].setEggCheck(false);

				frogCollScriptS[aa].setWallCheck(false);
				frogCollScriptS[aa].setEggCheck(false);
			}
			eggCheck = false;
		
			StartCoroutine(DelayTimer());
		}
        
        /*
        if (renderer.enabled == false & collider2D.enabled == false)
        {
            frogOffTimer += Time.deltaTime;
            if (frogOffTimer > 12f)
            {
                StartCoroutine (newFrog());
                frogOffTimer = 0f;
            }
        }
        */
	}
	
	IEnumerator MoveFrog()
	{
		moving = true;
		float jump = 0;
		while (jump < 1f)
		{
			transform.position = Vector2.Lerp (transform.position, newLocation, jump);
			jump += Time.deltaTime * speed;
			yield return null;
		}
		transform.position = newLocation;
		yield return new WaitForSeconds (Random.Range(1f, 3f));
		
		moving = false;
	}
	
	IEnumerator playClip()
	{
		if(!GetComponent<AudioSource>().isPlaying)
		{
			GetComponent<AudioSource>().Play();
			yield return new WaitForSeconds(GetComponent<AudioSource>().clip.length);
			GetComponent<AudioSource>().Stop();
		}
	}

	IEnumerator DelayTimer() 
	{
		delayTrigger = false;
		yield return new WaitForSeconds(delay); 				// waits x seconds
		delayTrigger = true; 								    // will make the update method pick up 
	}

	public IEnumerator newFrog () 
	{
		GetComponent<Renderer>().enabled = false;
		GetComponent<Collider2D>().enabled = false;
		GetComponent<AudioSource>().enabled = false;
        float rndWait = Random.Range(4f, 12f);
        Debug.Log ("frog renderer inactive, waiting time = " + rndWait);
        Debug.Log ("frog current time " + Time.timeSinceLevelLoad + ", new frog at " + (Time.timeSinceLevelLoad + rndWait));
        yield return new WaitForSeconds(rndWait);
		GetComponent<Renderer>().enabled = true;
		GetComponent<Collider2D>().enabled = true;
		GetComponent<AudioSource>().enabled = true;
		
		// random postions x,y and coin flip
		int rndPositionX = Random.Range(-21, 22);
		int rndPositionY = Random.Range(-18, 19);
		int coinToss = Random.Range (1, 5);
		
		switch (coinToss)
		{
		case 1:
			//frog starts at top of screen
			transform.position = new Vector2 ((float)rndPositionX, yMax);
			break;
		case 2:
			//from starts at right side of screen
			transform.position = new Vector2 (xMax, (float)rndPositionY);
			break;
		case 3:
			//frog starts at bottom of screen
			transform.position = new Vector2 ((float)rndPositionX, yMin);
			break;
		case 4:
			//from starts at left side of screen
			transform.position = new Vector2 (xMin, (float)rndPositionY);
			break;
		}
		
		newLocation = transform.position;						//new position
		newAngle = transform.rotation;							//new roatation
		
		currLocation = transform.position;						//current position
		currAngle = transform.rotation;							//current rotation
		
		for (int aa=0; aa < 3; aa++)	
        {								
			frogCollScriptE[aa].setWallCheck(false);
			frogCollScriptE[aa].setEggCheck(false);
			
			frogCollScriptN[aa].setWallCheck(false);
			frogCollScriptN[aa].setEggCheck(false);
			
			frogCollScriptW[aa].setWallCheck(false);
			frogCollScriptW[aa].setEggCheck(false);
			
			frogCollScriptS[aa].setWallCheck(false);
			frogCollScriptS[aa].setEggCheck(false);
		}
		eggCheck = false;
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		if ((coll.gameObject.tag == "egg") || (coll.gameObject.tag == "enemyEgg"))
		{
			Destroy(coll.gameObject);
		} 
		
		if (coll.gameObject.tag == "enemy" || coll.gameObject.tag == "head" || coll.gameObject.tag == "baby")
		{
			Vector3 collisionPoint = transform.position + 0.5f*(coll.transform.position - transform.position);
			Instantiate(frogSplosion, collisionPoint, Quaternion.identity);
			float angle = Mathf.Atan2(coll.transform.position.y - transform.position.y, coll.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
			//Debug.Log ("froghit angle = " + angle);
			Instantiate(frogHitGO, transform.position, Quaternion.Euler(0, 0, angle + 90));
			
			StartCoroutine (newFrog());
		}

		Debug.Log (gameObject.name + " trigger hit by " + coll.gameObject.name);
		Debug.Log (coll.gameObject.name + " position = " + coll.gameObject.transform.position);
		Debug.Log (gameObject.name + " position = " + gameObject.transform.position);
	}
	
	public bool IsInBounds()
	{
		if (transform.position.x < (xMax-3) & transform.position.x > (xMin+3) & transform.position.y > (yMin+3) & transform.position.y < (yMax-3))
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}


