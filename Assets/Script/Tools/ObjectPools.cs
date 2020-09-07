using Necromancy.UI;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPools : Singleton<ObjectPools>
{
    [System.Serializable]
    public class Pool
    {
        public string name;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        GetPoolsOnStart();
    }

    protected virtual void GetPoolsOnStart()
    {
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.name = pool.name;
                obj.transform.SetParent(this.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.SetActive(false);
                objPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.name, objPool);
        }
    }

    /// <summary>
    /// 【In RunTime】生成物件，於物件池
    /// </summary>
    /// <param name="obj">哪個物件</param>
    /// <param name="size">欲生成的物件數量</param>
    /// <param name="parent">指定父物件，若無則自動放入物件池中(this.transform)</param>
    public void RenderObjectPoolsInParent(GameObject obj, float size, Transform parent = null)
    {
        // 已經生成過的不會再次生成
        if (poolDictionary == null || poolDictionary.ContainsKey(obj.name))
            return;

        Queue<GameObject> objPool = new Queue<GameObject>();

        for (int i = 0; i < size; i++)
        {
            GameObject cloneObj = Instantiate(obj);
            cloneObj.name = obj.name;
            if (parent == null)
            {
                cloneObj.transform.SetParent(this.transform);
            }
            else
            {
                cloneObj.transform.SetParent(parent);
            }
            cloneObj.transform.localPosition = Vector3.zero;
            cloneObj.SetActive(false);
            objPool.Enqueue(cloneObj);
        }

        poolDictionary.Add(obj.name, objPool);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <param name="exhaust">Objects in pools will be exhaust with its pools capacity. 
    /// Need to use 【Reload】 method and can use again.</param>
    /// <returns></returns>
    public GameObject GetObjectInPools(string name, Vector3 position, bool exhaust = false)
    {
        if (CheckPool(name))
            return null;

        GameObject objectToSpawn = poolDictionary[name].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;

        if (!exhaust)
            poolDictionary[name].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public T GetObjectInPools<T>(string name, Vector3 position, bool exhaust = false)
    {
        if (CheckPool(name))
            return default;

        GameObject objectToSpawn = poolDictionary[name].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;

        if (!exhaust)
            poolDictionary[name].Enqueue(objectToSpawn);

        return objectToSpawn.GetComponent<T>();
    }

    public void Reload(string name, List<GameObject> objects)
    {
        if (poolDictionary == null || !poolDictionary.ContainsKey(name))
            return;

        foreach (var obj in objects)
        {
            obj.SetActive(false);
            poolDictionary[name].Enqueue(obj);
        }
    }

    private bool CheckPool(string name)
    {
        return poolDictionary == null || !poolDictionary.ContainsKey(name) || poolDictionary[name].Count == 0;
    }
}
