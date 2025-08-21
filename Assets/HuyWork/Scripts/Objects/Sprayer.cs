using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

public class Sprayer : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float shootSpeed = 10f;   // Tốc độ bắn
    public Transform shootPoint;     // Nơi xuất phát

    private EntityManager _em;
    private Entity _bulletPrefab;    // Entity prefab đã bake

    void Awake()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        _em = world.EntityManager;

    }

    private void Start()
    {
        var query = _em.CreateEntityQuery(ComponentType.ReadOnly<BulletPrefab>());
        if (!query.IsEmpty)
        {
            var singleton = query.GetSingleton<BulletPrefab>();
            _bulletPrefab = singleton.Value; // <- Đây là Entity prefab hợp lệ
        }
        query.Dispose();
    }

    public void Shoot()
    {

        // Bảo vệ
        var item = GetComponent<Item>();
        if (_bulletPrefab == Entity.Null || shootPoint == null || item == null || item.equipper == null)
        {
            return;
        }

        // Hướng bắn lấy từ player
        Vector3 dir = item.equipper.GetComponent<Player>().playerLookDirection.normalized;

        // Instantiate entity từ prefab
        Entity proj = _em.Instantiate(_bulletPrefab);

        // Đặt transform ban đầu
        _em.SetComponentData(proj, new LocalTransform
        {
            Position = (float3)shootPoint.position,
            Rotation = quaternion.LookRotationSafe((float3)dir, new float3(0, 1, 0)),
            Scale = 1f
        });

        // Đặt vận tốc để bay
        _em.SetComponentData(proj, new PhysicsVelocity
        {
            Linear = (float3)(dir * shootSpeed),
            Angular = float3.zero
        });
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))

            Shoot();
    }
}
