using UnityEngine;
using System.Collections;

public class SegmentScript : MonoBehaviour {

	public float timeTillEgg = 24f;					// time until segment starts egg
	public float segTime = 0f;						// timer
	Animator anim;									// handle to egg animation
	public bool eggState = false;					// egg status, true = lay egg, false = no egg
	
    [HideInInspector]
    public bool segHit = false;
    
    [HideInInspector]
	public bool clipDone = false;
    
	Animation layEgg;
    SpriteRenderer spriteRend;

	int layEggHash = Animator.StringToHash("layEgg");
	int eggSegStateHash = Animator.StringToHash("Base Layer.eggSegAnim");

	// Use this for initialization
	void Start () 
    {	
        SpriteRenderer segSprite = GetComponent<SpriteRenderer>();
        int layerOrder = segSprite.GetComponent<Renderer>().sortingOrder;
        
        spriteRend = transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRend.GetComponent<Renderer>().sortingOrder = layerOrder + 5;
        //spriteRend.enabled = false;
        
        anim = GetComponentInChildren<Animator>();
		anim.SetBool (layEggHash, false); 
        layEgg = GetComponentInChildren<Animation>();
		transform.position = GameObject.Find ("Head").transform.position;
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
        }

		if (segTime > timeTillEgg)
        {
			StartCoroutine(PlayAnimation());
        }
	}

	public IEnumerator PlayAnimation()
	{
		anim.SetBool (layEggHash, true);
		yield return new WaitForSeconds(layEgg.clip.length + 1f);
		clipDone = true;
	}

	public void resetSeg()                                  // if seg gets added to snake while egg anim is playing reset seg to original state
	{
		segHit = false;
		eggState = false;
		clipDone = false;
		
		//AnimatorStateInfo animInfo = anim.GetCurrentAnimatorStateInfo(0);
		//if (animInfo.nameHash == eggSegStateHash)
		//{
			//AnimationState currState = anim.animation["eggSegAnim"];
			//currState.time = 0.0f;
			//currState.enabled = true;
			//animation.Sample();
			//currState.enabled = false;
			anim.SetBool (layEggHash, false);
			anim.StopPlayback();
			spriteRend.sprite = null;
			spriteRend.color = Color.white;
		//}
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
		if (coll.gameObject.tag == "enemy") 
		{
			//StartCoroutine (SetSegHit());
			segHit = true;
			//Debug.Log (name + " hit " + coll.name);
		}
	}
	
	IEnumerator SetSegHit()
	{
		yield return new WaitForEndOfFrame();
		segHit = true;
	}

}
