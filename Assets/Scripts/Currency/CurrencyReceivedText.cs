using System;
using TMPro;
using UnityEngine;

public sealed class CurrencyReceivedText : MonoBehaviour
{
    public float duration;
    public float flickerDuration;
    public Color color1;
    public Color color2;

    [NonSerialized]
    public int amount;

    TextMeshPro text;
    float startTime;
    float nextFlickerCycle;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void Initialize()
    {
        startTime = GameTime.time;
        nextFlickerCycle = startTime + flickerDuration + flickerDuration;
    }

    void Update()
    {
        float time = GameTime.time;
    }
}
