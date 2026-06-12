// ============================================================
//  GameManager.cs
//  Ratpocalypse — Core/GameManager.cs
//
//  Singleton zarz¹dzaj¹cy globalnym stanem gry.
//  Odpowiada za: start gry, pauza, game over, zmiana scen.
//  NIE przechowuje danych gracza, to robi PlayerStats.
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // --------------------------------------------------------
    // Singleton
    // --------------------------------------------------------
    public static GameManager Instance { get; private set; }

    // --------------------------------------------------------
    // Referencje — przypisz w Inspectorze na obiekcie GameManager
    // --------------------------------------------------------
    [Header("Dane gracza")]
    [SerializeField] public PlayerStats playerStats;

    [Header("Ustawienia")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string firstGameSceneName = "Act1_City";

    // --------------------------------------------------------
    // Stan gry
    // --------------------------------------------------------
    public bool IsPaused { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // --------------------------------------------------------
    // Lifecycle
    // --------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        EventBus.Subscribe<OnPlayerDied>(HandlePlayerDied);
        EventBus.Subscribe<OnCheckpointReached>(HandleCheckpointReached);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<OnPlayerDied>(HandlePlayerDied);
        EventBus.Unsubscribe<OnCheckpointReached>(HandleCheckpointReached);
    }

    // --------------------------------------------------------
    // Publiczne API
    // --------------------------------------------------------

    /// <summary>Startuje now¹ grê od pocz¹tku.</summary>
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

    /// <summary>Wstrzymuje lub wznawia grê.</summary>
    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        EventBus.Publish(new OnGamePaused { isPaused = paused });
    }

    /// <summary>Prze³¹cza pauzê.</summary>
    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    /// <summary>Wraca do g³ównego menu.</summary>
    public void ReturnToMainMenu()
    {
        SetPaused(false);
        Time.timeScale = 1f;
        EventBus.Clear();
        SceneManager.LoadScene(mainMenuSceneName);
        CurrentState = GameState.MainMenu;
    }

    // --------------------------------------------------------
    // Handlery zdarzeñ
    // --------------------------------------------------------

    void HandlePlayerDied(OnPlayerDied _)
    {
        if (IsGameOver) return;
        IsGameOver = true;
        CurrentState = GameState.GameOver;

        // Krótkie opóŸnienie zanim pojawi siê ekran œmierci
        StartCoroutine(ShowGameOverDelayed());
    }

    void HandleCheckpointReached(OnCheckpointReached evt)
    {
        // SaveSystem.Instance.Save(evt.checkpointId); // odkomentuj gdy SaveSystem gotowy
        Debug.Log($"[GameManager] Checkpoint: {evt.checkpointId} — gra zapisana.");
    }

    System.Collections.IEnumerator ShowGameOverDelayed()
    {
        yield return new WaitForSeconds(2f);
        // UIManager.Instance.ShowGameOver(); // odkomentuj gdy UIManager gotowy
        Debug.Log("[GameManager] GAME OVER");
    }
}

// ============================================================
//  Enum stanu gry
// ============================================================
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Cutscene
}