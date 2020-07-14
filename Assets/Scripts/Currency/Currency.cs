using System;
using UnityEngine;

public class Currency : MonoBehaviour
{
    public float startTime;
    public Vector3 offset;
    [NonSerialized]
    public CurrencyType currencyType;

    [NonSerialized]
    public Vector3 startPosition;

    public void Initialize(Vector3 position_local)
    {
        Vector3 startPosition = position_local + offset;
        this.startPosition = startPosition;
        startTime = Time.time;
        transform.localPosition = startPosition;
    }

    public virtual void Collect()
    {
        CurrencyUpdate.instance.Collect(this);
    }

    public virtual void Completed() { }
}
