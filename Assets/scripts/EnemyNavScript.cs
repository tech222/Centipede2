using UnityEngine;
using System.Collections;

public class EnemyNavScript : MonoBehaviour {
	
	public GameObject enemySplosion;
	public GameObject enemySegPrefab;								// segment prefab object
	public GameObject enemyEggPrefab;
	public Sprite enemySpriteFaceYell;
	public Sprite enemySpriteFace;
	
	GameObject frog;
	GameObject head;
	GameObject target;
	
	RaycastHit2D leftFlag, rightFlag, forwardFlag, downFlag;
	
	public LayerMask hitLayer;										// layermask for raycast against maze walls
	public int initSegments = 5;									// initial number of segments
	public float startSpeed = 12.0f;								// speed for enemy, default at 12
	
	bool hit = false;												// hit detector flag
	Vector3 newDir = new Vector3();									// next direction for enemy
	Vector3 direction = new Vector3();								// current direction for enemy
	Vector3[] positions = new Vector3[100];							// array containing previous positions
	Vector3 newPosition;
	Vector3 adjPos;
	int segAmt;														// total amount of segments (array counter)
	float speed;
	int count = 0;													// segment counter
	int i = 0;														// array counter
	GameObject[] seg;												// array of segments as child gameobjects
	enemySegScript[] enemySegScript;								// array of each enemy segment's script
    SpriteRenderer[] segSprite;
	int rndTarget;
	
	bool skipAI = false;
	bool isWeak = false;											// is enemy smaller than player?
	bool segSplode = true;											// should segment splode on removal
	bool gateOpen = false;
	bool turned = false;											// did object turn?
	float turnDelay = 0;										    // delay after turn
    bool weakTargetSet = false;
    
    [HideInInspector]
    public bool playerGone = false;
	
	frogMoveScript frogScript;
	HeadScript headScript;
	GameObject playerEgg;
	
	[Range(0, 10)]
	[SerializeField] int aggressiveness = 8;						// public attack amounts (1=runaway, 5=random, 10=attack)

	[Range(0, 5)]
	[SerializeField]
	float turnDelayLimit;											    // frame delay on ai after making turns at interections and corners
	
	Animator anim;													// animator
	
	bool[] wallCheck = new bool[3];
	Rigidbody2D myRigidbody;
	
	Color32 snakeOrange = new Color(0.9255f, 0.43137f, 0f, 1f);
	Color32 snakeYellow = new Color(0.839215f, 0.8f, 0f, 1f);
	
	void Awake () {
		
		// get handle to animator
		anim = GetComponent<Animator>();
		anim.enabled = false;
		
		/* get handle to scene manager
		if (GameObject.Find ("SceneManager") != null){
			sceneManager = GameObject.Find ("SceneManager");
			sceneManagerScript = sceneManager.GetComponent<SceneManager> ();
		} 
        */
		
		// get handle to frog object and frog script
		if (GameObject.FindWithTag("frog") != null){
			frog = GameObject.FindWithTag("frog");
			frogScript = frog.GetComponent<frogMoveScript>();
		}
		
		// get handle to player head object and script
		if (GameObject.FindWithTag("head") != null){
			target = head = GameObject.FindWithTag("head");
			headScript = head.GetComponent<HeadScript> ();
		}

	}
	
