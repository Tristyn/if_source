using UnityEngine;

public sealed class Vars : ScriptableObject
{
    // These values will be overwritten by the scriptable object
    public Growth platformCostGrowth = new Growth(10000,1.35f, 1000);
    public Growth platformAreaGrowth = new Growth(80, 1.1f, 1000, GrowthRate.Quadratic);
    public Growth platformMinSizeGrowth = new Growth(5,1.01f, 1000, GrowthRate.Quadratic);
    public Growth platformMaxSizeGrowth = new Growth(7,1.1f, 1000, GrowthRate.Quadratic);
    public Growth MachineCostGrowth = new Growth(1,1.125f, 10, GrowthRate.Quadratic);
}
