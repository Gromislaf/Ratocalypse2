using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set; }

    [Header("Minimap bounds (world space XZ)")]
    [SerializeField] private Vector2 worldMin = new(-50f, -50f);
    [SerializeField] private Vector2 worldMax = new( 50f,  50f);

    [Header("Referencje UI")]
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private RectTransform markersParent;

    [Header("Ikony markerów")]
    [SerializeField] private Sprite playerIcon;
    [SerializeField] private Sprite npcIcon;
    [SerializeField] private Sprite enemyIcon;
    [SerializeField] private Sprite questItemIcon;

    [Header("Kolory markerów")]
    [SerializeField] private Color playerColor   = Color.white;
    [SerializeField] private Color npcColor      = Color.yellow;
    [SerializeField] private Color enemyColor    = Color.red;
    [SerializeField] private Color questItemColor = new Color(1f, 0.8f, 0f);

    [Header("Obszar questa")]
    [SerializeField] private RectTransform questAreaCircle;

    private readonly List<MinimapMarker> markers      = new();
    private readonly Dictionary<MinimapMarker, RectTransform> markerIcons = new();

    private const float MarkerSize = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnQuestAccepted>(OnQuestAccepted);
        EventBus.Subscribe<OnQuestCompleted>(OnQuestCompleted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnQuestAccepted>(OnQuestAccepted);
        EventBus.Unsubscribe<OnQuestCompleted>(OnQuestCompleted);
    }

    private void Start()
    {
        if (questAreaCircle != null) questAreaCircle.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        foreach (var marker in markers)
        {
            if (marker == null) continue;
            if (!markerIcons.TryGetValue(marker, out var icon)) continue;
            icon.anchoredPosition = WorldToMinimap(marker.transform.position);
        }
    }

    // ---- Markery ------------------------------------------------

    public void RegisterMarker(MinimapMarker marker)
    {
        if (markers.Contains(marker)) return;
        markers.Add(marker);

        var go = new GameObject(marker.markerType.ToString(), typeof(Image));
        go.transform.SetParent(markersParent, false);

        var img = go.GetComponent<Image>();
        img.sprite = GetSprite(marker.markerType);
        img.color  = GetColor(marker.markerType);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = Vector2.one * MarkerSize;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot     = Vector2.one * 0.5f;

        markerIcons[marker] = rt;
    }

    public void UnregisterMarker(MinimapMarker marker)
    {
        markers.Remove(marker);
        if (markerIcons.TryGetValue(marker, out var icon))
        {
            if (icon != null) Destroy(icon.gameObject);
            markerIcons.Remove(marker);
        }
    }

    // ---- Quest area circle --------------------------------------

    private void OnQuestAccepted(OnQuestAccepted e)
    {
        if (questAreaCircle == null)
        {
            Debug.LogWarning("[MinimapController] questAreaCircle nie jest przypisany w Inspektorze!");
            return;
        }

        questAreaCircle.anchorMin = Vector2.zero;
        questAreaCircle.anchorMax = Vector2.zero;
        questAreaCircle.pivot     = Vector2.one * 0.5f;
        questAreaCircle.gameObject.SetActive(true);
        questAreaCircle.anchoredPosition = WorldToMinimap(e.SearchAreaCenter);

        float mapWidth  = worldMax.x - worldMin.x;
        float mapHeight = worldMax.y - worldMin.y;
        float pixelsPerUnitX = minimapRect.rect.width  / mapWidth;
        float pixelsPerUnitY = minimapRect.rect.height / mapHeight;
        float pixelRadius = e.SearchAreaRadius * Mathf.Min(pixelsPerUnitX, pixelsPerUnitY);
        questAreaCircle.sizeDelta = Vector2.one * pixelRadius * 2f;
    }

    private void OnQuestCompleted(OnQuestCompleted _)
    {
        if (questAreaCircle != null) questAreaCircle.gameObject.SetActive(false);
    }

    // ---- Helpers ------------------------------------------------

    private Vector2 WorldToMinimap(Vector3 worldPos)
    {
        float nx = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
        float ny = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.z);
        return new Vector2(
            nx * minimapRect.rect.width,
            ny * minimapRect.rect.height);
    }

    private Sprite GetSprite(MarkerType type) => type switch
    {
        MarkerType.Player    => playerIcon,
        MarkerType.NPC       => npcIcon,
        MarkerType.Enemy     => enemyIcon,
        MarkerType.QuestItem => questItemIcon,
        _                    => null
    };

    private Color GetColor(MarkerType type) => type switch
    {
        MarkerType.Player    => playerColor,
        MarkerType.NPC       => npcColor,
        MarkerType.Enemy     => enemyColor,
        MarkerType.QuestItem => questItemColor,
        _                    => Color.white
    };
}