	void Start () 
    {
        SceneManager.Instance.addEnemy();
        
        speed = startSpeed;
        turnDelayLimit = 24/startSpeed;
		direction = Vector3.up;
		newPosition = transform.position;
		adjPos = transform.position;
		
		// set segment array and segment script array to initial index amount
		segAmt = initSegments;
		seg = new GameObject[segAmt];
		enemySegScript = new enemySegScript[segAmt];
        segSprite = new SpriteRenderer [segAmt];

		
		// zero out all elements in head positions array
		for (int bb = 0; bb < positions.Length; bb++) {
			positions [bb] = transform.position;	
		}
		
		// create segment child gameObjects at start of game
		for (int cc = 0; cc < segAmt; cc++ ) 
		{
			seg [cc] = (GameObject)Instantiate (enemySegPrefab, transform.position, Quaternion.identity);
			seg [cc].transform.parent = transform.parent;
			seg[cc].name = "enemySeg_" + (cc + 1).ToString();
			enemySegScript[cc] = seg [cc].GetComponent<enemySegScript> ();			// enemy segments' script handles
            segSprite[cc] = seg[cc].GetComponent<SpriteRenderer>();             // segments' sprite renderer handles    
            segSprite[cc].sortingOrder = -cc*10;
		}
		
		CheckColorChange();
		//StartCoroutine (StartColorCheck());
		//anim.enabled = true;

        
		//Debug.Log ("created enemy segments, segAmt = " + segAmt + ", seg.Length = " + seg.Length);
	}
	
	/*
	IEnumerator StartColorCheck()
	{
		if (headScript != null)
		{
			print ("enemy segAmt = " + segAmt + ", player count = " + headScript.getCount());
			if (headScript.getCount() > segAmt+1)				// if player is bigger then make enemy yellow
			{
				//anim.SetBool ("startWeak", true);
				//anim.SetBool ("isWeak", true);
				
				renderer.material.color = snakeYellow;
				for (int cc = 0; cc < segAmt; cc++ ) 
				{
					enemySegScript[cc].startWeak = true;
				} 
			}
			else
			{
				//anim.SetBool ("startWeak", false);
				
				renderer.material.color = snakeOrange;
				for (int cc = 0; cc < segAmt; cc++ ) 
				{
					enemySegScript[cc].startWeak = false;
				} 
			}
			print ("startweak = " + anim.GetBool ("startWeak"));
			yield return null;
		}
		else
		{
			// if no target is available default to orange
			if (renderer.material.color != snakeOrange)
			{
				renderer.material.color = snakeOrange;
			}
			
			for (int ii = 0; ii < count; ii++)
			{
				seg[ii].renderer.material.color = snakeOrange;
			}	 
		}
	}
	*/


	// Update is called once per frame
	void Update() 
	{

		// change position based on direction and speed
		transform.position = Vector3.Lerp (transform.position, transform.position + direction * speed, Time.fixedDeltaTime);
		
		// array of last 100 positions
		positions [i] = transform.position;
		
		// frame delay after enemy makes a turn before allowing enemy to turn again
		if (turned) 
		{
			turnDelay++;
		}
		
		// frame delay after a corner turn before allowing another corner turn (needed for raycasts to hit walls again after turning)
		if (speed != 0)
		{
			if (turnDelay > turnDelayLimit) 
			{
				turned = false;
				turnDelay = 0f;
			}
		}
		
		// increment position array counter if in motion and reset counter as nessicary
		i += 1;
		if (i >= 99) i = 0;
		
		for (int bb = 0; bb < seg.Length; bb++) 
		{
			int k;													// offset in head positions array for segments' positions
			int j;													// offset corrected for 99 elements
			k = (bb + 1) * 3;										// set segments positions to corresponding previous head positons 
			
			j = i - k;												// adjust array index for turnover at 99
			
			if (j < 0)
				j = j + 99;
			
			seg [bb].transform.position = positions [j];			// position segment at previous head location
		}

		// disable wall checks and AI when entering gate	
		if (!gateOpen)											
		{
			WallCheck();											// raycast check for walls
            
            if (playerGone)
            {
                RandomAI();
            }
            else
            {
                if (!skipAI)
    			{
    				if (Random.Range(1,11) <= aggressiveness)			// decide how to turn at intersections
    				{
    					AI ();
    					//print (transform.parent.name + " ai");
    				}
    				else
    				{
    					RandomAI();		
    					//print (transform.parent.name + " random ai");
    				}
    			}
            }
		}


		// get count of active segments
		count = 0;
		for (int ii = 0; ii < seg.Length; ii++)
		{
			if (seg[ii].activeInHierarchy == true)
				count++;
		}
		
		// segment hit by enemy, segment is removed
		for (int ii = 0; ii < count; ii++)
		{
			if (enemySegScript[ii].segHit == true)
			{
				removeSegment (ii);
			}
		}
		
		// no segments left, lose life
		if (count == 0)
		{
			removeEnemy();
		}
		
		// activate egg timer on last active segment
		if (count >= 1)
		{
			if (enemySegScript[count-1].eggState != true)
			{
				enemySegScript[count-1].startEggSeg();
			}
		}
		
		// if 2 or more active segments then check other segments for egg timer and deactivate if found
		if (count >= 2) 
		{
			for (int ii = 0; ii < count-2; ii++)
			{
				enemySegScript[ii].resetSeg();
			}
		}
		
		// if egg timer is up & player is not going back to gate, instantiate egg and remove last segment
		if (count >= 1)
		{
			if (enemySegScript[count-1].clipDone == true)
			{
				Instantiate (enemyEggPrefab, seg[count-1].transform.position, Quaternion.identity);
				segSplode = false;
				removeSegment(count-1);
				segSplode = true;
			}
			
		}
		
		if (isWeak == true & weakTargetSet == false)
		{
			WeakStateTarget();
		}
		
		if (frog == null & target == frog)
		{
			target = head;
		}
		
		if (playerEgg == null & target == playerEgg)
		{
			target = head;
		}
			
        CheckColorChange(); 
    }
    
