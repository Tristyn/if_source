using System.Runtime.CompilerServices;
using UnityEngine;

public sealed class MachineDropper : MonoBehaviour, IUpdate
{
    public MachineLandedSmoke machineDropSmoke;

    public float initialHeight = 2f;
    public Vector3 initialVelocity = new Vector3(0, -18f, 0);

    ParticleSystem smokeParticleSystem;
    Machine machine;
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

    public void Drop(Machine machine)
    {
        this.machine = machine;
        machineTransform = machine.instance.transform;
        machineDropSmoke.SetBounds(machine.bounds);
        velocity = initialVelocity;

        Vector3 position_local = machineTransform.localPosition;
        position_local.y = initialHeight;
        machineTransform.localPosition = position_local;

        landed = false;
        Entities.machineDroppers.Add(this);
    }

    void Land()
    {
        Events.MachineLanded?.Invoke(machine);
        machineDropSmoke.Landed();
    }

    void Recycle()
    {
        Entities.machineDroppers.Remove(this);
        ObjectPooler.instance.Recycle(this);
        machine = null;
        machineTransform = null;
    }
}