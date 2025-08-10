using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Diagnostics;

public partial class CubeLogicSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Đọc vị trí player từ singleton entity
        if (!SystemAPI.HasSingleton<PlayerPosition>())
            return;

        float3 playerPos = SystemAPI.GetSingleton<PlayerPosition>().Value;
        float detectDistance = 8f;

        foreach (var (transform, ecube, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ECube>>().WithAll<ETFCube>().WithEntityAccess())
        {
            float dist = math.distance(playerPos, transform.ValueRO.Position);
            ecube.ValueRW.isSmall = dist <= detectDistance;
        }

        // 2. Thay đổi kích thước dựa vào sự thay đổi của isSmall
        foreach (var (ecube, transform) in SystemAPI.Query<RefRO<ECube>, RefRW<LocalTransform>>().WithAll<ETFCube>())
        {
            if (ecube.ValueRO.isSmall && transform.ValueRW.Scale != 0.2f)
            {
                transform.ValueRW.Scale = 0.2f;
            }
            else if (!ecube.ValueRO.isSmall && transform.ValueRW.Scale != 1f)
            {
                transform.ValueRW.Scale = 1f;
            }
        }
    }
}