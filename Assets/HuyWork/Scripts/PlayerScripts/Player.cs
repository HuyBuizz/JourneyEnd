using StarterAssets;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Vector3 playerLookDirection;
    [SerializeField] GameObject playerCameraRoot;
    [SerializeField] GameObject crosshairObject;
    /// <summary>
    /// Player stats
    /// </summary>
    public float health = 100f;
    public float stamina = 100f;
    public float reachRange = 5f;

    void Start()
    {
        playerCameraRoot = this.gameObject.GetComponent<FirstPersonController>().CinemachineCameraTarget;
        playerLookDirection = playerCameraRoot.transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        PlaceCrosshair();
    }

    void PlaceCrosshair()
    {
        if (crosshairObject != null && playerCameraRoot != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCameraRoot.transform.position, playerCameraRoot.transform.forward, out hit, reachRange, LayerMask.GetMask("Interactable")))
            {
                crosshairObject.transform.position = hit.point;
            }
            else
            {
                crosshairObject.transform.position = playerCameraRoot.transform.position + playerCameraRoot.transform.forward * reachRange;
            }
        }
    }
}
