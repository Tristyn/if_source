using UnityEngine;

public sealed class CameraShake : Singleton<CameraShake>
{
    public float angularFrequency;
    public float dampingRatio;
    public float dropMomentum;
    public float cutoffAmount;

    DampedSpringMotionParams spring;
    Vector3 position_local;
    Vector3 velocity_local;

    protected override void Awake()
    {
        base.Awake();
        Harmonic.CalcDampedSpringMotionParams(out spring, TimeHelper.fixedTimeStep, angularFrequency, dampingRatio);
    }

    public void DoUpdate()
    {
        Vector3 targetPosition_local = Vector3.zero;
        Harmonic.UpdateDampedSpringMotion(ref position_local, ref velocity_local, in targetPosition_local, in spring);
        if (position_local.Abs().AllLessThan(cutoffAmount) &&
            velocity_local.Abs().AllLessThan(cutoffAmount))
        {
            Vector3 zero = Vector3.zero;
            position_local = zero;
            velocity_local = zero;
            transform.localPosition = zero;
            enabled = false;
        }
        else
        {
            transform.localPosition = position_local;
        }
    }

    public void MachineLanded()
    {
        velocity_local.y += dropMomentum;
        enabled = true;
    }
}
