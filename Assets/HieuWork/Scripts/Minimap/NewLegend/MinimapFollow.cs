using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Unity 6.2 Input System

public class MinimapFollow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Basic Settings")]
    public Transform player;
    public float height = 20f;

    [Header("Zoom Features")]
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float zoomSpeed = 5f;
    public bool rotateWithPlayer = true;

    [Header("UI References")]
    public GameObject smallMinimapContainer;  // Assign MinimapPanel
    public GameObject fullscreenPanel;        // Assign FullscreenMinimapPanel
    public RawImage fullscreenRawImage;      // Assign FullscreenRawImage
    public TextMeshProUGUI zoomText;
    public Button closeButton;               // Assign CloseButton

    [Header("PUBG Style Compass - Sliding")]
    public TextMeshProUGUI compassN, compassE, compassS, compassW;
    public float compassRange = 200f; // How far letters move left/right

    [Header("Controls Hint")]
    public GameObject controlsHintPanel; // Assign ControlsHint
    public bool showControlsHint = true;

    [Header("Legend Integration - MỚI")]
    public SimpleLegendController legendController; // Assign SimpleLegendController script
    public bool showLegendWithFullscreen = true; // Auto show legend with fullscreen

    [Header("Fullscreen Camera System")]
    public Camera fullscreenMinimapCamera; // Assign FullscreenMinimapCamera
    public RenderTexture fullscreenRenderTexture; // Assign FullscreenMinimapRT

    [Header("Player Control References")]
    public MonoBehaviour playerMouseLook; // Assign player's mouse look script
    public MonoBehaviour playerMovement;  // Assign player's movement script (NEW)
    public MonoBehaviour[] additionalScriptsToDisable; // Any other scripts to disable

    [Header("Fullscreen Zoom Settings")]
    public float fullscreenMinZoom = 5f;
    public float fullscreenMaxZoom = 100f;
    public float fullscreenZoomSpeed = 10f;

    [Header("Drag Settings")]
    public float dragSensitivity = 0.5f;
    public float maxDragDistance = 100f;

    [Header("Drag Zoom Settings")]
    public bool enableDragZoom = true;
    public float dragZoomSensitivity = 2f;
    public bool invertDragZoom = false; // true = drag up to zoom out

    [Header("Debug Info")]
    public bool showDebugLogs = true;

    private float currentZoom = 10f;
    private float currentFullscreenZoom = 20f;
    private bool isFullscreen = false;
    private Camera minimapCamera;

    // Drag variables
    private Vector3 dragOffset = Vector3.zero;
    private bool isDragging = false;
    private bool isDragZooming = false;
    private Vector2 dragStartPosition;
    private float dragStartZoom;

    // Store original states when entering fullscreen
    private bool originalMouseLookEnabled;
    private bool originalMovementEnabled;
    private bool[] originalAdditionalScriptsEnabled;
    private CursorLockMode originalCursorLockState;
    private bool originalCursorVisible;

    void Start()
    {
        // Get camera component - simplified
        minimapCamera = GetComponent<Camera>();
        if (minimapCamera != null)
        {
            currentZoom = minimapCamera.orthographicSize;
        }

        // Initialize fullscreen camera zoom
        if (fullscreenMinimapCamera != null)
        {
            currentFullscreenZoom = fullscreenMinimapCamera.orthographicSize;
        }

        SetupUI();
        UpdateZoomDisplay();
        UpdateControlsHintVisibility();
        StoreOriginalStates();

        if (showDebugLogs)
            Debug.Log("Enhanced minimap with legend integration ready! (Unity 6.2)");
    }

    void StoreOriginalStates()
    {
        // Store original cursor state
        originalCursorLockState = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        // Store original script states
        if (playerMouseLook != null)
            originalMouseLookEnabled = playerMouseLook.enabled;

        if (playerMovement != null)
            originalMovementEnabled = playerMovement.enabled;

        // Store additional scripts states
        if (additionalScriptsToDisable != null && additionalScriptsToDisable.Length > 0)
        {
            originalAdditionalScriptsEnabled = new bool[additionalScriptsToDisable.Length];
            for (int i = 0; i < additionalScriptsToDisable.Length; i++)
            {
                if (additionalScriptsToDisable[i] != null)
                    originalAdditionalScriptsEnabled[i] = additionalScriptsToDisable[i].enabled;
            }
        }
    }

    void SetupUI()
    {
        // Ensure fullscreen is hidden initially
        if (fullscreenPanel)
        {
            fullscreenPanel.SetActive(false);
            isFullscreen = false;
        }

        // Ensure legend is hidden initially
        if (legendController != null)
        {
            legendController.HideLegend();
        }

        // Setup close button to use toggle function
        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            closeButton.onClick.AddListener(() =>
            {
                if (isFullscreen) ToggleFullscreen();
            });
        }

        // Setup fullscreen render texture
        if (fullscreenRawImage && fullscreenRenderTexture)
        {
            fullscreenRawImage.texture = fullscreenRenderTexture;
        }
        // Fallback to regular minimap texture
        else if (fullscreenRawImage && minimapCamera && minimapCamera.targetTexture)
        {
            fullscreenRawImage.texture = minimapCamera.targetTexture;
        }
    }

    void Update()
    {
        UpdatePosition();
        HandleInput();
        UpdatePUBGCompass();
        UpdateFullscreenCamera();
        HandleFullscreenScrollZoom();
    }

    void UpdatePosition()
    {
        if (player == null) return;

        // Update camera position to follow player
        Vector3 newPos = player.position;
        newPos.y += height;
        transform.position = newPos;

        // Update camera rotation based on setting
        if (rotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    void UpdateFullscreenCamera()
    {
        if (fullscreenMinimapCamera == null) return;

        // Enable/disable fullscreen camera based on mode
        fullscreenMinimapCamera.gameObject.SetActive(isFullscreen);

        if (!isFullscreen) return;

        // Only update position if not being dragged
        if (!isDragging)
        {
            Vector3 playerPos = player.position;
            Vector3 fullscreenPos = new Vector3(playerPos.x, playerPos.y + height, playerPos.z);
            fullscreenMinimapCamera.transform.position = fullscreenPos;

            // Apply rotation
            if (rotateWithPlayer)
            {
                fullscreenMinimapCamera.transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
            }
            else
            {
                fullscreenMinimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }

    void HandleInput()
    {
        // Trong fullscreen mode, chỉ cho phép minimap controls
        if (isFullscreen)
        {
            HandleFullscreenInput();
            return;
        }

        // Normal mode controls
        HandleNormalModeInput();
    }

    void HandleNormalModeInput()
    {
        // Zoom controls
        if (Input.GetKey(KeyCode.I))
        {
            ZoomIn();
        }
        if (Input.GetKey(KeyCode.O))
        {
            ZoomOut();
        }

        // Toggle rotation with R key only
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotateWithPlayer = !rotateWithPlayer;
            if (showDebugLogs)
                Debug.Log("Rotate with player: " + rotateWithPlayer);
        }

        // M key: Toggle fullscreen + legend (both open and close)
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleFullscreen();
        }

        // N key: Toggle controls hint
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleControlsHint();
        }
    }

    void HandleFullscreenInput()
    {
        // ESC key: Close fullscreen + legend
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleFullscreen();
        }

        // M key: Close fullscreen + legend
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleFullscreen();
        }

        // R key: Toggle rotation (still works in fullscreen)
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotateWithPlayer = !rotateWithPlayer;
            if (showDebugLogs)
                Debug.Log("Rotate with player: " + rotateWithPlayer);
        }

        // Reset camera position with SPACE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetFullscreenCamera();
        }

        // Alternative zoom with +/- keys
        if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Equals))
        {
            FullscreenKeyboardZoomIn();
        }
        if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus))
        {
            FullscreenKeyboardZoomOut();
        }
    }

    void HandleFullscreenScrollZoom()
    {
        if (!isFullscreen || fullscreenMinimapCamera == null) return;

        // Mouse scroll wheel zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            FullscreenScrollZoom(scrollInput);
        }
    }

    void FullscreenScrollZoom(float scrollDelta)
    {
        float zoomChange = scrollDelta * fullscreenZoomSpeed;
        currentFullscreenZoom = Mathf.Clamp(currentFullscreenZoom - zoomChange, fullscreenMinZoom, fullscreenMaxZoom);

        if (fullscreenMinimapCamera != null)
        {
            fullscreenMinimapCamera.orthographicSize = currentFullscreenZoom;
        }
        UpdateZoomDisplay();

        if (showDebugLogs)
            Debug.Log($"Fullscreen zoom: {currentFullscreenZoom:F1}");
    }

    // ENHANCED DRAG HANDLERS - Support both camera movement and zoom
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isFullscreen || fullscreenMinimapCamera == null || player == null) return;

        dragStartPosition = eventData.position;

        // Determine drag mode based on modifier keys
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Camera drag mode (Ctrl + Drag)
            isDragging = true;
            isDragZooming = false;
            if (showDebugLogs)
                Debug.Log("Camera drag started (Ctrl + Drag)");
        }
        else if (enableDragZoom)
        {
            // Zoom drag mode (Normal Drag)
            isDragZooming = true;
            isDragging = false;
            dragStartZoom = currentFullscreenZoom;
            if (showDebugLogs)
                Debug.Log("Zoom drag started (Normal Drag)");
        }
        else
        {
            // Fallback to camera drag if zoom disabled
            isDragging = true;
            isDragZooming = false;
            if (showDebugLogs)
                Debug.Log("Camera drag started (Fallback)");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isFullscreen || fullscreenMinimapCamera == null || player == null) return;

        if (isDragZooming && enableDragZoom)
        {
            HandleDragZoom(eventData);
        }
        else if (isDragging)
        {
            HandleCameraDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragZooming)
        {
            isDragZooming = false;
            if (showDebugLogs)
                Debug.Log("Zoom drag ended");
        }

        if (isDragging)
        {
            isDragging = false;
            if (showDebugLogs)
                Debug.Log("Camera drag ended");
        }
    }

    void HandleDragZoom(PointerEventData eventData)
    {
        // Calculate vertical drag distance
        float verticalDrag = eventData.position.y - dragStartPosition.y;

        // Convert to zoom delta
        float zoomDelta = verticalDrag * dragZoomSensitivity * 0.01f;

        // Apply invert setting
        if (invertDragZoom)
        {
            zoomDelta = -zoomDelta;
        }

        // Calculate new zoom (drag up = zoom in = smaller orthographic size)
        float newZoom = Mathf.Clamp(dragStartZoom - zoomDelta, fullscreenMinZoom, fullscreenMaxZoom);

        // Apply zoom
        currentFullscreenZoom = newZoom;
        fullscreenMinimapCamera.orthographicSize = currentFullscreenZoom;
        UpdateZoomDisplay();
    }

    void HandleCameraDrag(PointerEventData eventData)
    {
        // Convert screen drag to world movement
        Vector2 dragDelta = eventData.delta * dragSensitivity;

        // Convert to world space (camera looks down, so X=X, Y=Z)
        Vector3 worldDrag = new Vector3(-dragDelta.x, 0, -dragDelta.y) * 0.05f;

        // Apply drag offset
        dragOffset += worldDrag;

        // Clamp to max distance from player
        Vector3 flatOffset = new Vector3(dragOffset.x, 0, dragOffset.z);
        if (flatOffset.magnitude > maxDragDistance)
        {
            flatOffset = flatOffset.normalized * maxDragDistance;
            dragOffset = new Vector3(flatOffset.x, dragOffset.y, flatOffset.z);
        }

        // Update camera position
        UpdateDragCameraPosition();
    }

    void UpdateDragCameraPosition()
    {
        if (fullscreenMinimapCamera && player)
        {
            Vector3 playerPos = player.position;
            Vector3 newCameraPos = playerPos + dragOffset;
            newCameraPos.y = playerPos.y + height;

            fullscreenMinimapCamera.transform.position = newCameraPos;
        }
    }

    // PUBG style sliding compass (giữ nguyên từ code cũ)
    void UpdatePUBGCompass()
    {
        if (player == null)
        {
            return;
        }

        // Get player rotation (0-360)
        float playerYaw = player.eulerAngles.y;

        // Calculate sliding positions for each direction
        UpdateCompassDirection(compassN, 0f, playerYaw);     // North at 0°
        UpdateCompassDirection(compassE, 90f, playerYaw);    // East at 90°
        UpdateCompassDirection(compassS, 180f, playerYaw);   // South at 180° 
        UpdateCompassDirection(compassW, 270f, playerYaw);   // West at 270°

        // Highlight current direction
        HighlightCurrentDirection(playerYaw);
    }

    void UpdateCompassDirection(TextMeshProUGUI compassText, float directionAngle, float playerYaw)
    {
        if (compassText == null) return;

        // Calculate angle difference between player and this direction
        float angleDiff = Mathf.DeltaAngle(playerYaw, directionAngle);

        // Convert angle to position on compass bar
        float normalizedPosition = angleDiff / 180f; // Range: -1 to 1

        // Convert to pixel position
        float xPosition = normalizedPosition * compassRange;

        // Apply position
        Vector2 currentPos = compassText.rectTransform.anchoredPosition;
        compassText.rectTransform.anchoredPosition = new Vector2(xPosition, currentPos.y);

        // Show/hide based on range
        bool shouldShow = Mathf.Abs(xPosition) <= compassRange;
        compassText.gameObject.SetActive(shouldShow);
    }

    void HighlightCurrentDirection(float rotation)
    {
        // Reset all colors to white first
        if (compassN) compassN.color = Color.white;
        if (compassE) compassE.color = Color.white;
        if (compassS) compassS.color = Color.white;
        if (compassW) compassW.color = Color.white;

        // Normalize rotation to 0-360
        float normalizedRotation = ((rotation % 360) + 360) % 360;

        // Determine which direction to highlight (like PUBG)
        if (normalizedRotation >= 315f || normalizedRotation < 45f)
        {
            if (compassN) compassN.color = Color.yellow;
        }
        else if (normalizedRotation >= 45f && normalizedRotation < 135f)
        {
            if (compassE) compassE.color = Color.yellow;
        }
        else if (normalizedRotation >= 135f && normalizedRotation < 225f)
        {
            if (compassS) compassS.color = Color.yellow;
        }
        else if (normalizedRotation >= 225f && normalizedRotation < 315f)
        {
            if (compassW) compassW.color = Color.yellow;
        }
    }

    void ToggleControlsHint()
    {
        showControlsHint = !showControlsHint;
        UpdateControlsHintVisibility();

        if (showDebugLogs)
            Debug.Log("Controls hint: " + (showControlsHint ? "SHOWN" : "HIDDEN"));
    }

    void UpdateControlsHintVisibility()
    {
        if (controlsHintPanel)
        {
            controlsHintPanel.SetActive(showControlsHint);
        }
    }

    void ToggleFullscreen()
    {
        // Flip the fullscreen state
        isFullscreen = !isFullscreen;

        if (isFullscreen)
        {
            // Entering fullscreen mode
            EnterFullscreenMode();
        }
        else
        {
            // Exiting fullscreen mode
            ExitFullscreenMode();
        }

        // Update UI visibility (bao gồm cả legend)
        UpdateUIVisibility();

        // Debug feedback
        if (showDebugLogs)
            Debug.Log($"Minimap fullscreen: {(isFullscreen ? "OPENED" : "CLOSED")} | Legend: {(isFullscreen && showLegendWithFullscreen ? "SHOWN" : "HIDDEN")}");
    }

    void EnterFullscreenMode()
    {
        // Reset drag and camera
        ResetFullscreenCamera();

        // Disable all player controls
        DisablePlayerControls();

        // Set cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (showDebugLogs)
            Debug.Log("Player controls DISABLED - Fullscreen mode active");
    }

    void ExitFullscreenMode()
    {
        // Re-enable all player controls
        EnablePlayerControls();

        // Restore original cursor state
        Cursor.lockState = originalCursorLockState;
        Cursor.visible = originalCursorVisible;

        if (showDebugLogs)
            Debug.Log("Player controls ENABLED - Normal mode active");
    }

    void DisablePlayerControls()
    {
        // Disable mouse look
        if (playerMouseLook != null)
        {
            playerMouseLook.enabled = false;
        }

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Disable additional scripts
        if (additionalScriptsToDisable != null)
        {
            for (int i = 0; i < additionalScriptsToDisable.Length; i++)
            {
                if (additionalScriptsToDisable[i] != null)
                {
                    additionalScriptsToDisable[i].enabled = false;
                }
            }
        }
    }

    void EnablePlayerControls()
    {
        // Enable mouse look
        if (playerMouseLook != null)
        {
            playerMouseLook.enabled = originalMouseLookEnabled;
        }

        // Enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = originalMovementEnabled;
        }

        // Enable additional scripts
        if (additionalScriptsToDisable != null && originalAdditionalScriptsEnabled != null)
        {
            for (int i = 0; i < additionalScriptsToDisable.Length; i++)
            {
                if (additionalScriptsToDisable[i] != null && i < originalAdditionalScriptsEnabled.Length)
                {
                    additionalScriptsToDisable[i].enabled = originalAdditionalScriptsEnabled[i];
                }
            }
        }
    }

    void UpdateUIVisibility()
    {
        // Toggle small minimap (hide when fullscreen is active)
        if (smallMinimapContainer)
        {
            smallMinimapContainer.SetActive(!isFullscreen);
        }

        // Toggle fullscreen panel (show when fullscreen is active)
        if (fullscreenPanel)
        {
            fullscreenPanel.SetActive(isFullscreen);
        }

        // *** LEGEND INTEGRATION - QUAN TRỌNG ***
        // Toggle legend panel cùng với fullscreen
        if (legendController != null && showLegendWithFullscreen)
        {
            if (isFullscreen)
            {
                legendController.ShowLegend();
            }
            else
            {
                legendController.HideLegend();
            }
        }
    }

    // Các methods zoom giữ nguyên từ code cũ
    void ZoomIn()
    {
        float newZoom = Mathf.Clamp(currentZoom - zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        if (Mathf.Abs(newZoom - currentZoom) > 0.01f)
        {
            currentZoom = newZoom;
            UpdateZoom();
        }
    }

    void ZoomOut()
    {
        float newZoom = Mathf.Clamp(currentZoom + zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        if (Mathf.Abs(newZoom - currentZoom) > 0.01f)
        {
            currentZoom = newZoom;
            UpdateZoom();
        }
    }

    void UpdateZoom()
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = currentZoom;
        }
        UpdateZoomDisplay();
    }

    void UpdateZoomDisplay()
    {
        if (zoomText != null)
        {
            if (isFullscreen)
            {
                zoomText.text = $"Zoom: {currentFullscreenZoom:F1}";
            }
            else
            {
                zoomText.text = $"Zoom: {currentZoom:F1}";
            }
        }
    }

    // PUBLIC METHODS FOR UI BUTTONS
    public void SetFullscreen(bool fullscreen)
    {
        if (isFullscreen != fullscreen)
        {
            ToggleFullscreen();
        }
    }

    public void ResetFullscreenCamera()
    {
        isDragging = false;
        isDragZooming = false;
        dragOffset = Vector3.zero;

        if (fullscreenMinimapCamera && player)
        {
            Vector3 playerPos = player.position;
            Vector3 resetPos = new Vector3(playerPos.x, playerPos.y + height, playerPos.z);
            fullscreenMinimapCamera.transform.position = resetPos;

            // Reset zoom to default value
            currentFullscreenZoom = 20f; // Default zoom level
            fullscreenMinimapCamera.orthographicSize = currentFullscreenZoom;
            UpdateZoomDisplay();
        }

        if (showDebugLogs)
            Debug.Log("Fullscreen camera and zoom reset to player");
    }

    public void FullscreenZoomIn()
    {
        if (fullscreenMinimapCamera)
        {
            currentFullscreenZoom = Mathf.Clamp(currentFullscreenZoom - 5f, fullscreenMinZoom, fullscreenMaxZoom);
            fullscreenMinimapCamera.orthographicSize = currentFullscreenZoom;
            UpdateZoomDisplay();
        }
    }

    public void FullscreenZoomOut()
    {
        if (fullscreenMinimapCamera)
        {
            currentFullscreenZoom = Mathf.Clamp(currentFullscreenZoom + 5f, fullscreenMinZoom, fullscreenMaxZoom);
            fullscreenMinimapCamera.orthographicSize = currentFullscreenZoom;
            UpdateZoomDisplay();
        }
    }

    void FullscreenKeyboardZoomIn()
    {
        if (fullscreenMinimapCamera)
        {
            currentFullscreenZoom = Mathf.Clamp(currentFullscreenZoom - fullscreenZoomSpeed * Time.deltaTime, fullscreenMinZoom, fullscreenMaxZoom);
            fullscreenMinimapCamera.orthographicSize = currentFullscreenZoom;
            UpdateZoomDisplay();
        }
    }

    void FullscreenKeyboardZoomOut()
    {
        if (fullscreenMinimapCamera)
        {
            currentFullscreenZoom = Mathf.Clamp(currentFullscreenZoom + fullscreenZoomSpeed * Time.deltaTime, fullscreenMinZoom, fullscreenMaxZoom);
            fullscreenMinimapCamera.orthographicSize = currentFullscreenZoom;
            UpdateZoomDisplay();
        }
    }

    // Unity 6.2 Enhanced Features
    // void OnApplicationFocus(bool hasFocus)
    // {
    //     // Tự động thoát fullscreen nếu alt-tab
    //     if (!hasFocus && isFullscreen)
    //     {
    //         if (showDebugLogs)
    //             Debug.Log("Application lost focus - exiting fullscreen");
    //         ToggleFullscreen();
    //     }
    // }

    void OnValidate()
    {
        minZoom = Mathf.Max(1f, minZoom);
        maxZoom = Mathf.Max(minZoom + 1f, maxZoom);
        zoomSpeed = Mathf.Max(0.1f, zoomSpeed);
        height = Mathf.Max(1f, height);

        // Validate fullscreen zoom settings
        fullscreenMinZoom = Mathf.Max(1f, fullscreenMinZoom);
        fullscreenMaxZoom = Mathf.Max(fullscreenMinZoom + 1f, fullscreenMaxZoom);
        fullscreenZoomSpeed = Mathf.Max(0.1f, fullscreenZoomSpeed);

        // Validate drag zoom settings
        dragZoomSensitivity = Mathf.Max(0.1f, dragZoomSensitivity);
    }

    // Cleanup on destroy
    void OnDestroy()
    {
        // Ensure player controls are restored if object is destroyed while in fullscreen
        if (isFullscreen)
        {
            EnablePlayerControls();
            Cursor.lockState = originalCursorLockState;
            Cursor.visible = originalCursorVisible;
        }
    }
}