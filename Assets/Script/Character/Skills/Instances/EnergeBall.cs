using UnityEngine;

public class EnergeBall : DisposableSkill
{
    public float lifeTime = 12f;
    private Coroutine last = null;

    protected override void AddAffectEvent()
    {
        
    }

    public void ResetEnergeBallLifeTime()
    {
        this.gameObject.SetActive(true);
        if (last != null)
        {
            Counter.Instance.StopCountDown(this, last);
        }
        last = Counter.Instance.StartCountDown(this, lifeTime, false, null, delegate { this.gameObject.SetActive(false); });
    }
}
