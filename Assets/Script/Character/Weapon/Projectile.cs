using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Character sourceCaster;
    private ElementType elementType;
    private float moveSpeed;
    private int duration;
    private float initialAngle;
    private float shootAngleIncrement;
    private Transform target;
    private float freeFlyDuration;

    private float timer;
    private Vector2 finalDirection;
    private bool startLaunch = false;

    public ProjectileStateType type;
    private IProjectileState _state;

    private void Start()
    {
        // Init type on start.
        SetStateType(type);
    }

    public void ProjectileSetup(ProjectileDirectSetting projectileDirectSetting)
    {
        if(projectileDirectSetting.sourceCaster != null)
        {
            this.sourceCaster = projectileDirectSetting.sourceCaster;
            this.elementType = sourceCaster.data.attackElement;
        }
        this.moveSpeed = projectileDirectSetting.moveSpeed;
        this.duration = projectileDirectSetting.duration;
        this.initialAngle = projectileDirectSetting.initialAngle;  //rad
        this.shootAngleIncrement = projectileDirectSetting.angleIncrement;   //deg
        this.target = projectileDirectSetting.target;
        this.freeFlyDuration = projectileDirectSetting.freeFlyDuration;
        startLaunch = true;
    }

    private void LateUpdate()
    {
        if (startLaunch && gameObject.activeInHierarchy && timer <= duration)
        {
            timer += Time.deltaTime;
            ProjectilePattern();
        }

        //if (???)
        //{
        //    Pattern();
        //}
    }

    private void ProjectilePattern()
    {
        transform.eulerAngles = new Vector3(0, 0, initialAngle);

        initialAngle += shootAngleIncrement * Mathf.Deg2Rad * Time.deltaTime;
        finalDirection = new Vector2(Mathf.Cos(initialAngle), Mathf.Sin(initialAngle));

        if (target != null)
        {
            if (timer < freeFlyDuration)
                transform.position += (Vector3)finalDirection * moveSpeed * Time.deltaTime;
            else if (timer < freeFlyDuration + 0.1f) { }
            else
                transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
        else
            transform.position += (Vector3)finalDirection * moveSpeed * Time.deltaTime;

        if (timer > duration)
        {
            gameObject.SetActive(false);
            timer = 0;
            startLaunch = false;
        }
    }

    public void Setup(ProjectileStateType type, ProjectileDirectSetting projectileDirectSetting)
    {
        // Init type first(Set type again). 
        SetStateType(type);

        // 通用的setup
        // moveSpeed = .......................

        // 該State專屬的Setup
        _state.Setup();
    }

    private void Pattern()
    {
        _state.Pattern();
    }
    private void SetStateType(ProjectileStateType type)
    {
        switch (type)
        {
            case ProjectileStateType.StraightWithTarget:
                //_state = new StraightWithTarget();
                break;
            case ProjectileStateType.StraightWithPosition:
                //_state = new StraightWithPosition();
                break;
            case ProjectileStateType.ParabolaWithTarget:
                //_state = new ParabolaWithTarget();
                break;
            case ProjectileStateType.ParabolaWithPosition:
                //_state = new ParabolaWithPosition();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Character target = collision.GetComponent<Character>();

        if (target  != null && target != sourceCaster && sourceCaster != null)
        {
            float damage = DamageController.Instance.GetAttackDamage(sourceCaster, target, AttackType.Attack, elementType, out bool isCritical);
            target.TakeDamage(new DamageData(sourceCaster.gameObject, sourceCaster.data.attackElement, (int)damage, isCritical));
        }
        else if(sourceCaster == null)   //not character shooting projectile
        {
            //write a GetTrapDamage Function in DamageController
        }
        else if(target == sourceCaster)
        {
            return;
        }
        gameObject.SetActive(false);
    }
}

public enum ProjectileStateType
{
    StraightWithTarget,
    StraightWithPosition,
    ParabolaWithTarget,
    ParabolaWithPosition,
    // others..........
}

public interface IProjectileState
{
    void Setup();
    void Launch();
    void Pattern();
}

public class StraightWithTarget : IProjectileState
{
    public float yourBounsSetting_1;
    public float yourBounsSetting_2;
    public float yourBounsSetting_3;
    public float yourBounsSetting_4;
    public float yourBounsSetting_5;

    public StraightWithTarget(float yourSetting_1, float yourSetting_2, float yourSetting_3, float yourSetting_4, float yourSetting_5)
    {
        // Init some settings.
    }
    public void Setup()
    {
        // Calculate or just setup basic and common setting in [Projectile] class.
        // If no needs to use, can just empty.
    }
    public void Launch()
    {
        // Set trigger and do some delegate event.
        // or just move this method to [Projectile] class for common use, delete method here.
    }
    public void Pattern()
    {
        // Different pattern.
        // maybe use yourBounsSetting_1
    }
}