    public void WeakStateTarget()
    {
		if (!frog.GetComponent<SpriteRenderer>().enabled && target == frog)		// if targeting frog and frog not enabled then target head
		{
			target = head;													
			weakTargetSet = true;
			GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");
			foreach (GameObject enemy in enemies)
			{
				enemy.GetComponent<EnemyNavScript>().WeakStateTarget();
			}
			Debug.Log ("frog sprite renderer turned off while enemy in weak state");
			return;
		}
		
		if (GameObject.FindGameObjectWithTag("egg") == null && target == playerEgg)		// if targeting egg and egg not enabled then target head
		{
			target = head;													
			weakTargetSet = true;
			return;
		}
		
		// set target for enemy while in weak (yellow) state
		rndTarget = Random.Range (0,3);
				
		if (rndTarget == 0)														// 33% chance enemy will try to target frog
		{
			if (frog.GetComponent<SpriteRenderer>().enabled & frogScript.IsInBounds())
			{
				target = frog;													// target frog when in weak state
				weakTargetSet = true;
			}
		}
		else if (rndTarget >= 1)												// 67% chance enemy will try to target egg
		{
			if (GameObject.FindGameObjectWithTag("egg") != null)
			{
				playerEgg = GameObject.FindGameObjectWithTag("egg");
				target = playerEgg;
				weakTargetSet = true;
			}
		}
		else                                                               		// target player if all other target fail
		{
			target = head;													
			weakTargetSet = true;
		}
		
    }
    
