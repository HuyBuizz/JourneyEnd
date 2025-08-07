using System.Collections.Generic;
using UnityEngine;

public class PlayerInventorySystem : MonoBehaviour
{
    [SerializeField]
    Transform playerInventoryRoot;
    [SerializeField]
    List<GameObject> playerItems;
    [SerializeField]
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInventoryRoot = transform.Find("PlayerCameraRoot/Inventory");
        if (playerInventoryRoot == null)
        {
            Debug.LogError("PlayerInventory root not found!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ChangeEquippedItem();
        LocalInventoryItemListUpdate(playerInventoryRoot, playerItems);
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

    public void AddToPlayerInventory(GameObject item)
    {
        if (item != null && !playerItems.Contains(item))
        {
            playerItems.Add(item);
        }
    }

    public void RemoveFromPlayerInventory(GameObject item)
    {
        if (item != null && playerItems.Contains(item))
        {
            playerItems.Remove(item);
        }
    }

    public void ChangeEquippedItem()
    {
        PlayerState playerState = GetComponent<PlayerState>();
        for (int i = 1; i <= 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                int index = i - 1;
                if (index < playerItems.Count && playerItems[index] != null)
                {
                    // Nếu đang equip item này thì ấn lại sẽ tắt nó đi
                    if (playerState.onHoldingItem == playerItems[index] && playerItems[index].activeSelf)
                    {
                        playerItems[index].SetActive(false);
                        playerState.onHoldingItem = null;
                    }
                    else
                    {
                        // Tắt tất cả item trong inventory
                        foreach (Transform transform in playerInventoryRoot.transform)
                        {
                            transform.gameObject.SetActive(false);
                        }
                        // Hiện item được chọn
                        playerItems[index].SetActive(true);
                        playerState.onHoldingItem = playerItems[index];
                    }
                }
            }
        }
    }
}