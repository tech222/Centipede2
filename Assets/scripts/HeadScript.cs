using UnityEngine;
using System.Collections;

public class HeadScript : MonoBehaviour 
{
    public GameObject playerSplosion;
	public GameObject segPrefab;							// segment prefab object
	public GameObject eggPrefab;							// egg prefab
    public Sprite enemySpriteFaceYell;
    public Sprite enemySpriteFace;

	[Range(6, 22)]
	[SerializeField] float eggTimer = 12f;					// time till last segment hatches egg
	
	public LayerMask layerMask;								// layermask for raycast against maze walls
	public float startSpeed = 12f;
	public int initSegments = 5;							// initial number of segments
	
	bool addSeg = false;
    int segAmt;	                                            // total amount of segments (array counter)
    
    [HideInInspector]       								
	public float speed;										// initial speed for head
	
    Vector3[] positions = new Vector3[100];					// array containing previous positions of object head
	int i = 0;												// array counter
	int count = 0;											// segment counter

	RaycastHit castHit;										// raycast for hit detection
	Vector3 yOffset = new Vector3(0f,0.3f,0f);				// raycast offset spacing on z axis
	Vector3 xOffset = new Vector3(0.5f,0f,0f);				// raycast offset spacing on x axis

	public Vector3 newDir = new Vector3();							// next direction for head
	public Vector3 direction;              						// current direction for head
    Quaternion oldRot;
    Quaternion newRot;

	GameObject[] seg;										// array of segments as child gameobjects
	SegmentScript[] segScript;								// array of each segment's script handle
	SpriteRenderer[] segSprite;

	GameObject frog;
	frogMoveScript frogScript;
    Animator faceAnim;
    
	bool leftFlag;
	bool rightFlag;
	bool upFlag;
	bool downFlag;

	//GameObject sceneManager;
	//SceneManager sceneManagerScript;

    //PlayerGateControls playerGate;
    //GameObject playerGate;
    //PlayerGateScript plyrGateScript;
    //GameObject playerGateMat;
    //Vector3 initGatePos;
    [HideInInspector]
	public bool startGate = true;
	//bool closeGateFlag = false;
	bool goingHome = false;
	//bool wallcheckOff = false;
    
    [HideInInspector]
	public bool snakeFold = false;
    
    bool segSplode = true;                                  // should segment splode on removal
    bool snakeFolding = false;
    bool targetHome = false;
    bool segCheckStarted = false;
    
    int pressCount = 0;
    int zRotation = 0;
    bool pressDelay = false;

	Transform target1;										// target for baby to player move toward at end of level
    Transform target2;
    Transform target;
    
	//bool leftFlag1, leftFlag2, rightFlag1, rightFlag2, forwardFlag1, forwardFlag2, downFlag1, downFlag2;
	//bool leftFlag, rightFlag, forwardFlag, downFlag;
	bool turned = false;									// did object turn?
	int turnDelay = 0;										// delay after turn
	bool[] wallCheck = new bool[3];
    bool skipAI = false;
    
    PlayerGateControls plyrGateControls;

    [System.Serializable]
    public class Points
    {
        public int egg;
        public int enemySeg;
        public int enemy;
        public int frog;
        
        public Points (int myEgg, int myEnSeg, int myEn, int myFrog)
        {
            egg = myEgg;
            enemySeg = myEnSeg;
            enemy = myEn;
            frog = myFrog;
        }
    }
 
    public Points myPoints = new Points(200, 100, 500, 300);

