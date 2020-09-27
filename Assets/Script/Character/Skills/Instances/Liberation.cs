using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liberation : DisposableSkill
{
    IEnumerable<Projectile> projectiles = new List<Projectile>();

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(GetAllProjectile);
        immediatelyAffect.AddListener(LaunchAllProjectile);
    }

    private void GetAllProjectile()
    {
        projectiles = ObjectPools.Instance.UnloadAll<Projectile>(sourceCaster.characterName);
    }

    private void LaunchAllProjectile()
    {
        foreach (Projectile proj in projectiles)
        { 
            ProjectileSetting setting = new ProjectileSetting
            {
                sourceCaster = sourceCaster,
                moveSpeed = 10f,
                lifeTime = 5,
                elementType = ElementType.None,
                initialAngle = 180,
            };
            proj.Setup(new ProjectileState.StraightWithDirection(), setting);
        }
    }
}
