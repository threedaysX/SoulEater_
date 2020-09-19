using StatsModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OperationSoundController))]
[RequireComponent(typeof(WeaponController))]
[RequireComponent(typeof(OperationController))]
[RequireComponent(typeof(SkillController))]
[RequireComponent(typeof(BuffController))]
[RequireComponent(typeof(CombatController))]
[RequireComponent(typeof(IconController))]
[RequireComponent(typeof(KnockStunSystem))]
[RequireComponent(typeof(DieController))]
[RequireComponent(typeof(CumulativeDataController))]
public class Character : MonoBehaviour
{
    #region 基礎參數
    [Header("基礎參數")]
    public string characterName;
    [HideInInspector] public bool isHealthDirty = false;
    [HideInInspector] public bool isManaDirty = false;
    [SerializeField] private float _currentHealth = 0;
    [SerializeField] private float _currentMana = 0;
    private float lastHealth = 0;   // 儲存上次的血量
    private float lastMana = 0;   // 儲存上次的魔力
    public float CurrentHealth
    {
        get
        {
            return _currentHealth;
        }
        set
        {
            _currentHealth = value;
            if (_currentHealth > data.maxHealth.Value)
            {
                _currentHealth = data.maxHealth.Value;
            }

            if (lastHealth != _currentHealth && !isHealthDirty)
            {
                lastHealth = _currentHealth;
                isHealthDirty = true;
            }
        }
    }
    public float CurrentMana
    {
        get
        {
            return _currentMana;
        }
        set
        {
            _currentMana = value;
            if (_currentMana > data.maxMana.Value)
            {
                _currentMana = data.maxMana.Value;
            }

            if (lastMana != _currentMana && !isManaDirty)
            {
                lastMana = _currentHealth;
                isManaDirty = true;
            }
        }
    }
    #endregion

    #region 狀態判定
    [Header("操作狀態判定")]
    [HideInInspector] public BasicOperation move;
    [HideInInspector] public BasicOperation jump;
    [HideInInspector] public BasicOperation evade;
    [HideInInspector] public BasicOperation attack;
    [HideInInspector] public BasicOperation useSkill;
    [HideInInspector] public BasicOperation freeDirection;  // 判斷是否被鎖定面對方向
    // 用來判斷是否完全無法行動
    public bool IsLockAction
    {
        get
        {
            if (move.CanDo && jump.CanDo && evade.CanDo
                && attack.CanDo && useSkill.CanDo && freeDirection.CanDo)
            {
                return false;
            }
            return true;
        }
    }

    [Header("特殊狀態判定")]
    [HideInInspector] public GameObject lastAttackMeTarget;   // 上一個攻擊我的目標是?
    [HideInInspector] public bool isKnockStun = false;   // 判斷是否被擊暈
    [SerializeField] private bool isImmune = false;  // 用來判斷角色是否無敵(不會被命中)

    [Header("攻擊細節")]
    [HideInInspector] public int cycleAttackCount = 2;    // 每一輪攻擊次數的循環 (完成一連串攻擊動作的總需求次數)
    [HideInInspector] public int attackComboCount = 0;    // 當前累積的攻擊次數
    [HideInInspector] public float attackDelayDuration = 1f;        // 每一段攻擊後的延遲(每一輪有N段)
    [HideInInspector] public float preAttackAnimDuration = 1f;      // 實際前搖攻擊動畫時間
    [HideInInspector] public float attackAnimDuration = 1f;         // 實際攻擊動畫時間
    [HideInInspector] public float nextAttackResetTime;
    private float attackResetDuration = 1f;         // 在前一攻擊完畢後N秒內，可以接續攻擊，若沒有接續攻擊，則會重置連段次數
    private float attackFinishedTime;               // 每個攻擊動畫撥放完預期的時間
    private float maxAttackAnimDuration = 1f;       // 最大攻擊動畫時間  
    private int _attackAnimNumber = 0;
    /// <summary>
    /// 第N次攻擊所播放的動畫
    /// </summary>
    public int AttackAnimNumber
    {
        get
        {
            return _attackAnimNumber;
        }
        set
        {
            _attackAnimNumber = value;
            if (_attackAnimNumber > cycleAttackCount)
                _attackAnimNumber = 1;
        }
    }

    [Header("閃避細節")]
    [HideInInspector] public float evadeCoolDownDuration;
    #endregion

    #region 角色資料
    [Header("詳細參數")]
    public CharacterData data;
    [Header("技能欄")]
    public List<Skill> skillFields;
    private Dictionary<string, int> skillDictionary;

