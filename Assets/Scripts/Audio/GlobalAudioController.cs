using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lab 3 - Global Audio Control via AudioListener
/// Controls: M = Mute/Unmute, P = Pause/Resume
/// </summary>
public class GlobalAudioController : MonoBehaviour
{
    [Header("Initial State")]
    [SerializeField] private bool startMuted = false;
    [SerializeField] private bool startPaused = false;
    
    [Header("UI Reference (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    private bool isMuted = false;
    private bool isPaused = false;
    private float previousVolume = 1f;
    
    private void Awake()
    {
        // Initialize state
        isMuted = startMuted;
        isPaused = startPaused;
        
        // Apply initial state
        if (isMuted)
        {
            previousVolume = AudioListener.volume;
            AudioListener.volume = 0f;
        }
        
        AudioListener.pause = isPaused;
    }
    
    private void Start()
    {
        UpdateStatusUI();
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // M to Mute/Unmute
        if (keyboard.mKey.wasPressedThisFrame)
        {
            ToggleMute();
        }
        
        // P to Pause/Resume
        if (keyboard.pKey.wasPressedThisFrame)
        {
            TogglePause();
        }
        
        UpdateStatusUI();
    }
    
    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        if (isMuted)
        {
            previousVolume = AudioListener.volume > 0 ? AudioListener.volume : 1f;
            AudioListener.volume = 0f;
            Debug.Log("[GlobalAudio] MUTED - AudioListener.volume = 0");
        }
        else
        {
            AudioListener.volume = previousVolume;
            Debug.Log($"[GlobalAudio] UNMUTED - AudioListener.volume = {previousVolume}");
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        AudioListener.pause = isPaused;
        Debug.Log($"[GlobalAudio] {(isPaused ? "PAUSED" : "RESUMED")} - AudioListener.pause = {isPaused}");
    }
    
    public void SetMute(bool mute)
    {
        if (mute != isMuted)
        {
            ToggleMute();
        }
    }
    
    public void SetPause(bool pause)
    {
        if (pause != isPaused)
        {
            TogglePause();
        }
    }
    
    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        AudioListener.volume = volume;
        previousVolume = volume;
        isMuted = volume <= 0f;
    }
    
    public bool IsMuted => isMuted;
    public bool IsPaused => isPaused;
    public float CurrentVolume => AudioListener.volume;
    
    private void UpdateStatusUI()
    {
        if (statusText != null)
        {
            string muteStatus = isMuted ? "🔇 MUTED" : "🔊 UNMUTED";
            string pauseStatus = isPaused ? "⏸️ PAUSED" : "▶️ PLAYING";
            string volumeLevel = $"Volume: {(AudioListener.volume * 100):F0}%";
            
            statusText.text = $"Global Audio Status:\n{muteStatus}\n{pauseStatus}\n{volumeLevel}\n\nControls:\nM = Mute/Unmute\nP = Pause/Resume";
        }
    }
}
