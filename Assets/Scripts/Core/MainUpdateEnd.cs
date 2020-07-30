using UnityEngine;

public sealed class MainUpdateEnd : MonoBehaviour
{
    MainUpdate mainUpdate;

    void Awake()
    {
        mainUpdate = GetComponent<MainUpdate>();
    }

    void Update()
    {
        mainUpdate.UpdateEnd();
    }

    void LateUpdate()
    {
        mainUpdate.LateUpdateEnd();
    }

    void FixedUpdate()
    {
        mainUpdate.FixedUpdateEnd();
    }
}