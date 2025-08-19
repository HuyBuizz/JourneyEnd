using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class ProxyPoseFollower : MonoBehaviour
{
    public Entity proxy;
    public Vector3 visualOffset;
    EntityManager _em;

    void Start(){ _em = World.DefaultGameObjectInjectionWorld.EntityManager; }

    void LateUpdate()
    {
        if (proxy == Entity.Null || !_em.Exists(proxy)) return;
        var lt = _em.GetComponentData<LocalTransform>(proxy);
        transform.SetPositionAndRotation((Vector3)(lt.Position + (float3)visualOffset), (Quaternion)lt.Rotation);
    }
}
