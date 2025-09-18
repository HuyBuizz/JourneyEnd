using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    public MissionStepType triggerType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (MissionManager.Instance == null)
            {
                Debug.LogError("[MissionTrigger] MissionManager Instance is null!");
                return;
            }
            MissionManager.Instance.TryProgressStep(triggerType);
        }
    }
}