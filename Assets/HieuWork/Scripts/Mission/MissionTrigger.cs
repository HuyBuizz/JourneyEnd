using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    public MissionStepType triggerType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.TryProgressStep(triggerType);
            }
        }
    }
}