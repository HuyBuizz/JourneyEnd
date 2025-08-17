using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

public struct Ball : IComponentData {}

[BurstCompile]
public partial struct BasicMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Ball>(); // Chỉ chạy khi có ít nhất 1 entity có Ball
    }

    // public void OnUpdate(ref SystemState state)
    // {
    //     float deltaTime = SystemAPI.Time.DeltaTime;
    //     float moveSpeed = 5f; // tốc độ di chuyển (unit/giây)

    //     // Đọc input từ bàn phím (UnityEngine)
    //     float horizontal = 0f;
    //     float vertical = 0f;

    //     if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
    //         vertical += 1f;
    //     if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
    //         vertical -= 1f;
    //     if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
    //         horizontal += 1f;
    //     if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
    //         horizontal -= 1f;

    //     float3 moveDir = new float3(horizontal, 0, vertical);
    //     if (math.lengthsq(moveDir) > 0)
    //     {
    //         moveDir = math.normalize(moveDir);

    //         foreach (var transform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Ball>())
    //         {
    //             transform.ValueRW.Position += moveDir * moveSpeed * deltaTime;
    //         }
    //     }
    // }
}
