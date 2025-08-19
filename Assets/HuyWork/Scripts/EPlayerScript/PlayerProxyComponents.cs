using Unity.Entities;
using Unity.Mathematics;

public struct PlayerProxyTag : IComponentData {}
public struct LastPos : IComponentData { public float3 Value; }

public struct ProxyMoveConfig : IComponentData
{
    public float MoveSpeed;
    public float SprintMultiplier;
    public float JumpSpeed;
    public float Gravity;         // dương, nội bộ sẽ trừ
    public float MaxSlopeDeg;
    public float Skin;            // biên an toàn khi cast
    public int   MaxSlideIters;
}

public struct ProxyRuntimeState : IComponentData
{
    public float3 Velocity;
    public bool   IsGrounded;
}

// Singleton input: được “bridge” từ Mono mỗi frame
public struct PlayerInputState : IComponentData
{
    public float2 Move;           // WASD
    public float3 MoveDirWorld;   // hướng đi đã xoay theo yaw camera, y=0
    public bool   Sprint;
    public bool   Jump;           // nhấn 1 frame
}
