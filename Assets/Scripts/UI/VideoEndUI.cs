using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// UI panel shown when video ends
/// Options: Replay, Continue, Skip
/// </summary>
public class VideoEndUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button skipButton;
    
    [Header("Settings")]
    [SerializeField] private string endTitle = "Video Complete";
    [SerializeField] private string endMessage = "What would you like to do?";
    [SerializeField] private string nextSceneName = "";
    
    [Header("References")]
    [SerializeField] private UnityEngine.Video.VideoPlayer videoPlayer;
    
    public event System.Action OnReplay;
    public event System.Action OnContinue;
    public event System.Action OnSkip;
    
    private void Awake()
    {
        // Setup button listeners
        if (replayButton != null)
            replayButton.onClick.AddListener(HandleReplay);
            
        if (continueButton != null)
            continueButton.onClick.AddListener(HandleContinue);
            
        if (skipButton != null)
            skipButton.onClick.AddListener(HandleSkip);
        
        // Hide panel initially
        if (endPanel != null)
            endPanel.SetActive(false);
    }
    
    private void Start()
    {
        UpdateText();
    }
    
    public void Show()
    {
        if (endPanel != null)
        {
            endPanel.SetActive(true);
            
            // Unlock cursor for button interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    public void Hide()
    {
        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }
    }
    
    private void UpdateText()
    {
        if (titleText != null)
            titleText.text = endTitle;
            
        if (messageText != null)
            messageText.text = endMessage;
    }
    
    public void SetText(string title, string message)
    {
        endTitle = title;
        endMessage = message;
        UpdateText();
    }
    
    private void HandleReplay()
    {
        Hide();
        
        // Replay video
        if (videoPlayer != null)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
        
        OnReplay?.Invoke();
        Debug.Log("[VideoEndUI] Replay clicked");
    }
    
    private void HandleContinue()
    {
        Hide();
        
        // Load next scene or invoke event
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        
        OnContinue?.Invoke();
        Debug.Log("[VideoEndUI] Continue clicked");
    }
    
    private void HandleSkip()
    {
        Hide();
        OnSkip?.Invoke();
        Debug.Log("[VideoEndUI] Skip clicked");
    }
    
    public void SetVideoPlayer(UnityEngine.Video.VideoPlayer vp)
    {
        videoPlayer = vp;
    }
    
    public void SetNextScene(string sceneName)
    {
        nextSceneName = sceneName;
    }
    
    private void OnDestroy()
    {
        if (replayButton != null)
            replayButton.onClick.RemoveListener(HandleReplay);
            
        if (continueButton != null)
            continueButton.onClick.RemoveListener(HandleContinue);
            
        if (skipButton != null)
            skipButton.onClick.RemoveListener(HandleSkip);
    }
    
    // Static factory for quick creation
    public static VideoEndUI CreateEndUI(Transform parent, UnityEngine.Video.VideoPlayer videoPlayer = null)
    {
        // Create canvas
        GameObject canvasObj = new GameObject("VideoEndCanvas");
        canvasObj.transform.SetParent(parent);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create panel (center)
        GameObject panelObj = new GameObject("EndPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 250);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // Create title
        GameObject titleObj = CreateText(panelObj.transform, "Title", "Video Complete", 28, 
            new Vector2(0.5f, 1), new Vector2(0, -20), Color.white);
        
        // Create message
        GameObject msgObj = CreateText(panelObj.transform, "Message", "What would you like to do?", 18,
            new Vector2(0.5f, 0.65f), Vector2.zero, new Color(0.8f, 0.8f, 0.8f));
        
        // Create buttons
        GameObject replayBtn = CreateButton(panelObj.transform, "ReplayButton", "Replay", 
            new Vector2(0.5f, 0.4f), new Vector2(150, 40), new Color(0.2f, 0.6f, 0.2f));
            
        GameObject continueBtn = CreateButton(panelObj.transform, "ContinueButton", "Continue",
            new Vector2(0.5f, 0.22f), new Vector2(150, 40), new Color(0.2f, 0.4f, 0.8f));
        
        // Add VideoEndUI component
        VideoEndUI endUI = canvasObj.AddComponent<VideoEndUI>();
        endUI.endPanel = panelObj;
        endUI.titleText = titleObj.GetComponent<TextMeshProUGUI>();
        endUI.messageText = msgObj.GetComponent<TextMeshProUGUI>();
        endUI.replayButton = replayBtn.GetComponent<Button>();
        endUI.continueButton = continueBtn.GetComponent<Button>();
        endUI.videoPlayer = videoPlayer;
        
        // Re-setup button listeners (Awake already ran)
        endUI.replayButton.onClick.AddListener(endUI.HandleReplay);
        endUI.continueButton.onClick.AddListener(endUI.HandleContinue);
        
        // Hide initially
        panelObj.SetActive(false);
        
        return endUI;
    }
    
    private static GameObject CreateText(Transform parent, string name, string text, int fontSize, 
        Vector2 anchor, Vector2 position, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(380, 50);
        
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        
        return obj;
    }
    
    private static GameObject CreateButton(Transform parent, string name, string text,
        Vector2 anchor, Vector2 size, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent);
        
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        
        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        
        return btnObj;
    }
}
