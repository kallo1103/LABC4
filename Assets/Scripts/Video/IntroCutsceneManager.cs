using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Mini Project - Intro Cutscene Game
/// Features: Video intro + BGM, Skip button, Auto-transition to gameplay
/// </summary>
public class IntroCutsceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip introVideo;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private RenderTexture renderTexture;
    
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
    [SerializeField] private TMPro.TextMeshProUGUI skipButtonText;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    [Header("Skip Settings")]
    [SerializeField] private Key skipKey = Key.Escape;
    [SerializeField] private bool requireHoldToSkip = false;
    [SerializeField] private float holdDuration = 1f;
    
    private bool isSkipping = false;
    private bool videoEnded = false;
    private float holdTimer = 0f;
    
    private void Awake()
    {
        // Setup video player
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
            
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            
            if (introVideo != null)
                videoPlayer.clip = introVideo;
                
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            
            // Create RenderTexture if needed
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(1920, 1080, 0);
            }
            videoPlayer.targetTexture = renderTexture;
            
            // Assign to RawImage
            if (videoDisplay != null)
            {
                videoDisplay.texture = renderTexture;
            }
            
            // Subscribe to events
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoEnded;
        }
        
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
        
        // Setup skip button
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipIntro);
        }
        
        // Initialize fade overlay
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 1); // Start black
        }
        
        // Hide loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
    }
    
    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
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
        // Show loading
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
            
        // Prepare video
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            videoPlayer.Prepare();
            
            // Wait for preparation
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }
        }
        
        // Hide loading
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
        
        // Fade in from black
        yield return StartCoroutine(FadeScreen(1f, 0f));
        
        // Start video and BGM
        if (videoPlayer != null)
            videoPlayer.Play();
            
        if (bgmSource != null && bgmClip != null)
            bgmSource.Play();
            
        Debug.Log("[IntroCutscene] Intro started playing");
    }
    
    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("[IntroCutscene] Video prepared");
    }
    
    private void OnVideoEnded(VideoPlayer vp)
    {
        if (!isSkipping)
        {
            videoEnded = true;
            Debug.Log("[IntroCutscene] Video ended naturally, transitioning to gameplay");
            StartCoroutine(TransitionToGameplay());
        }
    }
    
    public void SkipIntro()
    {
        if (isSkipping) return;
        
        isSkipping = true;
        Debug.Log("[IntroCutscene] Skipping intro...");
        
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
        
        // Fade to black
        yield return StartCoroutine(FadeScreen(0f, 1f));
        
        // Wait a bit
        yield return new WaitForSeconds(transitionDelay);
        
        // Load gameplay scene
        Debug.Log($"[IntroCutscene] Loading scene: {gameplaySceneName}");
        
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogWarning("[IntroCutscene] No gameplay scene specified!");
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
        
        if (videoPlayer == null) return;
        
        double currentTime = videoPlayer.time;
        double duration = videoPlayer.clip != null ? videoPlayer.clip.length : 0;
        
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
    }
}
