using Unity.Entities;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    public int damage = 10;

    class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new Bullet { damage = a.damage });
        }
    }
}

public struct Bullet : IComponentData
{
    public int damage;
}
