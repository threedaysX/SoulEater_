using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutFrag : Singleton<PutFrag>
{
    public Frag_m leftUp; 
    public Frag_m leftDown;  
    public Frag_m rightUp;      
    public Frag_m rightDown;

    public void PutFragment(Fragment _putfrag, FragSpot _spot)
    {
        switch (_spot) {
            case FragSpot.left_Up:
                leftUp.PutNewFrag(_putfrag);
                break;
            case FragSpot.left_Down:
                leftDown.PutNewFrag(_putfrag);
                break;
            case FragSpot.right_Up:
                rightUp.PutNewFrag(_putfrag);
                break;
            case FragSpot.right_Down:
                rightDown.PutNewFrag(_putfrag);
                break;
            }
    }
}
    public enum FragSpot { left_Up, left_Down, right_Up, right_Down };