    [Header("彈出文字訊息中心點")]
    public Transform popupDamageTextCenter;
    public Transform popupMessageCenter;

    [Header("受傷數字顏色")]
    public Color normalDamagedColor = new Color32(255, 255, 255, 255);
    public Color criticalDamagedColor = new Color32(255, 0, 0, 255);
    #endregion

    #region 控制器
    [HideInInspector] public OperationSoundController operationSoundController; // 操作聲音控制
    [HideInInspector] public WeaponController weaponController; // 武器控制
    [HideInInspector] public OperationController opc;    // 操作控制
    [HideInInspector] public SkillController skillController;   // 技能控制
    [HideInInspector] public BuffController buffController; // 狀態控制
    [HideInInspector] public CombatController combatController; // 戰鬥控制
    [HideInInspector] public KnockStunSystem knockBackSystem; // 擊退控制
    [HideInInspector] public DieController dieController; // 死亡控制
    [HideInInspector] public IconController iconController; // 提示圖示控制
    [HideInInspector] public CumulativeDataController cumulativeDataController; // 傷害儲存控制
    [HideInInspector] public Rigidbody2D rb;
    #region Anim
    [HideInInspector] public Animator anim;
    public AnimatorOverrideController animController;
    private AnimationClip defaultCastSkillClip;
    private AnimationClip defaultUseSkillClip;
    #endregion
    #endregion


    #region Interface
    public IJumpControl _jump;
    public IEvadeControl _evade;
    #endregion

    private void Awake()
    {
        operationSoundController = GetComponent<OperationSoundController>();
        weaponController = GetComponent<WeaponController>();
        opc = GetComponent<OperationController>();
        skillController = GetComponent<SkillController>();
        buffController = GetComponent<BuffController>();
        combatController = GetComponent<CombatController>();
        knockBackSystem = GetComponent<KnockStunSystem>();
        dieController = GetComponent<DieController>();
        iconController = GetComponent<IconController>();
        cumulativeDataController = GetComponent<CumulativeDataController>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        animController = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animController;
        cycleAttackCount = data.cycleAttackCount;
        nextAttackResetTime = 0;
        attackComboCount = 0;
        ReBorn();
    }

    public virtual void LateUpdate()
    {
        CheckOperationAvoidLockAllDays();
    }

    #region Damage & TakeDamage & Knock & Die
    /// <summary>
    /// 受到傷害
    /// </summary>
    public virtual bool TakeDamage(DamageData data)
    {
        lastAttackMeTarget = data.damageSource;
        if (isImmune)
        {
            return false;
        }
        if (data.damage <= 0)
        {
            data.damage = 0;
            DamagePopup(data.damage, data.isCritical);
            return true;
        }

        if (data.timesOfPerDamage <= 0 || data.duration <= 0)
        {
            CurrentHealth -= data.damage;
            if (cumulativeDataController != null)
            {
                cumulativeDataController.DataStore(CumulativeDataType.Take, data.damage);
            }
            if (CurrentHealth <= 0)
            {
                Die();
            }
            if (this != null)
            {
                StartCoroutine(TakeDamageColorChanged(0.1f));
            }
            DamagePopup(data.damage, data.isCritical);
            KnockBackCheck(data.damageDirectionX, data.weaponKnockBackForce);
        }
        else
        {
            // 流血...等持續性傷害
            StartCoroutine(TakeDamagePerSecondInDuration(data));
        }
        return true;
    }

    /// <summary>
    /// 自身受到超過KB值的傷害，會被擊退
    /// </summary>
    private void KnockBackCheck(float damageDirectionX, float knockBackForce)
    {
        float knockbackDamage = data.knockBackDamage.Value;
        string dscName = CumulativeDataController.Cumulative_DamageTake_KnockBack;
        CumulativeDataType dscType = CumulativeDataType.Take;
        if (cumulativeDataController.GetData(dscName, dscType) >= knockbackDamage && damageDirectionX != 0)
        {
            knockBackSystem.KnockStun(this, damageDirectionX, knockBackForce);
            cumulativeDataController.ModifyData(dscName, dscType, 0);
        }
    }

    private IEnumerator TakeDamagePerSecondInDuration(DamageData data)
    {
        while(data.duration >= 0)
        {
            if (data.damageImmediate)
            {
                // Would not trigger critical. (Ex: Blood, Ignite, Poison...)
                DamageData newData = new DamageData(data.damageSource, data.element, data.damage, false);
                TakeDamage(newData);
            }
            yield return new WaitForSeconds(data.timesOfPerDamage);
            data.duration -= data.timesOfPerDamage;
            data.damageImmediate = true;
        }
        yield break;
    }

