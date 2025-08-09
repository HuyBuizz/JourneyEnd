using Unity.Netcode;
using UnityEngine;

public class LocalCameraEnabler : NetworkBehaviour
{
    [SerializeField] Camera playerCamera;
    [SerializeField] AudioListener audioListener;

    void Awake()
    {
        if (!playerCamera) playerCamera = GetComponentInChildren<Camera>(true);
        if (!audioListener) audioListener = GetComponentInChildren<AudioListener>(true);

        // TẮT mặc định để tránh đụng nhau lúc spawn
        if (playerCamera) playerCamera.enabled = false;
        if (audioListener) audioListener.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        bool isMine = IsOwner; // hoặc IsLocalPlayer (nếu bạn dùng)
        if (playerCamera) playerCamera.enabled = isMine;
        if (audioListener) audioListener.enabled = isMine;
    }
}
