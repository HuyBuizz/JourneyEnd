using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class FakeGravityWithCollider : MonoBehaviour
{
    [Header("Gravity")]
    public float gravity = -9.81f;              // m/s^2
    public float maxFallSpeed = 50f;            // clamp rơi
    public bool enableBounce = false;
    public bool useGravity = true;              // bật tắt trọng lực
    [Range(0f, 1f)] public float bounce = 0.2f; // hệ số đàn hồi khi đập đất

    [Header("Collision")]
    public LayerMask groundLayers = ~0;         // lớp va chạm
    public float skin = 0.02f;                  // khoảng hở chống kẹt
    public float groundSnapDistance = 0.05f;    // tự dính đất nếu rất sát
    public bool useDeltaTime = true;

    [Header("Reference")]
    Item item;

    private float _vy;                           // vận tốc dọc
    private Collider _col;

    void Awake()
    {
        _col = GetComponent<Collider>();
        if (_col == null)
            Debug.LogError("[FakeGravity] Require a non-trigger Collider.");
        else if (_col.isTrigger)
            Debug.LogWarning("[FakeGravity] Collider is trigger. Set it to non-trigger for collision.");

        item = GetComponent<Item>();
        if (item == null)
            Debug.LogWarning("[FakeGravity] Item component not found. Gravity will not be applied if item is equipped.");
    }

    void Update()
    {
        useGravity = item.equipper == null ? true : false;
        if (!useGravity) return;

        if (_col == null) return;

        float dt = useDeltaTime ? Time.deltaTime : 1f;

        // 1) cộng gia tốc trọng lực
        _vy += gravity * dt;
        _vy = Mathf.Clamp(_vy, -maxFallSpeed, maxFallSpeed);

        // 2) di chuyển theo trục Y bằng shape-cast theo collider
        Vector3 move = new Vector3(0f, _vy * dt, 0f);
        MoveWithShapeCast(move, out bool hitGround, out RaycastHit hit);

        // 3) nếu rơi và chạm đất → xử lý bật nảy / đứng yên
        if (hitGround)
        {
            if (enableBounce && Mathf.Abs(_vy) > 0.1f)
                _vy = -_vy * bounce;
            else
                _vy = 0f;
        }
        else
        {
            // 4) snap xuống đất nếu rất sát (giảm rung viền bậc)
            if (_vy <= 0f && groundSnapDistance > 0f)
                SnapToGround();
        }

        // 5) depenetrate nếu có overlap (an toàn chống kẹt)
        ResolveOverlaps(2);
    }

    // ===== core movement =====
    void MoveWithShapeCast(Vector3 delta, out bool hitGround, out RaycastHit bestHit)
    {
        hitGround = false; bestHit = default;
        if (delta.y == 0f) return;

        float dist = Mathf.Abs(delta.y);
        Vector3 dir = delta.y > 0f ? Vector3.up : Vector3.down;

        if (TryCast(dir, dist + skin, out bestHit))
        {
            // dừng ngay trước khi chạm, trừ skin
            float moveDist = Mathf.Max(0f, bestHit.distance - skin);
            transform.position += dir * moveDist;

            // nếu đang rơi xuống và chạm mặt đất
            if (dir == Vector3.down) hitGround = true;
        }
        else
        {
            // không chạm → đi hết
            transform.position += delta;
        }
    }

    // ===== shape-cast theo loại collider =====
    bool TryCast(Vector3 dir, float distance, out RaycastHit hit)
    {
        hit = default;
        int lay = groundLayers;

        if (_col is CapsuleCollider cap)
        {
            GetWorldCapsule(cap, out Vector3 a, out Vector3 b, out float r);
            return Physics.CapsuleCast(a, b, r, dir, out hit, distance, lay, QueryTriggerInteraction.Ignore);
        }
        if (_col is SphereCollider sph)
        {
            Vector3 c = sph.transform.TransformPoint(sph.center);
            float r = Mathf.Max(
                Mathf.Abs(sph.transform.lossyScale.x),
                Mathf.Abs(sph.transform.lossyScale.y),
                Mathf.Abs(sph.transform.lossyScale.z)) * sph.radius;
            return Physics.SphereCast(c, r, dir, out hit, distance, lay, QueryTriggerInteraction.Ignore);
        }
        if (_col is BoxCollider box)
        {
            Vector3 center = box.transform.TransformPoint(box.center);
            Vector3 half = Vector3.Scale(box.size * 0.5f, Abs(box.transform.lossyScale));
            return Physics.BoxCast(center, half, dir, out hit, box.transform.rotation, distance, lay, QueryTriggerInteraction.Ignore);
        }

        // fallback: raycast từ bounds
        Bounds bds = _col.bounds;
        Vector3 origin = dir.y < 0 ? new Vector3(bds.center.x, bds.min.y + 0.001f, bds.center.z)
                                   : new Vector3(bds.center.x, bds.max.y - 0.001f, bds.center.z);
        return Physics.Raycast(origin, dir, out hit, distance, lay, QueryTriggerInteraction.Ignore);
    }

    // ===== snap to ground (nếu rất sát) =====
    void SnapToGround()
    {
        if (TryCast(Vector3.down, groundSnapDistance + skin, out RaycastHit hit))
        {
            // hạ xuống chạm bề mặt (trừ skin)
            transform.position += Vector3.down * Mathf.Max(0f, hit.distance - skin);
        }
    }

    // ===== resolve overlaps bằng ComputePenetration =====
    void ResolveOverlaps(int iterations)
    {
        var cols = Physics.OverlapBox(_col.bounds.center, _col.bounds.extents + Vector3.one * 0.001f,
                                      Quaternion.identity, groundLayers, QueryTriggerInteraction.Ignore);
        for (int it = 0; it < iterations; it++)
        {
            bool any = false;
            foreach (var other in cols)
            {
                if (other == null || other == _col) continue;
                if (Physics.ComputePenetration(
                    _col, transform.position, transform.rotation,
                    other, other.transform.position, other.transform.rotation,
                    out Vector3 dir, out float dist))
                {
                    if (dist > 0f)
                    {
                        transform.position += dir * (dist + skin * 0.5f);
                        any = true;
                    }
                }
            }
            if (!any) break;
        }
    }

    // ===== helpers =====
    static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    void GetWorldCapsule(CapsuleCollider cap, out Vector3 a, out Vector3 b, out float r)
    {
        // tính 2 đầu world-space của capsule (theo axis)
        var t = cap.transform;
        float3 lossy = (Vector3)Abs(t.lossyScale);
        r = cap.radius * Mathf.Max(lossy.x, lossy.z);
        float height = Mathf.Max(cap.height * lossy.y, 2f * r);
        Vector3 center = t.TransformPoint(cap.center);

        Vector3 axis = Vector3.up; // CapsuleCollider ở Unity mặc định theo Y
        float half = (height * 0.5f) - r;

        a = center + axis * half;
        b = center - axis * half;
    }
}