    /// <summary>
    /// 傷害吸血、吸魔
    /// </summary>
    /// <param name="damage">傷害量</param>
    /// <param name="isAttack">【True】為一般攻擊 ||【False】為技能攻擊</param>
    public void DamageDealtSteal(float damage, bool isAttack)
    {
        cumulativeDataController.DataStore(CumulativeDataType.Dealt, (int)damage);
        LifeSteal(damage, isAttack);
        ManaSteal(damage);
    }

    /// <summary>
    /// 傷害吸血
    /// </summary>
    protected virtual void LifeSteal(float damage, bool isAttack)
    {
        if (damage <= 0)
            return;

        switch (isAttack)
        {
            case true:
                float attackLifeSteal = data.attackLifeSteal.Value / 100;
                if (attackLifeSteal > 0)
                {
                    CurrentHealth += Mathf.Ceil(damage * attackLifeSteal);
                }
                break;
            case false:
                float skillLifeSteal = data.skillLifeSteal.Value / 100;
                if (skillLifeSteal > 0)
                {
                    CurrentHealth += Mathf.Ceil(damage * skillLifeSteal);
                }
                break;
        }
    }

    /// <summary>
    /// 傷害回魔
    /// </summary>
    protected virtual void ManaSteal(float damage)
    {
        if (damage <= 0)
            return;

        // 造成N點傷害量，就回復N點魔力
        float manaStealOfDamage = data.manaStealOfDamage.Value;
        string dscName = CumulativeDataController.Cumulative_DamageDealt_ManaAbsorb;
        CumulativeDataType dscType = CumulativeDataType.Dealt;
        if (cumulativeDataController.GetData(dscName, dscType) >= manaStealOfDamage)
        {
            CurrentMana += Mathf.Ceil(data.manaStealOfPoint.Value);
            cumulativeDataController.ModifyData(dscName, dscType, 0);
        }
    }

    public void DamagePopup(int damage, bool isCritical)
    {
        if (isCritical)
        {
            ObjectPools.Instance.GetObjectInPools<IDamageGenerator>("CriticalDamageText", popupDamageTextCenter.position).SetupDamage(isCritical, damage, criticalDamagedColor);
        }
        else
        {
            ObjectPools.Instance.GetObjectInPools<IDamageGenerator>("DamageText", popupDamageTextCenter.position).SetupDamage(isCritical, damage, normalDamagedColor);
        }
    }

    /// <summary>
    /// Pop english string message Only!
    /// </summary>
    public void PopTextMessage(string message, Color color, float? startSize = null)
    {
        ObjectPools.Instance.GetObjectInPools<ITextGenerator>("PopText", popupMessageCenter.position).SetupTextMessage(message, color, startSize);
    }

    public IEnumerator TakeDamageColorChanged(float duration)
    {
        var sprite = GetComponent<SpriteRenderer>();
        Color originColor = new Color32(255, 255, 255, 255);
        sprite.color = new Color32(255, 100, 100, 230);
        yield return new WaitForSeconds(duration);
        sprite.color = originColor;
    }

    public virtual void Die()
    {
        dieController.StartDie();
    }
    #endregion

    #region Skill
    public virtual bool UseSkill(Skill skill, bool ignoreCoolDown = false, bool ignoreCanDo = false, bool forceActToEnd = false)
    {
        if (skill == null || (!useSkill.CanDo && !ignoreCanDo))
            return false;
        return skillController.Trigger(skill, ignoreCoolDown, forceActToEnd);
    }

    public virtual void LearnSkill(Skill skill)
    {
        skillFields.Add(skill);
        skillDictionary.Add(skill.skillName, skillFields.Count - 1);
    }

    public virtual void RemoveSkill(Skill skill)
    {
        skillFields.RemoveAll(x => x.skillName == skill.skillName);
        ResetSkillDictionaryIndex();
    }

    public Skill GetSkillByName(string skillName)
    {
        return skillFields[skillDictionary[skillName]];
    }
    #endregion
    
    #region Attack
    public virtual bool StartAttack(AttackType attackType = AttackType.Attack, ElementType elementType = ElementType.None, bool forceActToEnd = false)
    {
        bool attackSuccess = false;
        if (attack.CanDo)
        {
            attackSuccess = StartAttackAnim(delegate 
            { 
                return combatController.Attack(attackType, elementType); 
            }
            , forceActToEnd);
        }

        if (attackSuccess)
        {
            cumulativeDataController.DataStore(CumulativeDataType.HitTimes, 1);
        }

        return attackSuccess;
    }
    #endregion