    void CheckColorChange()
    {
    	/*
    	if (anim.enabled != true)
    	{
    		anim.enabled = true;
    	}
    	*/
		// if enemy smaller than player change color
		//print (gameObject.transform.parent.name + " seg count = " + count + ", sees player seg count = " + headScript.getCount());
        if (target != null)
        {
            //int plyrLength = target.transform.parent.childCount;
            //int thisSnakeLength = transform.parent.childCount;
			if (headScript.getCount() > count)
			{
				isWeak = true;
				if (GetComponent<Renderer>().material.color != snakeYellow)
				{
					//anim.SetBool ("isWeak", true); 
					GetComponent<Renderer>().material.color = snakeYellow;
				}
				
				for (int ii = 0; ii < count; ii++)
				{
					//StartCoroutine (enemySegScript[ii].isWeak());
					seg[ii].GetComponent<Renderer>().material.color = snakeYellow;
				}
			}
			else
			{
				isWeak = false;
				if (GetComponent<Renderer>().material.color != snakeOrange)
				{
					//anim.SetBool ("isWeak", false); 
					GetComponent<Renderer>().material.color = snakeOrange;
				}
				
				for (int ii = 0; ii < count; ii++)
				{
					//StartCoroutine (enemySegScript[ii].isNormal());
					seg[ii].GetComponent<Renderer>().material.color = snakeOrange;
				}
			}
        }
        else
        {
			// if no target is available default to orange
			if (GetComponent<Renderer>().material.color != snakeOrange)
			{
				GetComponent<Renderer>().material.color = snakeOrange;
			}
			
			for (int ii = 0; ii < count; ii++)
			{
				seg[ii].GetComponent<Renderer>().material.color = snakeOrange;
			}									
        }
    }
	
	public void addSegment ()
	{
		int ii = 0;
		bool skip = false;
		
		// check for inactive segments and activate if present
		while ((ii < seg.Length) & !skip)
		{
			//Debug.Log (seg[ii].name + ", active " + seg [ii].activeInHierarchy);
			if ((seg [ii].activeInHierarchy == false) & (ii >= 1))
			{
				enemySegScript[ii].enabled = true;
				seg[ii].SetActive(true);
				enemySegScript[ii].startEggSeg();
				enemySegScript[ii-1].resetSeg();
				skip = true;
				//print ("adding segment: player count = " +headScript.getCount() + ", enemy count = " + (count+1));
				/*
                if (headScript.getCount() > (count + 1))				// if player is bigger after adding seg then start new seg yellow
				{
					enemySegScript[ii].startWeak = true;
				}
                */
				//Debug.Log ("setactive " + seg[ii].name + ", segAmt = " +segAmt+ ", seg.length = " + seg.Length);
			}
			ii++;
		}
		
		// if there are no inactive segments then instantiate a new segment
		if (!skip)
		{
            // increase size of arrays by 1
            System.Array.Resize(ref seg, seg.Length + 1);
            System.Array.Resize(ref enemySegScript, enemySegScript.Length + 1);
            System.Array.Resize(ref segSprite, segSprite.Length + 1);
            
			seg [seg.Length-1] = (GameObject)Instantiate (enemySegPrefab, transform.position, transform.rotation);
			seg [seg.Length-1].transform.parent = transform.parent;
			seg [seg.Length-1].name = "enemySeg_" + (count + 1).ToString();
			
			//Debug.Log ("created " + seg[seg.Length-1].name + ", segAmt = " +segAmt+ ", seg.length = " + seg.Length);
			
			if (seg.Length > 1) 
			{
				enemySegScript[seg.Length-2].resetSeg();										
			}
			
			enemySegScript[seg.Length-1] = seg [seg.Length-1].GetComponent<enemySegScript> ();				// set segScript for new last segment
			enemySegScript[seg.Length-1].startEggSeg();	
			
            segSprite[seg.Length-1] = seg [seg.Length-1].GetComponent<SpriteRenderer> ();               // set new seg's sprite below previous seg
            segSprite[seg.Length-1].sortingOrder = segSprite[seg.Length-2].sortingOrder - 10;        
            
			//print ("adding segment: player count = " +headScript.getCount() + ", enemy count = " + (count+1));
			/*
            if (headScript.getCount() > (count + 1))					                // if player is bigger after adding seg then start new seg yellow
			{
				enemySegScript[seg.Length-1].startWeak = true;
			}
            */
		}
		segAmt += 1;
        CheckColorChange();		
	}
	
