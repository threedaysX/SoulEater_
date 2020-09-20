﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ProjectileSetting
{
    public Transform initialPosition;
    public int amount;
    [Range(0, 360)]
    public int restrictAngle;
    [Range(0, 360)]
    public int angleOffset;     //從rotation的0度開始加
    public float shootDelay;     //每顆projectile射出ㄉ間格時間
}

[System.Serializable]
public struct ProjectileDirectSetting
{
    public float moveSpeed;
    public int lifeTime;
    public Transform target;
    public ElementType elementType;
    [HideInInspector] public float[] initialAngleArray;
    [HideInInspector] public float initialAngle;
    [HideInInspector] public Character sourceCaster;
}

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

    public void InstantiateProjectile(GameObject projectilePrefab, IProjectileState state, ProjectileSetting projectileSetting, ProjectileDirectSetting projectileDirectSetting)
    {
        if (projectileTypes == null)
            return;

        StartCoroutine(DelayProjectileSpawn(projectilePrefab, state, projectileSetting, projectileDirectSetting));
    }

    private IEnumerator DelayProjectileSpawn(GameObject projectilePrefab, IProjectileState state, ProjectileSetting projectileSetting, ProjectileDirectSetting projectileDirectSetting)
    {
        int angleChunk = projectileSetting.restrictAngle / projectileSetting.amount;    //在限制角度中平分角度
        int angle = projectileSetting.angleOffset;

        for (int i = 0; i < projectileSetting.amount; i++)
        {
            Transform p = ObjectPools.Instance.GetObjectInPools(projectilePrefab.name, projectileSetting.initialPosition.position).transform;
            projectileDirectSetting.initialAngle = projectileDirectSetting.initialAngleArray[i];
            p.GetComponent<Projectile>().Setup(state, projectileDirectSetting);

            if (p.GetComponent<ParticleSystem>() != null)
                p.GetComponent<ParticleSystem>().Play();

            angle += angleChunk;
            yield return new WaitForSeconds(projectileSetting.shootDelay);
        }
        yield break;
    }
}
