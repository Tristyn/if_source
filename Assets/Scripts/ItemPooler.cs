using UnityEngine;
using System.Collections.Generic;

public class ItemPooler : Singleton<ItemPooler>
{
    const int MAX_POOL_COUNT = 64;

    private Dictionary<ItemInfo, List<Item>> pools = new Dictionary<ItemInfo, List<Item>>();

    public Item Get(ItemInfo itemInfo)
    {
        if (!pools.TryGetValue(itemInfo, out List<Item> pool))
        {
            pool = new List<Item>();
            pools.Add(itemInfo, pool);
        }

        int count = pool.Count;
        if (count > 0)
        {
            Item item = pool[count - 1];
            pool.RemoveAt(count - 1);
            item.transform.SetParent(null, false);
            item.gameObject.SetActive(true);
            return item;
        }
        Item newItem = Instantiate(itemInfo.prefab);
        newItem.itemInfo = itemInfo;
        newItem.Initialize();
        return newItem;
    }

    public void Recycle(Item item)
    {
        item.gameObject.SetActive(false);
        item.transform.SetParent(transform, false);

        if (!pools.TryGetValue(item.itemInfo, out List<Item> pool))
        {
            pool = new List<Item>();
            pools.Add(item.itemInfo, pool);
        }

        if (pool.Count < MAX_POOL_COUNT)
        {
            item.gameObject.SetActive(false);
            pool.Add(item);
        }
        else
        {
            Destroy(item.gameObject);
        }
    }
}
