using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public struct ProjectileInitSetting{
    public Vector2 initialPosition;
    public int amount;
    [Range(0, 360)] public int restrictAngle;
    [Range(0, 360)] public int angleOffset;     //從rotation的0度開始加
    [HideInInspector] public float initialAngle{ get; private set; }    //return deg

    public ProjectileInitSetting(Vector2 initialPosition, int amount, int restrictAngle, int angleOffset){
        this.initialPosition = initialPosition;
        this.amount = amount;
        this.restrictAngle = restrictAngle;
        this.angleOffset = angleOffset;
        this.initialAngle = 0;
        this.initialAngle = GetInitialAngle();
    }

    [HideInInspector] public float GetInitialAngle(){
        int angleChunk = restrictAngle / amount;    //在限制角度中平分角度
        int angle = angleOffset;

        float dirX = initialPosition.x + Mathf.Sin(angle * Mathf.Deg2Rad);
        float dirY = initialPosition.y + Mathf.Cos(angle * Mathf.Deg2Rad);

        Vector2 projectileVector = new Vector2(dirX, dirY);
        Vector2 projectileDir = (projectileVector - initialPosition).normalized;

        return Mathf.Atan2(projectileDir.y, projectileDir.x) * Mathf.Rad2Deg;
    }
}

[System.Serializable]
public struct ProjectileSetting{
    public float initialAngle;
    public float moveSpeed;
    public int lifeTime;
    public float shootDelay;     //每顆projectile射出ㄉ間格時間
    public Transform target;
    public ElementType elementType;
    [HideInInspector] public Character sourceCaster;
}

public class Projectile : MonoBehaviour
{
    #region 基礎參數
    public Character sourceCaster;
    public ElementType elementType;
    public float moveSpeed;
    public int lifeTime;
    public float initialAngle;
    public Transform target = null;   //外部給
    #endregion

    private IProjectileState _state;
    private IProjectileHitState _hitState;

    protected virtual void Start()
    {
        ResetProjectileInTime(gameObject, false, lifeTime);
        transform.eulerAngles = new Vector3(0, 0, initialAngle);
    }

    protected virtual void Update()
    {
        if (_state != null)
            _state.Pattern();
        
    }

    public void Setup(IProjectileState state, ProjectileSetting projectileSetting)
    {
        // 通用的setup
        if (projectileSetting.sourceCaster != null)
        {
            this.sourceCaster = projectileSetting.sourceCaster;
            this.elementType = sourceCaster.data.attackElement;
        }
        this.moveSpeed = projectileSetting.moveSpeed;
        this.lifeTime = projectileSetting.lifeTime;
        this.initialAngle = projectileSetting.initialAngle;  //deg
        this.target = projectileSetting.target;

        _state = state;
        DetectHitState();
        // 該State專屬的Setup
        _state.Setup(this);
        _hitState.HitStateSetUp(this);
    }

    private void DetectHitState(){
        if(target == null)
            _hitState = new ProjectileHitState.HitCharacterWithoutTargetIt();
        else
            _hitState = new ProjectileHitState.HitCharacterWithTargetIt();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _hitState.CheckWhatFinalHitColIs(collision);

        gameObject.SetActive(false);
    }

    private void ResetProjectileInTime(GameObject obj, bool isActive, float time)
    {
        StartCoroutine(ResetProjectileInTimeCorutine(obj, isActive, time));
    }

    private IEnumerator ResetProjectileInTimeCorutine(GameObject obj, bool isActive, float time)
    {
        yield return new WaitForSeconds(time);
        _state = null;
        _hitState = null;
        obj.SetActive(isActive);
        yield break;
    }
}

public interface IProjectileState
{
    void Setup(Projectile projectile);
    void Pattern();
}

public class ProjectileState
{
    private Projectile projectile;

    public class Default : ProjectileState, IProjectileState
    {
        public void Setup(Projectile projectile)
        {
            // Calculate or just setup basic and common setting in [Projectile] class.
            // If no needs to use, can just empty.
            this.projectile = projectile;
        }

        public void Pattern()
        {
            
        }
    }

