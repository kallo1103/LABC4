using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Lab 4 - AudioClip Import & Optimization
/// Demonstrates different AudioClip configurations
/// Controls: 1-4 = Switch clips
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioClipManager : MonoBehaviour
{
    [Serializable]
    public class AudioClipConfig
    {
        public string name;
        public AudioClip clip;
        [TextArea(2, 4)]
        public string description;
        
        // Runtime info (readonly, for display)
        [HideInInspector] public string loadType;
        [HideInInspector] public string compressionFormat;
        [HideInInspector] public float length;
        [HideInInspector] public int channels;
        [HideInInspector] public int frequency;
        [HideInInspector] public bool loadInBackground;
    }
    
    [Header("Audio Clips Configuration")]
    [SerializeField] private AudioClipConfig[] audioClips = new AudioClipConfig[4];
    
    [Header("UI Reference (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private TMPro.TextMeshProUGUI reportText;
    
    private AudioSource audioSource;
    private int currentClipIndex = 0;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Populate clip info
        foreach (var config in audioClips)
        {
            if (config.clip != null)
            {
                config.length = config.clip.length;
                config.channels = config.clip.channels;
                config.frequency = config.clip.frequency;
                config.loadInBackground = config.clip.loadInBackground;
                
                // Note: The actual LoadType and CompressionFormat are set via Import Settings
                // We can't read them at runtime directly, but we document expected settings
            }
        }
    }
    
    private void Start()
    {
        if (audioClips.Length > 0 && audioClips[0].clip != null)
        {
            SelectClip(0);
        }
        
        UpdateStatusUI();
        GenerateReport();
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Number keys 1-4 to switch clips
        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            SelectClip(0);
        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            SelectClip(1);
        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            SelectClip(2);
        if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
            SelectClip(3);
        
        // Space to play/pause
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (audioSource.isPlaying)
                audioSource.Pause();
            else
                audioSource.Play();
        }
        
        UpdateStatusUI();
    }
    
    public void SelectClip(int index)
    {
        if (index < 0 || index >= audioClips.Length) return;
        if (audioClips[index].clip == null) return;
        
        currentClipIndex = index;
        audioSource.Stop();
        audioSource.clip = audioClips[index].clip;
        audioSource.Play();
        
        Debug.Log($"[AudioClipManager] Playing: {audioClips[index].name}");
    }
    
    private void UpdateStatusUI()
    {
        if (statusText == null) return;
        
        if (audioClips.Length == 0 || audioClips[currentClipIndex].clip == null)
        {
            statusText.text = "No clips loaded";
            return;
        }
        
        var config = audioClips[currentClipIndex];
        string playStatus = audioSource.isPlaying ? "▶️ Playing" : "⏹️ Stopped";
        
        statusText.text = $"Current Clip: {config.name}\n" +
                          $"{playStatus}\n" +
                          $"Length: {config.length:F2}s\n" +
                          $"Channels: {config.channels}\n" +
                          $"Frequency: {config.frequency}Hz\n\n" +
                          $"Controls:\n1-4 = Switch Clips\nSpace = Play/Pause";
    }
    
    private void GenerateReport()
    {
        if (reportText == null) return;
        
        string report = "=== Audio Optimization Report ===\n\n";
        
        report += "| Type | Load Type | Compression | Use Case |\n";
        report += "|------|-----------|-------------|----------|\n";
        report += "| BGM  | Streaming | Vorbis | Nhạc nền dài, tiết kiệm RAM |\n";
        report += "| SFX  | Decompress On Load | ADPCM | Hiệu ứng ngắn, low latency |\n";
        report += "| Voice | Compressed In Memory | Vorbis | Hội thoại, cân bằng RAM/CPU |\n";
        report += "| Ambient | Streaming | Vorbis | Âm thanh môi trường loop |\n\n";
        
        report += "=== Loaded Clips ===\n\n";
        
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].clip != null)
            {
                var c = audioClips[i];
                report += $"[{i + 1}] {c.name}\n";
                report += $"    Length: {c.length:F2}s | Channels: {c.channels} | Freq: {c.frequency}Hz\n";
                report += $"    {c.description}\n\n";
            }
        }
        
        reportText.text = report;
    }
    
    /// <summary>
    /// Get a formatted report string for documentation
    /// </summary>
    public string GetOptimizationReport()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine("# Audio Optimization Report - Lab 4");
        sb.AppendLine();
        sb.AppendLine("## Recommended Settings");
        sb.AppendLine();
        sb.AppendLine("| Audio Type | Load Type | Compression | Lý do |");
        sb.AppendLine("|------------|-----------|-------------|-------|");
        sb.AppendLine("| BGM (dài) | Streaming | Vorbis | Stream từ disk, không load toàn bộ vào RAM |");
        sb.AppendLine("| SFX (ngắn) | Decompress On Load | ADPCM | Giải nén sẵn, play ngay không delay |");
        sb.AppendLine("| Voice/Dialog | Compressed In Memory | Vorbis | Nén trong RAM, giải nén khi play |");
        sb.AppendLine("| Ambient Loop | Streaming | Vorbis | Tương tự BGM, chạy liên tục |");
        sb.AppendLine();
        sb.AppendLine("## Loaded Clips");
        sb.AppendLine();
        
        foreach (var config in audioClips)
        {
            if (config.clip != null)
            {
                sb.AppendLine($"### {config.name}");
                sb.AppendLine($"- Length: {config.length:F2} seconds");
                sb.AppendLine($"- Channels: {config.channels}");
                sb.AppendLine($"- Sample Rate: {config.frequency} Hz");
                sb.AppendLine($"- Description: {config.description}");
                sb.AppendLine();
            }
        }
        
        return sb.ToString();
    }
}
