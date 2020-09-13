using UnityEngine;

public abstract class EnemyModel : Character
{
    [Header("AI")]
    public AI ai;

    [Header("階級")]
    public EnemyLevel enemyLevel;

    [Header("動作前置提示")]
    public AudioClip preActSound;
    public ParticleSystem preActHint;

    public virtual void Start()
    {
        ai.OnStart();

        tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    public virtual void Update()
    {
        if (!AppControl.IsGamePaused())
        {
            ai.OnUpdate();
        }
    }
    public override void LateUpdate()
    {
        base.LateUpdate();
        ResetBarUI();

        // 外力因素影響會無法行動(異常狀態、使用技能等)
        if (IsLockAction && ai.canAction)
        {
            ai.canAction = false;
        }

        if (!IsLockAction && !ai.canAction)
        {
            ai.canAction = true;
        }
    }

    private void ResetBarUI()
    {
        if (isHealthDirty)
        {
            EnemyUIControl.Instance.SetHealthUI(characterName, data.maxHealth.Value, CurrentHealth);
            isHealthDirty = false;
        }
    }

    protected virtual void ResetAiSwitchOn()
    {
        ai.ResetAiSwitchOn();
        this.StopAllCoroutines();
        this.operationController.InterruptAnimOperation();
        this.LockOperation(LockType.All, false);
    }

    public override void Die()
    {
        base.Die();
        ResetBarUI();
    }

    public void SetEnemyLevel(EnemyLevel level)
    {
        enemyLevel = level;
    }

    protected virtual void DoPreActHint(ParticleSystem overridedPreActHintEffect = null, AudioClip overridedPreActSound = null)
    {
        if (overridedPreActHintEffect != null)
        {
            overridedPreActHintEffect.Play(true);
        }
        else if (preActHint != null)
        {
            preActHint.Play(true);
        }
        if (overridedPreActSound != null)
        {
            operationSoundController.PlaySound(overridedPreActSound);
        }
        else if (preActSound != null)
        {
            operationSoundController.PlaySound(preActSound);
        }
    }
}

public enum EnemyLevel 
{
    Boss,
    Normal
}

public class HorizontalFacement : IFacement
{
    private Character self;
    public void FaceTarget(MonoBehaviour mono, Transform target, bool force = false)
    {
        if (self == null)
            self = mono.GetComponent<Character>();
        if ((target == null || !self.freeDirection.CanDo) && !force)
            return;

        float faceDirX = self.transform.position.x - target.transform.position.x;
        if (faceDirX < 0)
        {
            self.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (faceDirX > 0)
        {
            self.transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }
}
