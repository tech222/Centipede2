using UnityEngine;
using System.Collections;

public class PseudoInputOnTouch : MonoBehaviour
{
	public enum PseudoInputDirecton {Left, Right, Up, Down}
	public PseudoInputDirecton direction;
    
    bool pressDelay = false;
    int delayCount = 0;
    public int minFramesBtwnPresses = 10;

	void Touched()
	{
        // frame delay between presses
        if (pressDelay)
        {
            delayCount++;
            PseudoInput.Instance.leftPressed = false;
            PseudoInput.Instance.rightPressed  = false;
            PseudoInput.Instance.upPressed = false;
            PseudoInput.Instance.downPressed = false;
        }
        
        if (delayCount > minFramesBtwnPresses)
        {
            pressDelay = false;
            delayCount = 0;
        }
        
        // press input
        if (!pressDelay)
        {
            if(direction == PseudoInputDirecton.Left)
    		{
    			PseudoInput.Instance.leftPressed = true;
                pressDelay = true;
    		}
    
    		if(direction == PseudoInputDirecton.Right)
    		{
    			PseudoInput.Instance.rightPressed = true;
                pressDelay = true;
    		}
            
            if(direction == PseudoInputDirecton.Up)
            {
                PseudoInput.Instance.upPressed = true;
                pressDelay = true;
            }
            
            if(direction == PseudoInputDirecton.Down)
            {
                PseudoInput.Instance.downPressed = true;
                pressDelay = true;
            }
        }
        

	}
}
