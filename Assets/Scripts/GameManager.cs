using UnityEngine;
using System.Collections;

// What it does: Centralizes game state, game-over handling, and restart orchestration.
// What it's used for: Acts as the top-level controller for starting, ending, and resetting a run of the game.
public class GameManager : MonoBehaviour
{
  public static GameManager Instance { get; private set; }

  [Header("UI")]
  [SerializeField] GameObject gameOverPanel;

  public bool IsGameOver { get; private set; } = false;

  private GameOverUIHandler _uiHandler;
  private BoardCleaner _boardCleaner;

  // What it does: Enforces a singleton instance, initializes handlers, and sets up initial game state.
  // What it's used for: Ensures there's exactly one persistent GameManager across scenes.
  void Awake()
  {
    if (Instance != null && Instance != this) 
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    // Initialize handlers
    _uiHandler = new GameOverUIHandler(gameOverPanel, this);
    _boardCleaner = new BoardCleaner();

    // Set initial state
    if (gameOverPanel)
      gameOverPanel.SetActive(false);

    Time.timeScale = 1f;
  }

  // What it does: Subscribes to the ceiling-cross event when enabled.
  // What it's used for: Lets GameManager trigger game-over when a boomba crosses the top boundary.
  void OnEnable()
  {
    EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling += HandleCeilingCross;
  }

  // What it does: Unsubscribes from the ceiling-cross event when disabled.
  // What it's used for: Prevents dangling event subscriptions if the GameManager is turned off or destroyed.
  void OnDisable()
  {
    EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling -= HandleCeilingCross;
  }

  // What it does: Handles the boomba-crossed-ceiling event by triggering game over.
  // What it's used for: Connects ceiling detection to the global game-over pipeline.
  void HandleCeilingCross(BoombaProperties who)
  {
    TriggerGameOver();
  }

  // What it does: Disables gameplay systems, shows the game-over UI, and pauses time.
  // What it's used for: Called when a fail condition happens (like crossing the ceiling).
  public void TriggerGameOver()
  {
    if (IsGameOver) return;
    
    IsGameOver = true;
    _uiHandler.ShowGameOverPanel();
    Time.timeScale = 0f;
  }

  // What it does: Restarts the game by resetting state, clearing the board, and restarting spawners.
  // What it's used for: Called by UI buttons or debug actions to start a fresh run without reloading the scene.
  public void Restart()
  {
    ResetGameState();
    _uiHandler.HideGameOverPanel();
    _boardCleaner.ClearBoard();
    RestartSpawners();
  }

  // What it does: Resets game state flags and unpauses time.
  // What it's used for: Prepares the game state for a fresh run.
  private void ResetGameState()
  {
    IsGameOver = false;
    Time.timeScale = 1f;
  }

  // What it does: Resets ceiling detection and restarts all spawning systems.
  // What it's used for: Kicks off fresh boomba and snack spawning after a restart.
  private void RestartSpawners()
  {
    Services.Ceiling?.ResetListening();

    var boombaSpawner = Services.BoombaSpawner;
    var snackSpawner = Services.SnackTouchSpawner;

    if (boombaSpawner != null) 
      boombaSpawner.ResetAndStartInitialSpawn();
    
    if (snackSpawner != null) 
      snackSpawner.ResetHeldAndArm();
  }

  // What it does: Starts a coroutine that hides the target GameObject after a real-time delay.
  // What it's used for: Used to temporarily show objects (like the final merged boomba) and then hide them cleanly.
  public void HideAfterSecondsRealtime(GameObject target, float seconds)
  {
    StartCoroutine(HideRoutine(target, seconds));
  }

  // What it does: Waits for the given unscaled time, then disables the target GameObject if it still exists.
  // What it's used for: Implements the timing behavior for HideAfterSecondsRealtime.
  private IEnumerator HideRoutine(GameObject target, float seconds)
  {
    yield return new WaitForSecondsRealtime(seconds);
    if (target) target.SetActive(false);
  }
}
