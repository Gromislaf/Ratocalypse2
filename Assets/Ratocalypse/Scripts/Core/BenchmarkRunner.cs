using System.Collections;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

// Orkiestrator deterministycznego benchmarku do pracy inżynierskiej.
//
// Workflow:
//   1. Spawn N wrogów w siatce wokół środka
//   2. Teleport gracza do pozycji startowej z nagrania
//   3. Warmup (2s) — czas na stabilizację FPS, inicjalizację AI
//   4. PerformanceMonitor.StartBenchmark()
//   5. Odtwarzanie klików z JSON przez PlayerController.MoveToWorldPos()
//   6. Po benchmarkSeconds → StopAndExport() → CSV
//
// Umieść na pustym GO w Benchmark.unity. Wymaga przypisania enemyPrefab i referencji.
[RequireComponent(typeof(PerformanceMonitor))]
public class BenchmarkRunner : MonoBehaviour
{
    [Header("Spawn wrogów")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int        enemyCount = 200;
    [SerializeField] private Vector3    spawnCenter;
    [SerializeField] private float      spawnRadius = 30f;
    [SerializeField] private int        randomSeed = 42;

    [Header("Nagranie inputu")]
    [SerializeField] private string recordingFilename = "benchmark_input.json";

    [Header("Timing")]
    [SerializeField] private float warmupSeconds = 2f;
    [SerializeField] private float benchmarkSeconds = 60f;

    [Header("Opcje")]
    [SerializeField] private bool autoQuitOnFinish = false;
    [SerializeField] private bool loopRecording = true;
    [Tooltip("Gracz nieśmiertelny podczas benchmarku — zapobiega przerwaniu pomiaru przez death")]
    [SerializeField] private bool makePlayerInvincible = true;

    private PlayerController _player;
    private PerformanceMonitor _perfMonitor;
    private BenchmarkInputRecorder.Recording _recording;

    private void Awake()
    {
        _perfMonitor = GetComponent<PerformanceMonitor>();
        _player = FindFirstObjectByType<PlayerController>();

        if (_player == null) { Debug.LogError("[Benchmark] PlayerController nie znaleziony w scenie"); enabled = false; return; }
        if (enemyPrefab == null) { Debug.LogError("[Benchmark] enemyPrefab nie przypisany"); enabled = false; return; }

        if (!LoadRecording()) { enabled = false; return; }
    }

    private IEnumerator Start()
    {
        TeleportPlayerToStart();
        if (makePlayerInvincible)
        {
            var hp = _player.GetComponent<PlayerHealthComponent>();
            if (hp != null) hp.IsInvincible = true;
        }
        SpawnEnemies();

        Debug.Log($"[Benchmark] Warmup {warmupSeconds}s...");
        yield return new WaitForSeconds(warmupSeconds);

        Debug.Log($"[Benchmark] Start — {_recording.events.Count} klików, {benchmarkSeconds}s");
        _perfMonitor.StartBenchmark();

        yield return PlaybackCoroutine();

        _perfMonitor.StopAndExport();
        Debug.Log("[Benchmark] Zakończony — CSV zapisany w persistentDataPath");

        if (autoQuitOnFinish)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private bool LoadRecording()
    {
        string path = Path.Combine(Application.persistentDataPath, recordingFilename);
        if (!File.Exists(path))
        {
            Debug.LogError($"[Benchmark] Nagranie nie znalezione: {path}. Uruchom BenchmarkInputRecorder (F4) w scenie Building.");
            return false;
        }

        _recording = JsonConvert.DeserializeObject<BenchmarkInputRecorder.Recording>(File.ReadAllText(path));
        return _recording != null && _recording.events.Count > 0;
    }

    private void TeleportPlayerToStart()
    {
        Vector3 pos = _recording.GetPlayerStartPos();
        var agent = _player.GetComponent<NavMeshAgent>();
        if (agent != null) agent.Warp(pos);
        else _player.transform.position = pos;
    }

    private void SpawnEnemies()
    {
        Random.InitState(randomSeed);
        int placed = 0;
        int attempts = 0;
        int maxAttempts = enemyCount * 10;

        while (placed < enemyCount && attempts < maxAttempts)
        {
            attempts++;
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = spawnCenter + new Vector3(offset.x, 0f, offset.y);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                continue;

            Instantiate(enemyPrefab, hit.position, Quaternion.identity, transform);
            placed++;
        }

        Debug.Log($"[Benchmark] Zespawnowano {placed}/{enemyCount} wrogów (próby: {attempts})");
    }

    private IEnumerator PlaybackCoroutine()
    {
        float startTime = Time.time;
        float loopStartTime = startTime;
        int eventIdx = 0;

        while (Time.time - startTime < benchmarkSeconds)
        {
            float loopElapsed = Time.time - loopStartTime;

            if (eventIdx >= _recording.events.Count)
            {
                if (!loopRecording) break;
                // Zapętlenie — nowy loop zaczyna się od końca poprzedniego, totalElapsed liczony od pierwszego startu
                loopStartTime = Time.time;
                eventIdx = 0;
                yield return null;
                continue;
            }

            var ev = _recording.events[eventIdx];
            if (loopElapsed >= ev.t)
            {
                _player.MoveToWorldPos(new Vector3(ev.x, ev.y, ev.z));
                eventIdx++;
            }

            yield return null;
        }
    }
}
