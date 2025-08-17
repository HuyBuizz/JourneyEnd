using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct FlamePointWallSpawner : IComponentData
{
    [Header("Prefab")]
    public Entity prefab;       // prefab entity (đã bake đúng scale/rotation)
    [Header("Spawn Settings")]
    public float margin;        // lề viền theo 2D u,v
    public float pointDensity;  // điểm/m² trên mặt tường
    public float normalYAbsMax; // ngưỡng |normal.y| để coi là “tường” (vd 0.2)
    public float jitterK;       // tỉ lệ jitter so với spacing (vd 0.2)
}

public class FlamePointWallSpawnerAuthoring : MonoBehaviour
{
    public GameObject prefab;
    [Min(0)] public float margin = 0.1f;
    [Min(0.0001f)] public float pointDensity = 0.5f;
    [Range(0f,1f)] public float normalYAbsMax = 0.2f;
    [Range(0f,1f)] public float jitterK = 0.2f;

    class Baker : Baker<FlamePointWallSpawnerAuthoring>
    {
        public override void Bake(FlamePointWallSpawnerAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.None); // singleton rỗng
            AddComponent(e, new FlamePointWallSpawner
            {
                prefab        = GetEntity(a.prefab, TransformUsageFlags.Dynamic),
                margin        = math.max(0f, a.margin),
                pointDensity  = math.max(0.0001f, a.pointDensity),
                normalYAbsMax = math.saturate(a.normalYAbsMax),
                jitterK       = math.saturate(a.jitterK)
            });
        }
    }
}
