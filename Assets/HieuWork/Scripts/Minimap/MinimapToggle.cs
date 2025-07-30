using StarterAssets;
using UnityEngine;

public class MinimapToggle : MonoBehaviour
{
    public GameObject minimapSmall;
    public GameObject minimapFull;
    public Camera minimapCamera;
    public FirstPersonController playerController;
    public float miniSize = 5f;
    public float fullSize = 10f;

    private bool isFull = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isFull = !isFull;

            minimapSmall.SetActive(!isFull);
            minimapFull.SetActive(isFull);

            // Đổi kích thước camera
            if (minimapCamera != null)
                minimapCamera.orthographicSize = isFull ? fullSize : miniSize;
            // Bật/tắt FirstPersonController khi chuyển map
            if (playerController != null)
                playerController.enabled = !isFull;

            // // (Tùy chọn) Không xoay camera khi ở fullscreen
            // var follow = minimapCamera.GetComponent<MinimapFollow>();
            // if (follow != null)
            //     follow.rotateWithPlayer = !isFull;
        }
    }
}
