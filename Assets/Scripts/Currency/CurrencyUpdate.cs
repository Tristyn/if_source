using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class CurrencyUpdate : Singleton<CurrencyUpdate>
{
    public AnimationCurve heightAnimationCurve;
    public AnimationCurve yawAnimationCurve;

    AnimationCurveNative heightAnimationCurveNative;
    AnimationCurveNative yawAnimationCurveNative;
    Transform cameraTransform;
    List<Currency> newCollecting = new List<Currency>();
    List<Currency> newSpringing = new List<Currency>();
    List<Currency> currencyCollectingInstances = new List<Currency>();
    List<Currency> currencySpringingInstances = new List<Currency>();
    NativeList<CurrencyUpdateData> currencyCollectingData;
    NativeList<CurrencyUpdateData> currencySpringingData;
    TransformAccessArray currencyCollectingTransforms;
    TransformAccessArray currencySpringingTransforms;
    NativeList<int> currencyCollectingCompletedIndices;
    NativeList<int> currencySpringingCompletedIndices;

    JobHandle jobHandleCurrencyCollecting;
    JobHandle jobHandleCurrencyCollectingCompletedFilter;
    JobHandle jobHandleCurrencySpringing;
    JobHandle jobHandleCurrencySpringingCompletedFilter;

    struct CurrencyUpdateData
    {
        public Vector3 startPosition;
        public float startTime;
    }

    [BurstCompile]
    struct CurrencyCollectingJob : IJobParallelForTransform
    {
        [ReadOnly]
        public float time;
        [ReadOnly]
        public Vector3 cameraPosition_world;
        [ReadOnly]
        public AnimationCurveNative heightAnimationCurve;
        [ReadOnly]
        public AnimationCurveNative yawAnimationCurve;
        [ReadOnly]
        public NativeList<CurrencyUpdateData> currenciesSpringing;
        [ReadOnly]
        public TransformAccessArray currencyTransforms;

        public void Execute(int i, TransformAccess transform)
        {
            CurrencyUpdateData currencyUpdateData = currenciesSpringing[i];
            
            float deltaStartTime = time - currencyUpdateData.startTime;
            Vector3 position_local = currencyUpdateData.startPosition;
            float yaw_world = Vector2.Angle(new Vector2(cameraPosition_world.x, cameraPosition_world.z), new Vector2(position_local.x, position_local.z));

            float height = heightAnimationCurve.Evaluate(deltaStartTime);
            position_local.y += height;
            transform.localPosition = position_local;

            Vector3 rotation_local = new Vector3(0, yaw_world, 0);
            transform.localRotation = Quaternion.Euler(rotation_local);
        }
    }

    [BurstCompile]
    struct CurrencySpringingJob : IJobParallelForTransform
    {
        [ReadOnly]
        public float time;
        [ReadOnly]
        public Vector3 cameraPosition_world;
        [ReadOnly]
        public AnimationCurveNative heightAnimationCurve;
        [ReadOnly]
        public AnimationCurveNative yawAnimationCurve;
        [ReadOnly]
        public NativeArray<CurrencyUpdateData> currencies;

        public TransformAccessArray currencyTransforms;

        public void Execute(int i, TransformAccess transform)
        {
            CurrencyUpdateData currencyUpdateData = currencies[i];

            float deltaStartTime = time - currencyUpdateData.startTime;
            Vector3 position_local = currencyUpdateData.startPosition;
            float yaw_world = Vector2.Angle(new Vector2(cameraPosition_world.x, cameraPosition_world.z), new Vector2(position_local.x, position_local.z));

            float height = heightAnimationCurve.Evaluate(deltaStartTime);
            position_local.y += height;
            transform.localPosition = position_local;

            Vector3 rotation_local = new Vector3(0, yaw_world, 0);
            transform.localRotation = Quaternion.Euler(rotation_local);
        }
    }

    [BurstCompile]
    struct CurrencySpringingCompletedFilterJob : IJobParallelForFilter
    {
        [ReadOnly]
        public float minimumStartTime;
        [ReadOnly]
        public NativeList<CurrencyUpdateData> currencies;

        public bool Execute(int i)
        {
            return currencies[i].startTime < minimumStartTime;
        }
    }

    [BurstCompile]
    struct CurrencyCollectingCompletedFilterJob : IJobParallelForFilter
    {
        [ReadOnly]
        public float minimumStartTime;
        [ReadOnly]
        public NativeList<CurrencyUpdateData> currencies;

        public bool Execute(int i)
        {
            return currencies[i].startTime < minimumStartTime;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        currencyCollectingData = new NativeList<CurrencyUpdateData>(4, Allocator.Persistent);
        currencySpringingData = new NativeList<CurrencyUpdateData>(4, Allocator.Persistent);
        TransformAccessArray.Allocate(4, 4, out currencyCollectingTransforms);
        TransformAccessArray.Allocate(4, 4, out currencySpringingTransforms);
        currencyCollectingCompletedIndices = new NativeList<int>(4, Allocator.Persistent);
        currencySpringingCompletedIndices = new NativeList<int>(4, Allocator.Persistent);

        Init.Bind += () =>
        {
            cameraTransform = MainCamera.instanceTransform;
        };
    }

    protected override void OnDestroy()
    {
        currencyCollectingTransforms.Dispose();
        currencySpringingTransforms.Dispose();
    }

    public void Collect(Currency currency)
    {
        newCollecting.Add(currency);
    }

    public void Spring(Currency currency)
    {
        newSpringing.Add(currency);
    }

    JobHandle CreateCurrencyCollectingJob()
    {
        return new CurrencyCollectingJob()
        {
            time = Time.time,
            cameraPosition_world = cameraTransform.position,
            heightAnimationCurve = heightAnimationCurveNative,
            yawAnimationCurve = yawAnimationCurveNative,
            currencyTransforms = currencyCollectingTransforms
        }.Schedule(currencyCollectingTransforms);
    }

    JobHandle CreateCurrencySpringingJob()
    {
        return new CurrencySpringingJob()
        {
            time = Time.time,
            cameraPosition_world = cameraTransform.position,
            heightAnimationCurve = heightAnimationCurveNative,
            yawAnimationCurve = yawAnimationCurveNative,
            currencyTransforms = currencySpringingTransforms
        }.Schedule(currencySpringingTransforms);
    }

    JobHandle CreateCurrencySpringingCompletedFilterJob()
    {
        return new CurrencySpringingCompletedFilterJob()
        {
            minimumStartTime = Time.time - GetSpringingAnimationLength(),
            currencies = currencySpringingData,
        }.ScheduleAppend(currencySpringingCompletedIndices, currencySpringingData.Length, 32, jobHandleCurrencySpringing);
    }

    JobHandle CreateCurrencyCollectingCompletedFilterJob()
    {
        return new CurrencyCollectingCompletedFilterJob()
        {
            minimumStartTime = Time.time - GetSpringingAnimationLength(),
            currencies = currencyCollectingData,
        }.ScheduleAppend(currencyCollectingCompletedIndices, currencyCollectingData.Length, 32, jobHandleCurrencyCollecting);
    }

    public void DoUpdate()
    {
        jobHandleCurrencyCollectingCompletedFilter.Complete();
        jobHandleCurrencySpringingCompletedFilter.Complete();

        ClearCompleted();
        AddNew();
        CreateNativeAnimationCurves();

        jobHandleCurrencyCollecting = CreateCurrencyCollectingJob();
        jobHandleCurrencySpringing = CreateCurrencySpringingJob();

        jobHandleCurrencyCollectingCompletedFilter = CreateCurrencyCollectingCompletedFilterJob();
        jobHandleCurrencySpringingCompletedFilter = CreateCurrencySpringingCompletedFilterJob();
    }

    public void DoLateUpdate()
    {
        jobHandleCurrencyCollecting.Complete();
        jobHandleCurrencySpringing.Complete();
    }

    void ClearCompleted()
    {
        NativeList<int> currencyCollectingCompletedIndices = this.currencyCollectingCompletedIndices;
        for (int i = 0, len = currencyCollectingCompletedIndices.Length; i < len; ++i)
        {
            int index = currencyCollectingCompletedIndices[i];
            currencyCollectingInstances[index].Completed();
            currencyCollectingInstances.RemoveAtSwapBack(index);
            currencyCollectingData.RemoveAtSwapBack(index);
            currencyCollectingTransforms.RemoveAtSwapBack(index);
        }
        currencyCollectingCompletedIndices.Clear();

        NativeList<int> currencySpringingCompletedIndices = this.currencySpringingCompletedIndices;
        for (int i = 0, len = currencySpringingCompletedIndices.Length; i < len; ++i)
        {
            int index = currencySpringingCompletedIndices[i];
            currencyCollectingInstances[index].Completed();
            currencyCollectingInstances.RemoveAtSwapBack(index);
            currencySpringingData.RemoveAtSwapBack(index);
            currencySpringingTransforms.RemoveAtSwapBack(index);
        }
        currencySpringingCompletedIndices.Clear();
    }

    void AddNew()
    {
        NativeList<CurrencyUpdateData> currencyCollectingData = this.currencyCollectingData;
        TransformAccessArray currencyCollectingTransforms = this.currencyCollectingTransforms;
        List<Currency> newCollecting = this.newCollecting;
        for (int i = 0, len = newCollecting.Count; i < len; ++i)
        {
            Currency currency = newCollecting[i];
            currencyCollectingData.Add(new CurrencyUpdateData
            {
                startPosition = currency.startPosition,
                startTime = currency.startTime
            });
            currencyCollectingInstances.Add(currency);
            currencyCollectingTransforms.Add(currency.transform);
        }
        newCollecting.Clear();

        NativeList<CurrencyUpdateData> currencySpringingData = this.currencySpringingData;
        TransformAccessArray currencySpringingTransforms = this.currencySpringingTransforms;
        List<Currency> newSpringing = this.newSpringing;
        for (int i = 0, len = newSpringing.Count; i < len; ++i)
        {
            Currency currency = newSpringing[i];
            currencySpringingData.Add(new CurrencyUpdateData
            {
                startPosition = currency.startPosition,
                startTime = currency.startTime
            });
            currencySpringingInstances.Add(currency);
            currencySpringingTransforms.Add(currency.transform);
        }
        newSpringing.Clear();
    }

    void CreateNativeAnimationCurves()
    {
        heightAnimationCurveNative.Dispose();
        yawAnimationCurveNative.Dispose();
        heightAnimationCurveNative = new AnimationCurveNative(heightAnimationCurve, 60);
        yawAnimationCurveNative = new AnimationCurveNative(yawAnimationCurve, 60);
    }

    float GetSpringingAnimationLength()
    {
        return Mathf.Max(
            yawAnimationCurve.keys[yawAnimationCurve.length - 1].time,
            heightAnimationCurve.keys[heightAnimationCurve.length - 1].time);
    }
}
