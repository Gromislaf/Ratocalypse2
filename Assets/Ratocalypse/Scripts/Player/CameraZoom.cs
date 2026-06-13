using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

// Zoom scrollem dla kamery izometrycznej.
// Umieść na tym samym obiekcie co CinemachineCamera i CinemachineFollow.
[RequireComponent(typeof(CinemachineFollow))]
public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    private CinemachineFollow cinemachineFollow;
    private Vector3 baseOffset;
    private float currentZoom;

    private void Awake()
    {
        cinemachineFollow = GetComponent<CinemachineFollow>();
        baseOffset = cinemachineFollow.FollowOffset;
        currentZoom = baseOffset.magnitude;
    }

    private void Update()
    {
        // Scroll wheel zwraca ~120 na notch — normalizujemy do rozsądnego float
        float scroll = Mouse.current.scroll.ReadValue().y * 0.01f;
        if (Mathf.Abs(scroll) < 0.001f) return;

        currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);
        cinemachineFollow.FollowOffset = baseOffset.normalized * currentZoom;
    }
}
