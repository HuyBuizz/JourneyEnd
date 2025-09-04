// EInteractableAuthoring.cs
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class EInteractableAuthoring : MonoBehaviour
{
    public EInteractableType interactableType;

    // Tuỳ chọn: gắn thêm tag theo loại để query nhanh
    public bool AddTypeTags = true;

    class Baker : Baker<EInteractableAuthoring>
    {
        public override void Bake(EInteractableAuthoring authoring)
        {
            // Chọn usage flags tuỳ nhu cầu:
            // - Static: TransformUsageFlags.Renderable
            // - Có thể di chuyển/tương tác vật lý: TransformUsageFlags.Dynamic
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EInteractable
            {
                eInteractableType = authoring.interactableType
            });

            if (authoring.AddTypeTags)
            {
                switch (authoring.interactableType)
                {
                    case EInteractableType.Takeable: AddComponent<TakeableTag>(entity); break;
                    case EInteractableType.Storage:  AddComponent<StorageTag>(entity);  break;
                    case EInteractableType.Climb:    AddComponent<ClimbTag>(entity);    break;
                }
            }
        }
    }
}

// Các tag rỗng để filter nhanh trong query (tuỳ chọn)
// Nếu muốn enable/disable không đổi cấu trúc, có thể đổi sang IEnableableComponent
public struct TakeableTag : IComponentData {}
public struct StorageTag  : IComponentData {}
public struct ClimbTag    : IComponentData {}


public enum EInteractableType : byte
{
    Takeable,
    Storage,
    Climb
}

public struct EInteractable : IComponentData
{
    public EInteractableType eInteractableType;
}