using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MarkerConfig
{
    [Header("Basic Settings")]
    public string tagName = "";

    [Header("Image Settings")]
    public Texture2D markerTexture; // Assign texture trực tiếp
    public string imageFileName = ""; // Tên file trong Resources/MarkerIcons/
    public string imageURL = ""; // URL để tải từ web (optional)
    public Color markerColor = Color.white;
    public Vector2 markerSize = new Vector2(20f, 20f);

    [Header("Position Settings")]
    public float heightOffset = 2f; // Chiều cao trên object
    public bool rotateWithObject = false;
    public bool showMarker = true;

    [Header("Legend Integration")]
    public string displayName = "Marker";
    public string description = "Object marker";
    public bool addToLegend = true;

    [Header("Visual Effects")]
    public bool enablePulse = false;
    public float pulseSpeed = 2f;
    public bool fadeWithDistance = false;
    public float maxViewDistance = 50f;
}

public class MarkerSpawner : MonoBehaviour
{
    [Header("Marker Configuration")]
    public MarkerConfig[] markerConfigs = new MarkerConfig[0];

    [Header("System Settings")]
    public int markerLayer = 5; // UI layer
    public bool autoSpawnOnStart = true;
    public bool enableDynamicSpawning = true;
    public float scanInterval = 1f; // Scan mỗi 1 giây

    [Header("Image Loading")]
    public bool enableURLLoading = false; // Cho phép load từ URL
    public string defaultImagePath = "MarkerIcons/"; // Path trong Resources

    [Header("Legend Integration")]
    public SimpleLegendController legendController;
    public bool autoFindLegendController = true;
    public bool syncWithLegend = true;

    [Header("Performance")]
    public int maxMarkersPerFrame = 10;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Private variables
    private Dictionary<string, MarkerConfig> configMap = new Dictionary<string, MarkerConfig>();
    private List<WorldMarker> activeMarkers = new List<WorldMarker>();
    private Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
    private float lastScanTime;

    void Start()
    {
        InitializeSystem();

        if (autoSpawnOnStart)
        {
            StartCoroutine(LoadTexturesAndSpawn());
        }

        LogDebug("MarkerSpawner initialized");
    }

    void InitializeSystem()
    {
        // Build config dictionary
        configMap.Clear();
        foreach (var config in markerConfigs)
        {
            if (!string.IsNullOrEmpty(config.tagName))
            {
                configMap[config.tagName] = config;
            }
        }

        // Auto-find legend controller
        if (legendController == null && autoFindLegendController)
        {
            legendController = Object.FindFirstObjectByType<SimpleLegendController>();
        }

        LogDebug($"Loaded {configMap.Count} marker configurations");
    }

    IEnumerator LoadTexturesAndSpawn()
    {
        // Load all textures first
        yield return StartCoroutine(LoadAllTextures());

        // Then spawn markers
        SpawnAllMarkers();
    }

    IEnumerator LoadAllTextures()
    {
        foreach (var config in markerConfigs)
        {
            if (config.markerTexture == null)
            {
                yield return StartCoroutine(LoadTextureForConfig(config));
            }
        }
    }

    IEnumerator LoadTextureForConfig(MarkerConfig config)
    {
        Texture2D loadedTexture = null;

        // Priority 1: Load from assigned texture
        if (config.markerTexture != null)
        {
            loadedTexture = config.markerTexture;
        }
        // Priority 2: Load from Resources using filename
        else if (!string.IsNullOrEmpty(config.imageFileName))
        {
            string resourcePath = defaultImagePath + config.imageFileName;
            loadedTexture = Resources.Load<Texture2D>(resourcePath);

            if (loadedTexture == null)
            {
                LogDebug($"Could not load texture from Resources: {resourcePath}");
            }
        }
        // Priority 3: Load from URL (if enabled)
        else if (enableURLLoading && !string.IsNullOrEmpty(config.imageURL))
        {
            yield return StartCoroutine(LoadTextureFromURL(config.imageURL, config));
        }

        // Store loaded texture
        if (loadedTexture != null)
        {
            loadedTextures[config.tagName] = loadedTexture;
            config.markerTexture = loadedTexture; // Update config
            LogDebug($"Loaded texture for {config.tagName}");
        }
        else
        {
            LogDebug($"No texture found for {config.tagName}");
        }
    }

