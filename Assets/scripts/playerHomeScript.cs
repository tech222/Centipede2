using UnityEngine;
using System.Collections;

public class playerHomeScript : MonoBehaviour 
{
    GameObject playerGate;
    PlayerGateScript plyrGateScript;
    GameObject playerGateMat;
    GameObject movingHead;
    public GameObject[] headList = new GameObject[100];
    BabyHeadScript babyScript;
    
    Vector3 startMovePos;
    Vector3 startGatePos;
    Vector3 initHeadPos;
    
    //GameObject sceneManager;
    //SceneManager sceneManagerScript;
    HeadScript headScript;
    
    bool flag = false;
    bool isHead = false;
    int headListindex = 0;
    
    float xx;
    
    Animator babyGateAnim;
    
    // Use this for initialization
    void Start () 
    {
        /*
        if (GameObject.Find ("SceneManager")!= null)
        {
            sceneManager = GameObject.Find ("SceneManager");
            sceneManagerScript = sceneManager.GetComponent<SceneManager> ();
        }
        */
        
        if (GameObject.Find ("playerGate")!= null)
        {
            playerGate = GameObject.Find ("playerGate");
            plyrGateScript = playerGate.GetComponent<PlayerGateScript>();
        }
        
        if (GameObject.Find ("playerBabyGateMat")!= null)
        {
            playerGateMat = GameObject.Find ("playerBabyGateMat");
            startGatePos = playerGateMat.transform.position;
        }
        
        if (GameObject.Find ("plyrBabyGateAnim")!= null)
        {
            
            babyGateAnim = GameObject.Find ("plyrBabyGateAnim").GetComponent<Animator>();
            //babyGateAnim.SetBool("openGate", false);
            //babyGateAnim.SetBool("cloaseGate", false);
        }
        
    }
    
    void Update()
    {
        //Debug.Log (babyGateAnim.GetBool("openGate"));
    }
    
    IEnumerator GateProcess (GameObject movingHead) 
    {
        //StartCoroutine ("OpenGate");                                                  // open gate
        //Debug.Log ("opening babygate");
        GameObject.Find ("plyrBabyGateAnim").GetComponent<Animator>().enabled = true;
        babyGateAnim.SetBool("openGate",true);
        yield return new WaitForSeconds (1f);
        StartCoroutine (MoveBabyIntoGate(movingHead));                                  // move baby into gate area
        yield return new WaitForSeconds (1f);
        //StartCoroutine ("CloseGate");                                                 // close gate
        babyGateAnim.SetBool("openGate",false);
        //Debug.Log ("closing babygate");
        yield return new WaitForSeconds (1f);
        StartCoroutine (CleanUp(movingHead));                                           // stop all invokes & coroutines, destroy baby, add life
        yield return null;
    }
    
    IEnumerator CleanUp(GameObject movingHead)
    {
        if (movingHead != null)
        {
            //movingHead.SetActive(false);
			Destroy(movingHead.transform.parent.gameObject);
			headListindex--;
			
            yield return StartCoroutine (plyrGateScript.SetGate());                         // move player icons inside gate    
            SceneManager.Instance.livesCount++;                                             // add 1 life to player    
                
            if (isHead & SceneManager.Instance.getEnemyCount() > 0)
            {
                SceneManager.Instance.NewPlayer();
                //isHead = false;
            }
            else if (isHead & SceneManager.Instance.getEnemyCount() == 0)
            {
                SceneManager.Instance.NextLevel ();
                //isHead = false;
            }
            isHead = false;
            //babyGateAnim.SetBool("openGate", false);
            //babyGateAnim.SetBool("closeGate", false);
        }
            
        yield return null;
    }
    
    IEnumerator MoveBabyIntoGate(GameObject movingHead)
    {
        if (!flag)
        {
            initHeadPos = movingHead.transform.position;
            xx = 0;
            flag = true;
        }
        if (isHead == true)
        {
			StartCoroutine(headScript.foldSnakeAtGate());
        }	
        while (xx <= 1f)
        {
            xx += Time.deltaTime;
            movingHead.transform.position = Vector3.Lerp (initHeadPos, initHeadPos + new Vector3 (3, 0, 0), xx);
            yield return null;
        }
        movingHead.transform.position = initHeadPos + new Vector3 (3, 0, 0);
        flag = false;
        yield return null;
  
    }
    
    void OnTriggerEnter2D(Collider2D coll)
    {
        //print (gameObject.name + "home hit" + coll.gameObject.name);
        if (coll.gameObject.tag == "baby")
        {
            //Debug.Log (coll.name + " hit playerhomegate trigger");
            //movingHead = coll.gameObject;
            //startMovePos = movingHead.transform.position;
            
            //startMovePos = headList[headListindex].transform.position;
            coll.gameObject.transform.eulerAngles = new Vector3(0, 0, -90);         // rotate face to look towared gate
            headList[headListindex] = coll.gameObject;
            babyScript = coll.gameObject.GetComponent<BabyHeadScript>();
            babyScript.GotHome ();                                                          // stop baby speed to zero when it hits playerhome collider
            babyScript.WallCheckOff();                                                      // disable wallcheck raycasts on babyhead
            StartCoroutine (GateProcess(headList[headListindex])); 
            headListindex++;
        }
        
        if (coll.gameObject.tag == "head")
        {
            headScript = coll.GetComponent<HeadScript>();
            if (headScript.getGoingHome())
            {
                //Debug.Log (coll.name + " hit playerhomegate trigger");
                isHead = true;
                //movingHead = coll.gameObject;
                //startMovePos = movingHead.transform.position;
                headList[headListindex] = coll.gameObject;
                //startMovePos = headList[headListindex].transform.position;
                headList[headListindex].transform.eulerAngles = new Vector3(0, 0, -90);     // rotate face to look towared gate
                
                headScript.AIWallCheckOff();
                //StartCoroutine(headScript.foldSnakeAtGate());
                StartCoroutine (GateProcess(headList[headListindex]));
                headListindex++;
            }   
        }
    }
}
