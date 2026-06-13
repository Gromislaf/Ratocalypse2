using UnityEngine;
using UnityEngine.InputSystem;

// Prosta kamera izometryczna z follow i zoom scrollem.
// Nie używa Cinemachine celowo — stały kąt isometric daje pełną kontrolę.
// Cinemachine zostaje do cutscen (Timeline).
public class CameraController : MonoBehaviour
{
    [Header("Cel")]
    [SerializeField] private Transform target;

    [Header("Pozycja")]
    [Tooltip("Offset od gracza — definiuje kąt i wysokość kamery izometrycznej")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
    [SerializeField] private float followSpeed = 8f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    private float currentZoom;

    private void Awake()
    {
        currentZoom = offset.magnitude;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        HandleZoom();
        Follow();
    }

    private void HandleZoom()
    {
        // Scroll wheel w Unity zwraca ~120 na notch — dzielimy przez 100 żeby dostać rozsądny float
        float scroll = Mouse.current.scroll.ReadValue().y * 0.01f;
        if (Mathf.Abs(scroll) < 0.001f) return;

        currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);
    }

    private void Follow()
    {
        Vector3 desiredPos = target.position + offset.normalized * currentZoom;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        transform.LookAt(target.position);
    }

    /// <summary>Zmiana celu w locie — np. po wczytaniu save'a lub zmianie sceny.</summary>
    public void SetTarget(Transform newTarget) => target = newTarget;
}
