using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    public MissionStepType triggerType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MissionManager.Instance.TryProgressStep(triggerType);
        }
    }
}
