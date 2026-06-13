using System.Collections.Generic;
using UnityEngine;

// Umieść na Main Camera.
// Layer "Buildings" ustaw na modułach z colliderami (dzieci budynku).
// Ukrywa renderer I collider — gracz może klikać na ziemię pod niewidocznymi ścianami.
public class OcclusionFader : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask buildingLayer;

    [Tooltip("Promień XZ wokół gracza — szukamy budynków w tym zasięgu")]
    [SerializeField] private float occlusionRadiusXZ = 5f;

    [Tooltip("Minimalna wysokość nad graczem żeby element budynku zniknął")]
    [SerializeField] private float hideAbovePlayerY = 0.5f;

    [Tooltip("Kąt ukrycia (dot product XZ): 1.0 = tylko ściany dokładnie za graczem, 0.0 = ściany w półkolu od strony kamery, -1.0 = wszystkie strony")]
    [SerializeField, Range(-1f, 1f)] private float occlusionDotThreshold = 0f;

    [Tooltip("Loguje ukrywane obiekty — wyłącz w produkcji")]
    [SerializeField] private bool debugMode = false;

    private readonly struct HiddenEntry
    {
        public readonly Collider col;
        public readonly Vector3 center;
        public HiddenEntry(Collider col, Vector3 center) { this.col = col; this.center = center; }
    }

    private readonly Dictionary<Renderer, HiddenEntry> occludedThisFrame = new();
    private readonly Dictionary<Renderer, HiddenEntry> hiddenObjects = new();
    private readonly List<Renderer> toRestore = new();

    // Obliczany raz na LateUpdate, używany też w ShouldHide
    private Vector2 camDirXZ;

    private void LateUpdate()
    {
        if (player == null) return;

        occludedThisFrame.Clear();

        // Kierunek od gracza do kamery w płaszczyźnie XZ (raz na klatkę)
        Vector2 camToPlayerXZ = new Vector2(
            transform.position.x - player.position.x,
            transform.position.z - player.position.z
        );
        camDirXZ = camToPlayerXZ.sqrMagnitude > 0.001f ? camToPlayerXZ.normalized : Vector2.up;

        // Krok 1: szukaj nowych obiektów (tylko WŁĄCZONE collidery widoczne dla OverlapSphere)
        Collider[] nearby = Physics.OverlapSphere(player.position, occlusionRadiusXZ, buildingLayer);
        foreach (var col in nearby)
        {
            Vector3 center = col.bounds.center;

            if (!PassesFilters(center)) continue;

            Renderer rend = col.GetComponent<Renderer>();
            if (rend == null) continue;

            occludedThisFrame[rend] = new HiddenEntry(col, center);
        }

        // Krok 2: już ukryte obiekty mają wyłączony collider — używamy cachedCenter
        foreach (var (rend, entry) in hiddenObjects)
        {
            if (rend == null || entry.col == null) continue;
            if (occludedThisFrame.ContainsKey(rend)) continue;

            if (PassesFilters(entry.center))
                occludedThisFrame[rend] = entry;
        }

        // Ukryj nowe
        foreach (var (rend, entry) in occludedThisFrame)
        {
            if (rend != null && rend.enabled)
            {
                rend.enabled = false;
                entry.col.enabled = false;
                hiddenObjects[rend] = entry;

                if (debugMode)
                    Debug.Log($"[OcclusionFader] UKRYJ: {entry.col.name} | center: {entry.center:F2}");
            }
        }

        // Przywróć te które wyszły poza zasięg lub zmieniły kierunek
        toRestore.Clear();
        foreach (var rend in hiddenObjects.Keys)
        {
            if (!occludedThisFrame.ContainsKey(rend))
                toRestore.Add(rend);
        }
        foreach (var rend in toRestore)
        {
            if (hiddenObjects.TryGetValue(rend, out var entry))
            {
                if (rend != null) rend.enabled = true;
                if (entry.col != null) entry.col.enabled = true;
            }
            hiddenObjects.Remove(rend);
        }
    }

    private bool PassesFilters(Vector3 center)
    {
        // Filtr wysokości
        if (center.y < player.position.y + hideAbovePlayerY) return false;

        // Filtr odległości XZ
        Vector2 centerXZ = new Vector2(center.x, center.z);
        Vector2 playerXZ = new Vector2(player.position.x, player.position.z);
        if (Vector2.Distance(centerXZ, playerXZ) > occlusionRadiusXZ) return false;

        // Filtr kierunku: ukryj tylko ściany po stronie kamery
        // dot > threshold oznacza że ściana jest w kierunku kamery od gracza
        Vector2 toWallXZ = (centerXZ - playerXZ);
        if (toWallXZ.sqrMagnitude > 0.001f)
        {
            float dot = Vector2.Dot(camDirXZ, toWallXZ.normalized);
            if (dot < occlusionDotThreshold) return false;
        }

        return true;
    }

    private void OnDisable()
    {
        foreach (var (rend, entry) in hiddenObjects)
        {
            if (rend != null) rend.enabled = true;
            if (entry.col != null) entry.col.enabled = true;
        }
        hiddenObjects.Clear();
    }
}
