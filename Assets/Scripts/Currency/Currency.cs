using System;
using UnityEngine;

public abstract class Currency : MonoBehaviour
{
    [NonSerialized]
    protected CurrencyType currencyType;

    Animator animator;
    static int flipUpId = Animator.StringToHash("FlipUp");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(Vector3 position_local)
    {
        position_local += CurrencySystem.instance.currencySpawnOffset;
        transform.localPosition = position_local;

        animator.SetTrigger(flipUpId);
    }

    public void FlipUpEnded()
    {
        Recycle();
    }

    void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
