using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays global audio status (mute, pause, volume)
/// </summary>
public class AudioStatusUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image muteIcon;
    [SerializeField] private Image pauseIcon;
    [SerializeField] private Slider volumeSlider;
    
    [Header("Icons")]
    [SerializeField] private Sprite mutedSprite;
    [SerializeField] private Sprite unmutedSprite;
    [SerializeField] private Sprite pausedSprite;
    [SerializeField] private Sprite playingSprite;
    
    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.red;
    
    private void Update()
    {
        UpdateStatusDisplay();
    }
    
    private void UpdateStatusDisplay()
    {
        bool isMuted = AudioListener.volume <= 0f;
        bool isPaused = AudioListener.pause;
        
        // Update status text
        if (statusText != null)
        {
            string muteStatus = isMuted ? "🔇 MUTED" : "🔊 SOUND ON";
            string pauseStatus = isPaused ? "⏸️ PAUSED" : "▶️ PLAYING";
            string volumeText = $"Volume: {(AudioListener.volume * 100):F0}%";
            
            statusText.text = $"{muteStatus}\n{pauseStatus}\n{volumeText}";
        }
        
        // Update icons
        if (muteIcon != null)
        {
            muteIcon.color = isMuted ? inactiveColor : activeColor;
            if (mutedSprite != null && unmutedSprite != null)
            {
                muteIcon.sprite = isMuted ? mutedSprite : unmutedSprite;
            }
        }
        
        if (pauseIcon != null)
        {
            pauseIcon.color = isPaused ? inactiveColor : activeColor;
            if (pausedSprite != null && playingSprite != null)
            {
                pauseIcon.sprite = isPaused ? pausedSprite : playingSprite;
            }
        }
        
        // Update volume slider
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(AudioListener.volume);
        }
    }
    
    public void OnVolumeSliderChanged(float value)
    {
        AudioListener.volume = value;
    }
    
    public void SetVisible(bool visible)
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(visible);
        }
    }
    
    // Static factory for quick UI creation
    public static AudioStatusUI CreateStatusUI(Transform parent)
    {
        // Create canvas
        GameObject canvasObj = new GameObject("AudioStatusCanvas");
        canvasObj.transform.SetParent(parent);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;
        
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create panel (top-right corner)
        GameObject panelObj = new GameObject("StatusPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(-20, -20);
        panelRect.sizeDelta = new Vector2(200, 100);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.6f);
        
        // Create status text
        GameObject textObj = new GameObject("StatusText");
        textObj.transform.SetParent(panelObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 16;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        
        // Add component
        AudioStatusUI statusUI = canvasObj.AddComponent<AudioStatusUI>();
        statusUI.statusPanel = panelObj;
        statusUI.statusText = tmp;
        
        return statusUI;
    }
}
