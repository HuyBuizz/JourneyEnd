using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "MoveToTarget",
    story: "Di chuyển agent(Self) tới vị trí [Target]",
    category: "Action",
    id: "59e66d2273a71082650c2e854a00dc0d")]
public partial class MoveToTargetAction : Action
{
    [CreateProperty]
    [SerializeReference] public BlackboardVariable<Transform> Target;
    private NavMeshAgent agent;
    private RTSUnitCommander commander;
    private bool initialized;
    private bool finishRequested;

    protected override Status OnStart()
    {
        if (Target == null || Target.Value == null)
        {
            Debug.LogWarning("[MoveToTarget] Blackboard.Target null!");
            return Status.Failure;
        }

        var go = this.GameObject;
        if (go == null)
        {
            Debug.LogWarning("[MoveToTarget] Không tìm thấy GameObject (Self)!");
            return Status.Failure;
        }

        agent = go.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogWarning("[MoveToTarget] Self không có NavMeshAgent!");
            return Status.Failure;
        }

        commander = UnityEngine.Object.FindFirstObjectByType<RTSUnitCommander>();
        if (commander == null)
        {
            Debug.LogWarning("[MoveToTarget] Không tìm thấy RTSUnitCommander!");
            return Status.Failure;
        }

        // đăng ký
        commander.OnForceStop += OnForceStopHandler;

        // reset cờ
        finishRequested = false;

        // bắt đầu di chuyển
        agent.isStopped = false;
        agent.SetDestination(Target.Value.position);

        initialized = true;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!initialized || agent == null) return Status.Failure;

        // >>> nếu có yêu cầu kết thúc cưỡng bức
        if (finishRequested)
        {
            // dừng ngay chuyển động
            if (agent.hasPath) agent.ResetPath();
            agent.isStopped = true;

            // trả về trạng thái đã yêu cầu (ở đây là Success)
            return Status.Failure;
        }

        if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            return Status.Running;

        return Status.Success;
    }

    protected override void OnEnd()
    {
        initialized = false;
        if (commander != null)
            commander.OnForceStop -= OnForceStopHandler;
    }

    // Handler đúng chữ ký Action (void)
    private void OnForceStopHandler()
    {
        finishRequested = true;
    }
}
