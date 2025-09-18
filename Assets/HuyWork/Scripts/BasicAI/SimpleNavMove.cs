using System;
using UnityEngine;
using UnityEngine.AI;

public class SimpleNavMove : MonoBehaviour
{
    public NavMeshAgent agent; // Kéo thả NavMeshAgent vào inspector
    public Transform target;   // Vị trí đích, có thể là 1 GameObject trên scene
    public float stopDistance = 0.5f; // Khoảng cách để coi là đã tới đích

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (target != null)
        {
            MoveToTarget(target.position);
        }
    }

    void Update()
    {
        if (agent != null && target != null)
        {
            // Kiểm tra xem agent đã gần đích chưa
            if (!agent.pathPending && agent.remainingDistance <= stopDistance)
            {
                OnReachDestination();
            }
        }
    }

    // Hàm di chuyển tới 1 vị trí Vector3
    public void MoveToTarget(Vector3 destination)
    {
        if (agent != null)
        {
            agent.SetDestination(destination);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            MoveToTarget(target.position);
        }
    }

    // Gọi khi agent đạt đích
    private void OnReachDestination()
    {
        // Tại đây có thể thêm hiệu ứng, âm thanh, v.v...
        Destroy(gameObject); // Destroy chính GameObject này
    }
}