    #region Operation
    /// <summary>
    /// 一次調整所有行動 (用在擊暈等重大影響的異常or特定動作，表示在此影響結束前，不得進行其他動作)
    /// </summary>
    /// <param name="islock">若True，代表鎖定所有行動，反之則恢復正常行動</param>
    public void LockOperation(LockType lockType, bool islock, bool ignoreTimeScale = false, float duration = -1)
    {
        if (islock)
        {
            move.Lock(lockType, this, ignoreTimeScale, duration);
            jump.Lock(lockType, this, ignoreTimeScale, duration);
            evade.Lock(lockType, this, ignoreTimeScale, duration);
            attack.Lock(lockType, this, ignoreTimeScale, duration);
            useSkill.Lock(lockType, this, ignoreTimeScale, duration);
            freeDirection.Lock(lockType, this, ignoreTimeScale, duration);
        }
        else
        {
            move.UnLock(lockType);
            jump.UnLock(lockType);
            evade.UnLock(lockType);
            attack.UnLock(lockType);
            useSkill.UnLock(lockType);
            freeDirection.UnLock(lockType);
        }
    }

    protected void CheckOperationAvoidLockAllDays()
    {
        
    }

    #region StartAnim
    public void StartMoveAnim(float horizontalMove)
    {
        if (anim == null)
            return;
        anim.SetFloat(AnimParameterKeyWord.moveSpeed, Mathf.Abs(horizontalMove));
    }

    public void StartJumpAnim(bool forceActToEnd = false)
    {
        string operationName = "StartJumpAnim";
        // 檢查動作列狀態
        if (opc.CheckOperation(operationName))
        {
            Operation jump = new Operation(operationName);
            jump.SetAction(StartTrueJump(jump.SetOperationState, _jump.Jump));
            jump.SetDelay(0);
            jump.SetEndEvent(SetOperationEndEvent);
            opc.AddOperation(jump, forceActToEnd);
        }
    }

    public void StartEvadeAnim(bool forceActToEnd = false)
    {
        string operationName = "StartEvadeAnim";
        // 檢查動作列狀態
        if (opc.CheckOperation(operationName))
        {
            Operation evade = new Operation(operationName);
            evade.SetAction(StartTrueEvade(evade.SetOperationState, evade.SetDelay, _evade.Evade));
            evade.SetEndEvent(SetOperationEndEvent);
            opc.AddOperation(evade, forceActToEnd);
        }
    }

    /// <param name="skillUseDurtaion">技能施放持續的時間(與使用技能的動作動畫時間不同)</param>
    public void StartUseSkillAnim(Action skillCastMethod, Action skillUseMethod, float castTime, float skillUseDurtaion, bool forceActToEnd = false, AnimationClip castSkillClip = default, AnimationClip useSkillClip = default)
    {
        if (skillUseMethod == null)
            return;

        string operationName = "StartUseSkillAnim";
        // 檢查動作列狀態
        if (opc.CheckOperation(operationName))
        {
            Operation skillUse = new Operation(operationName);
            skillUse.SetAction(StartTrueSkillUse(skillCastMethod, skillUseMethod, castTime, skillUseDurtaion, castSkillClip, useSkillClip));
            skillUse.SetEndEvent(
                delegate {
                    SetOperationEndEvent();
                    // 重設詠唱技能動作
                    if (defaultCastSkillClip != null)
                        animController[defaultCastSkillClip.name] = defaultCastSkillClip;
                    // 重設使用技能動作
                    if (defaultUseSkillClip != null)
                        animController[defaultUseSkillClip.name] = defaultUseSkillClip;
                });
            opc.AddOperation(skillUse, forceActToEnd);
        }
    }

    public bool StartAttackAnim(Func<bool> attackMethod, bool forceActToEnd = false)
    {
        bool attackSuccess = false;
        string operationName = "StartAttackAnim";
        // 檢查動作列狀態
        if (opc.CheckOperation(operationName))
        {
            Operation attack = new Operation(operationName);
            attack.SetAction(StartTrueAttack(attack.SetOperationState, attack.SetDelay, attackMethod));
            attack.SetEndEvent(SetOperationEndEvent);
            opc.AddOperation(attack, forceActToEnd);
            attackSuccess = true;
        }

        return attackSuccess;
    }

    private void SetOperationEndEvent()
    {
        // 重置角色動作
        LockOperation(LockType.OperationAction, false);
        // 重置角色動畫
        opc.InterruptAnimOperation();
    }
    #endregion

