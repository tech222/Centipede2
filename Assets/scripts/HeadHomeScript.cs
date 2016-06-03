using UnityEngine;
using System.Collections;

public class HeadHomeScript : MonoBehaviour {

	public float speed = 12.0f;								// speed for baby
	public GameObject target;								// target for baby to baby move toward
	
	RaycastHit castHit;										// raycast for hit detection
	Vector3 yOffset = new Vector3(0f,0.5f,0f);				// raycast spacing on y axis
	Vector3 xOffset = new Vector3(0.5f,0f,0f);				// raycast spacing on x axis
	
	bool leftFlag1, leftFlag2, rightFlag1, rightFlag2, forwardFlag1, forwardFlag2, downFlag1, downFlag2;
	bool leftFlag, rightFlag, forwardFlag, downFlag;
	
	
	bool frogHit = false;
	bool hit = false;										// hit detector flag
	Vector3 newDir;											// next direction for baby
	Vector3 direction;										// current direction for baby
	public LayerMask hitLayer;								// layermask for raycast against maze walls
	
	Vector3[] positions = new Vector3[100];					// array containing previous positions
	private int segAmt = 1;									// total amount of segments (array counter)
	private int i = 0;										// array counter
	GameObject[] seg;										// array of segments as child gameobjects
	SpriteRenderer[] segSprite;

	[HideInInspector]
	public int initSegments = 1;							// initial number of segments
	
	bool wallcheckOff = false;
	bool turned = false;									// did object turn?
	int turnDelay = 0;										// delay after turn
	float distX;											// x distance between target and baby
	float distY;											// y distance between target and baby
	
	bool[] wallCheck = new bool[3];


	public void setDirection(Vector3 newDirection)
	{
		direction = newDirection;
		target = GameObject.Find("playerHomePrefab");
	}

	public void TurnDelay ()
	{
		// frame delay after enemy makes a turn before allowing enemy to turn again
		if (turned) 
		{
			turnDelay++;
		}
		
		// frame delay after a corner turn before allowing another corner turn (needed for raycasts to hit walls again after turning)
		if (speed != 0)
		{
			if (turnDelay > 0)
			{
				turned = false;
				turnDelay = 0;
			}
		}
		//print ("turnDelay on " + this.gameObject.transform.parent.name + " = " + turnDelay);
		//print ("turned on " + this.gameObject.transform.parent.name + " = " + turned	);
	}

	public void WallCheck ()
	{
		float rayDist = 1.2f;
		Vector3 rayPosition = transform.position;
		Vector3 yOffset = new Vector3(0f,0.5f,0f);						// raycast spacing on y axis
		Vector3 xOffset = new Vector3(0.5f,0f,0f);						// raycast spacing on x axis
		
		// local coordinate raycasts
		leftFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left), rayDist, hitLayer);
		leftFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left), rayDist, hitLayer);
		rightFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right), rayDist, hitLayer);
		rightFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right), rayDist, hitLayer);
		forwardFlag = Physics2D.Raycast (rayPosition, transform.TransformDirection(Vector3.up), rayDist, hitLayer);
		//forwardFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up), rayDist, hitLayer);
		//downFlag1 = Physics2D.Raycast (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down), rayDist, hitLayer);
		//downFlag2 = Physics2D.Raycast (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down), rayDist, hitLayer);
		
		// debug raycasts
		Debug.DrawRay (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left) * rayDist, Color.white);
		Debug.DrawRay (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.left) * rayDist, Color.white);
		Debug.DrawRay (rayPosition + transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right) * rayDist, Color.white);
		Debug.DrawRay (rayPosition - transform.TransformDirection(yOffset), transform.TransformDirection(Vector3.right) * rayDist, Color.white);
		Debug.DrawRay (rayPosition, transform.TransformDirection(Vector3.up) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.up) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition + transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down) * rayDist, Color.white);
		//Debug.DrawRay (rayPosition - transform.TransformDirection(xOffset), transform.TransformDirection(Vector3.down) * rayDist, Color.white);

		
		
		wallCheck[0] = rightFlag1 || rightFlag2;
		wallCheck[1] = forwardFlag;
		wallCheck[2] = leftFlag1 || leftFlag2;
		//downFlag = downFlag1 || downFlag2;
	}

	public Vector3 AI()
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

		return (newDir);
	}
		
	public void WallCheckOff()
	{
		wallcheckOff = true;
		gameObject.GetComponent<Collider>().enabled = false;
		gameObject.GetComponentInChildren<Collider>().enabled = false;
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

