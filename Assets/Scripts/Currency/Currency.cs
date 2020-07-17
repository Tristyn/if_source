using System;
using UnityEngine;

public abstract class Currency : MonoBehaviour
{
    //[NonSerialized]
    //Vector3 startPosition;
    //[NonSerialized]
    //float startTime;
    [NonSerialized]
    protected CurrencyType currencyType;

    Animator animator;
    static int flipUpId = Animator.StringToHash("FlipUp");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(Vector3 position_local)
    {
        position_local += CurrencySystem.instance.currencySpawnOffset;
        //Vector3 cameraPosition_world = MainCamera.instanceTransform.position;
        //Vector2 positionDelta_world = new Vector2(position_world.x, position_world.z) - new Vector2(cameraPosition_world.x, cameraPosition_world.z);
        //positionDelta_world.Normalize();
        //float yaw_world = Mathf.Atan2(positionDelta_world.x, positionDelta_world.y) * Mathf.Rad2Deg;
        //Quaternion rotation_world = Quaternion.Euler(0, yaw_world, 0);
        transform.localPosition = position_local;

        animator.SetTrigger(flipUpId);
    }

    //void Update()
    //{
    //    CurrencySystem currencySystem = CurrencySystem.instance;

    //    float deltaStartTime = Time.time - startTime;
    //    Vector3 position_world = startPosition;

    //    Vector3 cameraPosition_world = MainCamera.instanceTransform.position;
    //    float pitch_world = Vector2.Angle(new Vector2(cameraPosition_world.x, cameraPosition_world.z), new Vector2(position_world.x, position_world.z));
    //    float yaw_world = currencySystem.yawAnimationCurve.Evaluate(deltaStartTime);
    //    Quaternion rotation_world = Quaternion.Euler(pitch_world, yaw_world, 0);
    //    position_world.y += currencySystem.heightAnimationCurve.Evaluate(deltaStartTime);
    //    transform.SetPositionAndRotation(position_world, rotation_world);

    //    if (startTime + currencySystem.collectAnimationDuration <= Time.time)
    //    {
    //        OnCollected();
    //    }
    //}

    public void FlipUpEnded()
    {
        Recycle();
    }

    void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
