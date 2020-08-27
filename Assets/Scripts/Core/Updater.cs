using UnityEngine;

public sealed class Updater : MonoBehaviour
{
    void Update()
    {
        GameTime.DoUpdate();
    }

    public void UpdateEnd()
    {

    }

    void LateUpdate()
    {

    }

    public void LateUpdateEnd()
    {

    }

    void FixedUpdate()
    {
        GameTime.DoFixedUpdate();
    }

    public void FixedUpdateEnd()
    {

    }
}