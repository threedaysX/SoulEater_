using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Detect/OverlapCircleDecision")]
public class OverlapCircleDecision : Detect
{
    public override bool StartDetectHaviour()
    {
        return OverlapCircle();
    }

    private bool OverlapCircle()
    {
        Collider2D col = Physics2D.OverlapCircle(Ai.transform.position, Ai.detectDistance, Ai.PlayerLayer);

        if (!col)
        {

        }
        else if (col.CompareTag("Player"))
        {
            Ai.SetChaseTarget(col.transform);
            return true;
        }
        return false;
    }
}