using UnityEngine;
using System.Collections;

public class CameraRotation : MonoBehaviour 
{
    public Transform player;

	// Use this for initialization
	void Start () 
    {
        
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (player == null)
        {
            player = GameObject.Find("Head").transform;
        }
        
        transform.position = new Vector3 (player.transform.position.x, player.transform.position.y, -12f );
	
	}
}
