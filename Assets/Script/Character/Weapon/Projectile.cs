using System;
using System.Collections;
using UnityEngine;

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

    protected virtual void Start()
    {
        ResetProjectileInTime(gameObject, false, lifeTime);
        transform.eulerAngles = new Vector3(0, 0, initialAngle);
    }

    protected virtual void Update()
    {
        _state.Pattern();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Character target = collision.GetComponent<Character>();

        if (target != null && target != sourceCaster && sourceCaster != null)
        {
            float damage = DamageController.Instance.GetAttackDamage(sourceCaster, target, AttackType.Attack, elementType, out bool isCritical);
            target.TakeDamage(new DamageData(sourceCaster.gameObject, sourceCaster.data.attackElement, (int)damage, isCritical));
        }
        else if (sourceCaster == null)   //not character shooting projectile
        {
            //write a GetTrapDamage Function in DamageController
        }
        else if (target == sourceCaster)
        {
            return;
        }
        gameObject.SetActive(false);
    }

    public void Setup(IProjectileState state, ProjectileDirectSetting projectileDirectSetting)
    {
        // 通用的setup
        if (projectileDirectSetting.sourceCaster != null)
        {
            this.sourceCaster = projectileDirectSetting.sourceCaster;
            this.elementType = sourceCaster.data.attackElement;
        }
        this.moveSpeed = projectileDirectSetting.moveSpeed;
        this.lifeTime = projectileDirectSetting.lifeTime;
        this.initialAngle = projectileDirectSetting.initialAngle;  //rad
        this.target = projectileDirectSetting.target;

        _state = state;
        // 該State專屬的Setup
        _state.Setup(this);
    }

    private void ResetProjectileInTime(GameObject obj, bool isActive, float time)
    {
        StartCoroutine(ResetProjectileInTimeCorutine(obj, isActive, time));
    }

    private IEnumerator ResetProjectileInTimeCorutine(GameObject obj, bool isActive, float time)
    {
        yield return new WaitForSeconds(time);
        _state = null;
        obj.SetActive(isActive);
        yield break;
    }
}

public enum ProjectileStateType
{
    StraightWithTarget,
    StraightWithDirection,
    ParabolaWithTarget,
    ParabolaWithDirection,
    // Others..........
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
