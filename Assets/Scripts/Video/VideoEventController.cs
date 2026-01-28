using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Lab 7 - Video Events & Control
/// Demonstrates prepareCompleted and loopPointReached events
/// Displays video via RenderTexture + RawImage
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoEventController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private bool autoPlay = true;
    [SerializeField] private bool loop = false;
    
    [Header("Render Settings")]
    [SerializeField] private RawImage displayRawImage;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private int videoWidth = 1920;
    [SerializeField] private int videoHeight = 1080;
    
    [Header("On Video End Action")]
    [SerializeField] private VideoEndAction endAction = VideoEndAction.ShowUI;
    [SerializeField] private string sceneToLoad = "Gameplay";
    [SerializeField] private GameObject endUIPanel;
    
    [Header("Events")]
    public UnityEvent OnVideoPrepared;
    public UnityEvent OnVideoStarted;
    public UnityEvent OnVideoEnded;
    
    [Header("UI Reference (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private TMPro.TextMeshProUGUI eventLogText;
    
    private VideoPlayer videoPlayer;
    private System.Text.StringBuilder eventLog = new System.Text.StringBuilder();
    private bool isPrepared = false;
    
    public enum VideoEndAction
    {
        Nothing,
        ShowUI,
        LoadScene,
        RestartVideo
    }
    
    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        // Create RenderTexture if not assigned
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(videoWidth, videoHeight, 0);
            renderTexture.name = "Lab7_VideoRenderTexture";
            renderTexture.Create();
        }
        
        // Configure VideoPlayer
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = loop;
        videoPlayer.playOnAwake = false;
        
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
        }
        
        // Audio
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.SetDirectAudioVolume(0, 1f);
        
        // Assign RenderTexture to RawImage
        if (displayRawImage != null)
        {
            displayRawImage.texture = renderTexture;
        }
        
        // Hide end UI initially
        if (endUIPanel != null)
        {
            endUIPanel.SetActive(false);
        }
        
        // Subscribe to events
        videoPlayer.prepareCompleted += HandlePrepareCompleted;
        videoPlayer.started += HandleVideoStarted;
        videoPlayer.loopPointReached += HandleLoopPointReached;
    }
    
    private void Start()
    {
        // Auto-find RawImage if not assigned
        if (displayRawImage == null)
        {
            displayRawImage = FindAnyObjectByType<RawImage>();
            if (displayRawImage != null)
            {
                displayRawImage.texture = renderTexture;
            }
        }
        
        LogEvent("Initializing VideoPlayer...");
        videoPlayer.Prepare();
        UpdateStatusUI();
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // V to play
        if (keyboard.vKey.wasPressedThisFrame)
        {
            if (isPrepared)
            {
                videoPlayer.Play();
            }
        }
        
        // Space to pause/resume
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else if (isPrepared)
                videoPlayer.Play();
        }
        
        // R to restart
        if (keyboard.rKey.wasPressedThisFrame)
        {
            RestartVideo();
        }
        
        // C to clear log
        if (keyboard.cKey.wasPressedThisFrame)
        {
            ClearEventLog();
        }
        
        UpdateStatusUI();
    }
    
    private void HandlePrepareCompleted(VideoPlayer vp)
    {
        isPrepared = true;
        LogEvent("✓ EVENT: prepareCompleted");
        Debug.Log("[VideoEvent] prepareCompleted - Video is ready to play");
        
        OnVideoPrepared?.Invoke();
        
        if (autoPlay)
        {
            videoPlayer.Play();
        }
    }
    
    private void HandleVideoStarted(VideoPlayer vp)
    {
        LogEvent("▶ EVENT: started");
        Debug.Log("[VideoEvent] started - Video playback started");
        
        OnVideoStarted?.Invoke();
    }
    
    private void HandleLoopPointReached(VideoPlayer vp)
    {
        LogEvent("⏹ EVENT: loopPointReached (Video Ended)");
        Debug.Log("[VideoEvent] loopPointReached - Video finished playing");
        
        OnVideoEnded?.Invoke();
        
        // Execute end action
        ExecuteEndAction();
    }
    
    private void ExecuteEndAction()
    {
        switch (endAction)
        {
            case VideoEndAction.Nothing:
                LogEvent("End Action: None");
                break;
                
            case VideoEndAction.ShowUI:
                LogEvent("End Action: Show UI Panel");
                if (endUIPanel != null)
                {
                    endUIPanel.SetActive(true);
                }
                break;
                
            case VideoEndAction.LoadScene:
                LogEvent($"End Action: Loading scene '{sceneToLoad}'");
                if (!string.IsNullOrEmpty(sceneToLoad))
                {
                    SceneManager.LoadScene(sceneToLoad);
                }
                break;
                
            case VideoEndAction.RestartVideo:
                LogEvent("End Action: Restart Video");
                RestartVideo();
                break;
        }
    }
    
    public void RestartVideo()
    {
        // Hide end UI
        if (endUIPanel != null)
        {
            endUIPanel.SetActive(false);
        }
        
        videoPlayer.time = 0;
        videoPlayer.Play();
        LogEvent("Video restarted");
    }
    
    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    
    private void LogEvent(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        eventLog.AppendLine($"[{timestamp}] {message}");
        
        if (eventLogText != null)
        {
            eventLogText.text = eventLog.ToString();
        }
    }
    
    private void ClearEventLog()
    {
        eventLog.Clear();
        if (eventLogText != null)
        {
            eventLogText.text = "";
        }
        LogEvent("Log cleared");
    }
    
    private void UpdateStatusUI()
    {
        if (statusText == null) return;
        
        string playStatus = videoPlayer.isPlaying ? "▶️ Playing" : (isPrepared ? "⏸️ Ready" : "⏳ Preparing");
        double currentTime = videoPlayer.time;
        double duration = videoPlayer.clip != null ? videoPlayer.clip.length : 0;
        
        statusText.text = $"Video Event Demo\n" +
                          $"Status: {playStatus}\n" +
                          $"Time: {FormatTime(currentTime)} / {FormatTime(duration)}\n" +
                          $"End Action: {endAction}\n\n" +
                          $"Controls:\nV = Play | Space = Pause\nR = Restart | C = Clear Log";
    }
    
    private string FormatTime(double seconds)
    {
        int mins = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{mins:00}:{secs:00}";
    }
    
    private void OnDestroy()
    {
        videoPlayer.prepareCompleted -= HandlePrepareCompleted;
        videoPlayer.started -= HandleVideoStarted;
        videoPlayer.loopPointReached -= HandleLoopPointReached;
        
        // Cleanup runtime RenderTexture
        if (renderTexture != null && renderTexture.name == "Lab7_VideoRenderTexture")
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
