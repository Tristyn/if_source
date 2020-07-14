﻿using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections;
using static Unity.Mathematics.math;

public struct AnimationCurveNative : IDisposable
{
	public bool IsCreated => values.IsCreated;

	private NativeArray<float> values;
	private NativeArray<Keyframe> keys;
	private WrapMode preWrapMode;
	private WrapMode postWrapMode;

	public AnimationCurveNative(AnimationCurve curve, int resolution)
    {
		if (curve == null)
			throw new NullReferenceException("Animation curve is null.");

		preWrapMode = curve.preWrapMode;
		postWrapMode = curve.postWrapMode;

		keys = new NativeArray<Keyframe>(curve.keys, Allocator.Persistent);

		values = new NativeArray<float>(resolution, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		
		for (int i = 0; i < resolution; i++)
			values[i] = curve.Evaluate((float)i / (float)resolution);
	}

	public float Evaluate(float t)
	{
		var count = keys.Length;

		if (count == 1)
			return keys[0].value;

		if (t < 0f)
		{
			switch (preWrapMode)
			{
				default:
					return keys[0].value;
				case WrapMode.Loop:
					t = 1f - (abs(t) % 1f);
					break;
				case WrapMode.PingPong:
					t = pingpong(t, 1f);
					break;
			}
		}
		else if (t > 1f)
		{
			switch (postWrapMode)
			{
				default:
					return keys[count - 1].value;
				case WrapMode.Loop:
					t %= 1f;
					break;
				case WrapMode.PingPong:
					t = pingpong(t, 1f);
					break;
			}
		}

		var it = t * (count - 1);

		var lower = (int)it;
		var upper = lower + 1;
		if (upper >= count)
			upper = count - 1;
		
		return lerp(values[lower], values[upper], it - lower);
	}

	public void Dispose()
	{
		if (values.IsCreated)
			values.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float repeat(float t, float length)
	{
		return clamp(t - floor(t / length) * length, 0, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float pingpong(float t, float length)
	{
		t = repeat(t, length * 2f);
		return length - abs(t - length);
	}
}