using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dane gracza")]
    [SerializeField] private PlayerStats playerStats;
    public PlayerStats PlayerStats => playerStats;

    [Header("FMOD")]
    [SerializeField] private FMODEvents fmodEvents;
    public static FMODEvents FMODEvents => Instance.fmodEvents;

    [Header("Ustawienia scen")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string firstGameSceneName = "Act1_City";

    public bool IsPaused { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnPlayerDied>(HandlePlayerDied);
        EventBus.Subscribe<OnCheckpointReached>(HandleCheckpointReached);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnPlayerDied>(HandlePlayerDied);
        EventBus.Unsubscribe<OnCheckpointReached>(HandleCheckpointReached);
    }

    public void StartNewGame()
    {
        if (playerStats == null)
        {
            Debug.LogError("[GameManager] PlayerStats nie przypisany!");
            return;
        }
        playerStats.InitializeForNewGame();
        IsGameOver = false;
        SetPaused(false);
        CurrentState = GameState.Playing;
        SceneManager.LoadScene(firstGameSceneName);
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        EventBus.Publish(new OnGamePaused { isPaused = paused });
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void ReturnToMainMenu()
    {
        SetPaused(false);
        // Nie czyścimy EventBus.Clear() — DontDestroyOnLoad obiekty (GameManager, AudioManager)
        // same zarządzają subskrypcjami przez OnEnable/OnDisable.
        SceneManager.LoadScene(mainMenuSceneName);
        CurrentState = GameState.MainMenu;
    }

    private void HandlePlayerDied(OnPlayerDied _)
    {
        if (IsGameOver) return;
        IsGameOver = true;
        CurrentState = GameState.GameOver;
        StartCoroutine(ShowGameOverDelayed());
    }

    private void HandleCheckpointReached(OnCheckpointReached evt)
    {
        // SaveSystem.Instance.Save(evt.checkpointId); // odkomentuj gdy SaveSystem gotowy
        Debug.Log($"[GameManager] Checkpoint: {evt.checkpointId} — gra zapisana.");
    }

    private System.Collections.IEnumerator ShowGameOverDelayed()
    {
        yield return new WaitForSeconds(2f);
        // UIManager.Instance.ShowGameOver(); // odkomentuj gdy UIManager gotowy
        Debug.Log("[GameManager] GAME OVER");
    }
}
