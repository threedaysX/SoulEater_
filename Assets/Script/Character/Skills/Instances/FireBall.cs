using UnityEngine;

public class FireBall : DisposableSkill
{
    public int igniteDuration;
    public GameObject projectile;
    public ProjectileSetting projectileSetting;
    public ProjectileInitSetting projectileInitSetting;
    private ProjectileState.StraightWithDirection projectileState = new ProjectileState.StraightWithDirection();

    public override void GenerateSkill(Character character, Skill skill)
    {
        base.GenerateSkill(character, skill);

        projectileInitSetting = new ProjectileInitSetting(sourceCaster.transform.position + Vector3.up * 0.7f, 6, 60, 100);
        projectileSetting = new ProjectileSetting { moveSpeed = 3, lifeTime = 3, elementType = ElementType.Fire, };

        ProjectileSpawner.Instance.InstantiateProjectile(projectile,projectileState, projectileInitSetting, projectileSetting);
    }

    protected override void AddAffectEvent()
    {
        hitAffect.AddListener(Ignite);
    }

    private void Ignite()
    {
        Debuff.Instance.Ignite(sourceCaster, target, igniteDuration);
    }
}
