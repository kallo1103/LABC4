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
    [SerializeField] private MeshFilter targetMeshFilter;
    
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
        
        // Create RenderTexture if not assigned
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(videoWidth, videoHeight, 0);
            renderTexture.name = "Lab6_VideoRenderTexture";
            renderTexture.Create();
        }
        
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
        
        // Setup UI display
        if (uiRawImage != null)
        {
            uiRawImage.texture = renderTexture;
        }
        
        // Setup material for 3D renderer
        SetupMaterial();
        
        // Events
        videoPlayer.prepareCompleted += OnPrepared;
    }
    
    private void Start()
    {
        // Apply initial mode
        ApplyRenderMode();
        
        // Prepare video
        videoPlayer.Prepare();
        
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
            }
        }
        
        // Space to play/pause
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else if (isPrepared)
                videoPlayer.Play();
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
            }
            else
            {
                Debug.LogWarning("[VideoRenderTarget] Could not find Unlit shader");
            }
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
        if (renderTexture != null && renderTexture.name == "Lab6_VideoRenderTexture")
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