    public class StraightWithTarget : ProjectileState, IProjectileState
    {
        public void Setup(Projectile projectile)
        {
            // Calculate or just setup basic and common setting in [Projectile] class.
            // If no needs to use, can just empty.
            this.projectile = projectile;
        }

        public void Pattern()
        {
            Vector2.MoveTowards(projectile.transform.position, projectile.target.transform.position, projectile.moveSpeed);
        }
    }

    public class StraightWithDirection : ProjectileState, IProjectileState
    {
        private Vector2 direction;

        public void Setup(Projectile projectile)
        {
            this.projectile = projectile;
            direction = new Vector2(Mathf.Cos(projectile.initialAngle), Mathf.Sin(projectile.initialAngle));
        }

        public void Pattern()
        {
            projectile.transform.position += (Vector3)direction * projectile.moveSpeed * Time.deltaTime;
        }
    }

    public class ParabolaWithTarget : ProjectileState, IProjectileState
    {
        private Vector3 destination;
        private float parabolaHeight;

        public ParabolaWithTarget(Projectile projectile, float parabolaHeight)
        {
            this.projectile = projectile;
            this.parabolaHeight = parabolaHeight;
        }

        public void Setup(Projectile projectile)
        {
            this.projectile = projectile;
            destination = projectile.target.position;
        }

        public void Pattern()
        {
            ParabolaController.Parabola(projectile.transform.position, destination, parabolaHeight, projectile.lifeTime);
        }
    }

    public class ParabolaWithDirection : ProjectileState, IProjectileState
    {
        private float angleIncrement;
        private float initialAngle;

        public ParabolaWithDirection(Projectile projectile, float angleIncrement)
        {
            this.projectile = projectile;
            this.angleIncrement = angleIncrement;
            this.initialAngle = projectile.initialAngle;
        }

        public void Setup(Projectile projectile)
        {
            this.projectile = projectile;
        }

        public void Pattern()
        {
            initialAngle += angleIncrement * Mathf.Deg2Rad * Time.deltaTime;
            Vector3 direction = new Vector2(Mathf.Cos(initialAngle), Mathf.Sin(initialAngle)).normalized;
            projectile.transform.position += direction * projectile.moveSpeed * Time.deltaTime;
        }
    }
}

public interface IProjectileHitState{
    void HitStateSetUp(Projectile projectile);
    void CheckWhatFinalHitColIs(Collider2D hitCol);
}

public class ProjectileHitState{
    private Projectile projectile;
    private float damage;
    private Character finalHitTarget;
    private bool isCritical;

    protected void CalculateDamage(Character finalHitTarget){
        damage = DamageController.Instance.GetAttackDamage(projectile.sourceCaster, finalHitTarget, AttackType.Attack, projectile.elementType, out bool isCritical);
        this.isCritical = isCritical;
    }

    public void DoDamage(Character finalHitTarget){
        finalHitTarget.TakeDamage(new DamageData(projectile.sourceCaster.gameObject, projectile.sourceCaster.data.attackElement, (int)damage, isCritical));
    }

    public class HitCharacterWithTargetIt :　ProjectileHitState, IProjectileHitState{
        private Character target;

        public void HitStateSetUp(Projectile projectile){
            this.projectile = projectile;
            target = projectile.target.GetComponent<Character>();
            CalculateDamage(target);
        }

        public void CheckWhatFinalHitColIs(Collider2D hitCol){
            finalHitTarget = hitCol.GetComponent<Character>();
            if(finalHitTarget == null)
                return;

            if(finalHitTarget != target)
                CalculateDamage(finalHitTarget);
            
            DoDamage(finalHitTarget);
        }
    }

    public class HitCharacterWithoutTargetIt :　ProjectileHitState, IProjectileHitState{
        private Character hitCol;
        
        public void HitStateSetUp(Projectile projectile){
            this.projectile = projectile;
        }

        public void CheckWhatFinalHitColIs(Collider2D hitCol){
            finalHitTarget = hitCol.GetComponent<Character>();
            if(finalHitTarget != null)
                return;

            CalculateDamage(finalHitTarget);
            DoDamage(finalHitTarget);
            
        }
    }
}
