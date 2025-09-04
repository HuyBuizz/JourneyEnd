using Unity.Entities;
using UnityEngine;

public class KeyActionMappingAuthoring : MonoBehaviour
{
    public KeyActionMapping mapping;

    class Baker : Baker<KeyActionMappingAuthoring>
    {
        public override void Bake(KeyActionMappingAuthoring authoring)
        {
            if (authoring.mapping == null || authoring.mapping.KeyActions == null)
                return;

            // var entity = GetEntity(TransformUsageFlags.None);

            // var buffer = AddBuffer<KeyActionMappingData>(entity);

            // foreach (var keyAction in authoring.mapping.KeyActions)
            // {
            //     buffer.Add(
            //         new KeyActionMappingData
            //         {
            //             KeyCode = keyAction.KeyCode,
            //             Action = keyAction.Action,
            //         }
            //     );
            // }
        }
    }
}
