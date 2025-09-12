using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SimpleLegendItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Auto-Find References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image backgroundImage;

    [Header("Hover Effects")]
    public Color normalColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color hoverColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

    void Start()
    {
        AutoFindComponents();
        SetupBackground();
    }

    void AutoFindComponents()
    {
        // Auto-find components nếu chưa assign
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();

        if (nameText == null)
            nameText = transform.Find("Name")?.GetComponent<TextMeshProUGUI>();

        if (descriptionText == null)
            descriptionText = transform.Find("Description")?.GetComponent<TextMeshProUGUI>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
    }

    void SetupBackground()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
}