using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// Lab 6 - Video Render Target
/// Display video via RenderTexture → RawImage (UI) or Material (3D Object)
/// Controls: Tab = Switch display mode, Space = Play/Pause, V = Play
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoRenderTargetController : MonoBehaviour
{
    public enum RenderMode
    {
        RawImage,       // UI RawImage
        MaterialObject  // 3D Object with Material
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
        // Auto-find UI RawImage if not assigned
        if (uiRawImage == null)
        {
            // Find by name first
            GameObject rawImageGO = GameObject.Find("VideoRawImage");
            if (rawImageGO != null)
            {
                uiRawImage = rawImageGO.GetComponent<RawImage>();
            }
            
            // Fallback to any RawImage
            if (uiRawImage == null)
            {
                uiRawImage = FindAnyObjectByType<RawImage>();
            }
        }
        
        // Setup UI display
        if (uiRawImage != null)
        {
            uiRawImage.texture = renderTexture;
            Debug.Log($"[Lab6] Found RawImage: {uiRawImage.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[Lab6] No RawImage found for video display!");
        }
        
        // Auto-find 3D Renderer if not assigned
        if (targetRenderer == null)
        {
            GameObject cube = GameObject.Find("Video3DCube");
            if (cube != null)
            {
                targetRenderer = cube.GetComponent<Renderer>();
            }
        }
        
        // Setup material for 3D renderer
        SetupMaterial();
        
        // Apply initial mode
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
    
    private void SetupMaterial()
    {
        if (targetRenderer != null)
        {
            // Create runtime material with Unlit/Texture shader
            Shader unlitShader = Shader.Find("Unlit/Texture");
            if (unlitShader == null)
            {
                unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            
            if (unlitShader != null)
            {
                runtimeMaterial = new Material(unlitShader);
                runtimeMaterial.mainTexture = renderTexture;
                targetRenderer.material = runtimeMaterial;
                Debug.Log("[Lab6] Setup 3D material for cube");
            }
        }
    }
    
    public void ToggleRenderMode()
    {
        currentMode = currentMode == RenderMode.RawImage ? RenderMode.MaterialObject : RenderMode.RawImage;
        ApplyRenderMode();
        Debug.Log($"[Lab6] Switched to: {currentMode}");
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
                if (targetRenderer != null)
                {
                    targetRenderer.gameObject.SetActive(false);
                }
                break;
                
            case RenderMode.MaterialObject:
                // Enable 3D, disable UI
                if (uiRawImage != null)
                {
                    uiRawImage.gameObject.SetActive(false);
                }
                if (targetRenderer != null)
                {
                    targetRenderer.gameObject.SetActive(true);
                    if (runtimeMaterial != null)
                    {
                        runtimeMaterial.mainTexture = renderTexture;
                    }
                }
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
        
        string modeName = currentMode == RenderMode.RawImage ? "UI RawImage" : "3D Material";
        string playStatus = videoPlayer.isPlaying ? "▶️ Playing" : (isPrepared ? "⏸️ Paused" : "⏳ Preparing");
        string resolution = $"{videoWidth}x{videoHeight}";
        
        double currentTime = videoPlayer.time;
        double duration = videoPlayer.clip != null ? videoPlayer.clip.length : 0;
        string timeText = $"{FormatTime(currentTime)} / {FormatTime(duration)}";
        
        statusText.text = $"Render Mode: {modeName}\n" +
                          $"Status: {playStatus}\n" +
                          $"Resolution: {resolution}\n" +
                          $"Time: {timeText}\n\n" +
                          $"Controls:\nTab = Switch Mode\nV = Play\nSpace = Pause";
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
