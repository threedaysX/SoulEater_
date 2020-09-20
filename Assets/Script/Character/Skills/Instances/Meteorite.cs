using UnityEngine;

public class Meteorite : DisposableSkill
{
    public int igniteDuration;
    public GameObject projectile;
    public ProjectileSetting projectileSetting = new ProjectileSetting { };
    public ProjectileDirectSetting projectileDirectSetting = new ProjectileDirectSetting { };

    public override void GenerateSkill(Character character, Skill skill)
    {
        base.GenerateSkill(character, skill);

        projectileSetting.initialPosition = sourceCaster.transform;
        projectileSetting.initialPosition.position += sourceCaster.transform.up * 2f;

        ProjectileDataInitializer dataInitializer = new ProjectileDataInitializer(projectileSetting);
        projectileDirectSetting.initialAngleArray = dataInitializer.GetInitialAngle();
        projectileDirectSetting.sourceCaster = sourceCaster;
        ProjectileSpawner.Instance.InstantiateProjectile(projectile, new ProjectileState.StraightWithDirection(), projectileSetting, projectileDirectSetting);
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
