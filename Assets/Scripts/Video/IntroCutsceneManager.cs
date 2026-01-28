using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Mini Project - Intro Cutscene Game
/// Features: Video intro + BGM, Skip button, Auto-transition to gameplay
/// Displays video via RenderTexture + RawImage
/// </summary>
public class IntroCutsceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip introVideo;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private int videoWidth = 1920;
    [SerializeField] private int videoHeight = 1080;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.5f;
    [SerializeField] private bool fadeAudioOnSkip = true;
    
    [Header("Scene Transition")]
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private float transitionDelay = 0.5f;
    
    [Header("Fade Effect")]
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("UI Elements")]
    [SerializeField] private Button skipButton;
    [SerializeField] private TextMeshProUGUI skipButtonText;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Header("Skip Settings")]
    [SerializeField] private Key skipKey = Key.Escape;
    [SerializeField] private bool requireHoldToSkip = false;
    [SerializeField] private float holdDuration = 1f;
    
    private bool isSkipping = false;
    private bool videoEnded = false;
    private float holdTimer = 0f;
    private bool isPrepared = false;
    
    private void Awake()
    {
        // Setup video player
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
        
        if (videoPlayer == null)
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            
        // Create RenderTexture
        renderTexture = new RenderTexture(videoWidth, videoHeight, 0);
        renderTexture.name = "MiniProject_VideoRenderTexture";
        renderTexture.Create();
        
        // Configure VideoPlayer
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        
        if (introVideo != null)
            videoPlayer.clip = introVideo;
        
        // Audio from video
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.SetDirectAudioVolume(0, 0.5f);
        
        // Subscribe to events
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoEnded;
        
        // Setup BGM
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();
            
        if (bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
    }
    
    private void Start()
    {
        // Auto-find RawImage
        if (videoDisplay == null)
        {
            GameObject rawImageGO = GameObject.Find("VideoDisplay");
            if (rawImageGO != null)
            {
                videoDisplay = rawImageGO.GetComponent<RawImage>();
            }
            
            if (videoDisplay == null)
            {
                videoDisplay = FindAnyObjectByType<RawImage>();
            }
        }
        
        // Assign RenderTexture to RawImage
        if (videoDisplay != null)
        {
            videoDisplay.texture = renderTexture;
            Debug.Log($"[MiniProject] Found video display: {videoDisplay.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[MiniProject] No RawImage found - creating one");
            CreateVideoDisplay();
        }
        
        // Auto-find fade overlay
        if (fadeOverlay == null)
        {
            GameObject fadeObj = GameObject.Find("FadeOverlay");
            if (fadeObj != null)
            {
                fadeOverlay = fadeObj.GetComponent<Image>();
            }
        }
        
        // Initialize fade overlay (start black)
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 1);
            fadeOverlay.gameObject.SetActive(true);
        }
        
        // Find skip button
        if (skipButton == null)
        {
            GameObject skipObj = GameObject.Find("SkipButton");
            if (skipObj != null)
            {
                skipButton = skipObj.GetComponent<Button>();
            }
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipIntro);
        }
        
        // Hide loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
        
        StartCoroutine(PlayIntroSequence());
    }
    
    private void CreateVideoDisplay()
    {
        // Create canvas for video
        GameObject canvasObj = new GameObject("VideoCanvas_Runtime");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -10;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create RawImage
        GameObject rawImageObj = new GameObject("VideoDisplay_Runtime");
        rawImageObj.transform.SetParent(canvasObj.transform);
        
        videoDisplay = rawImageObj.AddComponent<RawImage>();
        videoDisplay.texture = renderTexture;
        videoDisplay.color = Color.white;
        
        RectTransform rect = videoDisplay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Debug.Log("[MiniProject] Created runtime video display");
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Skip via keyboard
        if (!isSkipping && !videoEnded)
        {
            if (requireHoldToSkip)
            {
                // Hold to skip
                if (keyboard[skipKey].isPressed)
                {
                    holdTimer += Time.deltaTime;
                    UpdateSkipButtonText($"Hold to Skip ({holdDuration - holdTimer:F1}s)");
                    
                    if (holdTimer >= holdDuration)
                    {
                        SkipIntro();
                    }
                }
                else
                {
                    holdTimer = 0f;
                    UpdateSkipButtonText("Hold ESC to Skip");
                }
            }
            else
            {
                // Instant skip
                if (keyboard[skipKey].wasPressedThisFrame)
                {
                    SkipIntro();
                }
            }
        }
        
        UpdateStatusUI();
    }
    
    private IEnumerator PlayIntroSequence()
    {
        Debug.Log("[MiniProject] Starting intro sequence...");
        
        // Show loading
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
            
        // Prepare video
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            videoPlayer.Prepare();
            Debug.Log("[MiniProject] Preparing video...");
            
            // Wait for preparation with timeout
            float prepareTimeout = 10f;
            float elapsed = 0f;
            while (!isPrepared && elapsed < prepareTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (!isPrepared)
            {
                Debug.LogWarning("[MiniProject] Video prepare timeout!");
            }
        }
        else
        {
            Debug.LogWarning("[MiniProject] No video clip assigned!");
        }
        
        // Hide loading
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
        
        // Fade in from black
        Debug.Log("[MiniProject] Fading in...");
        yield return StartCoroutine(FadeScreen(1f, 0f));
        
        // Start video and BGM
        if (videoPlayer != null && isPrepared)
        {
            videoPlayer.Play();
            Debug.Log("[MiniProject] Video started playing");
        }
            
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.Play();
            Debug.Log("[MiniProject] BGM started");
        }
    }
    
    private void OnVideoPrepared(VideoPlayer vp)
    {
        isPrepared = true;
        Debug.Log("[MiniProject] Video prepared and ready!");
    }
    
    private void OnVideoEnded(VideoPlayer vp)
    {
        if (!isSkipping)
        {
            videoEnded = true;
            Debug.Log("[MiniProject] Video ended naturally, transitioning to gameplay");
            StartCoroutine(TransitionToGameplay());
        }
    }
    
    public void SkipIntro()
    {
        if (isSkipping) return;
        
        isSkipping = true;
        Debug.Log("[MiniProject] Skipping intro...");
        
        // Stop video
        if (videoPlayer != null)
            videoPlayer.Stop();
        
        // Fade out BGM
        if (fadeAudioOnSkip && bgmSource != null)
        {
            StartCoroutine(FadeAudio(bgmSource, 0f, fadeDuration));
        }
        
        StartCoroutine(TransitionToGameplay());
    }
    
    private IEnumerator TransitionToGameplay()
    {
        // Hide skip button
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
        
        // Ensure fade overlay exists
        if (fadeOverlay == null)
        {
            // Create fade overlay
            GameObject canvasObj = GameObject.Find("IntroCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("FadeCanvas_Runtime");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                canvasObj.AddComponent<CanvasScaler>();
            }
            
            GameObject fadeObj = new GameObject("FadeOverlay_Runtime");
            fadeObj.transform.SetParent(canvasObj.transform);
            
            fadeOverlay = fadeObj.AddComponent<Image>();
            fadeOverlay.color = new Color(0, 0, 0, 0);
            
            RectTransform rect = fadeOverlay.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        fadeOverlay.gameObject.SetActive(true);
        
        // Fade to black
        Debug.Log("[MiniProject] Fading out...");
        yield return StartCoroutine(FadeScreen(0f, 1f));
        
        // Wait a bit
        yield return new WaitForSeconds(transitionDelay);
        
        // Load gameplay scene
        Debug.Log($"[MiniProject] Loading scene: {gameplaySceneName}");
        
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogWarning("[MiniProject] No gameplay scene specified!");
        }
    }
    
    private IEnumerator FadeScreen(float startAlpha, float endAlpha)
    {
        if (fadeOverlay == null) yield break;
        
        float elapsed = 0f;
        Color color = fadeOverlay.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeOverlay.color = color;
            yield return null;
        }
        
        color.a = endAlpha;
        fadeOverlay.color = color;
    }
    
    private IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        
        source.volume = targetVolume;
        
        if (targetVolume <= 0f)
        {
            source.Stop();
        }
    }
    
    private void UpdateSkipButtonText(string text)
    {
        if (skipButtonText != null)
        {
            skipButtonText.text = text;
        }
    }
    
    private void UpdateStatusUI()
    {
        if (statusText == null) return;
        
        if (videoPlayer == null || videoPlayer.clip == null) return;
        
        double currentTime = videoPlayer.time;
        double duration = videoPlayer.clip.length;
        
        string timeText = $"{FormatTime(currentTime)} / {FormatTime(duration)}";
        string skipText = requireHoldToSkip ? $"Hold {skipKey} to Skip" : $"Press {skipKey} to Skip";
        
        statusText.text = $"{timeText}\n{skipText}";
    }
    
    private string FormatTime(double seconds)
    {
        int mins = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{mins:00}:{secs:00}";
    }
    
    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoEnded;
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipIntro);
        }
        
        // Cleanup runtime RenderTexture
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
