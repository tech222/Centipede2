using UnityEngine;
using System.Collections;

public class FrogHit : MonoBehaviour 
{
	public float hitForce = 20f;
	public float hitTorque = 10f;
	public ForceMode2D fMode;
	
	void Start()
	{
		GetComponent<Rigidbody2D>().AddForce(hitForce * transform.up, fMode);
		GetComponent<Rigidbody2D>().AddTorque(hitTorque, fMode);
	}

}
