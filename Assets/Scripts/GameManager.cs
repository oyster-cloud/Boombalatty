using UnityEngine;
using System.Collections;

// What it does: Centralizes game state, game-over handling, and restart orchestration.
// What it's used for: Acts as the top-level controller for starting, ending, and resetting a run of the game.
public class GameManager : MonoBehaviour
{
  public static GameManager Instance { get; private set; }

  [Header("UI")]
  [SerializeField] GameObject gameOverPanel;

  [Header("Whale Completion")]
  [SerializeField] int whaleValue = 7;
  [SerializeField] float whaleCompletionDelay = 2f; // Time to show whale before resetting

  public bool IsGameOver { get; private set; } = false;
  private bool _wasWhaleCompletion = false; // Track if last restart was from whale completion

  private GameOverUIHandler _uiHandler;
  private BoardCleaner _boardCleaner;

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

    if (gameOverPanel)
      gameOverPanel.SetActive(false);

    Time.timeScale = 1f;
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

  void HandleCeilingCross(BoombaProperties who)
  {
    TriggerGameOver();
  }

  void HandleMerge(BoombaProperties a, BoombaProperties b, BoombaProperties result)
  {
    // Only check if the result is a whale
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

  // What it does: Waits for the whale display duration, marks it as whale completion, then restarts.
  // What it's used for: Gives the player time to see their achievement before increasing difficulty and restarting.
  IEnumerator HandleWhaleCompletion()
  {
    yield return new WaitForSecondsRealtime(whaleCompletionDelay);

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

  // What it does: Restarts the game by resetting state, clearing the board, and restarting spawners.
  // What it's used for: Called by UI buttons (game over) or after whale completion (auto-restart with difficulty increase).
  public void Restart()
  {
    bool increaseDifficulty = _wasWhaleCompletion;
    
    ResetGameState();
    _uiHandler.HideGameOverPanel();
    _boardCleaner.ClearBoard();
    RestartSpawners(increaseDifficulty);
    
    _wasWhaleCompletion = false; // Reset flag after use
  }

  private void ResetGameState()
  {
    IsGameOver = false;
    Time.timeScale = 1f;
  }

  // What it does: Resets and restarts all spawning systems, optionally increasing difficulty.
  // What it's used for: Kicks off fresh spawning after a restart, with difficulty increase only after whale completion.
  private void RestartSpawners(bool increaseDifficulty)
  {
    var boombaSpawner = Services.BoombaSpawner;
    var snackSpawner = Services.SnackTouchSpawner;

    if (boombaSpawner != null)
    {
      // Pass the flag - only increases if true (whale completion)
      boombaSpawner.ResetAndStartInitialSpawn(increaseDifficulty);
    }
    
    if (snackSpawner != null) 
      snackSpawner.ResetHeldAndArm();
  }

  public void HideAfterSecondsRealtime(GameObject target, float seconds)
  {
    StartCoroutine(HideRoutine(target, seconds));
  }

  private IEnumerator HideRoutine(GameObject target, float seconds)
  {
    yield return new WaitForSecondsRealtime(seconds);
    if (target) target.SetActive(false);
  }
}
