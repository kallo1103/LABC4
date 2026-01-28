using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Displays lab instructions UI with keybindings
/// Toggle visibility with H key
/// </summary>
public class LabInstructionUI : MonoBehaviour
{
    [Header("Lab Info")]
    [SerializeField] private string labTitle = "Lab Title";
    [SerializeField] [TextArea(3, 10)] private string labDescription = "Description...";
    [SerializeField] [TextArea(3, 10)] private string controls = "Controls:\nSpace = Play";
    
    [Header("UI Elements")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI controlsText;
    
    [Header("Settings")]
    [SerializeField] private Key toggleKey = Key.H;
    [SerializeField] private bool showOnStart = true;
    
    private bool isVisible;
    
    private void Awake()
    {
        isVisible = showOnStart;
    }
    
    private void Start()
    {
        UpdateUI();
        SetVisibility(isVisible);
    }
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        if (keyboard[toggleKey].wasPressedThisFrame)
        {
            ToggleVisibility();
        }
    }
    
    private void UpdateUI()
    {
        if (titleText != null)
            titleText.text = labTitle;
            
        if (descriptionText != null)
            descriptionText.text = labDescription;
            
        if (controlsText != null)
            controlsText.text = controls;
    }
    
    public void ToggleVisibility()
    {
        isVisible = !isVisible;
        SetVisibility(isVisible);
    }
    
    public void SetVisibility(bool visible)
    {
        isVisible = visible;
        
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(visible);
        }
    }
    
    public void SetLabInfo(string title, string description, string controlsList)
    {
        labTitle = title;
        labDescription = description;
        controls = controlsList;
        UpdateUI();
    }
    
    // Static factory method for quick setup
    public static LabInstructionUI CreateInstructionUI(Transform parent, string title, string desc, string controls)
    {
        // Create canvas
        GameObject canvasObj = new GameObject("InstructionCanvas");
        canvasObj.transform.SetParent(parent);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create panel
        GameObject panelObj = new GameObject("InstructionPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -20);
        panelRect.sizeDelta = new Vector2(400, 300);
        
        UnityEngine.UI.Image panelImage = panelObj.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        // Create texts
        GameObject titleObj = CreateTextObject(panelObj.transform, "Title", title, 24, new Vector2(10, -10), new Vector2(380, 40));
        GameObject descObj = CreateTextObject(panelObj.transform, "Description", desc, 16, new Vector2(10, -60), new Vector2(380, 100));
        GameObject ctrlObj = CreateTextObject(panelObj.transform, "Controls", controls, 14, new Vector2(10, -170), new Vector2(380, 120));
        
        // Add component
        LabInstructionUI ui = canvasObj.AddComponent<LabInstructionUI>();
        ui.instructionPanel = panelObj;
        ui.titleText = titleObj.GetComponent<TextMeshProUGUI>();
        ui.descriptionText = descObj.GetComponent<TextMeshProUGUI>();
        ui.controlsText = ctrlObj.GetComponent<TextMeshProUGUI>();
        ui.labTitle = title;
        ui.labDescription = desc;
        ui.controls = controls;
        
        return ui;
    }
    
    private static GameObject CreateTextObject(Transform parent, string name, string text, int fontSize, Vector2 position, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        
        return textObj;
    }
}
