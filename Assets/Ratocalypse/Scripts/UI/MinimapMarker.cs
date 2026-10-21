using UnityEngine;

public enum MarkerType { Player, NPC, Enemy, QuestItem }

public class MinimapMarker : MonoBehaviour
{
    [SerializeField] public MarkerType markerType;

    private void OnEnable()  => MinimapController.Instance?.RegisterMarker(this);
    private void OnDisable() => MinimapController.Instance?.UnregisterMarker(this);
}