    #region IEnumerator Anim Instances
    private IEnumerator StartTrueJump(Action<OperationStateType> setOperationState, Action jumpMethod)
    {
        opc.isJumping = true;

        setOperationState(OperationStateType.Link);
        jumpMethod.Invoke();

        while (true)
        {
            if (opc.fallJump && opc.isGrounding)
            {
                opc.startJump = false;
                opc.fallJump = false;
                opc.endJump = true;
                float frameDelay = GetFrameTimeOffset(3);
                yield return new WaitForSeconds(frameDelay);
                yield return new WaitForSeconds(AnimationBase.Instance.GetCurrentAnimationLength(anim) - frameDelay);
                opc.endJump = false;
                break;
            }
            if (rb.velocity.y > 0 && !opc.startJump)
            {
                opc.startJump = true;
                opc.fallJump = false;
            }
            if (rb.velocity.y <= 0 && !opc.fallJump)
            {
                opc.startJump = false;
                opc.fallJump = true;
            }
            yield return null;
        }

        opc.isJumping = false;
    }

    private IEnumerator StartTrueEvade(Action<OperationStateType> setOperationState, Action<float> setDelay, Action evadeMethod)
    {
        opc.isEvading = true;
        GetIntoImmune(0.4f);

        // 重置閃避時間
        ResetEvadeCoolDownDuration();
        setDelay(evadeCoolDownDuration);

        // 鎖定攻擊、移動、方向
        attack.Lock(LockType.OperationAction);
        move.Lock(LockType.OperationAction);
        freeDirection.Lock(LockType.OperationAction);

        setOperationState(OperationStateType.Interrupt);
        yield return new WaitForSeconds(GetFrameTimeOffset(1));   // 等待一幀，使動畫開始撥放，否則會取到上一個動畫的狀態。

        evadeMethod.Invoke();

        float evadeAnimDuration = AnimationBase.Instance.GetCurrentAnimationLength(anim);
        yield return new WaitForSeconds(evadeAnimDuration * 0.7f);    // 等待動畫播放至準備收尾

        attack.UnLock(LockType.OperationAction); // 收尾動作的時候才可以開始攻擊

        yield return new WaitForSeconds(evadeAnimDuration * 0.3f);    // 等待動畫播放結束
        opc.isEvading = false;
    }

    private IEnumerator StartTrueAttack(Action<OperationStateType> setOperationState, Action<float> setDelay, Func<bool> attackMethod)
    {
        /// 攻擊前搖
        opc.isPreAttacking = true;

        // 鎖定移動
        move.Lock(LockType.OperationAction);

        // 重置每段攻擊時間間隔
        AttackAnimNumber++;
        ResetAttackDelayDuration();
        setDelay(attackDelayDuration);

        // 若是最後一段的攻擊動作，就無法預存下一個攻擊動作。
        if (CheckIsFinalAttack())
            setOperationState(OperationStateType.Interrupt);
        else
            setOperationState(OperationStateType.Trigger);
        yield return new WaitForSeconds(GetFrameTimeOffset(1));   // 等待一幀，使動畫開始撥放，否則會取到上一個動畫的狀態。

        // 攻速過快，動畫時間縮短
        preAttackAnimDuration = AnimationBase.Instance.GetCurrentAnimationLength(anim);
        if (maxAttackAnimDuration < preAttackAnimDuration)
            preAttackAnimDuration = maxAttackAnimDuration;
        yield return new WaitForSeconds(preAttackAnimDuration);    // 等待動畫播放結束

        /// 攻擊中
        opc.isPreAttacking = false;
        opc.isAttacking = true;

        // 鎖定跳躍與閃避、方向
        jump.Lock(LockType.OperationAction);
        evade.Lock(LockType.OperationAction);
        freeDirection.Lock(LockType.OperationAction);

        if (!CheckIsFinalAttack())
        {
            setOperationState(OperationStateType.Continuous);
        }
        else
        {
            setOperationState(OperationStateType.None);
        }
        yield return new WaitForSeconds(GetFrameTimeOffset(2));

        // 累積攻擊次數+1
        attackComboCount++;

        // 攻擊觸發
        operationSoundController.PlaySound(operationSoundController.attackSound);
        if (attackMethod.Invoke())
        {
            operationSoundController.PlaySound(operationSoundController.hitSound);
        }

        // 攻速過快，動畫時間縮短
        attackAnimDuration = AnimationBase.Instance.GetCurrentAnimationLength(anim);
        if (maxAttackAnimDuration < attackAnimDuration)
            attackAnimDuration = maxAttackAnimDuration;

        // 儲存下次攻擊重置時間
        attackFinishedTime = Time.time + attackAnimDuration;
        nextAttackResetTime = attackFinishedTime + attackResetDuration;

        float comboDuration = GetFrameTimeOffset(8);
        yield return new WaitForSeconds(attackAnimDuration - comboDuration);

        // 攻擊收尾，可連段
        setOperationState(OperationStateType.Link);

        yield return new WaitForSeconds(comboDuration);

        opc.isAttacking = false;
    }