    IEnumerator LoadTextureFromURL(string url, MarkerConfig config)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                loadedTextures[config.tagName] = texture;
                config.markerTexture = texture;
                LogDebug($"Downloaded texture from URL for {config.tagName}");
            }
            else
            {
                LogDebug($"Failed to download texture from URL: {www.error}");
            }
        }
    }

    void Update()
    {
        if (enableDynamicSpawning && Time.time - lastScanTime >= scanInterval)
        {
            ScanForNewObjects();
            lastScanTime = Time.time;
        }

        UpdateActiveMarkers();
    }

    void ScanForNewObjects()
    {
        int spawned = 0;

        foreach (var kvp in configMap)
        {
            string tag = kvp.Key;
            MarkerConfig config = kvp.Value;

            if (!config.showMarker) continue;

            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objects)
            {
                if (!HasMarker(obj) && spawned < maxMarkersPerFrame)
                {
                    if (CreateMarkerForObject(obj, config))
                    {
                        spawned++;
                    }
                }
            }
        }

        if (spawned > 0)
        {
            LogDebug($"Spawned {spawned} new markers");
            SyncWithLegendController();
        }
    }

    void UpdateActiveMarkers()
    {
        for (int i = activeMarkers.Count - 1; i >= 0; i--)
        {
            if (activeMarkers[i] == null || activeMarkers[i].targetObject == null)
            {
                activeMarkers.RemoveAt(i);
            }
        }
    }

    public void SpawnAllMarkers()
    {
        int spawned = 0;

        foreach (var kvp in configMap)
        {
            string tag = kvp.Key;
            MarkerConfig config = kvp.Value;

            if (!config.showMarker || config.markerTexture == null) continue;

            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objects)
            {
                if (CreateMarkerForObject(obj, config))
                {
                    spawned++;
                }
            }
        }

        LogDebug($"Spawned {spawned} markers total");
        SyncWithLegendController();
    }

    bool CreateMarkerForObject(GameObject targetObject, MarkerConfig config)
    {
        if (targetObject == null || config.markerTexture == null) return false;

        // Create world space canvas
        GameObject markerGO = new GameObject($"Marker_{targetObject.name}");
        markerGO.transform.SetParent(targetObject.transform);
        markerGO.layer = markerLayer;

        // Setup canvas
        Canvas canvas = markerGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        // Scale canvas appropriately
        markerGO.transform.localScale = Vector3.one * 0.01f; // Small world scale

        // Add CanvasScaler
        CanvasScaler scaler = markerGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        // Create marker image
        GameObject imageGO = new GameObject("MarkerImage");
        imageGO.transform.SetParent(markerGO.transform, false);
        imageGO.layer = markerLayer;

        Image markerImage = imageGO.AddComponent<Image>();
        RectTransform imageRect = imageGO.GetComponent<RectTransform>();

        // Configure marker image với Texture2D
        Sprite markerSprite = CreateSpriteFromTexture(config.markerTexture);
        markerImage.sprite = markerSprite;
        markerImage.color = config.markerColor;

        imageRect.sizeDelta = config.markerSize;

        // Position marker above object
        Vector3 markerPos = Vector3.up * config.heightOffset;
        markerGO.transform.localPosition = markerPos;

        // 👉 Fix xoay X = 90 độ
        markerGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Add WorldMarker component
        WorldMarker worldMarker = markerGO.AddComponent<WorldMarker>();
        worldMarker.Setup(targetObject, config, markerImage, imageRect);

        activeMarkers.Add(worldMarker);

        LogDebug($"Created marker for {targetObject.name} ({config.tagName})");
        return true;
    }

    Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        if (texture == null) return null;

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), // Pivot center
            100.0f // Pixels per unit
        );
    }

    bool HasMarker(GameObject obj)
    {
        return activeMarkers.Exists(marker => marker.targetObject == obj);
    }

    void SyncWithLegendController()
    {
        if (!syncWithLegend || legendController == null) return;

        // Add marker configs to legend
        foreach (var config in markerConfigs)
        {
            if (config.addToLegend && config.showMarker && HasObjectsWithTag(config.tagName))
            {
                // Check if already in legend
                bool alreadyInLegend = false;
                foreach (var entry in legendController.legendEntries)
                {
                    if (entry.itemName == config.displayName)
                    {
                        alreadyInLegend = true;
                        break;
                    }
                }

                if (!alreadyInLegend && config.markerTexture != null)
                {
                    // Convert texture to sprite for legend
                    Sprite legendSprite = CreateSpriteFromTexture(config.markerTexture);

                    legendController.AddLegendItem(
                        config.displayName,
                        config.description,
                        legendSprite,
                        config.markerColor
                    );
                }
            }
        }
    }

    bool HasObjectsWithTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return false;
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        return objects.Length > 0;
    }

    // Public methods
    public void AddMarkerConfig(string tag, string displayName, Texture2D texture, Color color, float height = 2f)
    {
        MarkerConfig newConfig = new MarkerConfig
        {
            tagName = tag,
            displayName = displayName,
            description = $"{displayName} markers",
            markerTexture = texture,
            markerColor = color,
            heightOffset = height,
            showMarker = true,
            addToLegend = true
        };

        // Add to array
        List<MarkerConfig> configList = new List<MarkerConfig>(markerConfigs);
        configList.Add(newConfig);
        markerConfigs = configList.ToArray();

        // Add to dictionary
        configMap[tag] = newConfig;

        // Spawn markers for existing objects
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            if (!HasMarker(obj))
            {
                CreateMarkerForObject(obj, newConfig);
            }
        }

        SyncWithLegendController();
        LogDebug($"Added marker config: {displayName}");
    }

    public void AddMarkerConfigFromFile(string tag, string displayName, string fileName, Color color, float height = 2f)
    {
        MarkerConfig newConfig = new MarkerConfig
        {
            tagName = tag,
            displayName = displayName,
            description = $"{displayName} markers",
            imageFileName = fileName,
            markerColor = color,
            heightOffset = height,
            showMarker = true,
            addToLegend = true
        };

        // Load texture
        StartCoroutine(LoadTextureForConfigAndSpawn(newConfig));
    }

    IEnumerator LoadTextureForConfigAndSpawn(MarkerConfig config)
    {
        yield return StartCoroutine(LoadTextureForConfig(config));

        // Add to arrays and spawn
        List<MarkerConfig> configList = new List<MarkerConfig>(markerConfigs);
        configList.Add(config);
        markerConfigs = configList.ToArray();
        configMap[config.tagName] = config;

        // Spawn for existing objects
        GameObject[] objects = GameObject.FindGameObjectsWithTag(config.tagName);
        foreach (GameObject obj in objects)
        {
            if (!HasMarker(obj))
            {
                CreateMarkerForObject(obj, config);
            }
        }

        SyncWithLegendController();
        LogDebug($"Added and loaded marker config: {config.displayName}");
    }

    public void RemoveMarkersWithTag(string tag)
    {
        // Remove active markers
        for (int i = activeMarkers.Count - 1; i >= 0; i--)
        {
            if (activeMarkers[i].targetObject != null && activeMarkers[i].targetObject.tag == tag)
            {
                DestroyImmediate(activeMarkers[i].gameObject);
                activeMarkers.RemoveAt(i);
            }
        }

        // Remove from config
        configMap.Remove(tag);
        loadedTextures.Remove(tag);
        List<MarkerConfig> configList = new List<MarkerConfig>(markerConfigs);
        configList.RemoveAll(c => c.tagName == tag);
        markerConfigs = configList.ToArray();

        LogDebug($"Removed markers with tag: {tag}");
    }

    public void ToggleMarkersWithTag(string tag, bool show)
    {
        if (configMap.ContainsKey(tag))
        {
            configMap[tag].showMarker = show;

            // Update array as well
            foreach (var config in markerConfigs)
            {
                if (config.tagName == tag)
                {
                    config.showMarker = show;
                    break;
                }
            }

            // Show/hide existing markers
            foreach (var marker in activeMarkers)
            {
                if (marker.targetObject != null && marker.targetObject.tag == tag)
                {
                    marker.gameObject.SetActive(show);
                }
            }

            LogDebug($"Tag '{tag}' markers visibility: {show}");
        }
    }

    public void ClearAllMarkers()
    {
        foreach (var marker in activeMarkers)
        {
            if (marker != null)
                DestroyImmediate(marker.gameObject);
        }
        activeMarkers.Clear();

        LogDebug("Cleared all markers");
    }

    public void RefreshAllMarkers()
    {
        ClearAllMarkers();
        StartCoroutine(LoadTexturesAndSpawn());
    }

    // Getters
    public int GetActiveMarkerCount()
    {
        return activeMarkers.Count;
    }

    public int GetActiveMarkerCountByTag(string tag)
    {
        return activeMarkers.FindAll(marker =>
            marker.targetObject != null && marker.targetObject.tag == tag).Count;
    }

    public List<string> GetActiveMarkerTags()
    {
        List<string> tags = new List<string>();
        foreach (var marker in activeMarkers)
        {
            if (marker.targetObject != null && !tags.Contains(marker.targetObject.tag))
            {
                tags.Add(marker.targetObject.tag);
            }
        }
        return tags;
    }

    void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[MarkerSpawner] {message}");
        }
    }

    void OnValidate()
    {
        scanInterval = Mathf.Max(0.1f, scanInterval);
        maxMarkersPerFrame = Mathf.Max(1, maxMarkersPerFrame);

        // Auto-find legend controller in editor
        if (legendController == null && autoFindLegendController)
        {
            legendController = Object.FindFirstObjectByType<SimpleLegendController>();
        }
    }
}

