using UnityEngine;

public abstract class EnemyModel : AI
{
    [Header("階級")]
    public EnemyLevel enemyLevel;

    [Header("動作前置提示")]
    public AudioClip preActSound;
    public ParticleSystem preActHint;

    public override void Start()
    {
        base.Start();

        tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    public override void Update()
    {
        if (!AppControl.IsGamePaused())
        {
            base.Update();
        }
    }
    public override void LateUpdate()
    {
        base.LateUpdate();
        ResetBarUI();

        // 外力因素影響會無法行動(異常狀態、使用技能等)
        if (IsLockAction && canAction)
        {
            canAction = false;
        }

        if (!IsLockAction && !canAction)
        {
            canAction = true;
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

    protected override void ResetAiSwitchOn()
    {
        base.ResetAiSwitchOn();
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
    public void FaceTarget(Character self, Transform target, bool force = false)
    {
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