    private IEnumerator StartTrueSkillUse(Action skillCastMethod, Action skillUseMethod, float castTime, float skillUseDurtaion, AnimationClip castSkillClip = default, AnimationClip useSkillClip = default)
    {
        // 鎖定行動
        LockOperation(LockType.OperationAction, true);

        if (skillCastMethod != null)
        {
            opc.isSkillCasting = true;
            skillCastMethod.Invoke();
            yield return new WaitForSeconds(GetFrameTimeOffset(1));

            // 設定詠唱技能動作
            string castSkillName = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            // Reset
            if (defaultCastSkillClip == null)
                defaultCastSkillClip = animController[castSkillName];
            animController[castSkillName] = castSkillClip;

            // 計時(詠唱中)
            float timer = 0;
            while (timer < castTime)
            {
                timer += GetFrameTimeOffset(1);
                // Render Casting GUI
                yield return null;
            }

            opc.isSkillCasting = false;
        }

        // 詠唱結束，施放技能
        skillUseMethod.Invoke();

        opc.isSkillUsing = true;
        yield return new WaitForSeconds(GetFrameTimeOffset(2));

        // 設定使用技能動作
        string useSkillName = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        // Reset
        if (defaultUseSkillClip == null)
            defaultUseSkillClip = animController[useSkillName];
        animController[useSkillName] = useSkillClip;

        if (skillUseDurtaion > 0)
        {
            // 計時(持續施放中)
            float timer = 0;
            while (timer < skillUseDurtaion)
            {
                timer += GetFrameTimeOffset(1);
                // Render Casting GUI
                yield return null;
            }
        }
        opc.isSkillUsing = false;
    }
    #endregion
    #endregion

    #region Immune
    /// <summary>
    /// 進入無敵狀態
    /// </summary>
    /// <param name="duration">無敵狀態持續時間</param>
    public void GetIntoImmune(float duration)
    {
        StartCoroutine(GetIntoImmuneCoroutine(duration));
    }

    public void GetIntoImmune(bool can)
    {
        isImmune = can;
    }

    /// <summary>
    /// 是否為無敵狀態
    /// </summary>
    public bool GetImmuneState()
    {
        return isImmune;
    }

    private IEnumerator GetIntoImmuneCoroutine(float duration)
    {
        isImmune = true;
        yield return new WaitForSeconds(duration);
        if (this == null)
        {
            yield break;
        }
        isImmune = false;
    }
    #endregion

    #region DataInitialize
    public virtual void ReBorn()
    {
        ResetBaseData();
        ResetStatsDirtyData();
        ResetSkillDictionaryIndex();
        InitBasicOperation();
    }

    public void ResetBaseData()
    {
        DataInitializer dataInitializer = new DataInitializer(data.status);
        data.maxHealth.BaseValue = dataInitializer.GetMaxHealth();
        data.maxMana.BaseValue = dataInitializer.GetMaxMana();
        data.attack.BaseValue = dataInitializer.GetAttack();
        data.magicAttack.BaseValue = dataInitializer.GetMagicAttack();
        data.defense.BaseValue = dataInitializer.GetDefense();
        data.critical.BaseValue = dataInitializer.GetCritical();
        data.criticalDamage.BaseValue = dataInitializer.GetCriticalDamage();
        data.knockBackDamage.BaseValue = dataInitializer.GetKnockbackDamage();
        data.manaStealOfPoint.BaseValue = dataInitializer.GetManaStealOfPoint();
        data.manaStealOfDamage.BaseValue = dataInitializer.GetManaStealOfDamage();
        data.jumpForce.BaseValue = dataInitializer.GetJumpForce();
        data.moveSpeed.BaseValue = dataInitializer.GetMoveSpeed();
        data.attackDelay.BaseValue = dataInitializer.GetAttackDelay();
        data.reduceSkillCoolDown.BaseValue = dataInitializer.GetSkillCoolDownReduce();
        data.reduceCastTime.BaseValue = dataInitializer.GetCastTimeReduce();
        data.evadeCoolDown.BaseValue = dataInitializer.GetEvadeCoolDownDuration();
        data.recoverFromKnockStunTime.BaseValue = dataInitializer.GetRecoverFromKnockStunTime();

        CurrentHealth = data.maxHealth.Value;
        CurrentMana = data.maxMana.Value;
    }

