using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

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
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI eventLogText;
    
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
        
        // Create RenderTexture
        renderTexture = new RenderTexture(videoWidth, videoHeight, 0);
        renderTexture.name = "Lab7_VideoRenderTexture";
        renderTexture.Create();
        
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
            GameObject rawImageGO = GameObject.Find("VideoRawImage");
            if (rawImageGO != null)
            {
                displayRawImage = rawImageGO.GetComponent<RawImage>();
            }
            
            if (displayRawImage == null)
            {
                displayRawImage = FindAnyObjectByType<RawImage>();
            }
        }
        
        // Assign RenderTexture to RawImage
        if (displayRawImage != null)
        {
            displayRawImage.texture = renderTexture;
            Debug.Log($"[Lab7] Found RawImage: {displayRawImage.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[Lab7] No RawImage found!");
        }
        
        // Auto-find Event Log Text
        if (eventLogText == null)
        {
            // Try to find by searching all TextMeshProUGUI
            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var txt in allTexts)
            {
                if (txt.gameObject.name.Contains("EventLog") || txt.gameObject.name.Contains("Text"))
                {
                    // Check if parent is a status canvas
                    Canvas parentCanvas = txt.GetComponentInParent<Canvas>();
                    if (parentCanvas != null && parentCanvas.gameObject.name.Contains("EventLog"))
                    {
                        eventLogText = txt;
                        break;
                    }
                }
            }
            
            // Fallback: find StatusCanvas's text
            if (eventLogText == null)
            {
                GameObject eventLogCanvas = GameObject.Find("EventLog");
                if (eventLogCanvas != null)
                {
                    eventLogText = eventLogCanvas.GetComponentInChildren<TextMeshProUGUI>();
                }
            }
        }
        
        if (eventLogText != null)
        {
            Debug.Log($"[Lab7] Found Event Log Text: {eventLogText.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[Lab7] No Event Log Text found - creating one");
            CreateEventLogUI();
        }
        
        // Find end UI panel
        if (endUIPanel == null)
        {
            GameObject endPanel = GameObject.Find("EndPanel");
            if (endPanel != null)
            {
                endUIPanel = endPanel;
            }
        }
        
        // Hide end UI initially
        if (endUIPanel != null)
        {
            endUIPanel.SetActive(false);
        }
        
        LogEvent("=== Lab 7: Video Events Demo ===");
        LogEvent("Initializing VideoPlayer...");
        
        if (videoPlayer.clip != null)
        {
            videoPlayer.Prepare();
        }
        else
        {
            LogEvent("ERROR: No video clip assigned!");
        }
        
        UpdateStatusUI();
    }
    
    private void CreateEventLogUI()
    {
        // Create a simple event log display
        GameObject canvasObj = new GameObject("EventLogCanvas_Runtime");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create background panel
        GameObject panelObj = new GameObject("EventLogPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.3f);
        panelRect.anchorMax = new Vector2(1, 0.9f);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.anchoredPosition = new Vector2(-20, 0);
        panelRect.sizeDelta = new Vector2(350, 0);
        
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);
        
        // Create text
        GameObject textObj = new GameObject("EventLogText");
        textObj.transform.SetParent(panelObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        eventLogText = textObj.AddComponent<TextMeshProUGUI>();
        eventLogText.fontSize = 14;
        eventLogText.color = Color.white;
        eventLogText.alignment = TextAlignmentOptions.TopLeft;
        
        Debug.Log("[Lab7] Created runtime Event Log UI");
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
                LogEvent("User pressed V - Playing video");
            }
        }
        
        // Space to pause/resume
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
                LogEvent("User pressed Space - Paused");
            }
            else if (isPrepared)
            {
                videoPlayer.Play();
                LogEvent("User pressed Space - Resumed");
            }
        }
        
        // R to restart
        if (keyboard.rKey.wasPressedThisFrame)
        {
            RestartVideo();
            LogEvent("User pressed R - Restarting");
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
        LogEvent("   → Video is ready to play!");
        Debug.Log("[Lab7] prepareCompleted - Video is ready");
        
        OnVideoPrepared?.Invoke();
        
        if (autoPlay)
        {
            videoPlayer.Play();
            LogEvent("   → Auto-play enabled, starting...");
        }
    }
    
    private void HandleVideoStarted(VideoPlayer vp)
    {
        LogEvent("▶ EVENT: started");
        LogEvent("   → Video playback started");
        Debug.Log("[Lab7] started - Video playback started");
        
        OnVideoStarted?.Invoke();
    }
    
    private void HandleLoopPointReached(VideoPlayer vp)
    {
        LogEvent("⏹ EVENT: loopPointReached");
        LogEvent("   → Video finished playing!");
        Debug.Log("[Lab7] loopPointReached - Video finished");
        
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
                LogEvent("End Action: Showing UI Panel");
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
                LogEvent("End Action: Restarting video");
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
        string logLine = $"[{timestamp}] {message}";
        eventLog.AppendLine(logLine);
        
        Debug.Log($"[Lab7 EventLog] {message}");
        
        if (eventLogText != null)
        {
            eventLogText.text = eventLog.ToString();
        }
    }
    
    private void ClearEventLog()
    {
        eventLog.Clear();
        LogEvent("=== Log Cleared ===");
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
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
