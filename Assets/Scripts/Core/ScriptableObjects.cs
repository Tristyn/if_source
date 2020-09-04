public sealed class ScriptableObjects : Singleton<ScriptableObjectMasterList>
{
    public ScriptableObjectMasterList masterList;

    protected override void Awake()
    {
        instance = masterList;
        masterList.Initialize();
    }
}
