using System.Text;
using TMPro;
using UnityEngine;

public sealed class UICurrency : MonoBehaviour
{
    TextMeshProUGUI text;
    StringBuilder textStringBuilder = new StringBuilder(6);

    long amount;

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
        amount = CurrencySystem.instance.save.money;
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
