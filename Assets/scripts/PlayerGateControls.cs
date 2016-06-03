using UnityEngine;
using System.Collections;

public class PlayerGateControls : MonoBehaviour 
{
    GameObject head;
    GameObject playerGate;
    PlayerGateScript plyrGateScript;
    //GameObject playerGateMat;
    GameObject playerGateAnimPrefab;
    HeadScript headScript;
    
    float speed;
    Vector3 initGatePos;
	bool gavePlyrControl;
    bool isDone;
    
    Animator playerGateAnim;
    
    void Awake()
    {
	
        isDone = false;
            
        if (GameObject.Find ("playerGate") != null)
        {
            playerGate = GameObject.Find ("playerGate");
            plyrGateScript = playerGate.GetComponent<PlayerGateScript>();
        }
        
        if (GameObject.Find ("playerGateAnim") != null)
        {
            playerGateAnimPrefab = GameObject.Find ("playerGateAnim");
            playerGateAnim = playerGateAnimPrefab.GetComponent<Animator>();
			//playerGateAnim.SetBool("openGate",true);
        }

        GetNewHead();
    }
    
    void GetNewHead()
    {
        if (GameObject.FindGameObjectWithTag("head") != null)
        {
            head = GameObject.FindGameObjectWithTag("head");
            headScript = head.GetComponent<HeadScript>();
        }
    }
    
    void Update()
    {

        if (head == null)
        {
            GetNewHead();
            gavePlyrControl = false;
        }
        
        if (head != null & gavePlyrControl == false)
        {
           	if (head.transform.position.y >= -15.5f) 
            {
                GivePlayerControls();
                gavePlyrControl = true;
            }
        }

    }
    

    public IEnumerator StartGate()
    {
        speed = 0f;
        
        GetNewHead();
                 
        //StartCoroutine (MoveGate(new Vector3(-2, 0, 0)));
        playerGateAnim.SetBool("openGate",true);
        yield return new WaitForSeconds(2f);
        
        StartCoroutine (GateMovePlayer());
        yield return new WaitForSeconds(0.2f);
        
        //StartCoroutine (MoveGate(new Vector3(2, 0, 0)));
        playerGateAnim.SetBool("openGate",false);
        yield return new WaitForSeconds(1.5f);
        
        StartCoroutine (ShiftPlayerIcons());
        isDone = true;
    }
    
    IEnumerator ShiftPlayerIcons()
    {
        Debug.Log ("ShiftPlayerIcons Method, livesCount = " + SceneManager.Instance.livesCount);
        // shift player snake icons inside gate area, livesCount after player leaves gate
        if (SceneManager.Instance.livesCount >= 3)
        {
            plyrGateScript.playerIcon3.SetActive(true);
            plyrGateScript.playerIcon2.SetActive(false);
            plyrGateScript.playerIcon1.SetActive(false);
            yield return null;
            StartCoroutine(plyrGateScript.Shift3to2(true));
            StartCoroutine(plyrGateScript.Shift2to1());
            yield return null;
            //plyrGateScript.playerIcon2.SetActive(true);
            //plyrGateScript.playerIcon1.SetActive(true);
            //yield return null;
        }
        else if (SceneManager.Instance.livesCount == 2)
        {   
            plyrGateScript.playerIcon3.SetActive(false);
            plyrGateScript.playerIcon2.SetActive(false);
            plyrGateScript.playerIcon1.SetActive(false);
            yield return null;
            StartCoroutine(plyrGateScript.Shift3to2(false));
            StartCoroutine(plyrGateScript.Shift2to1());
            yield return null;
            //plyrGateScript.playerIcon2.SetActive(true);
            //plyrGateScript.playerIcon1.SetActive(true);
            //yield return null;
        }
        else if (SceneManager.Instance.livesCount == 1)
        {   
            plyrGateScript.playerIcon3.SetActive(false);
            plyrGateScript.playerIcon2.SetActive(false);
            plyrGateScript.playerIcon1.SetActive(false);
            yield return null;
            yield return StartCoroutine(plyrGateScript.Shift2to1());
            //plyrGateScript.playerIcon1.SetActive(true);
            //yield return null;
            //plyrGateScript.playerIcon1.SetActive(true);
            //yield return null;
        }
        else 
        {   
            plyrGateScript.playerIcon3.SetActive(false);
            plyrGateScript.playerIcon2.SetActive(false);
            plyrGateScript.playerIcon1.SetActive(false);
            yield return null;
        }
    }

    IEnumerator GateMovePlayer()
    {
        head.GetComponent<Collider2D>().enabled = false;
        headScript.speed = headScript.startSpeed;
        headScript.direction = Vector3.up;
        headScript.newDir =  headScript.direction;
        SceneManager.Instance.livesCount--;
        plyrGateScript.playerIcon1.SetActive(false);
        yield return null;
    }
    
    void GivePlayerControls()
    {
        if (head.GetComponent<Collider2D>().enabled == false)
        {
            head.GetComponent<Collider2D>().enabled = true;
        }
        
        if (headScript.startGate == true)
        {
            headScript.startGate = false;
        }
    }   
}
