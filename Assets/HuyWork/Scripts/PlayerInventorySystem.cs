using System.Collections.Generic;
using UnityEngine;

public class PlayerInventorySystem : MonoBehaviour
{
    [SerializeField]
    GameObject playerInventory;
    [SerializeField]
    List<GameObject> playerItems;
    [SerializeField]
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInventory = GameObject.Find("PlayerCameraRoot/Inventory");
        playerItems = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeEquippedItem();
    }

    public void AddToInventory(GameObject item)
    {
        if (item != null && !playerItems.Contains(item))
        {
            playerItems.Add(item);
            Debug.Log("Added to inventory: " + item.name);
        }
        else
        {
            Debug.LogWarning("Item is null or already in inventory: " + item);
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
                        Debug.Log($"Unequipped item slot {i}: {playerItems[index].name}");
                    }
                    else
                    {
                        // Tắt tất cả item trong inventory
                        foreach (Transform transform in playerInventory.transform)
                        {
                            transform.gameObject.SetActive(false);
                        }
                        // Hiện item được chọn
                        playerItems[index].SetActive(true);
                        playerState.onHoldingItem = playerItems[index];
                        Debug.Log($"Equipped item slot {i}: {playerItems[index].name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"No item in slot {i}");
                }
            }
        }
    }
}