	void Awake () 
	{	
        //newDir = direction = Vector3.up;

		// set segment array, segment script array, and segment sprite arrays to initial index size
		segAmt = initSegments;
		seg = new GameObject[segAmt];
		segScript = new SegmentScript [segAmt];
		segSprite = new SpriteRenderer [segAmt];
        
        faceAnim = gameObject.GetComponentInChildren<Animator>();

		/*
        if (GameObject.Find ("SceneManager") != null)
		{
			sceneManager = GameObject.Find ("SceneManager");
			sceneManagerScript = sceneManager.GetComponent<SceneManager> ();
		}
        */
        if (GameObject.Find("playerGateController") != null)
        {
            GameObject tempGO = GameObject.Find("playerGateController");
            plyrGateControls = tempGO.GetComponent<PlayerGateControls>();
        }
        

		if (GameObject.FindWithTag("frog") != null)
		{
			frog = GameObject.FindWithTag("frog");
			frogScript = frog.GetComponent<frogMoveScript>();
		}
        
        // target location for going back to gate
        if (GameObject.Find ("playerHomePrefab")!= null)
        {
            target1 = GameObject.Find ("playerHomePrefab").transform;
        }
        else
        {
            target1 = null;
        }

		// zero out all elements in head positions array
		for (int bb = 0; bb < positions.Length; bb++) 
		{
			//System.Array.Resize(ref positions, positions.Length + 1);
			positions [bb] = transform.position;	
		}

		// create segment child gameObjects at start of game
		for (int cc = 0; cc < segAmt; cc++ ) 
		{
			seg [cc] = (GameObject)Instantiate (segPrefab, transform.position, transform.rotation);
			seg [cc].transform.parent = transform.parent;
			seg [cc].transform.position = transform.position;
			seg[cc].name = "headSeg_" + (cc + 1);
			segScript[cc] = seg [cc].GetComponent<SegmentScript> ();			// segments' script handles
			segSprite[cc] = seg[cc].GetComponent<SpriteRenderer>();				// segments' sprite renderer handles	
			segSprite[cc].sortingOrder = -cc*10;
		}
		//Debug.Log ("created player segments, segAmt = " + segAmt + ", seg.Length = " + seg.Length);
        segScript[segAmt-1].timeTillEgg = Random.Range(12,25);
		segScript[segAmt-1].startEggSeg();

	}

	void Start()
	{
        SceneManager.Instance.initPlayerPos = transform.position;
        
		// start open gate method in player gate controls script
        StartCoroutine(plyrGateControls.StartGate());
        targetHome = false;
	}

