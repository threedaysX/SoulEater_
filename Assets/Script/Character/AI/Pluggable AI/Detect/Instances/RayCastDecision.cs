using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Detect/RayCastDecision")]
public class RayCastDecision : Detect
{
    public override bool StartDetectHaviour()
    {
        return RayCast();
    }

    private bool RayCast()
    {
        Vector2 start = new Vector2(Ai.transform.position.x - Ai.detectDistance, Ai.transform.position.y);
        Vector2 end = new Vector2(Ai.transform.position.x + Ai.detectDistance, Ai.transform.position.y);

        RaycastHit2D hit = Physics2D.Linecast(start, end, Ai.playerLayer);
        Debug.DrawLine(start, end, Color.green);

        if (!hit)
        {

        }
        else if (hit.collider.CompareTag("Player"))
        {
            Ai.SetChaseTarget(hit.transform);
            return true;
        }
        return false;
    }
}
