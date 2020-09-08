using System.Collections;
using UnityEngine;

public class DistanceDetect : MonoBehaviour
{
    public AI ai;
    private Transform target;

    public float timeToAct;
    public float customDistance;
    public bool hasGetClose;

    [SerializeField] private float lastSecDistance;
    
    private void Update()
    {
        if (target != ai.ChaseTarget)
        {
            target = ai.ChaseTarget;
        }
        if (target != null)
        {
            ResetTargetDistance();
            if (timeToAct > 0)
            {
                StartCoroutine(LastSecDistance());
            }
        }
    }

    private void ResetTargetDistance()
    {
        lastSecDistance = (target.position - transform.position).sqrMagnitude;
    }

    private IEnumerator LastSecDistance()
    {
        yield return new WaitForSeconds(timeToAct);

        if (target == null)
            yield break;

        if (lastSecDistance - (target.position - transform.position).sqrMagnitude >= customDistance * customDistance)
        {
            hasGetClose = true;
        }
        else
        {
            ResetStats();
        }
    }

    private void ResetStats()
    {
        timeToAct = 0;
        hasGetClose = false;
    }
}