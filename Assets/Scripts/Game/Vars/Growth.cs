using UnityEngine;

public enum GrowthRate : byte
{
    Exponential = 0,
    Quadratic = 1,
    Log = 2,
    Linear = 3,
    Constant = 4,
}

public struct Growth
{
    public float value;
    public float coefficient;
    public long rounding;
    public GrowthRate growthRate;

    public Growth(float baseValue,float coefficient, long rounding)
    {
        this.value = baseValue;
        this.coefficient = coefficient;
        this.rounding = rounding;
        growthRate = GrowthRate.Exponential;
    }

    public Growth(float baseValue, float coefficient, long rounding, GrowthRate growthRate)
    {
        this.value = baseValue;
        this.coefficient = coefficient;
        this.rounding = rounding;
        this.growthRate = growthRate;
    }

    public long At(long count) => At(value, count);

    public long At(float value, long count)
    {
        return growthRate switch
        {
            GrowthRate.Constant => Constant(value, coefficient, count, rounding),
            GrowthRate.Linear => Linear(value, coefficient, count, rounding),
            GrowthRate.Log => Log(value, coefficient, count, rounding),
            GrowthRate.Quadratic => Quadratic(value, coefficient, count, rounding),
            GrowthRate.Exponential => Exponential(value, coefficient, count, rounding),
            _ => Mathx.RoundToInt(value, rounding),
        };
    }

    // Growth Rate Functions
    //
    public static long Exponential(float value, float exponential, float count, long rounding)
    {
        return Mathx.RoundToInt(value * Mathf.Pow(exponential, count), rounding);
    }

    public static long Quadratic(float value, float quadratic, float count, long rounding)
    {
        return Mathx.RoundToInt(value * quadratic * count, rounding);
    }

    public static long Log(float value, float log, float count, long rounding)
    {
        return Mathx.RoundToInt(value * Mathf.Log(log * count), rounding);
    }

    public static long Linear(float value, float linear, float count, long rounding)
    {
        return Mathx.RoundToInt(value + linear * count, rounding);
    }

    public static long Constant(float value, float constant, float count, long rounding)
    {
        return Mathx.RoundToInt(value, rounding);
    }
}