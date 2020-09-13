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
            float angle = 90 + Random.Range(45f, 75f) * transform.right.x;
            ProjectileDirectSetting setting = new ProjectileDirectSetting
            {
                sourceCaster = sourceCaster,
                angleIncrement = 0,
                moveSpeed = 10f,
                duration = 5,
                freeFlyDuration = 0f,
                elementType = ElementType.None,
                initialAngle = 180,
            };
            proj.ProjectileSetup(setting);
        }
    }
}
