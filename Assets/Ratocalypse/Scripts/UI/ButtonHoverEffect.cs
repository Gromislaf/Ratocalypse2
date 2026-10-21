using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform target;

    [Header("Scale")]
    public float normalScale = 1f;
    public float hoverScale = 1.08f;
    public float pressScale = 0.95f;
    public float speed = 12f;

    [Header("Color (TMP)")]
    public TextMeshProUGUI text;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.8f, 0.2f); // z³oty AAA

    private Vector3 targetScale;
    private Color targetTextColor;

    void Start()
    {
        targetScale = Vector3.one * normalScale;
        if (text != null) targetTextColor = normalColor;
    }

    void Update()
    {
        target.localScale = Vector3.Lerp(target.localScale, targetScale, Time.deltaTime * speed);

        if (text != null)
        {
            text.color = Color.Lerp(text.color, targetTextColor, Time.deltaTime * speed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
        if (text != null) targetTextColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one * normalScale;
        if (text != null) targetTextColor = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = Vector3.one * pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
    }
}