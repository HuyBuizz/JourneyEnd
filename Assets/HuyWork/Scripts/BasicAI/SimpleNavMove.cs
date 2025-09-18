using UnityEngine;
using UnityEngine.AI;

public class SimpleNavMove : MonoBehaviour
{
    public NavMeshAgent agent; // Kéo thả NavMeshAgent vào inspector
    public Transform target;   // Vị trí đích, có thể là 1 GameObject trên scene

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
        // Ví dụ: di chuyển tới vị trí nhấp chuột
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MoveToTarget(hit.point);
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
}
