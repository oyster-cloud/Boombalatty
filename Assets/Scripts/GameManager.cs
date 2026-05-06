using UnityEngine;
using System.Collections;

// What it does: Centralizes game state, game-over handling, and restart orchestration.
// What it's used for: Acts as the top-level controller for starting, ending, and resetting a run of the game.
[DefaultExecutionOrder(-100)] // Run before BoombaSpawner
public class GameManager : MonoBehaviour
{
  public static GameManager Instance { get; private set; }

  [Header("UI")]
  [SerializeField] GameObject gameOverPanel;
  [SerializeField] GameObject stageTransitionPanel;

  [Header("Whale Completion")]
  [SerializeField] int whaleValue = 7;
  [SerializeField] float whaleCompletionDelay = 2f;
  [SerializeField] float stageTransitionDuration = 2f;

  public bool IsGameOver { get; private set; } = false;
  public bool IsInteractionLocked { get; private set; } = false;
  private bool _wasWhaleCompletion = false;

  private GameOverUIHandler _uiHandler;
  private BoardCleaner _boardCleaner;
  private StageTransitionUI _stageTransitionUI;
  private GameStateManager _gameStateManager;
  private int _currentStage = 1;

  void Awake()
  {
    if (Instance != null && Instance != this) 
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    _uiHandler = new GameOverUIHandler(gameOverPanel, this);
    _boardCleaner = new BoardCleaner();
    _stageTransitionUI = new StageTransitionUI(stageTransitionPanel, this);
    _gameStateManager = new GameStateManager();

    if (gameOverPanel)
      gameOverPanel.SetActive(false);
    
    if (stageTransitionPanel)
      stageTransitionPanel.SetActive(false);

    Time.timeScale = 1f;
  }

  // What it does: Loads saved state after Awake but before any Start methods.
  // What it's used for: Ensures saved difficulty is loaded before BoombaSpawner.Start() runs.
  void Start()
  {
    LoadAndApplySavedState();
  }

  void OnEnable()
  {
    EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling += HandleCeilingCross;
    BoombaEvents.OnMerged += HandleMerge;
  }

  void OnDisable()
  {
    EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling -= HandleCeilingCross;
    BoombaEvents.OnMerged -= HandleMerge;
  }

  // What it does: Loads saved progress and applies it to the spawner before first spawn.
  // What it's used for: Restores player's stage and difficulty on game start.
  void LoadAndApplySavedState()
  {
    _currentStage = _gameStateManager.LoadCurrentStage();
    
    var spawner = Services.BoombaSpawner;
    if (spawner != null)
    {
      int savedBoombaCount = _gameStateManager.LoadCurrentBoombaCount(spawner.StartingBoombaCount);
      spawner.LoadSavedStateAndStart(savedBoombaCount);
      
      Debug.Log($"[GameManager] Loaded: Stage {_currentStage}, Boomba Count {savedBoombaCount}");
    }
    else
    {
      Debug.LogWarning("[GameManager] BoombaSpawner not found during load!");
    }
  }

  // What it does: Saves current stage and difficulty to PlayerPrefs.
  // What it's used for: Persists player progress so it survives game restarts.
  void SaveGameState()
  {
    var spawner = Services.BoombaSpawner;
    if (spawner != null)
    {
      _gameStateManager.SaveGameState(_currentStage, spawner.CurrentBoombaCount);
      Debug.Log($"[GameManager] Saved: Stage {_currentStage}, Boomba Count {spawner.CurrentBoombaCount}");
    }
  }

  void HandleCeilingCross(BoombaProperties who)
  {
    TriggerGameOver();
  }

  void HandleMerge(BoombaProperties a, BoombaProperties b, BoombaProperties result)
  {
    if (result != null && result.Value == whaleValue)
    {
      StartCoroutine(CheckForWhaleCompletion());
    }
  }

  IEnumerator CheckForWhaleCompletion()
  {
    yield return new WaitForEndOfFrame();

    if (IsGameOver) yield break;

    if (AllActiveBoombassAreWhales())
    {
      StartCoroutine(HandleWhaleCompletion());
    }
  }

