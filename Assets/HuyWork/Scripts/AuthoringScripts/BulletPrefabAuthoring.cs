using Unity.Entities;
using UnityEngine;

public class BulletPrefabAuthoring : MonoBehaviour
{
    // Kéo thả prefab đạn (GameObject) đã có PhysicsBody/PhysicsShape/BulletAuthoring
    public GameObject bulletPrefabGO;

    class Baker : Baker<BulletPrefabAuthoring>
    {
        public override void Bake(BulletPrefabAuthoring authoring)
        {
            // Entity "holder" để chứa singleton
            var holder = GetEntity(TransformUsageFlags.None);

            // Lấy Entity tương ứng của prefab đạn
            var bulletPrefabEntity = GetEntity(authoring.bulletPrefabGO, TransformUsageFlags.Dynamic);

            // Lưu vào singleton để runtime tra cứu
            AddComponent(holder, new BulletPrefab { Value = bulletPrefabEntity });
        }
    }
}

// Singleton chứa tham chiếu Entity prefab
public struct BulletPrefab : IComponentData
{
    public Entity Value;
}
