using UnityEngine;
using System.Collections;

public class frogColliderScript : MonoBehaviour {

	bool wallCheck;
	bool eggCheck;

	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.CompareTag("maze")) 
			wallCheck = true;

		if (coll.CompareTag("egg") || coll.CompareTag("enemyEgg"))
			eggCheck = true;

		//Debug.Log (this.gameObject + " enter hit " + coll.tag);
	} 

	void OnTriggerStay2D(Collider2D coll)
	{
		if (coll.CompareTag("maze")) 
			wallCheck = true;

		if (coll.CompareTag("egg") || coll.CompareTag("enemyEgg"))
			eggCheck = true;

		//Debug.Log (this.gameObject + " stay hit " + coll.tag);
	}

	void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.CompareTag("maze")) 
			wallCheck = true;

		if (coll.CompareTag("egg") || coll.CompareTag("enemyEgg"))
			eggCheck = true;

		//Debug.Log (this.gameObject + " exit hit " + coll.tag);
	} 

	public bool getWallCheck()	{
		return (wallCheck);
	}

	public void setWallCheck(bool set)	{
		wallCheck = set;
	}

	public bool getEggCheck()	{
		return (eggCheck);
	}
	
	public void setEggCheck(bool set)	{
		eggCheck = set;
	}

}
