using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;

// Nakładka diagnostyczna do pomiaru wydajności.
// Umieść na dowolnym GO w scenie. Nie wymaga referencji w Inspektorze.
//
// Sterowanie:
//   F3 — pokaż/ukryj overlay
//   B  — start/stop benchmark → eksport CSV do persistentDataPath
//
// Kolor FPS: zielony ≥60, żółty ≥30, czerwony <30
public class PerformanceMonitor : MonoBehaviour
{
    [SerializeField] private int  avgWindow      = 60;   // liczba klatek do uśredniania FPS
    [SerializeField] private bool visibleOnStart = true;

    // ProfilerRecorder działa zarówno w Editorze jak i w buildach (Unity.Profiling)
    private ProfilerRecorder _drawCallsRecorder;
    private ProfilerRecorder _setPassRecorder;
    private ProfilerRecorder _batchesRecorder;
    private ProfilerRecorder _trianglesRecorder;
    private ProfilerRecorder _gcAllocRecorder;

    // FPS
    private readonly Queue<float> _frameTimeQueue = new();
    private float _avgFps;
    private float _minFps = float.MaxValue;
    private float _maxFps;

    // Benchmark
    private bool              _recording;
    private float             _recordStart;
    private List<FrameSample> _samples = new();

    private bool     _overlayVisible;
    private GUIStyle _style;

    private struct FrameSample
    {
        public float timeS;
        public float avgFps;        // uśredniony po oknie avgWindow — statystycznie użyteczny
        public float frameMs;
        public long  drawCalls;
        public long  setPassCalls;
        public long  batches;
        public long  triangles;
        public long  gcAllocBytes;
        public long  memAllocMB;
    }

    private void Awake() => _overlayVisible = visibleOnStart;

    private void OnEnable()
    {
        _drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        _setPassRecorder   = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        _batchesRecorder   = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        _trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        _gcAllocRecorder   = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");

        Debug.Log($"[PerfMonitor] Counters valid: Draw={_drawCallsRecorder.Valid} " +
                  $"SetPass={_setPassRecorder.Valid} Batches={_batchesRecorder.Valid} " +
                  $"Tris={_trianglesRecorder.Valid} GC={_gcAllocRecorder.Valid}");
    }

    private void OnDisable()
    {
        _drawCallsRecorder.Dispose();
        _setPassRecorder.Dispose();
        _batchesRecorder.Dispose();
        _trianglesRecorder.Dispose();
        _gcAllocRecorder.Dispose();
    }

    private void Update()
    {
        float dt  = Time.unscaledDeltaTime;
        float fps = dt > 0f ? 1f / dt : 0f;

        // Uśrednianie FPS po oknie avgWindow klatek
        _frameTimeQueue.Enqueue(dt);
        if (_frameTimeQueue.Count > avgWindow) _frameTimeQueue.Dequeue();

        float sum = 0f;
        foreach (var t in _frameTimeQueue) sum += t;
        _avgFps = _frameTimeQueue.Count / sum;

        if (fps < _minFps) _minFps = fps;
        if (fps > _maxFps) _maxFps = fps;

        HandleInput();

        if (!_recording) return;

        _samples.Add(new FrameSample
        {
            timeS        = Time.time - _recordStart,
            avgFps       = _avgFps,
            frameMs      = dt * 1000f,
            drawCalls    = _drawCallsRecorder.Valid ? _drawCallsRecorder.LastValue : -1,
            setPassCalls = _setPassRecorder.Valid   ? _setPassRecorder.LastValue   : -1,
            batches      = _batchesRecorder.Valid   ? _batchesRecorder.LastValue   : -1,
            triangles    = _trianglesRecorder.Valid ? _trianglesRecorder.LastValue : -1,
            gcAllocBytes = _gcAllocRecorder.Valid   ? _gcAllocRecorder.LastValue   : -1,
            memAllocMB   = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024)
        });
    }

    private void HandleInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f3Key.wasPressedThisFrame)
            _overlayVisible = !_overlayVisible;

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            if (_recording) StopAndExport();
            else            StartBenchmark();
        }
    }

    // --- Public API (można wywołać z kodu np. z BenchmarkScene) ---

    public void StartBenchmark()
    {
        _samples.Clear();
        _minFps      = float.MaxValue;
        _maxFps      = 0f;
        _recordStart = Time.time;
        _recording   = true;
        Debug.Log("[PerfMonitor] Benchmark start");
    }

    public void StopAndExport()
    {
        _recording = false;
        ExportCSV();
    }

    // --- Eksport ---

    private void ExportCSV()
    {
        if (_samples.Count == 0) return;

        var sb = new StringBuilder();
        var ic = CultureInfo.InvariantCulture;
        sb.AppendLine("Time(s),AvgFPS,FrameTime(ms),DrawCalls,SetPassCalls,Batches,Triangles,GCAlloc(B),Memory(MB)");
        foreach (var s in _samples)
            sb.AppendLine(string.Format(ic,
                "{0:F3},{1:F1},{2:F2},{3},{4},{5},{6},{7},{8}",
                s.timeS, s.avgFps, s.frameMs,
                s.drawCalls, s.setPassCalls, s.batches, s.triangles,
                s.gcAllocBytes, s.memAllocMB));

        string path = Path.Combine(Application.persistentDataPath,
                                   $"perf_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"[PerfMonitor] {_samples.Count} klatek → {path}");
    }

    // --- Overlay ---

    private void OnGUI()
    {
        if (!_overlayVisible) return;

        if (_style == null)
            _style = new GUIStyle(GUI.skin.box) { fontSize = 13, alignment = TextAnchor.UpperLeft, padding = new RectOffset(8, 8, 6, 6) };

        long draws   = _drawCallsRecorder.Valid ? _drawCallsRecorder.LastValue : -1;
        long setPass = _setPassRecorder.Valid   ? _setPassRecorder.LastValue   : -1;
        long batches = _batchesRecorder.Valid   ? _batchesRecorder.LastValue   : -1;
        long gc      = _gcAllocRecorder.Valid   ? _gcAllocRecorder.LastValue   : -1;
        long memMB   = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);

        _style.normal.textColor = _avgFps >= 60f ? Color.green
                                : _avgFps >= 30f ? Color.yellow
                                : Color.red;

        string label =
            $"FPS: {_avgFps:F0}   min {_minFps:F0} / max {_maxFps:F0}\n" +
            $"Frame:       {Time.unscaledDeltaTime * 1000f:F2} ms\n" +
            $"DrawCalls:   {draws}\n" +
            $"SetPass:     {setPass}\n" +
            $"Batches:     {batches}\n" +
            $"GC/frame:    {gc} B\n" +
            $"Memory:      {memMB} MB\n" +
            "\n" +
            (_recording
                ? "● REC  [B] stop + export CSV"
                : "[F3] toggle   [B] start benchmark");

        GUI.Label(new Rect(10, 10, 290, 185), label, _style);
    }
}
