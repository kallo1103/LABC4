using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lab 2 - Audio 2D vs 3D (Spatial Audio)
/// Controls: T = Toggle 2D/3D mode
/// Player must have AudioListener attached
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SpatialAudioController : MonoBehaviour
{
    [Header("Spatial Settings")]
    [SerializeField] private bool startAs3D = true;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 20f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private bool loop = true;
    
    [Header("Visual Settings")]
    [SerializeField] private Color gizmoColor2D = Color.cyan;
    [SerializeField] private Color gizmoColor3D = Color.green;
    [SerializeField] private bool showRangeGizmo = true;
    
    [Header("UI Reference (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    private AudioSource audioSource;
    private bool is3D;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Configure AudioSource
        audioSource.loop = loop;
        audioSource.playOnAwake = true;
        
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
        }
        
        // Set initial spatial mode
        is3D = startAs3D;
        ApplySpatialSettings();
    }
    
    private void Start()
    {
        // Start playing
        if (audioSource.clip != null)
        {
            audioSource.Play();
        }
        
        UpdateStatusUI();
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // T to toggle 2D/3D
        if (keyboard.tKey.wasPressedThisFrame)
        {
            ToggleSpatialMode();
        }
        
        UpdateStatusUI();
    }
    
    public void ToggleSpatialMode()
    {
        is3D = !is3D;
        ApplySpatialSettings();
        Debug.Log($"[SpatialAudio] Mode changed to: {(is3D ? "3D Spatial" : "2D Flat")}");
    }
    
    public void Set3DMode(bool enable3D)
    {
        is3D = enable3D;
        ApplySpatialSettings();
    }
    
    private void ApplySpatialSettings()
    {
        if (is3D)
        {
            // 3D Spatial Audio
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }
        else
        {
            // 2D Flat Audio
            audioSource.spatialBlend = 0f;
        }
    }
    
    private void UpdateStatusUI()
    {
        if (statusText != null)
        {
            string mode = is3D ? "3D Spatial" : "2D Flat";
            string distanceInfo = is3D ? $"\nMin: {minDistance}m | Max: {maxDistance}m" : "";
            
            // Find player distance if in 3D mode
            string playerDistance = "";
            if (is3D)
            {
                AudioListener listener = FindAnyObjectByType<AudioListener>();
                if (listener != null)
                {
                    float dist = Vector3.Distance(transform.position, listener.transform.position);
                    playerDistance = $"\nPlayer Distance: {dist:F1}m";
                }
            }
            
            statusText.text = $"Audio Mode: {mode}{distanceInfo}{playerDistance}\n\nControls:\nT = Toggle 2D/3D\nWASD = Move Player";
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showRangeGizmo) return;
        
        Gizmos.color = is3D ? gizmoColor3D : gizmoColor2D;
        
        // Draw speaker icon
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        if (is3D)
        {
            // Draw min/max distance spheres
            Gizmos.color = new Color(gizmoColor3D.r, gizmoColor3D.g, gizmoColor3D.b, 0.3f);
            Gizmos.DrawWireSphere(transform.position, minDistance);
            
            Gizmos.color = new Color(gizmoColor3D.r, gizmoColor3D.g, gizmoColor3D.b, 0.1f);
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }
    }
    
    private void OnValidate()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource != null)
        {
            ApplySpatialSettings();
        }
    }
}
