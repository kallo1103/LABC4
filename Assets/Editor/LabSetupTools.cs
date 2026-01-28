using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Editor Tools for Lab Setup - One-click scene and prefab generation
/// Menu: Tools/LABC4/...
/// </summary>
public class LabSetupTools : EditorWindow
{
    private const string SCENES_PATH = "Assets/Scenes";
    private const string PREFABS_PATH = "Assets/Prefabs";
    private const string AUDIO_PATH = "Assets/Audio/SFX";
    private const string VIDEO_PATH = "Assets/Video";
    private const string MATERIALS_PATH = "Assets/Materials";
    private const string RENDER_TEXTURES_PATH = "Assets/RenderTextures";
    
    private static AudioClip defaultAudioClip;
    private static VideoClip defaultVideoClip;
    
    [MenuItem("Tools/LABC4/Setup Window", false, 0)]
    public static void ShowWindow()
    {
        GetWindow<LabSetupTools>("Lab Setup Tools");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("LABC4 - Audio & Video Labs", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // Generate All
        GUIStyle bigButton = new GUIStyle(GUI.skin.button);
        bigButton.fontSize = 14;
        bigButton.fontStyle = FontStyle.Bold;
        
        if (GUILayout.Button("🚀 Generate ALL Labs", bigButton, GUILayout.Height(40)))
        {
            GenerateAllLabs();
        }
        
        EditorGUILayout.Space(20);
        GUILayout.Label("Generate Individual Labs:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Lab 1 - AudioSource Basic")) GenerateLab1Scene();
        if (GUILayout.Button("Lab 2 - Spatial Audio")) GenerateLab2Scene();
        if (GUILayout.Button("Lab 3 - Global Audio Control")) GenerateLab3Scene();
        if (GUILayout.Button("Lab 4 - Audio Optimization")) GenerateLab4Scene();
        if (GUILayout.Button("Lab 5 - VideoPlayer Basic")) GenerateLab5Scene();
        if (GUILayout.Button("Lab 6 - Video Render Target")) GenerateLab6Scene();
        if (GUILayout.Button("Lab 7 - Video Events")) GenerateLab7Scene();
        if (GUILayout.Button("Mini Project - Intro Cutscene")) GenerateMiniProjectScene();
        
        EditorGUILayout.Space(20);
        GUILayout.Label("Utilities:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Prefabs"))
        {
            CreateAllPrefabs();
        }
        
