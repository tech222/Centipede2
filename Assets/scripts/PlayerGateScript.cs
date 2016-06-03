using UnityEngine;
using System.Collections;

public class PlayerGateScript : MonoBehaviour 
{
    [HideInInspector]
    public GameObject playerIcon1;
    
    [HideInInspector]
    public GameObject playerIcon2;
    
    [HideInInspector]
    public GameObject playerIcon3;
    //GameObject playerIconShift1;
    //GameObject playerIconShift2;
    
    public GameObject playerIconTemp;
    
    Vector3 plyrIcon1pos;
    Vector3 plyrIcon2pos;
    Vector3 plyrIcon3pos;
    
    int plyrLives;
    
    //int plyrLives;

	// Use this for initialization
	void Start () 
    {
        playerIcon1 = GameObject.Find("playerIcon1");
        playerIcon2 = GameObject.Find("playerIcon2");
        playerIcon3 = GameObject.Find("playerIcon3");
        
        plyrIcon1pos = playerIcon1.transform.position;
        plyrIcon2pos = playerIcon2.transform.position;
        plyrIcon3pos = playerIcon3.transform.position;
        
        plyrLives = SceneManager.Instance.livesCount;        
        if (plyrLives == 1)
        {
            playerIcon3.SetActive(false);
            playerIcon2.SetActive(false);
        }
        else if (plyrLives == 2)
        {
            playerIcon3.SetActive(false);
        }   
        
        //playerIcon1.SetActive(false);
	    
        //playerIconShift1 = Instantiate (playerIconTemp, plyrIcon2pos, Quaternion.identity) as GameObject; 
        //playerIconShift2 = Instantiate (playerIconTemp, plyrIcon2pos, Quaternion.identity) as GameObject;              

	}
    
    public IEnumerator SetGate()
    {   
        Debug.Log ("SetGate Method, playerLives = " + SceneManager.Instance.livesCount);
        
		if (!playerIcon1.activeSelf)													// if icon1 is off go to icon1 position
		{
			yield return StartCoroutine(Shift3to1());
			playerIcon1.SetActive(true);
		}
		else if (playerIcon1.activeSelf & !playerIcon2.activeSelf)						// if icon1 is on & icon2 is off go to icon2 position
		{
			yield return StartCoroutine(Shift3to2(false));								
			playerIcon2.SetActive(true);
		}	
		else 																			// if icon1 & icon2 are both on go to icon3 position
		{
			playerIcon3.SetActive(true);												
			yield return null;
		}
        
        
        /*
        if (SceneManager.Instance.livesCount == 0)
        {
            yield return StartCoroutine(Shift3to1());
            playerIcon1.SetActive(true);
        }
        else if (SceneManager.Instance.livesCount == 1)
        {
            yield return StartCoroutine(Shift3to2(false));
            playerIcon2.SetActive(true);
        }
        else if (SceneManager.Instance.livesCount >= 2)
        {
            playerIcon3.SetActive(true);
            yield return null;
        }
        */
    }
        
    public IEnumerator Shift2to1()
    {  
        playerIcon1.SetActive(false);   
        playerIcon2.SetActive(false);
        GameObject plyrIconTemp = Instantiate (playerIconTemp, plyrIcon2pos, Quaternion.identity) as GameObject;
        for (float t = 0f; t < 1.0f; t += Time.deltaTime)
        {
            plyrIconTemp.transform.position = Vector3.Lerp (plyrIcon2pos, plyrIcon1pos, t);
            yield return null;
        }
        Destroy(plyrIconTemp);
        playerIcon1.SetActive(true);
    }
    
    public IEnumerator Shift3to2(bool icon3)
    {   
        playerIcon2.SetActive(false);
		playerIcon3.SetActive(icon3);
        GameObject plyrIconTemp = Instantiate (playerIconTemp, plyrIcon3pos, Quaternion.identity) as GameObject;
        for (float t = 0f; t < 1.0f; t += Time.deltaTime)
        {
            plyrIconTemp.transform.position = Vector3.Lerp (plyrIcon3pos, plyrIcon2pos, t);
            yield return null;
        }
        Destroy(plyrIconTemp);
        playerIcon2.SetActive(true);
    }
    
    public IEnumerator Shift3to1()
    {   
        playerIcon1.SetActive(false);
        playerIcon2.SetActive(false);
        playerIcon3.SetActive(false);
        GameObject plyrIconTemp = Instantiate (playerIconTemp, plyrIcon3pos, Quaternion.identity) as GameObject;
        for (float t = 0f; t < 1.0f; t += Time.deltaTime)
        {
            plyrIconTemp.transform.position = Vector3.Lerp (plyrIcon3pos, plyrIcon2pos, t);
            yield return null;
        }
        for (float t = 0f; t < 1.0f; t += Time.deltaTime)
        {
            plyrIconTemp.transform.position = Vector3.Lerp (plyrIcon2pos, plyrIcon1pos, t);
            yield return null;
        }
        Destroy(plyrIconTemp);
    }
    
    /*
    public IEnumerator Shift3to1()
    {
        StartCoroutine (Shift3to1()); 
        //yield return StartCoroutine(Shift3to2());  
        //yield return StartCoroutine(Shift2to1());
        //yield return null;
    }
    */
}
