using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : DisposableSkill
{
    public int igniteDuration;
    public GameObject projectile;
    public ProjectileSetting projectileSetting = new ProjectileSetting { };
    public ProjectileDirectSetting projectileDirectSetting = new ProjectileDirectSetting { };

    private void Start()
    {
    }

    public override void GenerateSkill(Character character, Skill skill)
    {
        base.GenerateSkill(character, skill);

        projectileSetting.initialPosition = sourceCaster.transform;
        projectileSetting.initialPosition.position += sourceCaster.transform.up * 0.7f;

        ProjectileDataInitializer dataInitializer = new ProjectileDataInitializer(projectileSetting);
        projectileDirectSetting.initialAngleArray = dataInitializer.GetInitialAngle();
        projectileDirectSetting.sourceCaster = sourceCaster;
        ProjectileSpawner.Instance.InstantiateProjectile(projectile, projectileSetting, projectileDirectSetting);
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