  bool AllActiveBoombassAreWhales()
  {
    var allBoombas = FindObjectsByType<BoombaProperties>(FindObjectsSortMode.None);
    
    int activeCount = 0;
    int whaleCount = 0;

    foreach (var boomba in allBoombas)
    {
      if (boomba == null || !boomba.gameObject.activeInHierarchy)
        continue;

      if (boomba.gameObject.layer != LayerMask.NameToLayer("Boomba"))
        continue;

      activeCount++;
      if (boomba.Value == whaleValue)
        whaleCount++;
    }

    return activeCount > 0 && activeCount == whaleCount;
  }

  IEnumerator HandleWhaleCompletion()
  {
    yield return new WaitForSecondsRealtime(whaleCompletionDelay);

    _boardCleaner.ClearBoard();

    // Increment stage and show transition
    _currentStage++;
    yield return _stageTransitionUI.ShowStageTransition(_currentStage, stageTransitionDuration);

    _wasWhaleCompletion = true;
    Restart();
  }

  public void TriggerGameOver()
  {
    if (IsGameOver) return;
    
    IsGameOver = true;
    _uiHandler.ShowGameOverPanel();
    Time.timeScale = 0f;
  }

  public void Restart()
  {
    bool increaseDifficulty = _wasWhaleCompletion;
    
    ResetGameState();
    _uiHandler.HideGameOverPanel();
    
    if (!_wasWhaleCompletion)
    {
      _boardCleaner.ClearBoard();
      _currentStage = 1; // Reset stage on game over restart
    }
    
    RestartSpawners(increaseDifficulty);
    
    // Save progress after restart
    SaveGameState();
    
    _wasWhaleCompletion = false;
  }

  private void ResetGameState()
  {
    IsGameOver = false;
    IsInteractionLocked = false;
    Time.timeScale = 1f;
  }

  private void RestartSpawners(bool increaseDifficulty)
  {
    var boombaSpawner = Services.BoombaSpawner;
    var snackSpawner = Services.SnackTouchSpawner;

    if (boombaSpawner != null)
    {
      boombaSpawner.ResetAndStartInitialSpawn(increaseDifficulty);
    }
    
    if (snackSpawner != null) 
      snackSpawner.ResetHeldAndArm();
  }

  // What it does: Resets all progress back to stage 1.
  // What it's used for: Called from settings menu or "New Game" button.
  public void ResetProgress()
  {
    _gameStateManager.ResetProgress();
    _currentStage = 1;
    
    var spawner = Services.BoombaSpawner;
    if (spawner != null)
    {
      spawner.ResetToStartingDifficulty();
    }
    
    Debug.Log("[GameManager] Progress reset to Stage 1");
  }

  // What it does: Ends the current game session and resets all progress.
  // What it's used for: Called from "End Game" button to wipe progress and start fresh from Stage 1.
  public void EndGame()
  {
    // Reset all progress
    ResetProgress();
    
    // Clear the board
    _boardCleaner.ClearBoard();
    
    // Reset to stage 1
    _currentStage = 1;
    
    // Reset game state
    ResetGameState();
    
    // Hide any UI
    _uiHandler.HideGameOverPanel();
    
    // Restart spawners at base difficulty
    RestartSpawners(increaseDifficulty: false);
    
    Debug.Log("[GameManager] Game ended - reset to Stage 1");
  }

  private IEnumerator HideRoutine(GameObject target, float seconds)
  {
    yield return new WaitForSecondsRealtime(seconds);
    if (target) target.SetActive(false);
  }

  // What it does: Freezes physics and blocks input for the duration of a whale display.
  // What it's used for: Called when a whale is spawned so the player can't interact until it disappears.
  public void LockForWhaleDisplay(GameObject whaleGO, float seconds)
  {
    IsInteractionLocked = true;
    StartCoroutine(WhaleDisplayRoutine(whaleGO, seconds));
  }

  private IEnumerator WhaleDisplayRoutine(GameObject whaleGO, float seconds)
  {
    // Wait one fixed update so WhaleBehavior.FixedUpdate can orient the whale
    yield return new WaitForFixedUpdate();
    Time.timeScale = 0f;

    yield return new WaitForSecondsRealtime(seconds);

    if (whaleGO) whaleGO.SetActive(false);

    // Only unlock if we haven't transitioned to a proper game-over or completion state
    if (!IsGameOver)
    {
        IsInteractionLocked = false;
        Time.timeScale = 1f;
    }
  }
}
