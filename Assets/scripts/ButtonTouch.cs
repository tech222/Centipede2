using UnityEngine;
using System.Collections;

public class ButtonTouch : MonoBehaviour {

    public GameObject particle;
    
    void Update() 
    {
        int i = 0;
    
        while (i < Input.touchCount) 
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began) 
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                //Debug.DrawLine(Camera.main.transform.position, transform.position, Color.magenta);
                Debug.DrawRay(ray.origin, ray.direction, Color.magenta, 1f);
                if (Physics.Raycast(ray))
                    Instantiate(particle, transform.position, transform.rotation);
                
            }
            ++i;
        }
    }
}
