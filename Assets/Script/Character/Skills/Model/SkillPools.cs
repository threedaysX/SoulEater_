﻿using System.Collections.Generic;
using UnityEngine;

public class SkillPools : Singleton<SkillPools>
{
    [System.Serializable]
    public class Pool
    {
        public string name;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> skillPools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        GetSkillPools();
    }

    private void GetSkillPools()
    {
        foreach (Pool pool in skillPools)
        {
            Queue<GameObject> skillObjPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject skillObj = Instantiate(pool.prefab);
                skillObj.name = pool.prefab.name;
                skillObj.transform.SetParent(this.transform);
                skillObj.SetActive(false);
                skillObjPool.Enqueue(skillObj);
            }

            poolDictionary.Add(pool.prefab.name, skillObjPool);
        }
    }

    public GameObject SpawnSkillFromPool(Character character, Skill skill, Vector3 position, Quaternion rotation)
    {      
        string skillName = skill.prefab.name;
        if (!poolDictionary.ContainsKey(skillName))
            return null;

        GameObject objectToSpawn = poolDictionary[skillName].Dequeue();

        ISkillGenerator skillObj = objectToSpawn.GetComponent<ISkillGenerator>();
        
        if (skillObj != null)
        {
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            skillObj.GenerateSkill(character, skill);
        }

        poolDictionary[skillName].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
