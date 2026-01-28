using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// Lab 6 - Video Render Target
/// Display video via RenderTexture → RawImage (UI) or Material (3D Object)
/// Controls: Tab = Switch display mode
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
    
    [Header("Render Targets")]
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private RawImage uiRawImage;
    [SerializeField] private Renderer targetRenderer;
    
    [Header("Display Mode")]
    [SerializeField] private RenderMode currentMode = RenderMode.RawImage;
    
    [Header("UI Reference (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    private VideoPlayer videoPlayer;
    private Material runtimeMaterial;
    
    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        // Configure VideoPlayer
        videoPlayer.isLooping = loop;
        videoPlayer.playOnAwake = false;
        
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
        }
        
        // Set render mode to RenderTexture
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        
        // Create RenderTexture if not assigned
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(1920, 1080, 0);
            renderTexture.name = "VideoRenderTexture";
        }
        
        videoPlayer.targetTexture = renderTexture;
        
        // Setup material for 3D renderer
        SetupMaterial();
        
        // Apply initial mode
        ApplyRenderMode();
    }
    
    private void Start()
    {
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPrepared;
        
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
        
        // Space to play/pause
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else
                videoPlayer.Play();
        }
        
        UpdateStatusUI();
    }
    
    private void SetupMaterial()
    {
        if (targetRenderer != null)
        {
            // Create runtime material to avoid modifying shared material
            runtimeMaterial = new Material(Shader.Find("Unlit/Texture"));
            runtimeMaterial.mainTexture = renderTexture;
            targetRenderer.material = runtimeMaterial;
        }
    }
    
    public void ToggleRenderMode()
    {
        currentMode = currentMode == RenderMode.RawImage ? RenderMode.MaterialObject : RenderMode.RawImage;
        ApplyRenderMode();
        Debug.Log($"[VideoRenderTarget] Switched to: {currentMode}");
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
                }
                break;
        }
    }
    
    private void OnPrepared(VideoPlayer vp)
    {
        Debug.Log("[VideoRenderTarget] Video prepared");
        
        if (autoPlay)
        {
            videoPlayer.Play();
        }
    }
    
    private void UpdateStatusUI()
    {
        if (statusText == null) return;
        
        string modeName = currentMode == RenderMode.RawImage ? "UI RawImage" : "3D Material";
        string playStatus = videoPlayer.isPlaying ? "▶️ Playing" : "⏸️ Paused";
        
        statusText.text = $"Render Mode: {modeName}\n" +
                          $"Status: {playStatus}\n" +
                          $"RenderTexture: {renderTexture.width}x{renderTexture.height}\n\n" +
                          $"Controls:\nTab = Switch Mode\nSpace = Play/Pause";
    }
    
    private void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        
        // Cleanup runtime material
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
