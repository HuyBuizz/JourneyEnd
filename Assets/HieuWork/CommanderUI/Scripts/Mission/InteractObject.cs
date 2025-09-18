using UnityEngine;

public class InteractObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string objectId = "DefaultObject";
    public float interactDistance = 3f;
    public MissionStepType stepType = MissionStepType.InteractObject;

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
            Debug.LogError("[InteractObject] MissionManager Instance is null!");
            return;
        }

        Debug.Log($"[InteractObject] Interacted with {objectId}, stepType={stepType}");

        // Nếu là RescuePeople → cộng 1 người
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
