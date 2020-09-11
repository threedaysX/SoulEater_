using Necromancy.UI;
using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <param name="objName"></param>
    /// <param name="position"></param>
    /// <param name="exhaust">Objects in pools will be exhaust with its pools capacity. 
    /// Need to use 【Reload】 method and can use again.</param>
    /// <returns></returns>
    public GameObject GetObjectInPools(string objName, Vector3 position, string ownerName = default, bool exhaust = false)
    {
        if (ownerName == default)
            ownerName = this.name;
        if (ownerDictionary[ownerName].Count == 0 || ownerDictionary[ownerName][objName].Count == 0)
            return default;

        GameObject objectToSpawn = (GameObject)ownerDictionary[ownerName][objName].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;

        if (!exhaust)
            ownerDictionary[ownerName][objName].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public T GetObjectInPools<T>(string objName, Vector3 position, string ownerName = default, bool exhaust = false)
    {
        if (ownerName == default)
            ownerName = this.name;
        if (ownerDictionary[ownerName].Count == 0 || ownerDictionary[ownerName][objName].Count == 0)
            return default;

        GameObject objectToSpawn = (GameObject)ownerDictionary[ownerName][objName].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;

        if (!exhaust)
            ownerDictionary[ownerName][objName].Enqueue(objectToSpawn);

        return objectToSpawn.GetComponent<T>();
    }

    /// <summary>
    /// Reload a [GameObject].
    /// </summary>
    public void Reload(string objName, GameObject obj, bool setActive = false, string ownerName = default)
    {
        if (ownerName == default)
            ownerName = this.name;
        // Needs to check and init if ownerName not default.
        InitPool(ownerName, objName);

        obj.SetActive(setActive);
        ownerDictionary[ownerName][objName].Enqueue(obj);
    }

    /// <summary>
    /// Reload list of [GameObject].
    /// </summary>
    public void Reload(string objName, IEnumerable<GameObject> objects, bool setActive, string ownerName = default)
    {
        if (ownerName == default)
            ownerName = this.name;
        // Needs to check and init if ownerName not default.
        InitPool(ownerName, objName);

        foreach (var obj in objects)
        {
            obj.SetActive(setActive);
            ownerDictionary[ownerName][objName].Enqueue(obj);
        }
    }

    /// <summary>
    /// Reload a object with generic class type (Monobehaviour).
    /// </summary>
    public void Reload<T>(string objName, T obj, bool setActive = false, string ownerName = default) where T : MonoBehaviour
    {
        if (ownerName == default)
            ownerName = this.name;
        // Needs to check and init if ownerName not default.
        InitPool(ownerName, objName);

        obj.gameObject.SetActive(setActive);
        ownerDictionary[ownerName][objName].Enqueue(obj);
    }

    /// <summary>
    /// Reload objects list with generic class type (Monobehaviour).
    /// </summary>
    public void Reload<T>(string objName, IEnumerable<T> objects, bool setActive, string ownerName = default) where T : MonoBehaviour
    {
        if (ownerName == default)
            ownerName = this.name;
        // Needs to check and init if ownerName not default.
        InitPool(ownerName, objName);

        foreach (var obj in objects)
        {
            obj.gameObject.SetActive(setActive);
            ownerDictionary[ownerName][objName].Enqueue(obj);
        }
    }

    public T Unload<T>(string objName, string ownerName = default)
    {
        if (ownerName == default)
            ownerName = this.name;

        return (T)ownerDictionary[ownerName][objName].Dequeue();
    }

    public IEnumerable<T> UnloadAll<T>(string objName, string ownerName = default)
    {
        if (ownerName == default)
            ownerName = this.name;

        List<T> unloadItems = new List<T>();
        var queue = ownerDictionary[ownerName][objName];
        int count = queue.Count;
        while (count > 0)
        {
            var obj = queue.Dequeue();
            count--;
            if (obj is T resultObj)
            {
                unloadItems.Add(resultObj);
            }
            else
            {
                ownerDictionary[ownerName][objName].Enqueue(obj);
            }
        }
        return unloadItems;
    }

    public IEnumerable<T> UnloadAll<T>(string ownerName = default)
    {
        if (ownerName == default)
            ownerName = this.name;

        List<T> unloadItems = new List<T>();
        foreach (var pool in ownerDictionary[ownerName])
        {
            int count = pool.Value.Count;
            while (count > 0)
            {
                var obj = pool.Value.Dequeue();
                count--;
                if (obj is T resultObj)
                {
                    unloadItems.Add(resultObj);
                }
                else
                {
                    ownerDictionary[ownerName][pool.Key].Enqueue(obj);
                }
            }
        }

        return unloadItems;
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
            ownerDictionary[ownerName].Add(poolName, new Queue<object>());
        }
    }

    public class OwnerDictionary : Dictionary<string, PoolDictionary> { }
    public class PoolDictionary : Dictionary<string, Queue<object>> { }
}