// WorldMarker component để quản lý individual markers - KHÔNG THAY ĐỔI
public class WorldMarker : MonoBehaviour
{
    [Header("References")]
    public GameObject targetObject;
    public Image markerImage;
    public RectTransform markerRect;

    [Header("Settings")]
    public bool rotateWithTarget = false;
    public float heightOffset = 2f;
    public bool enablePulse = false;
    public float pulseSpeed = 2f;
    public bool fadeWithDistance = false;
    public float maxDistance = 50f;

    private Camera playerCamera;
    private float baseAlpha;
    private Vector3 lastTargetPosition;

    void Start()
    {
        if (markerImage != null)
        {
            baseAlpha = markerImage.color.a;
        }

        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = Object.FindFirstObjectByType<Camera>();
        }
    }

    void Update()
    {
        UpdatePosition();

        if (rotateWithTarget)
        {
            UpdateRotation();
        }

        if (enablePulse)
        {
            UpdatePulse();
        }

        if (fadeWithDistance)
        {
            UpdateDistanceFade();
        }

        // Always face camera for world space UI
        // if (playerCamera != null)
        // {
        //     transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward,
        //                    playerCamera.transform.rotation * Vector3.up);
        // }
    }

    public void Setup(GameObject target, MarkerConfig config, Image image, RectTransform rect)
    {
        targetObject = target;
        markerImage = image;
        markerRect = rect;

        rotateWithTarget = config.rotateWithObject;
        heightOffset = config.heightOffset;
        enablePulse = config.enablePulse;
        pulseSpeed = config.pulseSpeed;
        fadeWithDistance = config.fadeWithDistance;
        maxDistance = config.maxViewDistance;

        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (targetObject == null) return;

        Vector3 targetPos = targetObject.transform.position;

        if (Vector3.Distance(lastTargetPosition, targetPos) > 0.01f)
        {
            Vector3 markerWorldPos = targetPos + Vector3.up * heightOffset;
            transform.position = markerWorldPos;
            lastTargetPosition = targetPos;
        }
    }

    void UpdateRotation()
    {
        if (targetObject != null && markerRect != null)
        {
            float yaw = targetObject.transform.eulerAngles.y;
            markerRect.rotation = Quaternion.Euler(0, 0, -yaw);
        }
    }

    void UpdatePulse()
    {
        if (markerImage != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.3f;
            Color color = markerImage.color;
            color.a = baseAlpha + pulse;
            markerImage.color = color;
        }
    }

    void UpdateDistanceFade()
    {
        if (markerImage != null && playerCamera != null && targetObject != null)
        {
            float distance = Vector3.Distance(playerCamera.transform.position, targetObject.transform.position);
            float fadeRatio = Mathf.Clamp01(1f - (distance / maxDistance));

            Color color = markerImage.color;
            color.a = baseAlpha * fadeRatio;
            markerImage.color = color;
        }
    }
}