using UnityEngine;

public class ItemDetector : MonoBehaviour
{
    public float detectionRadius = 1f;
    public GameObject targetItem = null;
    private RTSUnitCommander commander;

    void Start()
    {
        commander = UnityEngine.Object.FindFirstObjectByType<RTSUnitCommander>();
        if (commander == null)
        {
            Debug.LogError("RTSUnitCommander not found!");
        }
    }

    void Update()
    {
        targetItem = commander.hitTargetClicked;
        if (targetItem == null) return;
        Detector();
    }

    private void Detector()
    {
        Vector3 toTarget = targetItem.transform.position - transform.position;
        if (toTarget.sqrMagnitude <= detectionRadius * detectionRadius)
        {
            Debug.Log("Found");
            targetItem = null; 
        }
    }
}