        if (GUILayout.Button("Create RenderTexture"))
        {
            CreateRenderTexture();
        }
    }
    
    [MenuItem("Tools/LABC4/Generate ALL Labs", false, 100)]
    public static void GenerateAllLabs()
    {
        Debug.Log("[LabSetup] Starting generation of all labs...");
        
        // Ensure directories exist
        EnsureDirectoriesExist();
        
        // Create assets first
        CreateRenderTexture();
        
        // Generate each lab
        GenerateLab1Scene();
        GenerateLab2Scene();
        GenerateLab3Scene();
        GenerateLab4Scene();
        GenerateLab5Scene();
        GenerateLab6Scene();
        GenerateLab7Scene();
        GenerateMiniProjectScene();
        
        // Create a simple gameplay scene for mini project
        GenerateGameplayScene();
        
        Debug.Log("[LabSetup] ✅ All labs generated successfully!");
        EditorUtility.DisplayDialog("Lab Setup Complete", 
            "All 7 labs and Mini Project scenes have been generated!\n\nCheck Assets/Scenes folder.", "OK");
    }
    
    private static void EnsureDirectoriesExist()
    {
        string[] dirs = { SCENES_PATH, PREFABS_PATH, AUDIO_PATH, VIDEO_PATH, MATERIALS_PATH, RENDER_TEXTURES_PATH };
        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        AssetDatabase.Refresh();
    }
    
    #region Lab 1 - AudioSource Basic
    
    [MenuItem("Tools/LABC4/Labs/Lab 1 - AudioSource Basic", false, 1)]
    public static void GenerateLab1Scene()
    {
        string scenePath = $"{SCENES_PATH}/Lab1_AudioSourceBasic.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Get default audio clip
        var audioClip = GetFirstAudioClip();
        
        // Create Audio Source Object
        GameObject audioObj = new GameObject("AudioSourceObject");
        audioObj.transform.position = Vector3.zero;
        
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (audioClip != null)
            audioSource.clip = audioClip;
        
        var controller = audioObj.AddComponent<AudioTriggerController>();
        
        // Create instruction UI
        CreateInstructionCanvas(
            "Lab 1 - AudioSource Basic",
            "Demonstrates basic AudioSource control.\nAudioSource gắn với AudioClip.\nPlay On Awake: OFF",
            "Controls:\nSpace = Play Audio\nS = Stop Audio"
        );
        
        // Create status display
        CreateStatusCanvas("StatusCanvas", new Vector2(1, 1), new Vector2(-20, -20), 
            "Waiting...\n\nPress Space to Play");
        
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[LabSetup] Created: {scenePath}");
    }
    
    #endregion
    
    #region Lab 2 - Spatial Audio
    
    [MenuItem("Tools/LABC4/Labs/Lab 2 - Spatial Audio", false, 2)]
    public static void GenerateLab2Scene()
    {
        string scenePath = $"{SCENES_PATH}/Lab2_SpatialAudio.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Remove default camera
        var mainCam = GameObject.Find("Main Camera");
        if (mainCam != null) Object.DestroyImmediate(mainCam);
        
        // Create Player with AudioListener
        GameObject player = CreatePlayer(new Vector3(0, 1, -5));
        
        // Create Sound Emitter
        GameObject emitter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        emitter.name = "SoundEmitter";
        emitter.transform.position = new Vector3(0, 1, 5);
        emitter.GetComponent<Renderer>().material.color = Color.cyan;
        
        AudioSource emitterAudio = emitter.AddComponent<AudioSource>();
        emitterAudio.loop = true;
        emitterAudio.playOnAwake = true;
        emitterAudio.spatialBlend = 1f; // Start as 3D
        
        var audioClip = GetFirstAudioClip();
        if (audioClip != null)
            emitterAudio.clip = audioClip;
        
        var spatialController = emitter.AddComponent<SpatialAudioController>();
        
        // Create ground
        CreateGround();
        
        // Create instruction UI
        CreateInstructionCanvas(
            "Lab 2 - Spatial Audio (2D vs 3D)",
            "Compare 2D and 3D audio.\nPlayer có AudioListener.\nSoundEmitter có AudioSource.",
            "Controls:\nWASD = Move Player\nMouse = Look Around\nT = Toggle 2D/3D Mode\n\n2D: Same volume everywhere\n3D: Volume changes with distance"
        );
        
        // Create status display for mode
        CreateStatusCanvas("ModeStatus", new Vector2(1, 1), new Vector2(-20, -20),
            "Mode: 3D Spatial\nDistance: 0m");
        
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[LabSetup] Created: {scenePath}");
    }
    
    #endregion
    
    #region Lab 3 - Global Audio Control
    
    [MenuItem("Tools/LABC4/Labs/Lab 3 - Global Audio Control", false, 3)]
    public static void GenerateLab3Scene()
    {
        string scenePath = $"{SCENES_PATH}/Lab3_GlobalAudioControl.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Create Global Audio Controller
        GameObject controller = new GameObject("GlobalAudioController");
        controller.AddComponent<GlobalAudioController>();
        
        // Create multiple audio sources
        var audioClips = GetAllAudioClips();
        
        for (int i = 0; i < Mathf.Min(3, audioClips.Length); i++)
        {
            GameObject audioObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            audioObj.name = $"AudioSource_{i + 1}";
            audioObj.transform.position = new Vector3((i - 1) * 3, 1, 0);
            audioObj.GetComponent<Renderer>().material.color = Color.HSVToRGB(i * 0.3f, 0.7f, 0.9f);
            
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = audioClips[i];
            source.loop = true;
            source.playOnAwake = true;
            source.spatialBlend = 0f; // 2D for clarity
        }
        
        // Create instruction UI
        CreateInstructionCanvas(
            "Lab 3 - Global Audio Control",
            "Control all audio globally via AudioListener.\nMultiple AudioSources đang chạy.",
            "Controls:\nM = Mute/Unmute (AudioListener.volume)\nP = Pause/Resume All (AudioListener.pause)"
        );
        
        // Create audio status UI
        CreateStatusCanvas("AudioStatus", new Vector2(1, 1), new Vector2(-20, -20),
            "🔊 UNMUTED\n▶️ PLAYING\nVolume: 100%");
        
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[LabSetup] Created: {scenePath}");
    }
    
    #endregion
    
    #region Lab 4 - Audio Optimization
    
    [MenuItem("Tools/LABC4/Labs/Lab 4 - Audio Optimization", false, 4)]
    public static void GenerateLab4Scene()
    {
        string scenePath = $"{SCENES_PATH}/Lab4_AudioOptimization.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Create Audio Clip Manager
        GameObject managerObj = new GameObject("AudioClipManager");
        AudioSource source = managerObj.AddComponent<AudioSource>();
        var manager = managerObj.AddComponent<AudioClipManager>();
        
        // Create instruction UI
        CreateInstructionCanvas(
            "Lab 4 - AudioClip Import & Optimization",
            "Learn about different AudioClip configurations.\n\nLoad Type:\n- Decompress On Load (SFX)\n- Compressed In Memory\n- Streaming (BGM)\n\nCompression:\n- PCM (uncompressed)\n- ADPCM (compressed)\n- Vorbis (highly compressed)",
            "Controls:\n1-4 = Switch AudioClips\nSpace = Play/Pause\n\nCheck Inspector for clip info!"
        );
        
        // Create report display
        CreateStatusCanvas("ReportPanel", new Vector2(1, 0.5f), new Vector2(-20, 0),
            "=== Audio Optimization Report ===\n\n" +
            "| Type | Load Type | Compression |\n" +
            "| BGM | Streaming | Vorbis |\n" +
            "| SFX | Decompress | ADPCM |\n" +
            "| Voice | Compressed | Vorbis |",
            400, 300);
        
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[LabSetup] Created: {scenePath}");
    }
    
    #endregion
    
    #region Lab 5 - VideoPlayer Basic
    
    [MenuItem("Tools/LABC4/Labs/Lab 5 - VideoPlayer Basic", false, 5)]
    public static void GenerateLab5Scene()
    {
        string scenePath = $"{SCENES_PATH}/Lab5_VideoPlayerBasic.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Create or get RenderTexture
        RenderTexture rt = GetOrCreateRenderTexture();
        
        // Create Video Player object
        GameObject videoObj = new GameObject("VideoPlayerObject");
        VideoPlayer videoPlayer = videoObj.AddComponent<VideoPlayer>();
        
        var videoClip = GetFirstVideoClip();
        if (videoClip != null)
            videoPlayer.clip = videoClip;
        
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = rt;
        
        // Audio
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        
        var controller = videoObj.AddComponent<VideoTriggerController>();
        
        // Create UI Canvas with fullscreen RawImage for video display
        GameObject uiCanvas = new GameObject("VideoDisplayCanvas");
        Canvas canvas = uiCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1; // Behind other UI
        
        CanvasScaler scaler = uiCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        uiCanvas.AddComponent<GraphicRaycaster>();
        
        // Create fullscreen RawImage for video
        GameObject rawImageObj = new GameObject("VideoRawImage");
        rawImageObj.transform.SetParent(uiCanvas.transform);
        
        RawImage rawImage = rawImageObj.AddComponent<RawImage>();
        rawImage.texture = rt;
        rawImage.color = Color.white;
        
        RectTransform rawRect = rawImage.GetComponent<RectTransform>();
        rawRect.anchorMin = new Vector2(0.1f, 0.15f);
        rawRect.anchorMax = new Vector2(0.9f, 0.85f);
        rawRect.offsetMin = Vector2.zero;
        rawRect.offsetMax = Vector2.zero;
        
        // Create instruction UI
        CreateInstructionCanvas(
            "Lab 5 - VideoPlayer Basic",
            "Basic VideoPlayer control.\nVideo (.mp4) imported và gắn VideoPlayer.\nVideo hiển thị qua RenderTexture + RawImage.",
            "Controls:\nV = Play Video\nSpace = Pause/Resume\nR = Restart"
        );
        
        // Create status display
        CreateStatusCanvas("VideoStatus", new Vector2(1, 1), new Vector2(-20, -20),
            "Video: Loading...\n00:00 / 00:00");
        
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[LabSetup] Created: {scenePath}");
    }
    
    #endregion
    
    #region Lab 6 - Video Render Target
    
    [MenuItem("Tools/LABC4/Labs/Lab 6 - Video Render Target", false, 6)]
    public static void GenerateLab6Scene()
    {
        string scenePath = $"{SCENES_PATH}/Lab6_VideoRenderTarget.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Create or load RenderTexture
        RenderTexture rt = GetOrCreateRenderTexture();
        
        // Create Video Player
        GameObject videoObj = new GameObject("VideoPlayerObject");
        VideoPlayer videoPlayer = videoObj.AddComponent<VideoPlayer>();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = rt;
        videoPlayer.isLooping = true;
        
        var videoClip = GetFirstVideoClip();
        if (videoClip != null)
            videoPlayer.clip = videoClip;
        
        // Create UI Canvas with RawImage
        GameObject uiCanvas = new GameObject("VideoUICanvas");
        Canvas canvas = uiCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiCanvas.AddComponent<CanvasScaler>();
        uiCanvas.AddComponent<GraphicRaycaster>();
        
        GameObject rawImageObj = new GameObject("VideoRawImage");
        rawImageObj.transform.SetParent(uiCanvas.transform);
        RawImage rawImage = rawImageObj.AddComponent<RawImage>();
        rawImage.texture = rt;
        
        RectTransform rawRect = rawImage.GetComponent<RectTransform>();
        rawRect.anchorMin = new Vector2(0.05f, 0.3f);
        rawRect.anchorMax = new Vector2(0.45f, 0.9f);
        rawRect.offsetMin = Vector2.zero;
        rawRect.offsetMax = Vector2.zero;
        
        // Create 3D Display Cube
        GameObject displayCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        displayCube.name = "Video3DCube";
        displayCube.transform.position = new Vector3(3, 1, 0);
        displayCube.transform.localScale = new Vector3(3, 2, 0.1f);
        
        // Create material for cube
        Material videoMat = new Material(Shader.Find("Unlit/Texture"));
        videoMat.mainTexture = rt;
        displayCube.GetComponent<Renderer>().material = videoMat;
        displayCube.SetActive(false); // Start hidden
        
        // Add controller
        var controller = videoObj.AddComponent<VideoRenderTargetController>();
        
        // Store references via SerializedObject would need more setup
        // Controller will find components at runtime
        
        // Create instruction UI
        CreateInstructionCanvas(
            "Lab 6 - Video Render Target",
            "Display video qua RenderTexture.\n\n• RawImage (UI)\n• Material trên 3D Object",
            "Controls:\nTab = Switch Display Mode\nSpace = Play/Pause\n\nMode 1: UI RawImage\nMode 2: 3D Cube with Material"
        );
        
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[LabSetup] Created: {scenePath}");
    }
    
    #endregion
    
    #region Lab 7 - Video Events
    
    [MenuItem("Tools/LABC4/Labs/Lab 7 - Video Events", false, 7)]
    public static void GenerateLab7Scene()
    {
        string scenePath = $"{SCENES_PATH}/Lab7_VideoEvents.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Create or get RenderTexture
        RenderTexture rt = GetOrCreateRenderTexture();
        
        // Create Video Player
        GameObject videoObj = new GameObject("VideoPlayerWithEvents");
        VideoPlayer videoPlayer = videoObj.AddComponent<VideoPlayer>();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = rt;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        
        // Audio
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        
        var videoClip = GetFirstVideoClip();
        if (videoClip != null)
            videoPlayer.clip = videoClip;
        
        var controller = videoObj.AddComponent<VideoEventController>();
        
        // Create UI Canvas with RawImage for video display
        GameObject videoCanvas = new GameObject("VideoDisplayCanvas");
        Canvas vCanvas = videoCanvas.AddComponent<Canvas>();
        vCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        vCanvas.sortingOrder = -1; // Behind other UI
        
        CanvasScaler vScaler = videoCanvas.AddComponent<CanvasScaler>();
        vScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        vScaler.referenceResolution = new Vector2(1920, 1080);
        
        videoCanvas.AddComponent<GraphicRaycaster>();
        
        // Create RawImage for video
        GameObject rawImageObj = new GameObject("VideoRawImage");
        rawImageObj.transform.SetParent(videoCanvas.transform);
        
        RawImage rawImage = rawImageObj.AddComponent<RawImage>();
        rawImage.texture = rt;
        rawImage.color = Color.white;
        
        RectTransform rawRect = rawImage.GetComponent<RectTransform>();
        rawRect.anchorMin = new Vector2(0.1f, 0.15f);
        rawRect.anchorMax = new Vector2(0.9f, 0.85f);
        rawRect.offsetMin = Vector2.zero;
        rawRect.offsetMax = Vector2.zero;
        
        // Create End Panel (hidden by default)
        GameObject endCanvas = new GameObject("VideoEndCanvas");
        Canvas canvas = endCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        endCanvas.AddComponent<CanvasScaler>();
        endCanvas.AddComponent<GraphicRaycaster>();
        
        GameObject endPanel = new GameObject("EndPanel");
        endPanel.transform.SetParent(endCanvas.transform);
        
        RectTransform panelRect = endPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.3f, 0.3f);
        panelRect.anchorMax = new Vector2(0.7f, 0.7f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelBg = endPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // End text
        GameObject endText = new GameObject("EndText");
        endText.transform.SetParent(endPanel.transform);
        
        RectTransform textRect = endText.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);
        
        TextMeshProUGUI tmp = endText.AddComponent<TextMeshProUGUI>();
        tmp.text = "🎬 Video Ended!\n\nThe loopPointReached event was triggered.\n\nPress R to Replay";
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        
        endPanel.SetActive(false);
        
        // Create instruction UI
        CreateInstructionCanvas(
            "Lab 7 - Video Events",
            "Demonstrate VideoPlayer events:\n• prepareCompleted\n• loopPointReached (video end)\nVideo hiển thị qua RenderTexture + RawImage.",
            "Controls:\nV = Play Video\nSpace = Pause/Resume\nR = Restart\nC = Clear Log\n\nWatch the Event Log!"
        );
        
        // Create event log display
        CreateStatusCanvas("EventLog", new Vector2(1, 0.5f), new Vector2(-20, 0),
            "=== Event Log ===\n[Waiting for events...]",
            350, 400);
        
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[LabSetup] Created: {scenePath}");
    }
    
    #endregion
    
    #region Mini Project - Intro Cutscene
    
    [MenuItem("Tools/LABC4/Labs/Mini Project - Intro Cutscene", false, 8)]
    public static void GenerateMiniProjectScene()
    {
        string scenePath = $"{SCENES_PATH}/MiniProject_IntroCutscene.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Create or get RenderTexture
        RenderTexture rt = GetOrCreateRenderTexture();
        
        // Create Intro Manager
        GameObject managerObj = new GameObject("IntroCutsceneManager");
        
        // Create VideoPlayer
        VideoPlayer videoPlayer = managerObj.AddComponent<VideoPlayer>();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = rt;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        
        var videoClip = GetFirstVideoClip();
        if (videoClip != null)
            videoPlayer.clip = videoClip;
        
        // Create BGM AudioSource
        AudioSource bgmSource = managerObj.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0.5f;
        
        var audioClip = GetFirstAudioClip();
        if (audioClip != null)
            bgmSource.clip = audioClip;
        
        // Add IntroCutsceneManager
        var introManager = managerObj.AddComponent<IntroCutsceneManager>();
        
        // Create UI Canvas
        GameObject uiCanvas = new GameObject("IntroCanvas");
        Canvas canvas = uiCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = uiCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        uiCanvas.AddComponent<GraphicRaycaster>();
        
        // Video Display (fullscreen RawImage)
        GameObject rawImageObj = new GameObject("VideoDisplay");
        rawImageObj.transform.SetParent(uiCanvas.transform);
        
        RawImage rawImage = rawImageObj.AddComponent<RawImage>();
        rawImage.texture = rt;
        rawImage.color = Color.white;
        
        RectTransform rawRect = rawImage.GetComponent<RectTransform>();
        rawRect.anchorMin = Vector2.zero;
        rawRect.anchorMax = Vector2.one;
        rawRect.offsetMin = Vector2.zero;
        rawRect.offsetMax = Vector2.zero;
        
        // Fade Overlay
        GameObject fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(uiCanvas.transform);
        
        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 1); // Start black
        
        RectTransform fadeRect = fadeImage.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        
        // Skip Button
        GameObject skipBtn = new GameObject("SkipButton");
        skipBtn.transform.SetParent(uiCanvas.transform);
        
        RectTransform skipRect = skipBtn.AddComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(1, 0);
        skipRect.anchorMax = new Vector2(1, 0);
        skipRect.pivot = new Vector2(1, 0);
        skipRect.anchoredPosition = new Vector2(-30, 30);
        skipRect.sizeDelta = new Vector2(120, 40);
        
        Image skipBg = skipBtn.AddComponent<Image>();
        skipBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        Button skipButton = skipBtn.AddComponent<Button>();
        skipButton.targetGraphic = skipBg;
        
        // Skip button text
        GameObject skipTextObj = new GameObject("Text");
        skipTextObj.transform.SetParent(skipBtn.transform);
        
        RectTransform skipTextRect = skipTextObj.AddComponent<RectTransform>();
        skipTextRect.anchorMin = Vector2.zero;
        skipTextRect.anchorMax = Vector2.one;
        skipTextRect.offsetMin = Vector2.zero;
        skipTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI skipText = skipTextObj.AddComponent<TextMeshProUGUI>();
        skipText.text = "Skip (ESC)";
        skipText.fontSize = 18;
        skipText.color = Color.white;
        skipText.alignment = TextAlignmentOptions.Center;
        
        // Time display
        GameObject timeObj = new GameObject("TimeDisplay");
        timeObj.transform.SetParent(uiCanvas.transform);
        
        RectTransform timeRect = timeObj.AddComponent<RectTransform>();
        timeRect.anchorMin = new Vector2(0, 0);
        timeRect.anchorMax = new Vector2(0, 0);
        timeRect.pivot = new Vector2(0, 0);
        timeRect.anchoredPosition = new Vector2(30, 30);
        timeRect.sizeDelta = new Vector2(200, 50);
        
        TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
        timeText.text = "00:00 / 00:00";
        timeText.fontSize = 20;
        timeText.color = Color.white;
        
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[LabSetup] Created: {scenePath}");
    }
    
    #endregion
    
    #region Gameplay Scene (for Mini Project)
    
    public static void GenerateGameplayScene()
    {
        string scenePath = $"{SCENES_PATH}/Gameplay.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Remove default camera
        var mainCam = GameObject.Find("Main Camera");
        if (mainCam != null) Object.DestroyImmediate(mainCam);
        
        // Create player
        CreatePlayer(new Vector3(0, 1, 0));
        
        // Create ground
        CreateGround();
        
        // Create some objects
        for (int i = 0; i < 5; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Object_{i}";
            cube.transform.position = new Vector3(Random.Range(-10f, 10f), 1, Random.Range(-10f, 10f));
            cube.GetComponent<Renderer>().material.color = Color.HSVToRGB(Random.value, 0.7f, 0.9f);
        }
        
        // Create welcome UI
        CreateInstructionCanvas(
            "🎮 Gameplay Scene",
            "You've completed the intro cutscene!\n\nThis is the main gameplay scene.",
            "Controls:\nWASD = Move\nMouse = Look\nESC = Unlock Cursor"
        );
        
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[LabSetup] Created: {scenePath}");
    }
    
    #endregion
    
    #region Prefab Creation
    
    [MenuItem("Tools/LABC4/Create Prefabs", false, 200)]
    public static void CreateAllPrefabs()
    {
        EnsureDirectoriesExist();
        
        CreatePlayerPrefab();
        CreateSoundEmitterPrefabs();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[LabSetup] All prefabs created!");
    }
    
    private static void CreatePlayerPrefab()
    {
        string prefabPath = $"{PREFABS_PATH}/Player/Player.prefab";
        
        if (File.Exists(prefabPath)) return;
        
        // Create player object hierarchy
        GameObject player = new GameObject("Player");
        
        // Add components
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = new Vector3(0, 1, 0);
        
        player.AddComponent<AudioListener>();
        player.AddComponent<SimplePlayerController>();
        
        // Camera child
        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        camObj.AddComponent<Camera>();
        
        // Save prefab
        Directory.CreateDirectory($"{PREFABS_PATH}/Player");
        PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
        Object.DestroyImmediate(player);
        
        Debug.Log($"[LabSetup] Created prefab: {prefabPath}");
    }
    
    private static void CreateSoundEmitterPrefabs()
    {
        // 2D Emitter
        string path2D = $"{PREFABS_PATH}/Audio/SoundEmitter2D.prefab";
        if (!File.Exists(path2D))
        {
            GameObject emitter2D = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emitter2D.name = "SoundEmitter2D";
            emitter2D.transform.localScale = Vector3.one * 0.5f;
            
            AudioSource source2D = emitter2D.AddComponent<AudioSource>();
            source2D.spatialBlend = 0f;
            source2D.loop = true;
            
            Directory.CreateDirectory($"{PREFABS_PATH}/Audio");
            PrefabUtility.SaveAsPrefabAsset(emitter2D, path2D);
            Object.DestroyImmediate(emitter2D);
        }
        
        // 3D Emitter
        string path3D = $"{PREFABS_PATH}/Audio/SoundEmitter3D.prefab";
        if (!File.Exists(path3D))
        {
            GameObject emitter3D = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emitter3D.name = "SoundEmitter3D";
            emitter3D.transform.localScale = Vector3.one * 0.5f;
            emitter3D.GetComponent<Renderer>().material.color = Color.green;
            
            AudioSource source3D = emitter3D.AddComponent<AudioSource>();
            source3D.spatialBlend = 1f;
            source3D.loop = true;
            source3D.minDistance = 1f;
            source3D.maxDistance = 20f;
            
            emitter3D.AddComponent<SpatialAudioController>();
            
            PrefabUtility.SaveAsPrefabAsset(emitter3D, path3D);
            Object.DestroyImmediate(emitter3D);
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    private static GameObject CreatePlayer(Vector3 position)
    {
        GameObject player = new GameObject("Player");
        player.transform.position = position;
        
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = new Vector3(0, 1, 0);
        
        player.AddComponent<AudioListener>();
        player.AddComponent<SimplePlayerController>();
        
        // Camera
        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        Camera cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.1f;
        
        return player;
    }
    
    private static void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(5, 1, 5);
        ground.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
    }
    
    private static void CreateInstructionCanvas(string title, string description, string controls)
    {
        GameObject canvasObj = new GameObject("InstructionCanvas");
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Panel
        GameObject panelObj = new GameObject("InstructionPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -20);
        panelRect.sizeDelta = new Vector2(450, 350);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.75f);
        
        // Title
        CreateUIText(panelObj.transform, "Title", title, 28, FontStyles.Bold,
            new Vector2(0, 1), new Vector2(20, -15), new Vector2(410, 50), TextAlignmentOptions.TopLeft);
        
        // Description
        CreateUIText(panelObj.transform, "Description", description, 16, FontStyles.Normal,
            new Vector2(0, 1), new Vector2(20, -60), new Vector2(410, 150), TextAlignmentOptions.TopLeft);
        
        // Controls
        CreateUIText(panelObj.transform, "Controls", controls, 14, FontStyles.Normal,
            new Vector2(0, 1), new Vector2(20, -200), new Vector2(410, 140), TextAlignmentOptions.TopLeft);
        
        // Toggle hint
        CreateUIText(panelObj.transform, "ToggleHint", "Press H to hide/show", 12, FontStyles.Italic,
            new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(200, 30), TextAlignmentOptions.Center);
        
        // Add toggle script
        var labUI = canvasObj.AddComponent<LabInstructionUI>();
    }
    
    private static void CreateStatusCanvas(string name, Vector2 anchor, Vector2 position, string text, 
        int width = 250, int height = 150)
    {
        GameObject canvasObj = new GameObject(name);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 49;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Panel
        GameObject panelObj = new GameObject("StatusPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = anchor;
        panelRect.anchorMax = anchor;
        panelRect.pivot = anchor;
        panelRect.anchoredPosition = position;
        panelRect.sizeDelta = new Vector2(width, height);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.6f);
        
        // Text
        GameObject textObj = new GameObject("StatusText");
        textObj.transform.SetParent(panelObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 10);
        textRect.offsetMax = new Vector2(-15, -10);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;
    }
    
    private static void CreateUIText(Transform parent, string name, string text, int fontSize, FontStyles style,
        Vector2 anchor, Vector2 position, Vector2 size, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.alignment = alignment;
    }
    
    private static AudioClip GetFirstAudioClip()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        return null;
    }
    
    private static AudioClip[] GetAllAudioClips()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
        AudioClip[] clips = new AudioClip[guids.Length];
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        
        return clips;
    }
    
    private static VideoClip GetFirstVideoClip()
    {
        string[] guids = AssetDatabase.FindAssets("t:VideoClip", new[] { "Assets/Video" });
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<VideoClip>(path);
        }
        return null;
    }
    
    [MenuItem("Tools/LABC4/Create RenderTexture", false, 201)]
    public static RenderTexture CreateRenderTexture()
    {
        string rtPath = $"{RENDER_TEXTURES_PATH}/VideoRenderTexture.renderTexture";
        
        if (!Directory.Exists(RENDER_TEXTURES_PATH))
        {
            Directory.CreateDirectory(RENDER_TEXTURES_PATH);
        }
        
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
        
        if (rt == null)
        {
            rt = new RenderTexture(1920, 1080, 0);
            rt.name = "VideoRenderTexture";
            AssetDatabase.CreateAsset(rt, rtPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LabSetup] Created RenderTexture: {rtPath}");
        }
        
        return rt;
    }
    
    private static RenderTexture GetOrCreateRenderTexture()
    {
        string rtPath = $"{RENDER_TEXTURES_PATH}/VideoRenderTexture.renderTexture";
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
        
        if (rt == null)
        {
            rt = CreateRenderTexture();
        }
        
        return rt;
    }
    
    #endregion
}
