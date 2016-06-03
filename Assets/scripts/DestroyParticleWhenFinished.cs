using UnityEngine;
using System.Collections;

public class DestroyParticleWhenFinished : MonoBehaviour
{	

	void Start ()
	{
		// Set the sorting layer of the particle system.
		GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = "Foreground";
		GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingOrder = 2;
	}


	// Update is called once per frame
	void Update ()
	{
		if(!GetComponent<ParticleSystem>().isPlaying)
		{
			Destroy(gameObject);
		}
	}
}