	void Update() 
	{
		// update positions for head and segments
		if (speed != 0)
		{
			// correct minor misplacement at turns
			if (direction == Vector3.right || direction == Vector3.left) {
				transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y), 0f);
			}
			if ((direction == Vector3.up || direction == Vector3.down))	{
				transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, 0f);
			}
			
			// change position based on direction and speed
			//transform.position = Vector3.Lerp(transform.position, transform.position + direction * speed, Time.deltaTime);
			transform.position += direction * speed * Time.fixedDeltaTime;
			
			// array of last 100 positions
			positions [i] = transform.position;

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
		}
		
		// raycasts to check for walls
		CheckWalls();
        
        // get player input and change direction
        if (!startGate)
		{
			if (Input.GetKey(KeyCode.LeftArrow) & !leftFlag) 
			{
				transform.rotation = Quaternion.Euler(0,0,90);
				newDir = Vector3.left;
				speed = startSpeed;
			} 
			
			if (Input.GetKey(KeyCode.RightArrow) & !rightFlag) 
			{
				transform.rotation = Quaternion.Euler(0,0,-90);
				newDir = Vector3.right;
				speed = startSpeed;
			}
			
			
			if (Input.GetKey(KeyCode.UpArrow) & !upFlag)
			{
				transform.rotation = Quaternion.Euler(0,0,0);
				newDir = Vector3.up;
				speed = startSpeed;
			}
			
			if (Input.GetKey(KeyCode.DownArrow) & !downFlag) 
			{
				transform.rotation = Quaternion.Euler(0,0,180);
				newDir = Vector3.down;
				speed = startSpeed;
			} 
			
			// ignore new direction if a wall is present
			if ((leftFlag) & (newDir == Vector3.left)) 
			{
				newDir = direction;
				speed = 0f;
			}
			
			if ((rightFlag) & (newDir == Vector3.right)) 
			{
				newDir = direction;
				speed = 0f;
			}
			
			if ((upFlag) & (newDir == Vector3.up)) 
			{
				newDir = direction;
				speed = 0f;
			}
			
			if ((downFlag) & (newDir == Vector3.down)) 
			{
				newDir = direction;
				speed = 0f;
			} 
			
			// change direction to new direction
			direction = newDir;
			//transform.rotation = Quaternion.Euler(direction);
		}

		// if no more enemies, take control and guide player home
		if (goingHome & !snakeFolding)
		{
			TurnDelay();
			AIWallCheck(); 
            //if (!skipAI) {
			AI();
            //}
            if (speed == 0)
            {
                speed = startSpeed;
            }
		}
		
		if (!segCheckStarted)
		{
			StartCoroutine(SegmentChecks());
			segCheckStarted = true;
		}

        // increment position array counter
		if (speed != 0)
			i += 1;

		//reset counter as nessicary
		if (i >= 99) i = 0;

	}
	
	IEnumerator SegmentChecks()
	{
		// get count of active segments
		count = 0;
		for (int ii = 0; ii < seg.Length; ii++)
		{
			if (seg[ii].activeInHierarchy == true)
				count++;
		}
		//print ("at time " + Time.timeSinceLevelLoad + " player seg count = " + count);
		// segment hit by enemy, segment is removed
		for (int ii = 0; ii < count; ii++)
		{
			if (segScript[ii].segHit == true)
			{
				removeSegment (ii);
			}
		}
		
		// no segments left, lose life and re-initialize player
		if (count == 0 & !snakeFold)
		{
			print ("player died from having zero segments");
			SceneManager.Instance.playerDied();
			StopAllCoroutines();
			RemoveSnake();
		}
		
		// activate egg timer on last active segment if player has 1 or more segments on body
		if (count >= 1)
		{
			if (segScript[count-1].eggState != true)
			{
				segScript[count-1].timeTillEgg = Random.Range(12,25);
				segScript[count-1].startEggSeg();
			}
		}
		
		// if egg timer is up, instantiate egg and remove last segment
		if (count >= 1)
		{
			if (segScript[count-1].clipDone == true)
			{
				Instantiate (eggPrefab, seg[count-1].transform.position, Quaternion.identity);
				segSplode = false;
				removeSegment(count-1);
				segSplode = true;
			}
			
		}
		
		// if enemy and enemy egg count is zero then send player home
		if (SceneManager.Instance.getEnemyCount() == 0)
		{
			
			//Debug.Log("gameobjects tagged enemy in hierarchy:" + GameObject.FindGameObjectsWithTag("enemy").Length);
			//Debug.Log ("number of enemies = " + SceneManager.Instance.getEnemyCount());
			
			int enemiesLeft = 0;
			foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("enemy"))
			{
				if (enemy.transform.parent != null)
				{
					enemiesLeft++;
					//Debug.Log ("enemy name = " + enemy.transform.parent.name);
					//enemy.renderer.material.color = Color.cyan;
				}
			}
			
			enemiesLeft += GameObject.FindGameObjectsWithTag("enemyEgg").Length;
			
			//Debug.Log ("enemiesLeft = " + enemiesLeft);
			
			//if (GameObject.FindGameObjectsWithTag("enemy").Length == 0)    
			if (enemiesLeft == 0)                   
			{
				//goHome();
				startGate = true;
				goingHome = true;
				
				// if there's a home2 in scene then send player there first
				if (GameObject.FindGameObjectWithTag("home2") != null & targetHome == false)									
				{
					target = GameObject.FindGameObjectWithTag("home2").transform;
				}
				else
				{
					target = GameObject.FindGameObjectWithTag("home").transform;
				}
				//print ("player going home");
			}
		}
		
		yield return new WaitForSeconds (0.05f);
		StartCoroutine (SegmentChecks());
		//yield return null;
	}
	
	void CheckWalls()
	{
		if (!startGate)
		{
			// raycasts to check for wall hits
			//bool leftFlag1 = Physics2D.Raycast (transform.position + yOffset, Vector3.left, 1.2f, layerMask);
			//bool leftFlag2 = Physics2D.Raycast (transform.position - yOffset, Vector3.left, 1.2f, layerMask);
			bool leftFlag3 = Physics2D.Linecast (transform.position + new Vector3(-1f, 0.6f, 0f), transform.position + new Vector3(-1f, -0.6f, 0f), layerMask);
			//bool rightFlag1 = Physics2D.Raycast (transform.position + yOffset, Vector3.right, 1.2f, layerMask);
			//bool rightFlag2 = Physics2D.Raycast (transform.position - yOffset, Vector3.right, 1.2f, layerMask);
			bool rightFlag3 = Physics2D.Linecast (transform.position + new Vector3(1f, 0.6f, 0f), transform.position + new Vector3(1f, -0.6f, 0f), layerMask);
			//bool upFlag1 = Physics2D.Raycast (transform.position + xOffset, Vector3.up, 1.2f, layerMask);
			//bool upFlag2 = Physics2D.Raycast (transform.position - xOffset, Vector3.up, 1.2f, layerMask);
			bool upFlag3 = Physics2D.Linecast (transform.position + new Vector3(0.6f, 1f, 0f), transform.position + new Vector3(-0.6f, 1f, 0f), layerMask);
			//bool downFlag1 = Physics2D.Raycast (transform.position + xOffset, Vector3.down, 1.2f, layerMask);
			//bool downFlag2 = Physics2D.Raycast (transform.position - xOffset, Vector3.down, 1.2f, layerMask);
			bool downFlag3 = Physics2D.Linecast (transform.position + new Vector3(0.6f, -1f, 0f), transform.position + new Vector3(-0.6f, -1f, 0f), layerMask);
			
			//leftFlag = leftFlag1 || leftFlag2;
			leftFlag = leftFlag3;
			//rightFlag = rightFlag1 || rightFlag2;
			rightFlag = rightFlag3;
			//upFlag = upFlag1 || upFlag2;
			upFlag = upFlag3;
			//downFlag = downFlag1 || downFlag2;
			downFlag = downFlag3;
			
			//draw raycasts on scene
			//Debug.DrawRay (transform.position + yOffset, Vector3.left * 1.2f, Color.white);
			//Debug.DrawRay (transform.position - yOffset, Vector3.left * 1.2f, Color.white);
			Debug.DrawLine (transform.position + new Vector3(-1f, 0.6f, 0f), transform.position + new Vector3(-1f, -0.6f, 0f),Color.white);
			Debug.DrawLine (transform.position + new Vector3(1f, 0.6f, 0f), transform.position + new Vector3(1f, -0.6f, 0f), Color.white);
			Debug.DrawLine (transform.position + new Vector3(0.6f, 1f, 0f), transform.position + new Vector3(-0.6f, 1f, 0f), Color.white);
			Debug.DrawLine (transform.position + new Vector3(0.6f, -1f, 0f), transform.position + new Vector3(-0.6f, -1f, 0f), Color.white);
			//Debug.DrawRay (transform.position - yOffset, Vector3.right * 1.2f, Color.white);
			//Debug.DrawRay (transform.position + xOffset, Vector3.up * 1.2f, Color.white);
			//Debug.DrawRay (transform.position - xOffset, Vector3.up * 1.2f, Color.white);
			//Debug.DrawRay (transform.position + xOffset, Vector3.down * 1.2f, Color.white);
			//Debug.DrawRay (transform.position - xOffset, Vector3.down * 1.2f, Color.white);
			/*
            Debug.Log ("left hit = " + leftFlag);
            Debug.Log ("right hit = " + rightFlag);
            Debug.Log ("up hit = " + upFlag);
            Debug.Log ("down hit = " + downFlag);
	        */
        }
	}
    
    void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.tag == "frog") 
		{
			addSegment();											// add segment
            SceneManager.score += myPoints.frog;                  	// add score
			StartCoroutine (YellFace());							// yellface
			
			transform.Rotate(180, 0, 0);							// turn around 
			newDir = -newDir;
			speed = startSpeed;

		}

		if (coll.gameObject.tag == "enemy")
		{
			EnemyNavScript enemyScript = coll.gameObject.GetComponent<EnemyNavScript> ();	
            print ("hit:" + gameObject.transform.parent.name + " seg count = " + count + ", " + coll.gameObject.transform.parent.name + " seg count = " + enemyScript.getCount ());
			if (enemyScript.getCount () >= count)						// enemy is bigger than or equal to player
			{
                SceneManager.Instance.playerDied();
                RemoveSnake();
                print ("player died from hit with enemy");
			}
			else 															// enemy is smaller than player
			{
				addSegment();
                SceneManager.score += myPoints.enemy;             			// add score
                coll.GetComponent<Collider2D>().enabled = false;
				if (segScript[0] != null & segScript[0].segHit == true)		// if enemy head hit 1st seg thru head then reset seg hit value
				{
					segScript[0].segHit = false;
				}
				print ("player destroyed enemy segment");
			}
		}

		if (coll.gameObject.tag == "enemyEgg") 
		{
			Destroy (coll.gameObject);
			addSegment();
            SceneManager.score += myPoints.egg;                   // add score
		}
        
        if (coll.gameObject.tag == "enemySegment") 
        {
            SceneManager.score += myPoints.enemySeg;              // add score
        }
        
		Debug.Log (gameObject.name + " hit " + coll.gameObject.name);

	}
	
	void addSegment ()
	{
		bool skip = false;

		// check for inactive segments and activate if present
		int ii = 0;
		while (ii < seg.Length & !skip)
		{
			//Debug.Log (seg[ii].name + ", active " + seg [ii].activeInHierarchy);
			if (seg[ii] != null & seg [ii].activeInHierarchy == false) 
			{
				if (ii >= 1)
				{
                	segScript[ii-1].resetSeg();                                	// reset previous last seg egg anim state
                }
				segScript[ii].enabled = true;
				seg[ii].SetActive(true);
				segScript[ii].startEggSeg();                                    // start egg anim countdown on new last seg
				skip = true;
				//Debug.Log ("setactive " + seg[ii].name + ", segAmt = " +segAmt+ ", seg.length = " + seg.Length);
			}
			ii++;
		}

		// if there are no inactive segments then instantiate a new segment
		if (!skip)
		{
			// increase size of arrays by 1
			System.Array.Resize(ref seg, seg.Length + 1);
			System.Array.Resize(ref segScript, segScript.Length + 1);
			System.Array.Resize(ref segSprite, segSprite.Length + 1);
            
            if (seg.Length > 1) 
            {
                segScript[seg.Length-2].resetSeg();                             // reset previous last seg egg anim state                                    
            }

			// setup new player segment
			seg [seg.Length-1] = (GameObject)Instantiate (segPrefab, transform.position, transform.rotation);
			seg [seg.Length-1].transform.parent = transform.parent;
			seg [seg.Length-1].name = "headSeg_" + (transform.parent.childCount - 1);

			Debug.Log ("created " + seg[seg.Length-1].name + ", segAmt = " +segAmt+ ", seg.length = " + seg.Length);

			segScript[seg.Length-1] = seg [seg.Length-1].GetComponent<SegmentScript> ();				// set segScript for new last segment
			segScript[seg.Length-1].startEggSeg();	

			segSprite[seg.Length-1] = seg [seg.Length-1].GetComponent<SpriteRenderer> ();				// set new seg's sprite below previous seg
			segSprite[seg.Length-1].sortingOrder = segSprite[seg.Length-2].sortingOrder - 10;			
		}
		segAmt += 1;
        Debug.Log ("player snake add segment method");
	}
	
	void removeSegment (int hitSeg)
	{
		if (hitSeg < seg.Length)
		{
			for (int aa = hitSeg; aa < seg.Length; aa++)
			{
				if (seg[aa].activeInHierarchy)
				{
					segScript[aa].resetSeg();
					segScript[aa].enabled = false;
					seg[aa].SetActive(false);
                    if (segSplode) 
                    {                                            
                        Instantiate (playerSplosion, seg[aa].transform.position, Quaternion.identity);
                    }
					segAmt --;
				}
			}
		}
        
		if (segAmt != 0)
		{
			//Debug.Log ("inside removeSegment(): seg.Length = " + seg.Length);
			//Debug.Log ("inside removeSegment(): segAmt = " + segAmt);
			StartCoroutine (YellFace());
        }
		//Debug.Log ("removed player segment #" + seg[hitSeg].name + ", seg.Length = " + seg.Length + ", SegAmt = " + segAmt);
	
	}
    
    IEnumerator YellFace()
    {
		//Debug.Log ("inside YellFace(): seg.Length = " + seg.Length);
        //gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = enemySpriteFaceYell;
        faceAnim.SetBool("snakeYell", true);
        yield return new WaitForSeconds (0.5f);
        //gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = enemySpriteFace;
        faceAnim.SetBool("snakeYell", false);
        yield return null;
    }                   
    
	public int getCount ()
	{
		return (count);
	}

	public bool getGoingHome()
	{
		return goingHome;
	}
    
    /*
	public void goHome ()												// move player back to home gate
	{
        startGate = true;
        goingHome = true;
	}
    */

	public void AIWallCheckOff()
	{
        speed = 0;
		snakeFolding = true;
		
		// correct minor misplacement at turns
		transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y), 0f);

        //wallcheckOff = true;
        //seg[0].collider2D.enabled = false;
        gameObject.GetComponent<Collider2D>().enabled = false;
        Collider2D[] tempSegs = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D tempSeg in tempSegs)
        {
            tempSeg.enabled = false;
        }
        
		//wallcheckOff = true;
		gameObject.GetComponent<Collider2D>().enabled = false;
		gameObject.GetComponentInChildren<Collider2D>().enabled = false;
        
        Debug.Log ("inside WallCheckOff");
        Debug.Log ("head collider = " + gameObject.GetComponent<Collider2D>().enabled.ToString());
	}

    public IEnumerator foldSnakeAtGate ()
    {
        snakeFolding = true;
        speed = 0;
        float t = 0;
        float foldSpeed = 0.35f;
        
        //Transform tempFace = gameObject.transform.GetChild(0);
        //tempFace.rotation = Quaternion.LookRotation(transform.right, -transform.forward);
        //tempFace.rotation = new Quaternion(0, 0, -0.7071068f, 0.7071068f);              //rotate face to look toward gate
        
        while (seg[0].transform.position != transform.position)
        {
            int p = 0;
            for (int m = count-1; m >= p; m--)
            {
                if (m == 0)
                {
                    seg[m].transform.position = Vector3.Lerp(seg[m].transform.position, transform.position, foldSpeed*t);
                }
                else
                {
                    seg[m].transform.position = Vector3.Lerp(seg[m].transform.position, seg[m-1].transform.position, foldSpeed*t);
                }
            }
            p++;
            t += Time.deltaTime;
            yield return null;
        }
        
		for (int m = count-1; m >= 0; m--)
		{
			seg[m].transform.position = seg[0].transform.position;
		}
        
        yield return null;
        snakeFold = true;
        targetHome = false;
 
        /*
        for (int i = 0; i < count; i++)
        {
            seg[i].SetActive(false);
        }
        */
        
        //GameObject plyrHome = GameObject.Find("playerHomePrefab");
        //playerHomeScript plyrHomeScript = plyrHome.GetComponent<playerHomeScript>();
        //StartCoroutine(plyrHomeScript.HomeProcess());
    }

	public Vector3[] getPositions()
	{
		return positions;
	}

	void TurnDelay ()
	{
		// frame delay after enemy makes a turn before allowing enemy to turn again
		if (turned) 
		{
			turnDelay++;
		}
		
		// frame delay after a corner turn before allowing another corner turn (needed for raycasts to hit walls again after turning)
		if (speed != 0)
		{
			if (turnDelay > 24/speed)
			{
				turned = false;
				turnDelay = 0;
			}
		}
		//print ("turnDelay on " + this.gameObject.transform.parent.name + " = " + turnDelay);
		//print ("turned on " + this.gameObject.transform.parent.name + " = " + turned	);
	}
	
	void AIWallCheck ()
	{
        skipAI = false;
		float rayDist = 1.0f;
		Vector3 rayPosition = transform.position;
		Vector3 yOffset = new Vector3(0f,0.5f,0f);						// raycast spacing on y axis
		Vector3 xOffset = new Vector3(0.5f,0f,0f);						// raycast spacing on x axis
		
		// local coordinate raycasts
		bool leftFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left), rayDist, layerMask);
		bool leftFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left), rayDist, layerMask);
		bool rightFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right), rayDist, layerMask);
		bool rightFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right), rayDist, layerMask);
		bool forwardFlag = Physics2D.Raycast (rayPosition + transform.TransformDirection(new Vector3(0f,0.5f,0f)), transform.TransformDirection(Vector3.up), 0.7f, layerMask);
		//forwardFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up), rayDist, layerMask);
		//downFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down), rayDist, layerMask);
		//downFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down), rayDist, layerMask);
		
		// debug raycasts
		Debug.DrawRay (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left) * rayDist, Color.white);
		Debug.DrawRay (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left) * rayDist, Color.white);
		Debug.DrawRay (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right) * rayDist, Color.white);
		Debug.DrawRay (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right) * rayDist, Color.white);
		Debug.DrawRay (rayPosition + transform.TransformDirection(new Vector3(0f,0.5f,0f)), transform.TransformDirection(Vector3.up) * 0.7f, Color.white);
		//Debug.DrawRay (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down) * rayDist, Color.white);
		
		wallCheck[0] = rightFlag1 || rightFlag2;
		wallCheck[1] = forwardFlag;
		wallCheck[2] = leftFlag1 || leftFlag2;
		//downFlag = downFlag1 || downFlag2;
        
        if (wallCheck[0] & !wallCheck[1] & wallCheck[2])
        {
            //skipAI = true;
        }
	}
	
	void AI()
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
						distCheck[q] = (target.position - (transform.position + 2f * distCheckDir[q])).sqrMagnitude;
						Debug.DrawLine(transform.position + 2f * distCheckDir[q], target.position, Color.red, 1f);
						q++;
					}
				}
				
				if (distCheck[0] <= distCheck[1])
				{
					if (wallCheck[flagIndex[0]] == false)
					{
						newDir = new Vector3 (0, 0, turnDir[flagIndex[0]]);					// take shortest path to target
						turned = true;
					}
				}
				else
				{
					if (wallCheck[flagIndex[1]] == false)
					{
						newDir = new Vector3 (0, 0, turnDir[flagIndex[1]]);
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
					distCheck[q] = (target.position - (transform.position + 2f * distCheckDir[q])).sqrMagnitude;
					Debug.DrawLine(transform.position + 2f * distCheckDir[q], target.position, Color.cyan, 1f);
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
				
				newDir = new Vector3 (0, 0, turnDir[flagIndex[finalIndex]]);				// take shortest path to target
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
    
	public void SwitchTarget()
    {
        target = GameObject.FindGameObjectWithTag("home").transform;
        targetHome = true;
    }
    
    void RemoveSnake()
    {
        Instantiate (playerSplosion, transform.position, Quaternion.identity);
        removeSegment(0);
        StopAllCoroutines();
        gameObject.transform.parent.gameObject.SetActive(false);
        /*
        if (GameObject.Find ("playerIcon(Clone)")!= null)                       // if gate icons still moving then delay gate control script removal for 1 second
        {   
            renderer.enabled = false;
            gameObject.GetComponent<PlayerGateControls>().enabled = true;
            Invoke ("DelayedRemoval", 1f);
        }
        */
    }
    /*
    void DelayedRemoval()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
    }
    
    void OnDisable()
    {
        Debug.Log ("snake disabled");
    }
    */
}

