using System.Runtime.CompilerServices;
using UnityEngine;

public static class GameTime
{
    public struct Save
    {
        public float time;
        public float fixedTime;
        public float unscaledTime;
    }

    public static float deltaTime;
    public static float fixedDeltaTime;
    public static float unscaledDeltaTime;
    public static Save save;

    public const float fixedTimeStep = 0.02f;
    public const float fixedFrameRate = 1f / fixedTimeStep;

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

    public static float unscaledTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return save.unscaledTime; }
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
        GameTime.deltaTime = deltaTime;
        save.time += deltaTime;
    }

    public static void DoFixedUpdate()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;
        float unscaledDeltaTime = Time.unscaledDeltaTime;
        GameTime.fixedDeltaTime = fixedDeltaTime;
        save.fixedTime += fixedDeltaTime;
        GameTime.unscaledDeltaTime = unscaledDeltaTime;
        save.unscaledTime += unscaledDeltaTime;
    }
}
