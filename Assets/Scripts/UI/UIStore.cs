using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIStore : MonoBehaviour
{
    public Button buttonCancel;

    UIBehaviour[] uibehaviours;

    void Awake()
    {
        uibehaviours = gameObject.GetComponentsInChildren<UIBehaviour>();
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        uibehaviours.SetEnabled(visible);
    }
}