using System.Collections.Generic;

public sealed class ItemPooler : Singleton<ItemPooler>
{
    const int MAX_POOL_COUNT = 64;

    private Dictionary<ItemInfo, Stack<Item>> pools = new Dictionary<ItemInfo, Stack<Item>>();

    public Item Get(ItemInfo itemInfo)
    {
        if (!pools.TryGetValue(itemInfo, out Stack<Item> pool))
        {
            pool = new Stack<Item>();
            pools.Add(itemInfo, pool);
        }

        int count = pool.Count;
        if (count > 0)
        {
            Item item = pool.Pop();
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

        if (!pools.TryGetValue(item.itemInfo, out Stack<Item> pool))
        {
            pool = new Stack<Item>();
            pools.Add(item.itemInfo, pool);
        }

        if (pool.Count < MAX_POOL_COUNT)
        {
            item.gameObject.SetActive(false);
            pool.Push(item);
        }
        else
        {
            Destroy(item.gameObject);
        }
    }
}
