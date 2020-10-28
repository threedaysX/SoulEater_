using UnityEngine;
using System.Linq;
using System.Collections;
using UniRx;
using System;

public class CombatController : MonoBehaviour
{
    private Character character;
    public Transform attackCenterPoint;
    [HideInInspector] public ParticleSystem hitEffect;
    [HideInInspector] public AttackHitboxList attackHitboxes;

    [Header("擊中系統")]
    private bool haveLastAttackTarget;
    private float nextResetTimes;
    public Character lastAttackTarget;
    public float damageDirectionX;
    public float resetDuration = 0.5f;
    private Subject<Character> _attackHitSubject = new Subject<Character>();
    public IObservable<Character> AttackHitAsObservable
    {
        get
        {
            return _attackHitSubject.AsObservable();
        }
    }

    [Header("擊退系統")]
    public float basicShakeCameraForce;
    public float basicShakeCameraFrequency;

    [Header("命中檢查")]
    public bool hasHit = false;
    public float hasHitInTime;
    public float takeHowMuchDamage;

    [SerializeField] private float attackPointBasicRange = 1f;

    private void Start()
    {
        character = GetComponent<Character>();
        RenderAttackHitboxes(true);
        RenderHitEffect();

        AttackHitAsObservable
            .ObserveOnMainThread()
            .Subscribe(target => {
                SetLastAttackTarget(target);
                TriggerHitEffect(target.transform);                
            })
        .AddTo(this);
    }

    private void Update()
    {
        if (haveLastAttackTarget && Time.time >= nextResetTimes)
        {
            lastAttackTarget = null;
        }
    }

    public bool Attack(AttackType attackType = AttackType.Attack, ElementType elementType = ElementType.None)
    {
        bool attackSuccess = false;
        Collider2D[] hits = DrawAttackRange();
        foreach (Collider2D target in hits)
        {
            if (target.CompareTag("Untagged"))
                continue;

            // 不會打到自己人
            if (target != null && !target.CompareTag(character.tag))
            {
                var enemy = target.gameObject.GetComponent<Character>();
                if (enemy != null && !enemy.GetImmuneState())
                {
                    // 重新確認目標位置
                    if (CheckIsTargetStillInAttackRange(target))
                    {
                        // 命中事件觸發階段
                        attackSuccess = true;
                        _attackHitSubject.OnNext(enemy);

                        // 傷害階段
                        float damage = DamageController.Instance.GetAttackDamage(character, enemy, attackType, elementType, out bool isCritical);
                        DamageData data = new DamageData(character.gameObject, elementType, (int)damage, isCritical, damageDirectionX, character.data.weaponKnockBackForce);
                        enemy.TakeDamage(data);
                        CameraControl.Shake.Instance.ShakeCamera(basicShakeCameraForce * character.data.weaponKnockBackForce, basicShakeCameraFrequency, 0.1f, false, 0f, true);
                        character.DamageDealtSteal(damage, true);
                        StartCoroutine(HasHit());
                    }
                }

            }
        }

        return attackSuccess;
    }

    public void SetLastAttackTarget(Character target)
    {
        lastAttackTarget = target;
        damageDirectionX = character.transform.position.x - target.transform.position.x;
        haveLastAttackTarget = true;
        nextResetTimes = Time.time + resetDuration;
    }

    private bool CheckIsTargetStillInAttackRange(Collider2D target)
    {
        Collider2D[] reDrawTarget = DrawAttackRange();
        if (reDrawTarget.Contains(target))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 根據武器調整繪製方式
    /// </summary>
    /// <returns></returns>
    private Collider2D[] DrawAttackRange()
    {
        if (attackHitboxes != null)
        {
            int attackOrder = character.AttackAnimNumber;
            attackHitboxes.gameObject.SetActive(true);
            Collider2D[] hits = attackHitboxes.GetAttackHits(attackOrder);
            attackHitboxes.gameObject.SetActive(false);
            return hits;
        }
        return Physics2D.OverlapCircleAll(GetAttackPoint(), character.data.attackRange.Value);
    }

    public void TriggerHitEffect(Transform target)
    {
        if (hitEffect != null)
        {
            hitEffect.transform.position = target.position;
            hitEffect.Play(true);
        }
    }

    public void RenderAttackHitboxes(bool reRenderOnce = false)
    {
        if (character.data.attackHitBoxPrefab == null)
            return;
        var attackHitboxObj = PrefabRenderer.Instance.RenderPrefabInParent<AttackHitboxList>(character.transform, character.data.attackHitBoxPrefab, "_AttackHitboxes", false, reRenderOnce);
        attackHitboxes = attackHitboxObj.GetComponent<AttackHitboxList>();
    }

    public void RenderHitEffect()
    {
        hitEffect = PrefabRenderer.Instance.RenderPrefab<ParticleSystem>(character.data.hitEffectPrefab, character.characterName + "_HitEffect", true);
    }

    private Vector2 GetAttackPoint()
    {
        Vector3 attackPoint = attackCenterPoint.position != null ? attackCenterPoint.position : character.transform.position;
        return attackPoint + character.transform.right * attackPointBasicRange;
    }

    private IEnumerator HasHit()
    {
        hasHit = true;
        yield return new WaitForSeconds(hasHitInTime);
        hasHit = false;
        yield break;
    }

    private void OnDrawGizmos()
    {
        if (character == null || attackHitboxes != null)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GetAttackPoint(), character.data.attackRange.Value);
    }
}
