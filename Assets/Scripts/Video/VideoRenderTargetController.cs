using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// Lab 6 - Video Render Target
/// Display video via RenderTexture → RawImage (UI) or Material (3D Cube)
/// Controls: Tab = Switch display mode, Space = Play/Pause, V = Play
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoRenderTargetController : MonoBehaviour
{
    public enum RenderMode
    {
        RawImage,       // UI RawImage
        MaterialObject  // 3D Cube with Material
    }
    
    [Header("Video Settings")]
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private bool autoPlay = true;
    [SerializeField] private bool loop = true;
    
    [Header("Render Texture")]
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private int videoWidth = 1920;
    [SerializeField] private int videoHeight = 1080;
    
    [Header("UI Display")]
    [SerializeField] private RawImage uiRawImage;
    
    [Header("3D Display")]
    [SerializeField] private GameObject videoCube;
    [SerializeField] private Renderer targetRenderer;
    
    [Header("Display Mode")]
    [SerializeField] private RenderMode currentMode = RenderMode.RawImage;
    
    [Header("UI Reference (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    private VideoPlayer videoPlayer;
    private Material runtimeMaterial;
    private bool isPrepared = false;
    
    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        // Create RenderTexture
        renderTexture = new RenderTexture(videoWidth, videoHeight, 0);
        renderTexture.name = "Lab6_VideoRenderTexture";
        renderTexture.Create();
        
        // Configure VideoPlayer
        videoPlayer.isLooping = loop;
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
        }
        
        // Audio
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.SetDirectAudioVolume(0, 1f);
        
        // Events
        videoPlayer.prepareCompleted += OnPrepared;
    }
    
    private void Start()
    {
        // Auto-find or create UI RawImage
        if (uiRawImage == null)
        {
            GameObject rawImageGO = GameObject.Find("VideoRawImage");
            if (rawImageGO != null)
            {
                uiRawImage = rawImageGO.GetComponent<RawImage>();
            }
            
            if (uiRawImage == null)
            {
                uiRawImage = FindAnyObjectByType<RawImage>();
            }
        }
        
        if (uiRawImage != null)
        {
            uiRawImage.texture = renderTexture;
            Debug.Log($"[Lab6] Found RawImage: {uiRawImage.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[Lab6] No RawImage found - creating one");
            CreateUIDisplay();
        }
        
        // Auto-find or create 3D Cube
        if (videoCube == null)
        {
            videoCube = GameObject.Find("Video3DCube");
        }
        
        if (videoCube == null)
        {
            Debug.Log("[Lab6] Creating 3D Video Cube...");
            Create3DCube();
        }
        else
        {
            targetRenderer = videoCube.GetComponent<Renderer>();
            Debug.Log("[Lab6] Found existing Video3DCube");
        }
        
        // Setup material for 3D renderer
        SetupMaterial();
        
        // Apply initial mode (start with RawImage visible, cube hidden)
        ApplyRenderMode();
        
        // Prepare video
        if (videoPlayer.clip != null)
        {
            videoPlayer.Prepare();
            Debug.Log("[Lab6] Preparing video...");
        }
        else
        {
            Debug.LogWarning("[Lab6] No video clip assigned!");
        }
        
        UpdateStatusUI();
    }
    
    private void CreateUIDisplay()
    {
        // Create canvas for UI video
        GameObject canvasObj = new GameObject("VideoUICanvas_Runtime");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create RawImage
        GameObject rawImageObj = new GameObject("VideoRawImage_Runtime");
        rawImageObj.transform.SetParent(canvasObj.transform);
        
        uiRawImage = rawImageObj.AddComponent<RawImage>();
        uiRawImage.texture = renderTexture;
        uiRawImage.color = Color.white;
        
        RectTransform rect = uiRawImage.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.15f);
        rect.anchorMax = new Vector2(0.9f, 0.85f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Debug.Log("[Lab6] Created UI RawImage display");
    }
    
    private void Create3DCube()
    {
        // Create 3D Cube for video display
        videoCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        videoCube.name = "Video3DCube_Runtime";
        
        // Position it in front of camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            videoCube.transform.position = mainCam.transform.position + mainCam.transform.forward * 5f;
        }
        else
        {
            videoCube.transform.position = new Vector3(0, 2, 5);
        }
        
        // Scale to TV-like aspect ratio (16:9)
        videoCube.transform.localScale = new Vector3(4f, 2.25f, 0.2f);
        
        // Get renderer
        targetRenderer = videoCube.GetComponent<Renderer>();
        
        // Start hidden (Mode 1 is RawImage by default)
        videoCube.SetActive(false);
        
        Debug.Log($"[Lab6] Created 3D Video Cube at {videoCube.transform.position}");
    }
    
    private void SetupMaterial()
    {
        if (targetRenderer != null)
        {
            // Try different shaders
            Shader unlitShader = Shader.Find("Unlit/Texture");
            
            if (unlitShader == null)
            {
                unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            
            if (unlitShader == null)
            {
                unlitShader = Shader.Find("Standard");
            }
            
            if (unlitShader != null)
            {
                runtimeMaterial = new Material(unlitShader);
                runtimeMaterial.mainTexture = renderTexture;
                
                // For Standard shader, set to emissive so it's visible
                if (unlitShader.name == "Standard")
                {
                    runtimeMaterial.SetColor("_EmissionColor", Color.white);
                    runtimeMaterial.EnableKeyword("_EMISSION");
                }
                
                targetRenderer.material = runtimeMaterial;
                Debug.Log($"[Lab6] Setup material with shader: {unlitShader.name}");
            }
            else
            {
                Debug.LogError("[Lab6] Could not find any suitable shader!");
            }
        }
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Tab to switch mode
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            ToggleRenderMode();
        }
        
        // V to play
        if (keyboard.vKey.wasPressedThisFrame)
        {
            if (isPrepared)
            {
                videoPlayer.Play();
                Debug.Log("[Lab6] Playing video");
            }
        }
        
        // Space to play/pause
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
                Debug.Log("[Lab6] Paused");
            }
            else if (isPrepared)
            {
                videoPlayer.Play();
                Debug.Log("[Lab6] Resumed");
            }
        }
        
        UpdateStatusUI();
    }
    
    public void ToggleRenderMode()
    {
        currentMode = currentMode == RenderMode.RawImage ? RenderMode.MaterialObject : RenderMode.RawImage;
        ApplyRenderMode();
        
        string modeName = currentMode == RenderMode.RawImage ? "UI RawImage" : "3D Cube";
        Debug.Log($"[Lab6] Switched to Mode: {modeName}");
    }
    
    public void SetRenderMode(RenderMode mode)
    {
        currentMode = mode;
        ApplyRenderMode();
    }
    
    private void ApplyRenderMode()
    {
        switch (currentMode)
        {
            case RenderMode.RawImage:
                // Enable UI, disable 3D
                if (uiRawImage != null)
                {
                    uiRawImage.gameObject.SetActive(true);
                    uiRawImage.texture = renderTexture;
                }
                if (videoCube != null)
                {
                    videoCube.SetActive(false);
                }
                Debug.Log("[Lab6] Mode 1: UI RawImage (2D overlay)");
                break;
                
            case RenderMode.MaterialObject:
                // Enable 3D, disable UI
                if (uiRawImage != null)
                {
                    uiRawImage.gameObject.SetActive(false);
                }
                if (videoCube != null)
                {
                    videoCube.SetActive(true);
                    
                    // Update material texture
                    if (runtimeMaterial != null)
                    {
                        runtimeMaterial.mainTexture = renderTexture;
                    }
                }
                Debug.Log("[Lab6] Mode 2: 3D Cube with video material");
                break;
        }
    }
    
    private void OnPrepared(VideoPlayer vp)
    {
        isPrepared = true;
        Debug.Log("[Lab6] Video prepared and ready!");
        
        if (autoPlay)
        {
            videoPlayer.Play();
            Debug.Log("[Lab6] Auto-playing video");
        }
    }
    
    private void UpdateStatusUI()
    {
        if (statusText == null) return;
        
        string modeName = currentMode == RenderMode.RawImage ? "Mode 1: UI RawImage" : "Mode 2: 3D Cube";
        string playStatus = videoPlayer.isPlaying ? "▶️ Playing" : (isPrepared ? "⏸️ Paused" : "⏳ Preparing");
        
        double currentTime = videoPlayer.time;
        double duration = videoPlayer.clip != null ? videoPlayer.clip.length : 0;
        string timeText = $"{FormatTime(currentTime)} / {FormatTime(duration)}";
        
        statusText.text = $"Render Target Demo\n" +
                          $"Current: {modeName}\n" +
                          $"Status: {playStatus}\n" +
                          $"Time: {timeText}\n\n" +
                          $"Controls:\n" +
                          $"Tab = Switch Mode\n" +
                          $"V = Play | Space = Pause";
    }
    
    private string FormatTime(double seconds)
    {
        int mins = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{mins:00}:{secs:00}";
    }
    
    private void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        
        // Cleanup runtime material
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
        
        // Cleanup runtime RenderTexture
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
