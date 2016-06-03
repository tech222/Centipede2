using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneManager : MonoBehaviour 
{
    public static SceneManager Instance
    {
        get
        {
            if(_instance != null)
            {
                return _instance;
            }
            else
            {
                GameObject sceneManager = new GameObject("SceneManager");
                _instance = sceneManager.AddComponent<SceneManager>();
                return _instance;
            }
        }
    }
    
    private static SceneManager _instance;
    private GameObject nextLevel;
    public GameObject[] levels;
	public GameObject playerSnake;
	GameObject mySnake;
	GameObject playerHead;
	HeadScript headScript;
    
    [HideInInspector]
    public Vector3 initPlayerPos;

	[HideInInspector]
	public int livesCount;			        // number of player lives
    
    [HideInInspector]
    public static int score = 0;              

    public int initLives = 3;				// amount of player lives to start game

	int enemiesCount = 0;					// number of enemies
	int enemyEggCount = 0;					// number of enemy eggs
	int playerEggCount = 0;					// number of player eggs
    
    [HideInInspector]
    public int levelIndex;
    
	public GameObject enemySplosion;		// explosion for any enemy eggs left on screen after calling NextLevel()

	bool paused = false;
	bool isPaused = false;
	bool isGameOver = false;
	bool isPlaying = true;

	void Start ()
	{
        if(_instance != this)
        {
            if(_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        DontDestroyOnLoad(gameObject);
    
    
        //levelIndex = Application.loadedLevel;
        levelIndex = 1;
        
        if (initLives < 1)
        {
            initLives = 1;
        }
		livesCount = initLives;
        /*
        if (GameObject.FindGameObjectWithTag ("playerSnake") != null) 
        {
            if (mySnake != GameObject.FindGameObjectWithTag ("playerSnake"))
    		{
    			mySnake = GameObject.FindGameObjectWithTag ("playerSnake");
    			playerHead = mySnake.transform.GetChild (0).gameObject; 							// get new player head gameobject
    			headScript = playerHead.GetComponent<HeadScript>();
                initPlayerPos = playerHead.transform.position;
    		}
        }
        */
        /*
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");						// array of all enemies in hierarchy
		foreach (GameObject enemy in enemies) 
		{
			if (enemy != null)
			{
				if (enemy.name == "Enemy")  
				{
					enemiesCount++;
				}
			}
		}
        */
       
	}

	public void addEnemy ()
	{
		enemiesCount++;
	}

	public void subtractEnemy()
	{
		enemiesCount--;
	}
    
     public int getEnemyCount()
    {
        return enemiesCount;
    }
    
    public void NextLevel ()
    {
        //Application.LoadLevel(levelIndex);
        //string lvlName = "screenMaze_0" + levelIndex.ToString();
        GameObject finalEggCheck = GameObject.FindGameObjectWithTag("egg");
        GameObject finalBabyCheck = GameObject.FindGameObjectWithTag("baby");
        GameObject[] finalEnemyEggCheck = GameObject.FindGameObjectsWithTag("enemyEgg");
        GameObject[] finalEnemyCheck = GameObject.FindGameObjectsWithTag("enemy");
        GameObject finalFrogCheck = GameObject.FindGameObjectWithTag("frog");
        
        if (finalEnemyEggCheck != null)											// destroy any remaining enemy eggs
        {
        	foreach (GameObject enemyEgg in finalEnemyEggCheck)
        	{
        		Destroy (enemyEgg);
				//Instantiate (enemySplosion, transform.position, Quaternion.identity);
        	}
        }
        
		if (finalEnemyCheck != null)											// destroy any remaining enemies
		{
			foreach (GameObject enemy in finalEnemyCheck)
			{
				Destroy (enemy.transform.parent);
				//Instantiate (enemySplosion, transform.position, Quaternion.identity);
			}
		}	
		if (finalFrogCheck.GetComponent<SpriteRenderer>() == true)							// reset frog if remaining
		{
			finalFrogCheck.GetComponent<frogMoveScript>().newFrog();
		}
		       
        if (finalEggCheck == null & finalBabyCheck == null)             		//check for active egg or baby before starting new level
        {
            GameObject maze = GameObject.FindGameObjectWithTag("level");
            Destroy(maze);
            levelIndex++;
            Instantiate(levels[levelIndex-1], Vector3.zero, Quaternion.identity);
            GameObject.Find("enemyGateAnim").GetComponent<enemyGateScript>().NewLevel();
            NewPlayer();
        }
        else                                                            		// if active egg or baby delay for 1 second and recheck
        {   
            StartCoroutine (WaitTillEnd());
            Debug.Log ("egg and/or baby active, waiting to load next level");
        }
    }
    
    IEnumerator WaitTillEnd()
    {
        yield return new WaitForSeconds(1f);
        NextLevel();
    }

	public void NewPlayer()
	{
        //if (!GameObject.Find("Head").activeInHierarchy)
        //{
    		//mySnake = (GameObject) Instantiate(Resources.Load<GameObject>("playerSnakeResource"), new Vector3(0,0,0), Quaternion.identity);	        // for testing only		
            mySnake = (GameObject) Instantiate(playerSnake, new Vector3(0,0,0), Quaternion.identity);           // new player empty gameobject
    		playerHead = mySnake.transform.GetChild (0).gameObject; 											// new player head child gameobject
    		headScript = playerHead.GetComponent<HeadScript>();
            playerHead.transform.position = initPlayerPos;
            
    
    		GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");						    // array of all enemies in hierarchy	
    		//print (enemies.Length);
    
    		foreach (GameObject enemy in enemies) 
    		{
    			if (enemy != null)
    			{
    				if (enemy.name == "Enemy")  
    				{
    					enemy.GetComponent<EnemyNavScript>().ResetTarget(playerHead);					// reset target on all active enemies to new player
    					print ("found " + enemy.name + " at " + enemy.transform.position);
    				}
    			}
    		}
        //}
	}

	public void playerDied ()	
	{
		if (livesCount > 0 & enemiesCount > 0) 
		{
			//Destroy(mySnake);
			Invoke ("NewPlayer", 0.5f);
		}
		else
		{
			//Destroy(mySnake);
			isGameOver = true;
		}
	}

}





