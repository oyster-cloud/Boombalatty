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

  // What it does: Subscribes to game events when enabled.
  // What it's used for: Lets GameManager respond to ceiling crosses and whale creation.
  void OnEnable()
  {
    EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling += HandleCeilingCross;
    BoombaEvents.OnMerged += HandleMerge;
  }

  // What it does: Unsubscribes from game events when disabled.
  // What it's used for: Prevents dangling event subscriptions if the GameManager is turned off or destroyed.
  void OnDisable()
  {
    EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling -= HandleCeilingCross;
    BoombaEvents.OnMerged -= HandleMerge;
  }

  // What it does: Handles the boomba-crossed-ceiling event by triggering game over.
  // What it's used for: Connects ceiling detection to the global game-over pipeline.
  void HandleCeilingCross(BoombaProperties who)
  {
    TriggerGameOver();
  }

  // What it does: Handles merge events and checks if all boombas are now whales.
  // What it's used for: Detects when the player has achieved full whale completion.
  void HandleMerge(BoombaProperties a, BoombaProperties b, BoombaProperties result)
  {
    // Only check if the result is a whale
    if (result != null && result.Value == whaleValue)
    {
      StartCoroutine(CheckForWhaleCompletion());
    }
  }

  // What it does: Checks if all active boombas are whales and triggers completion if true.
  // What it's used for: Waits a frame for physics to settle, then checks whale status.
  IEnumerator CheckForWhaleCompletion()
  {
    yield return new WaitForEndOfFrame();

    if (IsGameOver) yield break;

    if (AllActiveBoombassAreWhales())
    {
      StartCoroutine(HandleWhaleCompletion());
    }
  }

  // What it does: Checks if all currently active boombas have the whale value.
  // What it's used for: Determines if the player has achieved full whale completion.
  bool AllActiveBoombassAreWhales()
  {
    var allBoombas = FindObjectsByType<BoombaProperties>(FindObjectsSortMode.None);
    
    int activeCount = 0;
    int whaleCount = 0;

    foreach (var boomba in allBoombas)
    {
      if (boomba == null || !boomba.gameObject.activeInHierarchy)
        continue;

      // Only count boombas on the Boomba layer (not snacks)
      if (boomba.gameObject.layer != LayerMask.NameToLayer("Boomba"))
        continue;

      activeCount++;
      if (boomba.Value == whaleValue)
        whaleCount++;
    }

    // Need at least one whale and all active boombas must be whales
    return activeCount > 0 && activeCount == whaleCount;
  }

  // What it does: Waits for the whale display duration, then restarts the game.
  // What it's used for: Gives the player time to see their achievement before resetting.
  IEnumerator HandleWhaleCompletion()
  {
    // Wait to let player see the whale(s)
    yield return new WaitForSecondsRealtime(whaleCompletionDelay);

    // Restart the game
    Restart();
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
