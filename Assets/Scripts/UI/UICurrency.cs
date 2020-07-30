using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class UICurrency : MonoBehaviour
{
    TextMeshProUGUI text;
    StringBuilder textStringBuilder = new StringBuilder(6);

    int amount;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        Init.Bind += () =>
        {
            CurrencySystem.instance.moneyChanged.AddListener(OnMoneyChanged);
        };
    }

    void OnDestroy()
    {
        if (CurrencySystem.instance)
        {
            CurrencySystem.instance.moneyChanged.RemoveListener(OnMoneyChanged);
        }
    }

    void OnMoneyChanged()
    {
        SetMoney(CurrencySystem.instance.save.money);
    }

    public void AddMoney(int amount)
    {
        this.amount += amount;
        UpdateMoneyText();
    }

    public void SetMoney(int amount)
    {
        this.amount = amount;
        UpdateMoneyText();
    }

    void UpdateMoneyText()
    {
        textStringBuilder.Clear();
        textStringBuilder.Append('$');
        textStringBuilder.Append(amount);
        text.SetText(textStringBuilder);
    }
}
