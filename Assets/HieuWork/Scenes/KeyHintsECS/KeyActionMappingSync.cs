using Unity.Entities;
using UnityEngine;

public class KeyActionMappingSync : MonoBehaviour
{
    public KeyActionMapping mapping; // gắn ScriptableObject vào đây

    private Entity entity;
    private EntityManager em;

    void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Tạo 1 entity singleton để lưu buffer key mapping
        entity = em.CreateEntity();
        em.AddBuffer<KeyActionMappingData>(entity);

        // Lần đầu load
        ApplyMapping();
    }

    /// <summary>
    /// Copy dữ liệu từ ScriptableObject sang ECS Buffer
    /// </summary>
    public void ApplyMapping()
    {
        if (mapping == null || mapping.KeyActions == null)
            return;

        var buffer = em.GetBuffer<KeyActionMappingData>(entity);
        buffer.Clear();

        foreach (var keyAction in mapping.KeyActions)
        {
            buffer.Add(
                new KeyActionMappingData { KeyCode = keyAction.KeyCode, Action = keyAction.Action }
            );
        }
    }
}
