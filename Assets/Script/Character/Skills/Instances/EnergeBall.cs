using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergeBall : DisposableSkill
{
    public float lifeTime = 20f;

    protected override void AddAffectEvent()
    {
        
    }

    public void ResetEnergeBallLifeTime()
    {
        this.gameObject.SetActive(true);
        var mono = this.GetComponent<MonoBehaviour>();
        Counter.Instance.StopAllCountDown(mono);
        Counter.Instance.StartCountDown(mono, lifeTime, false, null, delegate { this.gameObject.SetActive(false); });
    }
}
