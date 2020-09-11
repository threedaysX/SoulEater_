using System;
using UnityEngine;

public class EnergeBall : Projectile
{
    public float lifeTime = 12f;
    private Coroutine last = null;

    public void ResetEnergeBallLifeTime(Action afterResetEvent)
    {
        this.gameObject.SetActive(true);
        if (last != null)
        {
            Counter.Instance.StopCountDown(this, last);
        }
        last = Counter.Instance.StartCountDown(this, lifeTime, false, null, 
            delegate 
            { 
                this.gameObject.SetActive(false);
                afterResetEvent.Invoke();
            });
    }
}
