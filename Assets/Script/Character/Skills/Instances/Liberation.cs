using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liberation : DisposableSkill
{
    protected override void Start()
    {
        GetAllProjectile();
    }

    protected override void AddAffectEvent()
    {
        
    }

    private void GetAllProjectile()
    {
        var projectiles = ObjectPools.Instance.UnloadAll<Projectile>(sourceCaster.characterName);
    }

    private void LaunchAllProjectile()
    {

    }
}
