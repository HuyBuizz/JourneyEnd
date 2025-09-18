using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcId = "NPC_Captain";
    public float interactDistance = 3f;
    public MissionStepType stepType = MissionStepType.TalkToNPC;

    private Transform player;

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (MissionManager.Instance == null)
        {
            Debug.LogError("[NPCInteract] MissionManager Instance is null!");
            return;
        }

        Debug.Log($"[NPCInteract] Talking to {npcId}, stepType={stepType}");

        if (stepType == MissionStepType.RescuePeople)
        {
            MissionManager.Instance.ReportRescueDelta(1);
        }
        else if (stepType == MissionStepType.ExtinguishFire)
        {
            MissionManager.Instance.ReportExtinguishDelta(9999f);
        }
        else
        {
            MissionManager.Instance.TryProgressStep(stepType);
        }
    }
}
