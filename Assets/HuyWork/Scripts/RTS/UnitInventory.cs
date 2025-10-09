using UnityEngine;

public class UnitInventory : MonoBehaviour
{
    [SerializeField] private GameObject isHoldingItem = null;
    [SerializeField] private Transform inventory;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = transform.Find("Inventory");
        if (inventory == null)
        {
            Debug.LogError("Inventory root not found!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PickupItem(GameObject item)
    {
        isHoldingItem = item;
        item.transform.parent = inventory;

        // Reset transform  
        item.transform.localPosition = Vector3.zero + Vector3.up * 2f;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;
    }
    
    public void DropItem()
    {
        isHoldingItem.transform.parent = null;
        isHoldingItem = null;
    }

}
