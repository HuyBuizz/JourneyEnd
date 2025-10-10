using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "WaitForCommandNode",
    story: "Chờ RTSUnitCommander phát lệnh và ghi vào Blackboard [Command], [TargetPos] và [TargetGameObject]",
    category: "Action",
    id: "abcd1234")]
public partial class WaitForCommandNodeAction : Unity.Behavior.Action
{
    [CreateProperty]
    [SerializeReference]
    public BlackboardVariable<Command> Command;

    [CreateProperty]
    [SerializeReference]
    public BlackboardVariable<Transform> TargetPos;

    [CreateProperty]
    [SerializeReference]
    public BlackboardVariable<GameObject> TargetGameObject;

    private bool received;
    private RTSUnitCommander commander;
    private Unit unit;

    protected override Status OnStart()
    {
        received = false;
        commander = UnityEngine.Object.FindFirstObjectByType<RTSUnitCommander>();
        if (commander == null) return Status.Failure;

        unit = this.GameObject.GetComponent<Unit>();
        if (unit == null) return Status.Failure;
        if (!unit.isSelected) return Status.Failure;

        // commander.OnCommandIssued += OnIssued;

        return Status.Running;
    }

    private void OnIssued(string cmd, Vector3 pos, GameObject target)
    {
        switch (cmd)
        {
            case "Spray": Command.Value = global::Command.Spray; break;
            case "Take": Command.Value = global::Command.Take; break;
            case "Move": Command.Value = global::Command.Move; break;
            default: Command.Value = global::Command.None; break;
        }

        // Gán vị trí
        if (TargetPos.Value != null)
        {
            TargetPos.Value.position = pos;
        }
        else
        {
            var temp = new GameObject("TargetPosTemp");
            temp.transform.position = pos;
            TargetPos.Value = temp.transform;
        }

        // Gán target GameObject
        if (TargetGameObject.Value != null)
        {
            TargetGameObject.Value = target;
        }
        else
        {
            var temp = new GameObject("TargetGameObjectTemp");
            TargetGameObject.Value = target;
        }

        received = true;
    }


    protected override Status OnUpdate()
    {
        return received ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        // if (commander != null)
            // commander.OnCommandIssued -= OnIssued;
    }
}
