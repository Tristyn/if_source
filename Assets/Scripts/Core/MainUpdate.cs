using UnityEngine;

public class MainUpdate : MonoBehaviour
{
    CurrencyUpdate currencyUpdate;

    private void Awake()
    {
        Init.Bind += () =>
        {
            currencyUpdate = CurrencyUpdate.instance;
        };
    }

    void Update()
    {
        currencyUpdate.DoUpdate();
    }

    public void UpdateEnd()
    {

    }

    void LateUpdate()
    {

    }

    public void LateUpdateEnd()
    {
        currencyUpdate.DoLateUpdate();
    }

    void FixedUpdate()
    {

    }

    public void FixedUpdateEnd()
    {

    }
}