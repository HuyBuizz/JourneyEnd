using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public partial class CubeLogicSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float3 targetPos = new float3(2f, 2f, 2f);
        float detectDistance = 2f;

        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<ETFCube>().WithEntityAccess())
        {
            float dist = math.distance(targetPos, transform.ValueRO.Position);
            if (dist <= detectDistance)
            {
                transform.ValueRW.Scale = transform.ValueRO.Scale * 0.2f;
            }
        }
    }
}