using UnityEngine;
using System.Collections;

public class enemyGateScript : MonoBehaviour {
	
	public GameObject enemyInvsiGate;
	public GameObject[] enemySnake = new GameObject[3];
    public GameObject newEnemySnake;
    public Transform[] enemyPositions = new Transform[3];
	
	private Transform[] enemyHead = new Transform[3];
	private EnemyNavScript[] enemyScript = new EnemyNavScript[3];
    Animator enemyGateAnim;
	
	Vector3 initGatePos;
	Vector3 [] pos = new Vector3[3];
	int amount = 0;
	float t = 0f;						// timer
	float startTime = 0f;
	float speed = 6f;
	
	
	void Awake () 
	{
		int ii = 0;
		foreach (GameObject enemy in enemySnake) 
		{
			if (enemy.activeInHierarchy)
			{
				enemySnake[ii] = enemy;																	// enemySnake gameobject handles
				enemyHead[ii] = enemySnake[ii].transform.GetChild(0);									// enemy head transform handles
				enemyScript[ii] = enemyHead[ii].GetComponentInChildren<EnemyNavScript>();
				enemyScript[ii].enabled = false;														// enemy head script handles
				//pos[ii] = enemyHead[ii].transform.position;											
				enemyHead[ii].GetComponent<Collider2D>().enabled = false;
				ii++;
			}
		}
		
		for (int aa = 0; aa < pos.Length; aa++)
		{
			pos[aa] = enemyPositions[aa].position;														// store starting enemy positions
			Debug.Log ("enemy position = " + pos[aa]);
		}
		
		//initGatePos = transform.position;
        if (GameObject.Find ("enemyGateAnim")!= null)
        {
            enemyGateAnim = GameObject.Find ("enemyGateAnim").GetComponent<Animator>();
        }
	}
	
	IEnumerator Start () 
    {
		for (int aa = 0; aa < 3; aa++)
		{
			StartCoroutine (GateRoutine());										// gate routine to release enemy
			yield return new WaitForSeconds(6f);
		}
		StopAllCoroutines();
	}
	
	IEnumerator GateRoutine()
	{
		//StartCoroutine (MoveEnemyGate(new Vector3(2, 0, 0)));				// open gate

        //GameObject.Find ("enemyGateAnim").GetComponent<Animator>().enabled = true;
        enemyGateAnim.SetBool("openGate",true);
        yield return new WaitForSeconds(2f);
        
		StartCoroutine(ActivateEnemy());									// release enemy from inside gate
		yield return new WaitForSeconds(0.25f);
		
		StartCoroutine(EnemyRayOn());										// turn released enemy wallcheck raycasts back on
		yield return new WaitForSeconds(0.25f);
		
		//StartCoroutine (MoveEnemyGate(new Vector3(-2, 0, 0)));				// close gate
        enemyGateAnim.SetBool("openGate",false);
		yield return new WaitForSeconds(2f);
		
		amount ++;
		if (amount == 1)
			StartCoroutine(ShiftEnemies1());								// shift enemies inside gate area
		else if (amount == 2)
			StartCoroutine(ShiftEnemies2());
		else
			yield return null;
		
		yield return null;
	}
	
	IEnumerator ShiftEnemies1()
	{
        if (enemyHead[1] != null & enemyHead[2] != null)
        {
    		for (float t = 0f; t < 1.0f; t += Time.deltaTime)
    		{
    			enemyHead[1].transform.position = Vector3.Lerp (pos[1], pos[0], t);
    			enemyHead[2].transform.position = Vector3.Lerp (pos[2], pos[1], t);
    			yield return null;
    		}
        }
	}
	
	IEnumerator ShiftEnemies2()
	{
        if (enemyHead[2] != null)
        {
    		for (float t = 0f; t < 1.0f; t += Time.deltaTime)
    		{
    			enemyHead[2].transform.position = Vector3.Lerp (pos[1], pos[0], t);
    			yield return null;
    		}
        }
	}
	
	IEnumerator ActivateEnemy()
	{
		//enemyHead[amount].collider2D.enabled = false;
        if (enemyHead[amount] != null)
        {
    		enemyScript[amount].enabled = true;
    		enemyInvsiGate.SetActive(true);												// turn on invisible wall inside gate area
            if (enemyHead[amount].transform.position.y < pos[2].y)						// if enemy is inside gate turn off up facing wall check raycast to pass thru wall
    		{
                enemyScript[amount].isGateOpen(true);		
            }
        }						
		yield return null;
	}
	
	IEnumerator EnemyRayOn ()
	{
        if (enemyHead[amount] != null)
        {
    		enemyHead[amount].GetComponent<Collider2D>().enabled = true;
    		enemyScript[amount].isGateOpen(false);									    // turn enemy wallcheck raycasts back on
    		enemyInvsiGate.SetActive(false);
        }										
		yield return null;
	}
	
	IEnumerator MoveEnemyGate(Vector3 gateChange)
	{
		for (float t = 0f; t < 1.0f; t += Time.deltaTime)
		{
			transform.position = Vector3.Lerp(initGatePos, initGatePos + gateChange, t);
			yield return null;
		}
		transform.position = initGatePos + gateChange;
		initGatePos = transform.position;
	}
    
    public void NewLevel()
    {
        /*
        GameObject[] oldEnemies = GameObject.FindGameObjectsWithTag("enemy");
        foreach (GameObject oldEnemy in oldEnemies)
        {
            Destroy (oldEnemy.transform.parent.gameObject);
        } 
        */
    
        for (int ii=0; ii<3; ii++)
        {
            enemySnake[ii] = Instantiate(newEnemySnake, pos[ii], Quaternion.identity) as GameObject;                                                                 
            enemyHead[ii] = enemySnake[ii].transform.GetChild(0);                                   
            enemyScript[ii] = enemyHead[ii].GetComponentInChildren<EnemyNavScript>();
            enemyScript[ii].enabled = false;                                                        
            //pos[ii] = enemyHead[ii].transform.position;                                             // store 
            enemyHead[ii].GetComponent<Collider2D>().enabled = false;
        }
        initGatePos = transform.position;
        amount = 0;
        StartCoroutine(Start());
        
    }
	
}
