using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lab 1 - AudioSource Basic (Sound Trigger)
/// Controls: Space = Play, S = Stop
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioTriggerController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private bool playOnAwake = false;
    
    [Header("UI Reference (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Configure AudioSource
        audioSource.playOnAwake = playOnAwake;
        
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
        }
    }
    
    private void Start()
    {
        UpdateStatusUI();
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Space to Play
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            PlayAudio();
        }
        
        // S to Stop
        if (keyboard.sKey.wasPressedThisFrame)
        {
            StopAudio();
        }
        
        UpdateStatusUI();
    }
    
    public void PlayAudio()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
            Debug.Log($"[AudioTrigger] Playing: {audioSource.clip.name}");
        }
        else
        {
            Debug.LogWarning("[AudioTrigger] No AudioClip assigned!");
        }
    }
    
    public void StopAudio()
    {
        audioSource.Stop();
        Debug.Log("[AudioTrigger] Audio stopped");
    }
    
    public void SetClip(AudioClip clip)
    {
        audioClip = clip;
        audioSource.clip = clip;
    }
    
    private void UpdateStatusUI()
    {
        if (statusText != null)
        {
            string status = audioSource.isPlaying ? "Playing" : "Stopped";
            string clipName = audioSource.clip != null ? audioSource.clip.name : "No Clip";
            statusText.text = $"Status: {status}\nClip: {clipName}\n\nControls:\nSpace = Play\nS = Stop";
        }
    }
    
    // Editor helper
    private void OnValidate()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource != null)
        {
            audioSource.playOnAwake = playOnAwake;
            if (audioClip != null)
                audioSource.clip = audioClip;
        }
    }
}
