using System.Runtime.CompilerServices;
using UnityEngine;

public static class GameTime
{
    public struct Save
    {
        public float time;
        public float deltaTime;
        public float fixedTime;
        public float fixedDeltaTime;
        public float unscaledTime;
        public float unscaledDeltaTime;
    }

    public static Save save;

    public static float time
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return save.time; }
    }

    public static float fixedTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return save.fixedTime; }
    }

    public static float deltaTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return save.deltaTime; }
    }

    public static float fixedDeltaTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return save.fixedDeltaTime; }
    }

    public static void StartOfTime()
    {
        save.time = 0f;
        save.fixedTime = 0f;
        save.unscaledTime = 0f;
    }

    public static void DoUpdate()
    {
        float deltaTime = Time.deltaTime;
        save.deltaTime = deltaTime;
        save.time += deltaTime;
    }

    public static void DoFixedUpdate()
    {
        if (save.fixedDeltaTime > Time.fixedDeltaTime)
        {
            Debug.LogWarning("fixed delta time grew from " + save.fixedDeltaTime + " to " + Time.fixedDeltaTime);
        }
        float fixedDeltaTime = Time.fixedDeltaTime;
        save.fixedDeltaTime = fixedDeltaTime;
        save.fixedTime += fixedDeltaTime;
        float unscaledDeltaTime = Time.unscaledDeltaTime;
        save.unscaledDeltaTime = unscaledDeltaTime;
        save.unscaledTime += unscaledDeltaTime;
    }
}
