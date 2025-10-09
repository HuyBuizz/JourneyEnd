using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DetectItemCondition", story: "Detect [item] condition", category: "Action", id: "15329020b4ec2139d3f7e9ad2a525ee0")]
public partial class DetectItemConditionAction : Action
{
    private GameObject targetItem = null;
    [SerializeReference] public BlackboardVariable<GameObject> Item;

    protected override Status OnStart()
    {   
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

