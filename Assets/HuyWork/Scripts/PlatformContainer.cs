using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor utility: Gắn PlatformAuthoring cho tất cả child platform trong Editor (không phải ở runtime).
/// </summary>
public class PlatformContainer : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Add PlatformAuthoring To All Children")]
    void AddPlatformAuthoringToAllChildren()
    {
        foreach (Transform platform in transform)
        {
            if (platform.GetComponent<PlatformAuthoring>() == null)
            {
                Undo.AddComponent<PlatformAuthoring>(platform.gameObject);
            }
        }
        Debug.Log("Đã gắn PlatformAuthoring cho tất cả child platform.");
    }
#endif
}