using UnityEngine;
using UnityEngine.Events;

public struct PickableTriggerState
{
    public bool mouseHovered;
    public bool mouseClicked;
}

[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class Pickable : MonoBehaviour
{
    public PickableTriggerState state;
    public UnityEvent picked;

    BoxCollider boxCollider;

    void Awake()
    {
        gameObject.layer = Layer.worldClickable;
        boxCollider = gameObject.AddOrGetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    public Vector3 size
    {
        get => boxCollider.size;
        set => boxCollider.size = value;
    }
}
