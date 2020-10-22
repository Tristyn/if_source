using System.Runtime.CompilerServices;
using UnityEngine;

public sealed class MachineDropper : MonoBehaviour, IUpdate
{
    public MachineLandedSmoke machineDropSmoke;

    public float initialHeight = 2f;
    public Vector3 initialVelocity = new Vector3(0, -18f, 0);

    ParticleSystem smokeParticleSystem;
    Transform machineTransform;
    Vector3 velocity;
    bool landed;

    void Awake()
    {
        smokeParticleSystem = machineDropSmoke.smokeParticleSystem;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoUpdate()
    {
        if (!landed)
        {
            // Not accurate to do physics with unfixed time but who cares
            velocity += Physics.gravity * GameTime.deltaTime;
            Vector3 position_local = machineTransform.localPosition;
            position_local += velocity * GameTime.deltaTime;
            position_local.y = Mathf.Max(position_local.y, 0);

            machineTransform.localPosition = position_local;

            if (position_local.y < Mathf.Epsilon)
            {
                landed = true;
                Land();
            }
        }
        else
        {
            if (!smokeParticleSystem.IsAlive())
            {
                Recycle();
            }
        }
    }

    public void Drop(in Bounds3Int machineBounds, Transform machineTransform)
    {
        this.machineTransform = machineTransform;
        machineDropSmoke.SetBounds(in machineBounds);
        velocity = initialVelocity;

        Vector3 position_local = machineTransform.localPosition;
        position_local.y = initialHeight;
        machineTransform.localPosition = position_local;

        landed = false;
        Updates.machineDroppers.Add(this);
    }

    void Land()
    {
        MachineSystem.instance.MachineLanded();
        machineDropSmoke.Landed();
    }

    void Recycle()
    {
        Updates.machineDroppers.Remove(this);
        ObjectPooler.instance.Recycle(this);
        machineTransform = null;
    }
}