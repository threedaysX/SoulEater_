using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectileType
{
    public string name;
    public GameObject projectile;
    public int poolSize;
}

public class ProjectileSpawner : Singleton<ProjectileSpawner>
{
    public List<ProjectileType> projectileTypes = new List<ProjectileType>();

    private void Start()
    {
        foreach(ProjectileType p in projectileTypes)
            ObjectPools.Instance.RenderObjectPoolsInParent(p.projectile, p.poolSize);
    }

    public void InstantiateProjectile(GameObject projectilePrefab, IProjectileState state, ProjectileInitSetting projectileInitSetting, ProjectileSetting projectileSetting)
    {
        if (projectileTypes == null)
            return;

        StartCoroutine(DelayProjectileSpawn(projectilePrefab, state, projectileInitSetting, projectileSetting));
    }

    private IEnumerator DelayProjectileSpawn(GameObject projectilePrefab, IProjectileState state, ProjectileInitSetting projectileInitSetting, ProjectileSetting projectileSetting)
    {
        int angleChunk = projectileInitSetting.restrictAngle / projectileInitSetting.amount;    //在限制角度中平分角度
        int angle = projectileInitSetting.angleOffset;

        for (int i = 0; i < projectileInitSetting.amount; i++)
        {
            Transform p = ObjectPools.Instance.GetObjectInPools(projectilePrefab.name, projectileInitSetting.initialPosition).transform;
            projectileSetting.initialAngle = projectileInitSetting.initialAngle;
            p.GetComponent<Projectile>().Setup(state, projectileSetting);

            if (p.GetComponent<ParticleSystem>() != null)
                p.GetComponent<ParticleSystem>().Play();

            angle += angleChunk;
            yield return new WaitForSeconds(projectileSetting.shootDelay);
        }
        yield break;
    }
}
