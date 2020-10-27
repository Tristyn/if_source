using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

public static class UIBehaviourExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetEnabled(this UIBehaviour[] uiBehaviours, bool enabled)
    {
        for(int i = 0, len = uiBehaviours.Length; i < len; ++i)
        {
            uiBehaviours[i].enabled = enabled;
        }
    }
}
