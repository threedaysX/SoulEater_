using System.Collections.Generic;
using UnityEngine;

public abstract class BossModel : EnemyModel, IBossOpeningEvent, IBossRootEvemt
{
    [Header("Boss開場效果")]
    public ParticleSystem openingEffect;

    [Header("Boss觸發根性效果")]
    public ParticleSystem knockOutRootParticle;
    public AudioClip knockOutRootSound;
    protected bool startDie = false;

    protected IBossStageChangeEvent _stage;

    public virtual float StartOpeningAction()
    {
        CameraLockOn(2f);
        MusicOpeningPlay();
        CameraControl.Shake.Instance.ShakeCamera(1f, 10f, 1.5f, false, 0.2f);
        float duration = openingEffect.main.startLifetime.constant + 1f;
        openingEffect.Play(true);
        this.LockOperation(LockType.TypeChange, true, false, duration);
        return duration;
    }

    public abstract void CameraLockOn(float lockDuration);
    public abstract void MusicOpeningPlay();
    public abstract void SetupStage(string stageName);
    public abstract void OnRootStart();

    public override void Start()
    {
        base.Start();
        if (CurrentHealth > 0)
        {
            startDie = false;
        }
    }

    public override void Die()
    {
        if (enemyLevel == EnemyLevel.Boss)
        {
            // Boss would avoid die once.
            if (!startDie)
            {
                opsc.PlaySound(knockOutRootSound);
                knockOutRootParticle.Play(true);
                LockHealthAvoidDie(2f);
                OnRootStart();
            }
            // True Die (And Generate Soul)
            else
            {
                base.Die();
            }
        }
        else
        {
            base.Die();
        }
    }

    protected virtual void LockHealthAvoidDie(float duration)
    {
        CurrentHealth = 1f;
        StopAllCoroutines();
        GetIntoImmune(duration);
        LockOperation(LockType.Die, true);
        opc.InterruptAnimOperation();
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        StartCoroutine(FadeScreen.Instance.FadeIn(duration));
        TimeScaleController.Instance.DoSlowMotion(0.05f, 0f, duration);

        // Highlight Objs
        List<GameObject> highlightObjs = new List<GameObject>();
        highlightObjs.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        highlightObjs.Add(this.gameObject);
        FadeScreen.Instance.HighlightObjects(0.4f, highlightObjs.ToArray());

        ai._facement.FaceTarget(this, ai.ChaseTarget, true);

        startDie = true;
    }
}

public interface IBossOpeningEvent
{
    float StartOpeningAction();
    void MusicOpeningPlay();
}

public interface IBossStageChangeEvent
{
    void StartStageChangeAction();
}

public interface IBossRootEvemt
{
    void OnRootStart();
}
