using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    private CharacterController controller;
    private float verticalRotation = 0f;
    private Camera _camera;
    private CinemachineBrain _cinemachineBrain;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();
        _camera = GetComponentInChildren<Camera>();
        _cinemachineBrain = GetComponentInChildren<CinemachineBrain>();

        if (!IsOwner)
        {
            if (_camera != null) _camera.enabled = false;
            if (_cinemachineBrain != null) _cinemachineBrain.enabled = false;
            return;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // Lấy input WASD
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Xoay theo chuột
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
