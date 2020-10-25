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
    public static float unscaledDeltaTime;
    public static Save save;

    public const float fixedDeltaTime = 0.01666668f;
    public const float fixedFrameRate = 1f / fixedDeltaTime;

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
        float unscaledDeltaTime = Time.unscaledDeltaTime;
        save.fixedTime += fixedDeltaTime;
        GameTime.unscaledDeltaTime = unscaledDeltaTime;
        save.unscaledTime += unscaledDeltaTime;
    }
}
