using UnityEngine;
using System.Collections;

public class HeadHomeScript2 : MonoBehaviour {
	
	public float speed = 12.0f;								// speed for baby
	public GameObject target;								// target for baby to baby move toward

	bool leftFlag1, leftFlag2, rightFlag1, rightFlag2, forwardFlag1, forwardFlag2, downFlag1, downFlag2;
	bool leftFlag, rightFlag, forwardFlag, downFlag;

	bool frogHit = false;
	bool hit = false;										// hit detector flag
	Vector3 newDir;											// next direction for baby
	Vector3 direction;										// current direction for baby
	public LayerMask hitLayer;								// layermask for raycast against maze walls
	
	Vector3[] positions = new Vector3[100];					// array containing previous positions
	private int segAmt = 1;									// total amount of segments (array counter)
	public int initSegments = 1;							// initial number of segments
	private int i = 0;										// array counter
	GameObject[] seg;										// array of segments as child gameobjects
	SpriteRenderer[] segSprite;

	
	bool wallcheckOff = false;
	bool turned = false;									// did object turn?
	int turnDelay = 0;										// delay after turn
	float distX;											// x distance between target and baby
	float distY;											// y distance between target and baby

	bool[] wallCheck = new bool[3];

	HeadScript headScript;

	void Start () {

		segAmt = initSegments;
		seg = new GameObject[segAmt];
		segSprite = new SpriteRenderer [segAmt];
		
		headScript = gameObject.GetComponent<HeadScript>();
		positions = headScript.getPositions();
		
		/*	 create segment child gameObjects at start of game
		for (int cc = 0; cc < segAmt; cc++ ) 
		{
			seg [cc] = (GameObject)Instantiate (BabySegPrefab, transform.position, Quaternion.identity);
			seg [cc].transform.parent = transform.parent;
			seg [cc].transform.position = transform.position;
			segSprite[cc] = seg[cc].GetComponent<SpriteRenderer>();				// segments' sprite renderer handles	
			segSprite[cc].sortingOrder = -1;
		}
		*/
		
		if (GameObject.Find ("playerHomePrefab")!= null)
			target = GameObject.Find ("playerHomePrefab");
		else
			target = null;

	}
	

	void Update() {
			
		// change position based on direction and speed
		transform.position = Vector3.Lerp (transform.position, transform.position + direction * speed, Time.fixedDeltaTime);

		// array of last 100 positions
		positions [i] = transform.position;

		// frame delay after enemy makes a turn before allowing enemy to turn again
		if (turned) 
		{
			turnDelay++;
			print ("turnDelay on " + this.gameObject.transform.parent.name + " = " + turnDelay);
		}
		
		// frame delay after a corner turn before allowing another corner turn (needed for raycasts to hit walls again after turning)
		if (speed != 0)
		{
			if (turnDelay == 48/Mathf.FloorToInt(speed)) 
			{
				turned = false;
				turnDelay = 0;
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
		if (!wallcheckOff)											
		{
			// raycast check for walls
			WallCheck();
			
			// decide how to turn at intersections
			EnemyAI();
		}
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.tag == "frog") 
		{
			frogHit = true;
			Destroy(gameObject);
		}
		
		if (coll.gameObject.tag == "enemy")
		{
			EnemyNavScript enemyScript = coll.gameObject.GetComponent<EnemyNavScript> ();	
			if (enemyScript.getCount () > segAmt)
			{
				Destroy(gameObject);
			}
		}
		//Debug.Log (gameObject.name + " hit " + coll.gameObject.name);
	}

	void WallCheck ()
	{
		float rayDist = 1.5f;
		Vector3 rayPosition = transform.position;
		Vector3 yOffset = new Vector3(0f,0.6f,0f);						// raycast spacing on y axis
		Vector3 xOffset = new Vector3(0.6f,0f,0f);						// raycast spacing on x axis

		// local coordinate raycasts
		bool leftFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left), rayDist, hitLayer);
		bool leftFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left), rayDist, hitLayer);
		bool rightFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right), rayDist, hitLayer);
		bool rightFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right), rayDist, hitLayer);
		bool forwardFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up), rayDist, hitLayer);
		bool forwardFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up), rayDist, hitLayer);
		//downFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down), rayDist, hitLayer);
		//downFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down), rayDist, hitLayer);
		
		// debug raycasts
		Debug.DrawRay (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left) * rayDist, Color.white);
		Debug.DrawRay (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left) * rayDist, Color.white);
		Debug.DrawRay (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right) * rayDist, Color.white);
		Debug.DrawRay (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right) * rayDist, Color.white);
		Debug.DrawRay (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up) * rayDist, Color.white);
		Debug.DrawRay (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down) * rayDist, Color.white);
				
		wallCheck[0] = rightFlag1 || rightFlag2;
		wallCheck[1] = forwardFlag1 || forwardFlag2;
		wallCheck[2] = leftFlag1 || leftFlag2;
		//downFlag = downFlag1 || downFlag2;

		//print ("baby right flag = " + wallCheck[0]);
		//print ("baby forward flag = " + wallCheck[1]);
		//print ("baby left flag = " + wallCheck[2]);

	}

	public void WallCheckOff()
	{
		wallcheckOff = true;
		gameObject.GetComponent<Collider2D>().enabled = false;
		gameObject.GetComponentInChildren<Collider2D>().enabled = false;
	}

	void EnemyAI ()
	{
		int hit = 0;
		
		Vector3[] lookDir = new Vector3[3];							// vector3 direction array corresponding to flags
		lookDir[0] = Vector3.right;
		lookDir[1] = Vector3.up;
		lookDir[2] = Vector3.left;
		
		float[] turnDir = new float[3];								// float local coord z-rotation array corresponding to flags
		turnDir[0] = -90f;
		turnDir[1] = 0f;
		turnDir[2] = 90f;
		
		Vector3 newDir = new Vector3();
		
		for (int aa = 0; aa < 3; aa++)								// get wall hit flag count
		{
			if (wallCheck[aa] == true){
				hit++;
			}
		}
		
		if ((hit == 2) & !turned)									// 2 walls hit, 1 available path
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
		
		if ((hit == 1) & !turned)									// 1 wall hit, 2 available paths
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
					distCheck[q] = (target.transform.position - (transform.position + 2f * distCheckDir[q])).magnitude;
					Debug.DrawLine(transform.position + 2f * distCheckDir[q], target.transform.position, Color.red, 1f);
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
		
		if ((hit == 0) & !turned)									// no wall hits, 3 available paths
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
				distCheck[q] = (target.transform.position - (transform.position + 2f * distCheckDir[q])).magnitude;
				Debug.DrawLine(transform.position + 2f * distCheckDir[q], target.transform.position, Color.cyan, 1f);
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
		if (direction == Vector3.left || direction == Vector3.right) {
			transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y), 0f);
		}
		if ((direction == Vector3.up || direction == Vector3.down))	{
			transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, 0f);
		}
		
	}

	public void GotHome ()
	{
		speed = 0f;
		StartCoroutine (MoveSeg(seg[0].transform.position));
	}

	IEnumerator MoveSeg (Vector3 initSegPos)
	{
		for (int aa = 0; aa <= 1; aa++)	{
			seg[0].transform.position = Vector3.Lerp (initSegPos, transform.position, aa);
			yield return null;
		}
	}
	
}