	void removeSegment (int hitSeg)
	{
		CheckColorChange();
		StartCoroutine (YellFace());
		if (hitSeg < seg.Length)
		{
			for (int aa = hitSeg; aa < seg.Length; aa++)
			{
				if (seg[aa].activeInHierarchy)
				{
					enemySegScript[aa].resetSeg();
					enemySegScript[aa].enabled = false;
					seg[aa].SetActive(false);
					if (segSplode) {											
						Instantiate (enemySplosion, seg[aa].transform.position, Quaternion.identity);
					}
					segAmt--;
				}
			}
		}
        
		//Debug.Log ("removed enemy segment #" + seg[hitSeg].name + ", seg.Length = " + seg.Length + ", SegAmt = " + segAmt);
	}
	
	IEnumerator YellFace()
	{
		gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = enemySpriteFaceYell;
		yield return new WaitForSeconds (0.5f);
		gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = enemySpriteFace;
	}
	
	void OnTriggerEnter2D(Collider2D coll)
	{
		
		if (coll.gameObject.tag == "frog") 
		{
			addSegment();
			//frogMoveScript tempfrogsrpt = coll.GetComponent<frogMoveScript>();
			//StartCoroutine(tempfrogsrpt.newFrog());
			if (weakTargetSet == true)
			{
				weakTargetSet = false;
			}
		}
		
		
		if (coll.gameObject.tag == "head")
		{
			HeadScript headCollScript = coll.gameObject.GetComponent<HeadScript> ();	
			if (headCollScript.getCount () > count)
			{
				removeEnemy();                      // enemy snake is destroyed
			}
			else
			{
				addSegment();                       // player is destroyed
			}
			//Debug.Log ("player count = " + headCollScript.getCount () + ", enemy count = " + count);
		}
		
		if (coll.gameObject.tag == "egg") 
		{
			Destroy (coll.gameObject);
			addSegment();
			if (weakTargetSet == true)
			{
				weakTargetSet = false;
			}
		}
		
		if (coll.gameObject.tag == "baby") 
		{
			Destroy (coll.gameObject.transform.parent.gameObject);
			addSegment();
		}
		
		Debug.Log (gameObject.name + " of " + gameObject.transform.parent.name + " hit " + coll.gameObject.name);
		
	}
	
	void removeEnemy()
	{
		transform.parent.gameObject.SetActive(false);
		GetComponent<Collider2D>().enabled = false;
		SceneManager.Instance.subtractEnemy();
        Instantiate (enemySplosion, transform.position, Quaternion.identity);
        //removeSegment(0);
	}
	