    public void ResetStatsDirtyData()
    {
        #region NormalData
        data.maxHealth.ResetDirtyStats();
        data.maxMana.ResetDirtyStats();
        data.attack.ResetDirtyStats();
        data.attackDelay.ResetDirtyStats();
        data.attackLifeSteal.ResetDirtyStats();
        data.attackRange.ResetDirtyStats();
        data.critical.ResetDirtyStats();
        data.criticalDamage.ResetDirtyStats();
        data.defense.ResetDirtyStats();
        data.evadeCoolDown.ResetDirtyStats();
        data.jumpForce.ResetDirtyStats();
        data.knockBackDamage.ResetDirtyStats();
        data.magicAttack.ResetDirtyStats();
        data.manaStealOfDamage.ResetDirtyStats();
        data.manaStealOfPoint.ResetDirtyStats();
        data.maxHealth.ResetDirtyStats();
        data.maxMana.ResetDirtyStats();
        data.moveSpeed.ResetDirtyStats();
        data.penetrationMagnification.ResetDirtyStats();
        data.penetrationValue.ResetDirtyStats();
        data.recoverFromKnockStunTime.ResetDirtyStats();
        data.reduceCastTime.ResetDirtyStats();
        data.reduceSkillCoolDown.ResetDirtyStats();
        #endregion
        #region Resistance
        data.resistance.air.ResetDirtyStats();
        data.resistance.dark.ResetDirtyStats();
        data.resistance.earth.ResetDirtyStats();
        data.resistance.fire.ResetDirtyStats();
        data.resistance.light.ResetDirtyStats();
        data.resistance.thunder.ResetDirtyStats();
        data.resistance.thunder.ResetDirtyStats();
        data.resistance.water.ResetDirtyStats();
        #endregion
        #region Status
        data.skillLifeSteal.ResetDirtyStats();
        data.status.agility.ResetDirtyStats();
        data.status.dexterity.ResetDirtyStats();
        data.status.intelligence.ResetDirtyStats();
        data.status.lucky.ResetDirtyStats();
        data.status.strength.ResetDirtyStats();
        data.status.vitality.ResetDirtyStats();
        #endregion
        #region Skill
        foreach (Skill skill in skillFields)
        {
            skill.castTime.ResetDirtyStats();
            skill.coolDown.ResetDirtyStats();
            skill.cost.ResetDirtyStats();
            skill.damageMagnification.ResetDirtyStats();
            skill.fixedCastTime.ResetDirtyStats();
            skill.range.ResetDirtyStats();
        }
        #endregion
    }

    public void ResetSkillDictionaryIndex()
    {
        if (skillDictionary == null)
        {
            skillDictionary = new Dictionary<string, int>();
        }
        else
        {
            skillDictionary.Clear();
        }

        int index = 0;
        foreach (Skill skillField in skillFields)
        {
            skillDictionary.Add(skillField.skillName, index++);
        }
    }

    private void InitBasicOperation()
    {
        move.operationType = BasicOperationType.Move;
        move.UnLock();
        jump.operationType = BasicOperationType.Jump;
        jump.UnLock();
        evade.operationType = BasicOperationType.Evade;
        evade.UnLock();
        attack.operationType = BasicOperationType.Attack;
        attack.UnLock();
        useSkill.operationType = BasicOperationType.UseSkill;
        useSkill.UnLock();
        freeDirection.operationType = BasicOperationType.LockDirection;
        freeDirection.UnLock();
    }
    #endregion

    #region GetData
    public float GetSkillCost(CostType costType)
    {
        float resultCost = CurrentMana;
        switch (costType)
        {
            case CostType.Health:
                resultCost = CurrentHealth;
                break;
            case CostType.Mana:
                break;
        }

        return resultCost;
    }

    public Stats GetAttackData(AttackType attackType)
    {
        Stats resultAttack = data.attack;
        switch (attackType)
        {
            case AttackType.Attack:
                break;
            case AttackType.Magic:
                resultAttack = data.magicAttack;
                break;
        }

        return resultAttack;
    }

