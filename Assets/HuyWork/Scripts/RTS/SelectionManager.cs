using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class SelectionManager : MonoBehaviour
{
    [Header("UI Selection Box")]
    [SerializeField, Tooltip("UI Image đại diện cho khung chọn")]
    private RectTransform selectionBox;

    [Header("Tag cho các Unit có thể chọn")]
    [SerializeField, Tooltip("Tag của các Unit được phép chọn")]
    private string selectableTag = "Unit";

    [Header("Màu sắc khi chọn / bỏ chọn")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    private Vector2 startPos;
    private Camera cam;
    private Canvas canvas;
    private bool isDragging;

    [Header("Danh sách Unit được chọn (reset mỗi lần)")]
    [SerializeField] private List<GameObject> units = new List<GameObject>();

    /// <summary>
    /// Cho script khác truy cập danh sách hiện tại.
    /// </summary>
    public IReadOnlyList<GameObject> SelectedUnits => units;

    private void Start()
    {
        if (selectionBox != null)
        {
            canvas = selectionBox.GetComponentInParent<Canvas>();
            selectionBox.gameObject.SetActive(false);
        }

        if (cam == null)
        {
            cam = Camera.main;
        }

    }

    private void Update()
    {
        if (selectionBox == null || cam == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = false; // Reset dragging state
        }

        if (Input.GetMouseButton(0))
        {
            // Consider it a drag if the mouse moves significantly
            if (Vector2.Distance(Input.mousePosition, startPos) > 5f) // Threshold to detect dragging
            {
                if (!isDragging)
                {
                    // Only activate selection box when drag starts
                    selectionBox.gameObject.SetActive(true);
                    isDragging = true;
                }
                UpdateSelectionBox(Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                // Handle drag selection
                SelectObjectsInBox(Input.mousePosition);
                selectionBox.gameObject.SetActive(false);
            }
            else
            {
                // Handle single click selection
                SelectSingleObject();
            }
        }
    }

    private void UpdateSelectionBox(Vector2 currentMousePos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, startPos, canvas.worldCamera, out Vector2 localStart);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, currentMousePos, canvas.worldCamera, out Vector2 localEnd);

        Vector2 size = localEnd - localStart;

        selectionBox.anchoredPosition = localStart + size / 2;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
    }

    private void SelectObjectsInBox(Vector2 endPos)
    {
        // Reset previous selection
        ResetSelection();

        // Calculate selection bounds
        Vector2 min = Vector2.Min(startPos, endPos);
        Vector2 max = Vector2.Max(startPos, endPos);

        // Check each unit
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag(selectableTag))
        {
            Vector3 screenPos = cam.WorldToScreenPoint(unit.transform.position);

            if (screenPos.x > min.x && screenPos.x < max.x &&
                screenPos.y > min.y && screenPos.y < max.y)
            {
                units.Add(unit);
                var renderer = unit.GetComponent<Renderer>();
                if (renderer != null) renderer.material.color = selectedColor;
                unit.GetComponent<Unit>().isSelected = true;
            }
        }
    }

    private void SelectSingleObject()
    {
        // Reset previous selection
        ResetSelection();

        // Raycast to find object under mouse
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject unit = hit.collider.gameObject;
            if (unit.CompareTag(selectableTag))
            {
                units.Add(unit);
                var renderer = unit.GetComponent<Renderer>();
                if (renderer != null) renderer.material.color = selectedColor;
                unit.GetComponent<Unit>().isSelected = true;
            }
        }
    }

    private void ResetSelection()
    {
        // Reset color of previously selected units
        foreach (var unit in units)
        {
            var renderer = unit.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = defaultColor;
            unit.GetComponent<Unit>().isSelected = false;
        }

        // Clear the list
        units.Clear();
    }
}