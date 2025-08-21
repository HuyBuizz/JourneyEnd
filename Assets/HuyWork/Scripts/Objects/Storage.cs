using UnityEngine;
using System.Collections.Generic;

public class Storage : MonoBehaviour
{
    Transform storageRoot;
    [SerializeField]
    List<GameObject> storageItems;
    int maxStorageCapacity = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        storageRoot = transform.Find("Inventory");
        if (storageRoot == null)
        {
            Debug.LogError("StorageInventory root not found!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        LocalInventoryItemListUpdate(storageRoot, storageItems);
    }

   void LocalInventoryItemListUpdate(Transform inventoryRoot, List<GameObject> itemList)
    {
        if (inventoryRoot != null)
        {
            itemList = new List<GameObject>();
            foreach (Transform child in inventoryRoot.transform)
            {
                itemList.Add(child.gameObject);
            }
        }
    }

    public void TransferItemToStorage(GameObject item, GameObject player)
    {
        if (storageItems.Count < maxStorageCapacity && item != null && !storageItems.Contains(item))
        {
            ChangeParent(item, player);
            ResetObjectTransform(item);
        }
    }

    void ChangeParent(GameObject item, GameObject player)
    {
        if (player == null) return;
        item.transform.SetParent(storageRoot);

    }
    void ResetObjectTransform(GameObject item)
    {
        if (item == null) return;
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;
        item.transform.localRotation = Quaternion.identity;
    }
}
