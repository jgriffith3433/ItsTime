using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Update frequency in seconds. Lower = more frequent updates (more expensive)")]
    public float updateInterval = 0.5f;
    
    [Tooltip("Position of the display on screen")]
    public TextAnchor screenPosition = TextAnchor.UpperLeft;
    
    [Tooltip("Font size for the display")]
    public int fontSize = 14;
    
    [Tooltip("Background color of the display panel")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    
    [Tooltip("Text color")]
    public Color textColor = Color.white;
    
    [Header("Display Options")]
    [Tooltip("Show FPS (Frames Per Second)")]
    public bool showFPS = true;
    
    [Tooltip("Show draw calls and batches")]
    public bool showDrawCalls = true;
    
    [Tooltip("Show memory usage")]
    public bool showMemory = true;
    
    [Tooltip("Show screen resolution")]
    public bool showResolution = true;
    
    // Internal state
    private float lastUpdateTime;
    private Canvas canvas;
    private GameObject uiPanel;
    private TextMeshProUGUI statsText;
    
    // FPS tracking
    private float deltaTime = 0.0f;
    private float fps = 0.0f;
    private int frameCount = 0;
    private float fpsTimer = 0.0f;
    
    // Stats
    private int setPassCalls;
    private int drawCalls;
    private int batches;
    private float totalMemoryMB;
    private float usedMemoryMB;

    void Start()
    {
        CreateUI();
        UpdateStats();
    }

    void Update()
    {
        // Update FPS counter every frame for smooth readings
        if (showFPS)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            frameCount++;
            fpsTimer += Time.unscaledDeltaTime;
            
            if (fpsTimer >= 0.25f) // Update FPS display 4 times per second
            {
                fps = frameCount / fpsTimer;
                frameCount = 0;
                fpsTimer = 0.0f;
            }
        }
        
        // Update other stats at specified interval
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateStats();
            lastUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// Creates the UI elements for displaying poly count
    /// </summary>
    void CreateUI()
    {
        // Create Canvas if it doesn't exist
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PolyCountCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create panel
        uiPanel = new GameObject("PolyCountPanel");
        uiPanel.transform.SetParent(canvas.transform, false);
        
        Image panelImage = uiPanel.AddComponent<Image>();
        panelImage.color = backgroundColor;
        
        RectTransform panelRect = uiPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(320, CalculatePanelHeight());
        
        // Position based on screen position setting
        PositionPanel(panelRect);
        
        // Create text
        GameObject textObj = new GameObject("StatsText");
        textObj.transform.SetParent(uiPanel.transform, false);
        
        statsText = textObj.AddComponent<TextMeshProUGUI>();
        statsText.fontSize = fontSize;
        statsText.color = textColor;
        statsText.alignment = TextAlignmentOptions.TopLeft;
        statsText.enableWordWrapping = false;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
    }

    /// <summary>
    /// Calculates the panel height based on enabled options
    /// </summary>
    int CalculatePanelHeight()
    {
        int baseHeight = 40; // Base padding
        int lineHeight = 20; // Height per line
        int lines = 1; // Title
        
        if (showFPS) lines += 1;
        if (showDrawCalls) lines += 3; // SetPass, Draw Calls, Batches
        if (showMemory) lines += 2; // Total and Used memory
        if (showResolution) lines += 1;
        
        return baseHeight + (lines * lineHeight);
    }

    /// <summary>
    /// Positions the panel based on the screen position setting
    /// </summary>
    void PositionPanel(RectTransform panelRect)
    {
        switch (screenPosition)
        {
            case TextAnchor.UpperLeft:
                panelRect.anchorMin = new Vector2(0, 1);
                panelRect.anchorMax = new Vector2(0, 1);
                panelRect.pivot = new Vector2(0, 1);
                panelRect.anchoredPosition = new Vector2(10, -10);
                break;
            case TextAnchor.UpperCenter:
                panelRect.anchorMin = new Vector2(0.5f, 1);
                panelRect.anchorMax = new Vector2(0.5f, 1);
                panelRect.pivot = new Vector2(0.5f, 1);
                panelRect.anchoredPosition = new Vector2(0, -10);
                break;
            case TextAnchor.UpperRight:
                panelRect.anchorMin = new Vector2(1, 1);
                panelRect.anchorMax = new Vector2(1, 1);
                panelRect.pivot = new Vector2(1, 1);
                panelRect.anchoredPosition = new Vector2(-10, -10);
                break;
            case TextAnchor.MiddleLeft:
                panelRect.anchorMin = new Vector2(0, 0.5f);
                panelRect.anchorMax = new Vector2(0, 0.5f);
                panelRect.pivot = new Vector2(0, 0.5f);
                panelRect.anchoredPosition = new Vector2(10, 0);
                break;
            case TextAnchor.MiddleCenter:
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
                break;
            case TextAnchor.MiddleRight:
                panelRect.anchorMin = new Vector2(1, 0.5f);
                panelRect.anchorMax = new Vector2(1, 0.5f);
                panelRect.pivot = new Vector2(1, 0.5f);
                panelRect.anchoredPosition = new Vector2(-10, 0);
                break;
            case TextAnchor.LowerLeft:
                panelRect.anchorMin = new Vector2(0, 0);
                panelRect.anchorMax = new Vector2(0, 0);
                panelRect.pivot = new Vector2(0, 0);
                panelRect.anchoredPosition = new Vector2(10, 10);
                break;
            case TextAnchor.LowerCenter:
                panelRect.anchorMin = new Vector2(0.5f, 0);
                panelRect.anchorMax = new Vector2(0.5f, 0);
                panelRect.pivot = new Vector2(0.5f, 0);
                panelRect.anchoredPosition = new Vector2(0, 10);
                break;
            case TextAnchor.LowerRight:
                panelRect.anchorMin = new Vector2(1, 0);
                panelRect.anchorMax = new Vector2(1, 0);
                panelRect.pivot = new Vector2(1, 0);
                panelRect.anchoredPosition = new Vector2(-10, 10);
                break;
        }
    }

    /// <summary>
    /// Updates the statistics
    /// </summary>
    void UpdateStats()
    {
        // Draw calls and batches (from Unity Stats)
        if (showDrawCalls)
        {
            #if UNITY_EDITOR
            setPassCalls = UnityEditor.UnityStats.setPassCalls;
            drawCalls = UnityEditor.UnityStats.drawCalls;
            batches = UnityEditor.UnityStats.batches;
            #else
            // In build, we can't access UnityStats, so we'll use estimates
            setPassCalls = -1;
            drawCalls = -1;
            batches = -1;
            #endif
        }

        // Memory usage
        if (showMemory)
        {
            totalMemoryMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
            usedMemoryMB = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1048576f;
        }

        UpdateDisplayText();
    }

    /// <summary>
    /// Updates the display text with current stats
    /// </summary>
    void UpdateDisplayText()
    {
        if (statsText == null) return;

        string text = "<b>Performance Stats</b>\n";
        
        // FPS
        if (showFPS)
        {
            Color fpsColor = GetFPSColor(fps);
            text += $"<color=#{ColorUtility.ToHtmlStringRGB(fpsColor)}>FPS: {fps:F1}</color>\n";
        }
        
        // Draw calls
        if (showDrawCalls)
        {
            #if UNITY_EDITOR
            text += $"SetPass Calls: {setPassCalls}\n";
            text += $"Draw Calls: {drawCalls}\n";
            text += $"Batches: {batches}\n";
            #else
            text += "Draw Calls: N/A (Build)\n";
            #endif
        }
        
        // Memory
        if (showMemory)
        {
            text += $"Allocated: {totalMemoryMB:F1} MB\n";
            text += $"Reserved: {usedMemoryMB:F1} MB\n";
        }
        
        // Resolution
        if (showResolution)
        {
            text += $"Resolution: {Screen.width}x{Screen.height}\n";
        }

        statsText.text = text.TrimEnd('\n');
        
        // Adjust panel height based on content
        RectTransform panelRect = uiPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(320, CalculatePanelHeight());
    }
    
    /// <summary>
    /// Returns a color based on FPS performance
    /// </summary>
    Color GetFPSColor(float currentFPS)
    {
        if (currentFPS >= 55f) return Color.green;
        if (currentFPS >= 30f) return Color.yellow;
        return Color.red;
    }

    /// <summary>
    /// Formats large numbers with commas for readability
    /// </summary>
    string FormatNumber(int number)
    {
        return number.ToString("N0");
    }

    /// <summary>
    /// Public method to manually force an update
    /// </summary>
    public void ForceUpdate()
    {
        UpdateStats();
    }

    /// <summary>
    /// Allows runtime repositioning of the display
    /// </summary>
    public void SetPosition(TextAnchor newPosition)
    {
        screenPosition = newPosition;
        if (uiPanel != null)
        {
            PositionPanel(uiPanel.GetComponent<RectTransform>());
        }
    }

    /// <summary>
    /// Toggle visibility of the display
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(visible);
        }
    }

    void OnDestroy()
    {
        // Clean up UI elements when destroyed
        if (uiPanel != null)
        {
            Destroy(uiPanel);
        }
    }
}
