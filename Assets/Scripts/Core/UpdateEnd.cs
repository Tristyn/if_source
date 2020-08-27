using UnityEngine;

public sealed class UpdateEnd : MonoBehaviour
{
    // Just use Updater

    Updater updater;

    void Awake()
    {
        updater = GetComponent<Updater>();
    }

    void Update()
    {
        updater.UpdateEnd();
    }

    void LateUpdate()
    {
        updater.LateUpdateEnd();
    }

    void FixedUpdate()
    {
        updater.FixedUpdateEnd();
    }
}