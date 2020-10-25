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
        Harmonic.CalcDampedSpringMotionParams(out spring, GameTime.fixedDeltaTime, angularFrequency, dampingRatio);
        Events.MachineLanded += MachineLanded;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Events.MachineLanded -= MachineLanded;
    }

    public void DoFixedUpdate()
    {
        Vector3 targetPosition_local = new Vector3();
        Harmonic.UpdateDampedSpringMotion(ref position_local, ref velocity_local, in targetPosition_local, in spring);
    }

    public void DoUpdate()
    {
        if (position_local.Abs().AllLessThan(cutoffAmount) &&
            velocity_local.Abs().AllLessThan(cutoffAmount))
        {
            transform.localPosition = new Vector3();
        }
        else
        {
            transform.localPosition = position_local;
        }
    }

    void MachineLanded(Machine machine)
    {
        velocity_local.y += dropMomentum;
    }
}
