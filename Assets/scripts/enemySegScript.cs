using UnityEngine;
using System.Collections;

public class enemySegScript : MonoBehaviour {
	
	public float timeTillEgg = 24f;					// time until segment starts egg
	public float segTime = 0f;						// timer
	Animator animSeg;							    // handle to segment animation
    Animator animEgg;                               // handle to egg animation on child
    SpriteRenderer spriteRend;
    
	public bool eggState = false;					// egg status, true = lay egg, false = no egg
	
	[HideInInspector]
	public bool segHit = false;
	
	[HideInInspector]
	public bool clipDone = false;
	
	//public bool enemyWeak = false;
	int layEggHash = Animator.StringToHash("layEgg");


	// Use this for initialization
	void Start () 
	{	
		SpriteRenderer segSprite = GetComponent<SpriteRenderer>();                      // get sorting layer for sprite
        int layerOrder = segSprite.GetComponent<Renderer>().sortingOrder;
        
        spriteRend = transform.GetChild(0).GetComponent<SpriteRenderer>();              // set egg sprite sorting layer
        spriteRend.GetComponent<Renderer>().sortingOrder = layerOrder + 5;
        spriteRend.enabled = false;                                                     // disable sprite renderer on egg
        

        animEgg = transform.GetChild(0).GetComponent<Animator>();                       // seg egg animator component
		animEgg.SetBool (layEggHash, false); 

        
        
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (eggState == true)
		{
			segTime += Time.deltaTime;
		}
		else
		{
			segTime = 0f;
			spriteRend.enabled = false;
			animEgg.SetBool (layEggHash, false);
		}
		
		if (segTime > timeTillEgg)
		{
			spriteRend.enabled = true;
			StartCoroutine(PlayEggAnimation());
		}
	}
	
	public IEnumerator PlayEggAnimation()
	{
		animEgg.SetBool (layEggHash, true);
		yield return new WaitForSeconds(7f);
		clipDone = true;
	}

	public void resetSeg()
	{
		segHit = false;
		segTime = 0f;
		eggState = false;
		clipDone = false;
		if (animEgg.GetBool(layEggHash) == true)
		{
			animEgg.SetBool (layEggHash, false);
			animEgg.StopPlayback();
			spriteRend.sprite = null;
			spriteRend.color = Color.white;
		}
	}
	
	public void startEggSeg()
	{
		segHit = false;
		segTime = 0f;
		eggState = true;
		clipDone = false;
	}
	
	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.tag == "head") 
		{
			segHit = true;
			//Debug.Log (name + " hit " + coll.name);
		}
		
	}

	
}
