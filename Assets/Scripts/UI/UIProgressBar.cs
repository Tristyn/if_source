using System.Collections.Generic;
using UnityEngine;

public class UIProgressBar : MonoBehaviour
{
    public UISlider uiSlider;

    public long last;
    public int goalIndex;
    ProgressionInfo[] goals;
    long goal => goals != null && goalIndex < goals.Length ? goals[goalIndex].moneyCondition : 0;

    private void Awake()
    {
        Init.Bind += Bind;
        SaveLoad.LoadComplete += LoadComplete;
    }

    void Bind()
    {
        CurrencySystem.instance.moneyChanged.AddListener(OnMoneyChanged);
    }

    void LoadComplete()
    {
        goals = ProgressionSystem.instance.progressionScanner.RequiringMoneyGreaterThan(1).ToArray();
        goalIndex = 0;
        long money = CurrencySystem.instance.save.money;
        GetGoal(money);
        UpdateProgressBar();
    }

    void OnMoneyChanged()
    {
        UpdateProgressBar();

    }
    
    void UpdateProgressBar()
    {
        long money = CurrencySystem.instance.save.money;
        if (money > goal)
        {
            GetGoal(money);
        }

        uiSlider.CurrentValue = money / goal;
    }

    void GetGoal(long money)
    {
        for (int len = goals.Length; goalIndex < len; ++goalIndex)
        {
            ProgressionInfo goal = goals[goalIndex];
            if (goal.moneyCondition > money && !ProgressionStore.instance.GetProgressionComplete(goal))
            {
                break;
            }
        }
    }
}
