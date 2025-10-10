using UnityEngine;

public class UnitInventorySys : MonoBehaviour
{
    public float detectionRadius = 1f;
    public GameObject targetItem = null;
    private RTSUnitCommander commander;
    [SerializeField] private GameObject isHoldingItem = null;
    [SerializeField] private Transform inventory;

    void Start()
    {
        commander = UnityEngine.Object.FindFirstObjectByType<RTSUnitCommander>();
        if (commander == null)
        {
            Debug.LogError("RTSUnitCommander not found!");
        }
        inventory = transform.Find("Inventory");
        if (inventory == null)
        {
            Debug.LogError("Inventory root not found!");
            return;
        }
    }

    void Update()
    {
        // targetItem = commander.hitTargetClicked;
        if (targetItem == null) return;
        Detector();
    }

    private void Detector()
    {
        Vector3 toTarget = targetItem.transform.position - transform.position;
        if (toTarget.sqrMagnitude <= detectionRadius * detectionRadius)
        {
            PickupItem(targetItem);
            targetItem = null;
        }
    }

    public void PickupItem(GameObject item)
    {
        isHoldingItem = item;
        item.GetComponent<Rigidbody>().isKinematic = true;
        item.GetComponent<Interactable>().owner = this.gameObject;
        item.transform.parent = inventory;

        // Reset transform  
        item.transform.localPosition = Vector3.zero + Vector3.up * 2f;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;

        // commander.hitTargetClicked = null;
        targetItem = null;
    }

    public void DropItem()
    {
        isHoldingItem.GetComponent<Rigidbody>().isKinematic = false;
        isHoldingItem.GetComponent<Interactable>().owner = null;
        isHoldingItem.transform.parent = null;
        isHoldingItem = null;
    }
}
