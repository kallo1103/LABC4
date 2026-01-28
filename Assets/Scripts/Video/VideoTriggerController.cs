using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

/// <summary>
/// Lab 5 - VideoPlayer Basic
/// Controls: V = Play, Space = Pause/Resume, R = Restart
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoTriggerController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private bool playOnAwake = false;
    [SerializeField] private bool loop = false;
    
    [Header("Audio Settings")]
    [SerializeField] private bool playAudio = true;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;
    
    [Header("UI Reference (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    private VideoPlayer videoPlayer;
    private bool isPrepared = false;
    
    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        // Configure VideoPlayer
        videoPlayer.playOnAwake = playOnAwake;
        videoPlayer.isLooping = loop;
        
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
        }
        
        // Audio output
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.SetDirectAudioVolume(0, volume);
        
        // Events
        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.started += OnVideoStarted;
        videoPlayer.loopPointReached += OnLoopPointReached;
    }
    
    private void Start()
    {
        // Prepare video
        if (videoPlayer.clip != null)
        {
            videoPlayer.Prepare();
        }
        
        UpdateStatusUI();
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // V to Play
        if (keyboard.vKey.wasPressedThisFrame)
        {
            PlayVideo();
        }
        
        // Space to Pause/Resume
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            TogglePause();
        }
        
        // R to Restart
        if (keyboard.rKey.wasPressedThisFrame)
        {
            RestartVideo();
        }
        
        UpdateStatusUI();
    }
    
    public void PlayVideo()
    {
        if (!isPrepared)
        {
            Debug.Log("[VideoTrigger] Video not prepared yet, preparing...");
            videoPlayer.Prepare();
            return;
        }
        
        videoPlayer.Play();
        Debug.Log("[VideoTrigger] Playing video");
    }
    
    public void TogglePause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("[VideoTrigger] Video paused");
        }
        else if (isPrepared)
        {
            videoPlayer.Play();
            Debug.Log("[VideoTrigger] Video resumed");
        }
    }
    
    public void StopVideo()
    {
        videoPlayer.Stop();
        Debug.Log("[VideoTrigger] Video stopped");
    }
    
    public void RestartVideo()
    {
        videoPlayer.time = 0;
        videoPlayer.Play();
        Debug.Log("[VideoTrigger] Video restarted");
    }
    
    public void SetVolume(float vol)
    {
        volume = Mathf.Clamp01(vol);
        videoPlayer.SetDirectAudioVolume(0, volume);
    }
    
    private void OnPrepareCompleted(VideoPlayer vp)
    {
        isPrepared = true;
        Debug.Log("[VideoTrigger] Video prepared and ready to play");
    }
    
    private void OnVideoStarted(VideoPlayer vp)
    {
        Debug.Log("[VideoTrigger] Video started playing");
    }
    
    private void OnLoopPointReached(VideoPlayer vp)
    {
        Debug.Log("[VideoTrigger] Video reached end (loop point)");
    }
    
    private void UpdateStatusUI()
    {
        if (statusText == null) return;
        
        string clipName = videoPlayer.clip != null ? videoPlayer.clip.name : "No Clip";
        string preparedStatus = isPrepared ? "✓ Ready" : "⏳ Preparing...";
        string playStatus = videoPlayer.isPlaying ? "▶️ Playing" : (videoPlayer.isPaused ? "⏸️ Paused" : "⏹️ Stopped");
        
        double currentTime = videoPlayer.time;
        double duration = videoPlayer.clip != null ? videoPlayer.clip.length : 0;
        string timeText = $"{FormatTime(currentTime)} / {FormatTime(duration)}";
        
        statusText.text = $"Video: {clipName}\n" +
                          $"Status: {preparedStatus}\n" +
                          $"{playStatus}\n" +
                          $"Time: {timeText}\n\n" +
                          $"Controls:\nV = Play\nSpace = Pause/Resume\nR = Restart";
    }
    
    private string FormatTime(double seconds)
    {
        int mins = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{mins:00}:{secs:00}";
    }
    
    private void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnPrepareCompleted;
        videoPlayer.started -= OnVideoStarted;
        videoPlayer.loopPointReached -= OnLoopPointReached;
    }
}