	void WallCheck ()
	{
		skipAI = false;
		float rayDist = 1.2f;
		Vector3 rayPosition = transform.position;
		Vector3 yOffset = new Vector3(0f,0.5f,0f);						// raycast spacing on y axis
		Vector3 xOffset = new Vector3(0.5f,0f,0f);						// raycast spacing on x axis
		
		// local coordinate raycasts
		bool leftFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left), rayDist, hitLayer);
		bool leftFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left), rayDist, hitLayer);
		bool rightFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right), rayDist, hitLayer);
		bool rightFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right), rayDist, hitLayer);
		bool forwardFlag = Physics2D.Raycast (rayPosition + transform.TransformDirection(new Vector3(0f,0.5f,0f)), transform.TransformDirection(Vector3.up), rayDist, hitLayer);
		//forwardFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up), rayDist, hitLayer);
		//downFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down), rayDist, hitLayer);
		//downFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down), rayDist, hitLayer);
		
		// debug raycasts
		Debug.DrawRay (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left) * rayDist, Color.white);
		Debug.DrawRay (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left) * rayDist, Color.white);
		Debug.DrawRay (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right) * rayDist, Color.white);
		Debug.DrawRay (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right) * rayDist, Color.white);
		Debug.DrawRay (rayPosition + transform.TransformDirection(new Vector3(0f,0.5f,0f)), transform.TransformDirection(Vector3.up) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down) * rayDist, Color.white);
		
		wallCheck[0] = rightFlag1 || rightFlag2;
		wallCheck[1] = forwardFlag;
		wallCheck[2] = leftFlag1 || leftFlag2;
		//downFlag = downFlag1 || downFlag2;

		if (wallCheck[0] & !wallCheck[1] & wallCheck[2])
		{
			skipAI = true;
		}
	}
	
	void AI()
	{	
		int hit = 0;
		
		// vector3 direction array corresponding to flags
		Vector3[] lookDir = {					
			Vector3.right,
			Vector3.up,
			Vector3.left
		};
		
		// float local coord z-rotation array corresponding to flags
		float[] turnDir = {							
			-90f,					// right turn
			0f,						// straight, no turn
			90f						// left turn
		};
		
		Vector3 newDir = new Vector3();
		
		if (!turned)
		{
			for (int aa = 0; aa < 3; aa++)								// get wall hit flag count
			{
				if (wallCheck[aa] == true)
				{
					hit++;
				}
			}
			
			if (hit == 2)												// 2 walls hit, 1 available path
			{
				for (int aa = 0; aa < 3; aa++)
				{
					if (wallCheck[aa] == false)
					{
						newDir = new Vector3 (0, 0, turnDir[aa]);
						turned = true;
					}
				}
			}
			
			if (hit == 1)												// 1 wall hit, 2 available paths
			{
				int[] flagIndex = new int[2];							// array of indexes to wallcheck flags
				float[] distCheck = new float[2];						// distances from enemy after turn to target
				Vector3[] distCheckDir = new Vector3[2];				// local coord direction available to turn
				int q = 0;
				for (int aa = 0; aa < 3; aa++)
				{
					if (wallCheck[aa] == false)
					{
						flagIndex[q] = aa;
						distCheckDir[q] = transform.TransformDirection(lookDir[aa]);
						if (target != null)
						{
							distCheck[q] = (target.transform.position - (transform.position + 2f * distCheckDir[q])).sqrMagnitude;
							Debug.DrawLine(transform.position + 2f * distCheckDir[q], target.transform.position, Color.red, 1f);
						}
						q++;
					}
				}
				
				if (distCheck[0] <= distCheck[1])
				{
					if (wallCheck[flagIndex[0]] == false)
					{
						if (!isWeak || target == frog || target == playerEgg) 
						{
							newDir = new Vector3 (0, 0, turnDir[flagIndex[0]]);					// if not weak or targeting frog or egg, take shortest path to target
						} else
						{
							newDir = new Vector3 (0, 0, turnDir[flagIndex[1]]);					// if weak & targeting head, take path away from target
						}
						turned = true;
					}
				}
				else
				{
					if (wallCheck[flagIndex[1]] == false)
					{
						if (!isWeak || target == frog || target == playerEgg) 
						{
							newDir = new Vector3 (0, 0, turnDir[flagIndex[1]]);					// if not weak or targeting frog or egg, take shortest path to target
						} 
						else 
						{
							newDir = new Vector3 (0, 0, turnDir[flagIndex[0]]);					// if weak & targeting head, take path away from target
						}
						turned = true;
					}
				}
			} 
			
			if (hit == 0)								// no wall hits, 3 available paths
			{
				
				int[] flagIndex = new int[3];
				float[] distCheck = new float[3];						
				Vector3[] distCheckDir = new Vector3[3];
				float minDist;
				int finalIndex = 0;
				
				for (int q = 0; q < 3; q++)
				{
					flagIndex[q] = q;
					distCheckDir[q] = transform.TransformDirection(lookDir[q]);
					if (target != null)
					{
						distCheck[q] = (target.transform.position - (transform.position + 2f * distCheckDir[q])).sqrMagnitude;
						Debug.DrawLine(transform.position + 2f * distCheckDir[q], target.transform.position, Color.cyan, 1f);
					}
				}
				
				minDist = distCheck[0];
				for (int i = 0; i < 3; i++)
				{
					if (minDist > distCheck[i])
					{
						minDist = distCheck[i];
						finalIndex = i;
					}
				}
				
				if (!isWeak || target == frog || target == playerEgg) 
				{
					newDir = new Vector3 (0, 0, turnDir[flagIndex[finalIndex]]);				// if not weak or targeting frog or egg, take shortest path to target
				} 
				else 
				{
					newDir = new Vector3 (0, 0, turnDir[Random.Range(0,3)]);					// if weak & targeting head, take path away from target
				}
				turned = true;
			} 
			
			
			// turn based on results of AI
			transform.Rotate(newDir);
			
			// forward snake direction in world space coordinates
			direction = this.transform.TransformDirection(Vector3.up);				
			
			// correct minor misplacement at turns
			if (direction == Vector3.left || direction == Vector3.right) 
			{
				transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y), 0f);
			}
			if ((direction == Vector3.up || direction == Vector3.down))	
			{
				transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, 0f);
			}
		}
	}

	void RandomAI()
	{	
		int hit = 0;
		Vector3[] lookDir = new Vector3[3];							// vector3 direction array corresponding to flags
		lookDir[0] = Vector3.right;
		lookDir[1] = Vector3.up;
		lookDir[2] = Vector3.left;
		
		float[] turnDir = new float[3];								// float local coord z-rotation array corresponding to flags
		turnDir[0] = -90f;				// right turn
		turnDir[1] = 0f;				// straight, no turn
		turnDir[2] = 90f;				// left turn
		
		Vector3 newDir = new Vector3();
		
		if (!turned)
		{
			for (int aa = 0; aa < 3; aa++)								// get wall hit flag count
			{
				if (wallCheck[aa] == true)
				{
					hit++;
				}
			}
			
			if (hit == 2)												// 2 walls hit, 1 available path
			{
				for (int aa = 0; aa < 3; aa++)
				{
					if (wallCheck[aa] == false)
					{
						newDir = new Vector3 (0, 0, turnDir[aa]);
						turned = true;
					}
				}
			}
			
			if (hit == 1)												// 1 wall hit, 2 available paths
			{
				int q = 0;
				int[] flagIndex = new int[2];							// array of indexes to wallcheck flags
				for (int aa = 0; aa < 3; aa++)
				{
					if (wallCheck[aa] == false)
					{
						flagIndex[q] = aa;
						q++;
					}
				}
				
				int rndDir = Random.Range(0, 2);

				if (rndDir == 0)
				{
					newDir = new Vector3 (0, 0, turnDir[flagIndex[0]]);					
					turned = true;
				}
				else if (rndDir == 1)
				{
					newDir = new Vector3 (0, 0, turnDir[flagIndex[1]]);
					turned = true;
				}

			} 
			
			if (hit == 0)								// no wall hits, 3 available paths
			{
				int rndDir = Random.Range(0, 3);
				newDir = new Vector3 (0, 0, turnDir[rndDir]);
				turned = true;
			} 
			
			
			// turn based on results of AI
			transform.Rotate(newDir);
			
			// forward snake direction in world space coordinates
			direction = this.transform.TransformDirection(Vector3.up);				
			
			// correct minor misplacement at turns
			if (direction == Vector3.left || direction == Vector3.right) 
			{
				transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y), 0f);
			}
			if ((direction == Vector3.up || direction == Vector3.down))	
			{
				transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, 0f);
			}
		}
	}
	
	public void isGateOpen (bool gateState)
	{
		gateOpen = gateState;
	}
	
	public int getCount ()
	{
		return (count);
	}
	
	public void ResetTarget(GameObject newPlayer)
	{
        playerGone = true;
		target = newPlayer;
		head = newPlayer;
		headScript = head.GetComponent<HeadScript> ();
        Invoke ("PlayerBack", 3f);                                          // set playerGone back to false after 3 second delay to get out of gate
		//print ("reset enemy target, new target name = " + newPlayer.name + ", new player count = " + headScript.getCount());
	}
    
    void PlayerBack()
    {
        playerGone = false;
    }
    	
}