    public Stats GetResistance(ElementType elementType = ElementType.None)
    {
        Stats resultResistance = data.resistance.none;
        switch (elementType)
        {
            case ElementType.None:
                break;
            case ElementType.Fire:
                resultResistance = data.resistance.fire;
                break;
            case ElementType.Water:
                resultResistance = data.resistance.water;
                break;
            case ElementType.Earth:
                resultResistance = data.resistance.earth;
                break;
            case ElementType.Air:
                resultResistance = data.resistance.air;
                break;
            case ElementType.Thunder:
                resultResistance = data.resistance.thunder;
                break;
            case ElementType.Light:
                resultResistance = data.resistance.light;
                break;
            case ElementType.Dark:
                resultResistance = data.resistance.dark;
                break;
        }

        return resultResistance;
    }
    #endregion

    #region Others
    /// <summary>
    /// 攻擊重置的時間間隔，會根據攻擊後延遲調整
    /// </summary>
    private void ResetAttackDelayDuration()
    {
        float delay = data.attackDelay.Value;

        // 攻擊後延遲(連段中的攻擊延遲較短，為1/5)
        if (AttackAnimNumber == cycleAttackCount)
        {
            attackDelayDuration = delay;
        }
        else
        {
            attackDelayDuration = delay / 5;
        }

        // 最大攻擊動畫時間受到攻擊後延遲影響
        maxAttackAnimDuration = delay;
        // 攻擊動畫時間下限
        if (maxAttackAnimDuration < 0.02f)
        {
            maxAttackAnimDuration = 0.02f;
        }

        // 重置攻擊恢復時間(重置連段中攻擊)
        // 若攻擊動畫時間過長，則加長此時間(attackResetDuration)
        if (maxAttackAnimDuration >= attackResetDuration)
        {
            attackResetDuration = maxAttackAnimDuration;
        }
        else if (attackResetDuration != 1f)
        {
            attackResetDuration = 1;
        }
    }

    private bool CheckIsFinalAttack()
    {
        return AttackAnimNumber == cycleAttackCount;
    }

    /// <summary>
    /// 閃避重置的時間間隔
    /// </summary>
    private void ResetEvadeCoolDownDuration()
    {
        evadeCoolDownDuration = data.evadeCoolDown.Value;
    }

    /// <summary>
    /// 目押的時間間隔 (從該動作【動畫撥放結束前】到【動畫播放完】的時間差)
    /// </summary>
    /// <param name="frame">目押的幀數</param>
    private float GetFrameTimeOffset(int frame)
    {
        return Time.unscaledDeltaTime * frame;
    }
    #endregion
}

#region struct
/// <param name="damageSource">傷害來源</param>
/// <param name="damage">單次傷害</param>
/// <param name="isCritical">是否爆擊</param>
/// <param name="damageDirectionX">傷害來源方向(大於0為右側，小於為左)</param>
/// <param name="timesOfPerDamage">造成單次傷害所需時間</param>
/// <param name="duration">持續時間</param>
/// <param name="damageImmediate">是否立即造成傷害</param>
public struct DamageData
{
    public GameObject damageSource;
    public ElementType element;
    public int damage;
    public bool isCritical;
    public float damageDirectionX;
    public float weaponKnockBackForce;
    public float timesOfPerDamage;
    public float duration;
    public bool damageImmediate;

    public DamageData(ElementType element, int damage)
    {
        this.damageSource = null;
        this.element = element;
        this.damage = damage;
        this.isCritical = false;
        this.damageDirectionX = 0;
        this.weaponKnockBackForce = 0;
        this.timesOfPerDamage = 1;
        this.duration = 0;
        this.damageImmediate = true;
    }

    public DamageData(DamageData data)
    {
        this.damageSource = data.damageSource;
        this.element = data.element;
        this.damage = data.damage;
        this.isCritical = data.isCritical;
        this.damageDirectionX = data.damageDirectionX;
        this.weaponKnockBackForce = data.weaponKnockBackForce;
        this.timesOfPerDamage = data.timesOfPerDamage;
        this.duration = data.duration;
        this.damageImmediate = data.damageImmediate;
    }

    public DamageData(GameObject damageSource, ElementType element, int damage, bool isCritical, float damageDirectionX = 0, float weaponKnockBackForce = 0, float timesOfPerDamage = 0, float duration = 0, bool damageImmediate = true)
    {
        this.damageSource = damageSource;
        this.element = element;
        this.damage = damage;
        this.isCritical = isCritical;
        this.damageDirectionX = damageDirectionX;
        this.weaponKnockBackForce = weaponKnockBackForce;
        this.timesOfPerDamage = timesOfPerDamage;
        this.duration = duration;
        this.damageImmediate = damageImmediate;
    }
}
#endregion

public interface IJumpControl
{
    void Jump();
}

public interface IEvadeControl
{
    void Evade();
}
