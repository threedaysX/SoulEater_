using Necromancy.UI;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPools : Singleton<ObjectPools>
{
    [System.Serializable]
    public class Pool
    {
        public string name;
        public string ownerName = string.Empty;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public OwnerDictionary ownerDictionary = new OwnerDictionary();

    private void Start()
    {
        GetPoolsOnStart();
    }

    protected virtual void GetPoolsOnStart()
    {
        foreach (Pool pool in pools)
        {
            string ownerName = pool.ownerName;
            if (ownerName == string.Empty)
            {
                ownerName = this.name;
                pool.ownerName = this.name;
            }
            InitPool(ownerName, pool.name);

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.name = pool.name;
                obj.transform.SetParent(this.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.SetActive(false);
                ownerDictionary[ownerName][pool.name].Enqueue(obj);
            }
        }
    }

    /// <summary>
    /// 【In RunTime】生成物件，於物件池
    /// </summary>
    /// <param name="obj">哪個物件</param>
    /// <param name="size">欲生成的物件數量</param>
    /// <param name="parent">指定父物件，若無則自動放入物件池中(this.transform)</param>
    public void RenderObjectPoolsInParent(GameObject obj, float size, Transform parent = default, string ownerName = default)
    {
        if (ownerName == default)
        {
            ownerName = this.name;
        }

        // 已經生成過的不會再次生成
        if (ownerDictionary == null || !CheckOwnerExist(ownerName) || ownerDictionary[ownerName].ContainsKey(obj.name))
            return;

        InitPool(ownerName, obj.name);

        for (int i = 0; i < size; i++)
        {
            GameObject cloneObj = Instantiate(obj);
            cloneObj.name = obj.name;
            if (parent == default)
            {
                cloneObj.transform.SetParent(this.transform);
            }
            else
            {
                cloneObj.transform.SetParent(parent);
            }
            cloneObj.transform.localPosition = Vector3.zero;
            cloneObj.SetActive(false);
            ownerDictionary[ownerName][obj.name].Enqueue(cloneObj);
        }
    }

    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <param name="exhaust">Objects in pools will be exhaust with its pools capacity. 
    /// Need to use 【Reload】 method and can use again.</param>
    /// <returns></returns>
    public GameObject GetObjectInPools(string name, Vector3 position, string ownerName = default, bool exhaust = false)
    {
        if (ownerName == default)
            ownerName = this.name;
        if (ownerDictionary[ownerName].Count == 0 || ownerDictionary[ownerName][name].Count == 0)
            return default;

        GameObject objectToSpawn = ownerDictionary[ownerName][name].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;

        if (!exhaust)
            ownerDictionary[ownerName][name].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public T GetObjectInPools<T>(string name, Vector3 position, string ownerName = default, bool exhaust = false)
    {
        if (ownerName == default)
            ownerName = this.name;
        if (ownerDictionary[ownerName].Count == 0 || ownerDictionary[ownerName][name].Count == 0)
            return default;

        GameObject objectToSpawn = ownerDictionary[ownerName][name].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;

        if (!exhaust)
            ownerDictionary[ownerName][name].Enqueue(objectToSpawn);

        return objectToSpawn.GetComponent<T>();
    }

    public void Reload(string name, GameObject obj, bool setActive = false, string ownerName = default)
    {
        if (ownerName == default)
        {
            ownerName = this.name;
        }
        // Needs to check if ownerName not default.
        else
        {
            InitPool(ownerName, name);
        }

        obj.SetActive(setActive);
        ownerDictionary[ownerName][name].Enqueue(obj);
    }

    public void Reload(string name, IEnumerable<GameObject> objects, bool setActive, string ownerName = default)
    {
        if (ownerName == default)
        {
            ownerName = this.name;
        }
        else
        {
            InitPool(ownerName, name);
        }

        foreach (var obj in objects)
        {
            obj.SetActive(setActive);
            ownerDictionary[ownerName][name].Enqueue(obj);
        }
    }

    public GameObject Unload(string objName, string ownerName = default)
    {
        if (ownerName == default)
            ownerName = this.name;

        return ownerDictionary[ownerName][objName].Dequeue();
    }

    public IEnumerable<GameObject> UnloadAll(string objName, string ownerName = default)
    {
        if (ownerName == default)
            ownerName = this.name;

        IEnumerable<GameObject> allItem = ownerDictionary[ownerName][objName];
        ownerDictionary[ownerName][objName].Clear();
        return allItem;
    }

    private bool CheckOwnerExist(string name)
    {
        return ownerDictionary.ContainsKey(name);
    }

    private void InitPool(string ownerName, string poolName)
    {
        // This two condition means not exists, add new.
        if (!CheckOwnerExist(ownerName))
        {
            ownerDictionary.Add(ownerName, new PoolDictionary());
        }
        if (!ownerDictionary[ownerName].ContainsKey(poolName))
        {
            ownerDictionary[ownerName].Add(poolName, new Queue<GameObject>());
        }
    }

    public class OwnerDictionary : Dictionary<string, PoolDictionary> { }
    public class PoolDictionary : Dictionary<string, Queue<GameObject>> { }
}
