using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class ObjectPooler : Singleton<ObjectPooler>
{
    const int MAX_POOL_COUNT = 64;

    public Behaviour[] prefabs;
    private Dictionary<Type, Component> prefabMap = new Dictionary<Type, Component>();
    private Dictionary<Type, List<Component>> pools = new Dictionary<Type, List<Component>>();

    protected override void Awake()
    {
        base.Awake();
        for(int i = 0, len = prefabs.Length; i < len; ++i)
        {
            Behaviour prefab = prefabs[i];
            prefabMap.Add(prefab.GetType(), prefab);
            pools.Add(prefab.GetType(), new List<Component>(4));
        }
    }

    public T Get<T>() where T : Component
    {
        if(!pools.TryGetValue(typeof(T), out List<Component> pool))
        {
            pool = new List<Component>();
            pools.Add(typeof(T), pool);
        }

        int count = pool.Count;
        if (count > 0)
        {
            T item = (T)pool[count - 1];
            pool.RemoveAt(count - 1);
            item.transform.SetParent(null, false);
            item.gameObject.SetActive(true);
            return item;
        }
        GameObject obj = Instantiate(prefabMap[typeof(T)].gameObject);
        return obj.GetComponent<T>();
    }

    public void Recycle(Behaviour obj)
    {
        Type poolKey = obj.GetType();

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform, false);

        Assert.IsTrue(pools.ContainsKey(poolKey));
        List<Component> pool = pools[poolKey];

        if (pool.Count < MAX_POOL_COUNT)
        {
            obj.gameObject.SetActive(false);
            pool.Add(obj);
        }
        else
        {
            Destroy(obj.gameObject);
        }
    }
}
