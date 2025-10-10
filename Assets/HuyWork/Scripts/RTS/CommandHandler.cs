using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CommandHandler : MonoBehaviour
{
    private RTSUnitCommander commander;
    private NavMeshAgent agent;
    private Unit unitComp;

    private Coroutine arrivalRoutine;   // routine theo dõi đến nơi
    private GameObject pendingPickup;   // target đang chờ tương tác (tuỳ bạn xử lý)

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!agent) Debug.LogWarning("[CommandHandler] Thiếu NavMeshAgent.");
        unitComp = GetComponent<Unit>(); // nếu không có cũng OK
    }

    private void OnEnable()
    {
        commander = FindFirstObjectByType<RTSUnitCommander>();
        if (!commander)
        {
            Debug.LogWarning("[CommandHandler] Không tìm thấy RTSUnitCommander!");
            return;
        }

        commander.OnMoveCommandIssued += OnMoveCommandIssuedHandler;
        commander.OnPickupCommandIssued += OnPickupCommandIssuedHandler;
        commander.OnForceStop += OnForceStopHandler;
    }

    private void OnDisable()
    {
        if (!commander) return;
        commander.OnMoveCommandIssued -= OnMoveCommandIssuedHandler;
        commander.OnPickupCommandIssued -= OnPickupCommandIssuedHandler;
        commander.OnForceStop -= OnForceStopHandler;

        StopArrivalRoutine();
    }

    // ==== Handlers ====

    private void OnMoveCommandIssuedHandler(Vector3 pos)
    {
        if (!agent || !agent.isOnNavMesh) return;

        pendingPickup = null; // không còn pickup nào đang chờ
        if (TrySetDestination(pos))
        {
            SetUnitStateMoving();
            RestartArrivalRoutine(); // bắt đầu theo dõi đến nơi
        }
    }

    private void OnPickupCommandIssuedHandler(GameObject target)
    {
        if (!agent || !agent.isOnNavMesh || !target) return;

        pendingPickup = target;

        // Điểm dưới chân target
        Vector3 dest = GetGroundPointBelow(target.transform.position, 100f, out bool ok);
        if (!ok) dest = target.transform.position;

        if (TrySetDestination(dest))
        {
            SetUnitStateMoving();
            RestartArrivalRoutine(); // theo dõi đến nơi rồi xử lý pickup (nếu bạn muốn)
        }
    }

    private void OnForceStopHandler()
    {
        if (agent && agent.isOnNavMesh) agent.ResetPath();
        StopArrivalRoutine();
        ResetUnitState();
        pendingPickup = null;
    }

    // ==== Movement helpers ====

    private bool TrySetDestination(Vector3 pos)
    {
        if (!agent) return false;

        // Tìm điểm hợp lệ gần nhất trên NavMesh
        if (NavMesh.SamplePosition(pos, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            pos = navHit.position;

        agent.SetDestination(pos);
        return true;
    }

    private Vector3 GetGroundPointBelow(Vector3 from, float maxDistance, out bool hitSomething)
    {
        from += Vector3.up * 0.1f; // tránh kẹt trong mesh
        hitSomething = Physics.Raycast(from, Vector3.down, out RaycastHit hit, maxDistance, ~0, QueryTriggerInteraction.Ignore);
        return hitSomething ? hit.point : from;
    }

    private bool HasArrived()
    {
        if (!agent) return true;
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;
        // đến nơi hoặc gần như đứng yên
        return !agent.hasPath || agent.velocity.sqrMagnitude <= 0.001f;
    }

    private void SetUnitStateMoving()
    {
        if (unitComp) unitComp.currentState = Unit.UnitState.Moving;
    }

    private void ResetUnitState()
    {
        if (unitComp) unitComp.currentState = Unit.UnitState.Idle;
    }

    // ==== Arrival coroutine ====

    private void RestartArrivalRoutine()
    {
        StopArrivalRoutine();
        arrivalRoutine = StartCoroutine(WaitUntilArrivedThenIdle());
    }

    private void StopArrivalRoutine()
    {
        if (arrivalRoutine != null)
        {
            StopCoroutine(arrivalRoutine);
            arrivalRoutine = null;
        }
    }

    private IEnumerator WaitUntilArrivedThenIdle()
    {
        // Đợi đến khi agent coi như “đã tới”
        while (!HasArrived())
            yield return null;

        // Tuỳ ý: nếu đang có lệnh Pickup, bạn trigger tương tác ở đây
        if (pendingPickup != null)
        {
            // TODO: gọi animation/nhặt đồ,… rồi mới Idle nếu cần
            // Ví dụ:
            // InteractWith(pendingPickup);
            pendingPickup = null;
        }

        ResetUnitState();
        arrivalRoutine = null;
    }
}
