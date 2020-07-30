using UnityEngine;

public sealed class MachineLandedSmoke : MonoBehaviour
{
    public ParticleSystem smokeParticleSystem;
    public float radiusExtra;
    public Vector3 offset;
    float radius;

    public void Landed()
    {
        
        ParticleSystem.ShapeModule shape = smokeParticleSystem.shape;
        shape.radius = radius;
        smokeParticleSystem.Play();
    }

    public void SetBounds(in Bounds3Int machineBounds)
    {
        Vector3 size = machineBounds.size;
        radius = Mathf.Min(size.x, size.z) + radiusExtra;
        transform.localPosition = machineBounds.bottomCenter + offset;
    }
}