using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class LegendEntry
{
    public string itemName = "Item Name";
    public string description = "Description";
    public Sprite iconSprite;
    public Color iconColor = Color.white;

    [Header("Optional")]
    public bool showItem = true;
}

public class SimpleLegendController : MonoBehaviour
{
    [Header("Legend Panel Reference")]
    public GameObject legendPanel;

    [Header("Legend Content")]
    public Transform legendContent; // Content area của ScrollView
    public TextMeshProUGUI legendTitle;

    [Header("Legend Items (Để trống ban đầu)")]
    public LegendEntry[] legendEntries = new LegendEntry[0]; // Empty array

    [Header("Legend Item Prefab")]
    public GameObject legendItemPrefab;

    [Header("Settings")]
    public bool showDebugLogs = true;

    private List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        // Ẩn legend panel ban đầu
        if (legendPanel != null)
        {
            legendPanel.SetActive(false);
        }

        // Setup title
        if (legendTitle != null)
        {
            legendTitle.text = "LEGEND";
        }

        if (showDebugLogs)
            Debug.Log("SimpleLegendController: Initialized");
    }

    public void ShowLegend()
    {
        if (legendPanel != null)
        {
            legendPanel.SetActive(true);
            RefreshLegendItems();

            if (showDebugLogs)
                Debug.Log("Legend Panel: SHOWN");
        }
    }

    public void HideLegend()
    {
        if (legendPanel != null)
        {
            legendPanel.SetActive(false);

            if (showDebugLogs)
                Debug.Log("Legend Panel: HIDDEN");
        }
    }

    public void ToggleLegend()
    {
        if (legendPanel != null)
        {
            bool isActive = legendPanel.activeInHierarchy;
            if (isActive)
            {
                HideLegend();
            }
            else
            {
                ShowLegend();
            }
        }
    }

    void RefreshLegendItems()
    {
        // Clear existing items
        ClearAllItems();

        // Create items from legendEntries
        CreateLegendItems();
    }

    void ClearAllItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            if (item != null)
            {
                DestroyImmediate(item);
            }
        }
        spawnedItems.Clear();
    }

    void CreateLegendItems()
    {
        if (legendContent == null || legendItemPrefab == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("Legend Content or Prefab not assigned!");
            return;
        }

        foreach (LegendEntry entry in legendEntries)
        {
            if (entry != null && entry.showItem)
            {
                CreateSingleItem(entry);
            }
        }

        if (showDebugLogs)
            Debug.Log($"Created {spawnedItems.Count} legend items");
    }

    void CreateSingleItem(LegendEntry entry)
    {
        // Instantiate prefab
        GameObject newItem = Instantiate(legendItemPrefab, legendContent);
        newItem.name = $"LegendItem_{entry.itemName}";

        // Find components in prefab
        Image iconImage = newItem.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = newItem.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = newItem.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();

        // Setup icon
        if (iconImage != null)
        {
            iconImage.sprite = entry.iconSprite;
            iconImage.color = entry.iconColor;

            // Ẩn icon nếu không có sprite
            iconImage.gameObject.SetActive(entry.iconSprite != null);
        }

        // Setup name
        if (nameText != null)
        {
            nameText.text = entry.itemName;
        }

        // Setup description
        if (descText != null)
        {
            descText.text = entry.description;
        }

        spawnedItems.Add(newItem);
    }

    // Public methods để thêm/xóa items runtime
    public void AddLegendItem(string name, string desc, Sprite sprite = null, Color color = default)
    {
        if (color == default)
            color = Color.white;

        LegendEntry newEntry = new LegendEntry
        {
            itemName = name,
            description = desc,
            iconSprite = sprite,
            iconColor = color,
            showItem = true
        };

        // Add to array
        List<LegendEntry> entryList = new List<LegendEntry>(legendEntries);
        entryList.Add(newEntry);
        legendEntries = entryList.ToArray();

        // Refresh if panel is active
        if (legendPanel != null && legendPanel.activeInHierarchy)
        {
            RefreshLegendItems();
        }

        if (showDebugLogs)
            Debug.Log($"Added legend item: {name}");
    }

    public void ClearAllLegendItems()
    {
        legendEntries = new LegendEntry[0];
        ClearAllItems();

        if (showDebugLogs)
            Debug.Log("Cleared all legend items");
    }

    public bool IsLegendVisible()
    {
        return legendPanel != null && legendPanel.activeInHierarchy;
    }

    public int GetItemCount()
    {
        return legendEntries.Length;
    }
}