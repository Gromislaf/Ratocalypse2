using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

// Nagrywa ground-clicki gracza do JSON, który BenchmarkRunner odtwarza.
// Umieść na dowolnym GO w scenie Building. Nie nagrywa klików w wrogów/interactable
// — benchmark testuje tylko ruch po trasie + reakcję AI, nie walkę.
//
// Sterowanie:
//   F4 — start/stop recording → eksport do persistentDataPath/benchmark_input.json
public class BenchmarkInputRecorder : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private string    outputFilename = "benchmark_input.json";

    [System.Serializable]
    public class MoveEvent
    {
        public float t;
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class Recording
    {
        public float           duration;
        public float           startX;
        public float           startY;
        public float           startZ;
        public List<MoveEvent> events = new();

        public Vector3 GetPlayerStartPos() => new(startX, startY, startZ);
    }

    private Camera    _cam;
    private Recording _recording;
    private float     _recordStart;
    private bool      _isRecording;

    private void Awake() => _cam = Camera.main;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f4Key.wasPressedThisFrame)
        {
            if (_isRecording) StopAndExport();
            else              StartRecording();
        }

        if (!_isRecording) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer)) return;

        _recording.events.Add(new MoveEvent
        {
            t = Time.time - _recordStart,
            x = hit.point.x,
            y = hit.point.y,
            z = hit.point.z
        });
    }

    private void StartRecording()
    {
        var player = FindFirstObjectByType<PlayerController>();
        Vector3 pos = player != null ? player.transform.position : Vector3.zero;
        _recording = new Recording
        {
            startX = pos.x,
            startY = pos.y,
            startZ = pos.z
        };
        _recordStart = Time.time;
        _isRecording = true;
        Debug.Log("[BenchmarkRecorder] Recording start — kliknij po trasie, F4 by zakończyć");
    }

    private void StopAndExport()
    {
        _isRecording = false;
        _recording.duration = Time.time - _recordStart;

        string path = Path.Combine(Application.persistentDataPath, outputFilename);
        File.WriteAllText(path, JsonConvert.SerializeObject(_recording, Formatting.Indented));
        Debug.Log($"[BenchmarkRecorder] {_recording.events.Count} klików, {_recording.duration:F1}s → {path}");
    }
}
