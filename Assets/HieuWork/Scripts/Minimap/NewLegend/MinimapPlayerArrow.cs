using UnityEngine;
using UnityEngine.UI;

public class MinimapPlayerArrow : MonoBehaviour
{
    [Header("Player Arrow Settings")]
    public Transform player;             // Player cần follow
    public Image arrowImage;             // Image UI gắn sẵn trong Canvas
    public float arrowHeight = 25f;      // Chiều cao trên player

    [Header("Arrow Appearance")]
    public Color arrowColor = Color.white;
    public Vector2 arrowSize = new Vector2(20f, 20f);
    public bool rotateWithPlayer = true;

    [Header("Arrow Sprite")]
    public Sprite arrowSprite; // Sprite PNG/JPG import vào Unity

    private RectTransform arrowRect;

    void Start()
    {
        if (arrowImage != null)
        {
            arrowRect = arrowImage.GetComponent<RectTransform>();
            arrowImage.color = arrowColor;
            arrowRect.sizeDelta = arrowSize;

            // Gắn sprite PNG vào arrow
            if (arrowSprite != null)
            {
                arrowImage.sprite = arrowSprite;
                arrowImage.preserveAspect = true; // Giữ tỉ lệ ảnh
            }
        }
    }

    void Update()
    {
        if (player == null || arrowRect == null) return;

        UpdateArrowPosition();
        UpdateArrowRotation();
    }

    void UpdateArrowPosition()
    {
        Vector3 arrowPosition = player.position;
        arrowPosition.y += arrowHeight;
        transform.position = arrowPosition;
    }

    void UpdateArrowRotation()
    {
        if (rotateWithPlayer)
        {
            float playerYaw = player.eulerAngles.y;
            // Thêm 180 độ để phù hợp camera top-down
            arrowRect.rotation = Quaternion.Euler(0, 0, -(playerYaw + 180f));
        }
    }

    public void SetArrowColor(Color newColor)
    {
        arrowColor = newColor;
        if (arrowImage != null)
            arrowImage.color = arrowColor;
    }

    public void SetArrowSize(Vector2 newSize)
    {
        arrowSize = newSize;
        if (arrowRect != null)
            arrowRect.sizeDelta = arrowSize;
    }
}
