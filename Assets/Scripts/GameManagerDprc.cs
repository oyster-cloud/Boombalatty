// using UnityEngine;
// using System.Collections;

// // What it does: Centralizes game state, game-over handling, board clearing, and restart behavior.
// // What it's used for: Acts as the top-level controller for starting, ending, and resetting a run of the game.
// public class GameManager : MonoBehaviour
// {
//   public static GameManager Instance { get; private set; }

//   [Header("What to disable when game ends")]
//   [SerializeField] MonoBehaviour[] toDisable;   // e.g., BoombaSpawner, BoombaTouchSpawner

//   [Header("UI")]
//   [SerializeField] GameObject gameOverPanel;
//   [SerializeField] bool pauseOnEnd = true;

//   [SerializeField] BoombaSpawner boombaSpawner;
//   [SerializeField] SnackTouchSpawner snackTouchSpawner;

//   public bool IsGameOver { get; private set; } = false;

//   // What it does: Enforces a singleton instance, initializes game state, and hides the game-over UI on startup.
//   // What it's used for: Ensures there’s exactly one persistent GameManager across scenes and that time/UI start in a known state.
//   void Awake()
//   {
//     if (Instance != null && Instance != this) {
//       Destroy(gameObject); return;
//     }

//     Instance = this;

//     DontDestroyOnLoad(gameObject);

//     if (gameOverPanel)
//       gameOverPanel.SetActive(false);

//     Time.timeScale = 1f;
//   }

//   // What it does: Starts a coroutine that hides the target GameObject after a real-time delay.
//   // What it's used for: Used to temporarily show objects (like the final merged boomba) and then hide them cleanly.
//   public void HideAfterSecondsRealtime(GameObject target, float seconds)
//   {
//     StartCoroutine(HideRoutine(target, seconds));
//   }

//   // What it does: Waits for the given unscaled time, then disables the target GameObject if it still exists.
//   // What it's used for: Implements the timing behavior for HideAfterSecondsRealtime.
//   IEnumerator HideRoutine(GameObject target, float seconds)
//   {
//     yield return new WaitForSecondsRealtime(seconds);
//     if (target) target.SetActive(false);
//   }

//   // What it does: Subscribes to the ceiling-cross event when enabled.
//   // What it's used for: Lets GameManager trigger game-over when a boomba crosses the top boundary.
//   void OnEnable()
//   {
//     EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling += HandleCeilingCross;
//   }

//   // What it does: Unsubscribes from the ceiling-cross event when disabled.
//   // What it's used for: Prevents dangling event subscriptions if the GameManager is turned off or destroyed.
//   void OnDisable()
//   {
//     EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling -= HandleCeilingCross;
//   }

//   // What it does: Handles the boomba-crossed-ceiling event by triggering game over.
// // What it's used for: Connects ceiling detection to the global game-over pipeline.
//   void HandleCeilingCross(BoombaProperties who)
//   {
//     TriggerGameOver();
//   }

//   // What it does: Restarts the game by unpausing, hiding the game-over UI, re-enabling systems, clearing the board, and restarting spawners.
//   // What it's used for: Called by UI buttons or debug actions to start a fresh run without reloading the scene.
//   public void Restart()
//   {
//     // I don't think I need pauseOnEnd
//     if (pauseOnEnd) Time.timeScale = 1f;

//     // 2) Start fading out the panel (keep raycasts ENABLED during fade so clicks don’t slip through)
//     if (gameOverPanel)
//     {
//       var cg = gameOverPanel.GetComponent<CanvasGroup>() ?? gameOverPanel.AddComponent<CanvasGroup>();
//       cg.interactable = false;
//       cg.blocksRaycasts = true; // keep blocking while fading so the user can’t click through
//       StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, 0.2f, () =>
//       {
//         cg.blocksRaycasts = false;
//         gameOverPanel.SetActive(false);
//       }));
//     }

//     // Do I need this? Am I disabling anything?
//     if (toDisable != null)
//       for (int i = 0; i < toDisable.Length; i++)
//         if (toDisable[i]) toDisable[i].enabled = true;

//     // Clear the board (Boombas + Snacks + any held snack)
//     ClearBoard();

//     Services.Ceiling?.ResetListening();

//     // Prefer registry (latest scene instances)
//     var boomba = Services.BoombaSpawner;
//     var snack  = Services.SnackTouchSpawner;

//     // Kick off fresh run
//     if (boomba != null) boomba.ResetAndStartInitialSpawn();
//     if (snack  != null) snack.ResetHeldAndArm();

//     IsGameOver = false;
//   }

//   // What it does: Destroys all active boomba and snack objects in the scene, plus any stragglers on snack-related layers.
//   // What it's used for: Resets the board so a new game can start with a clean playfield.
//   void ClearBoard()
//   {
//     // Boombas (anything that has BoombaProperties)
//     var boombas = FindObjectsByType<BoombaProperties>(FindObjectsSortMode.None);
//     for (int i = 0; i < boombas.Length; i++)
//       if (boombas[i]) Destroy(boombas[i].gameObject);

//     // Snacks (either by component or by layer names)
//     var snacks = FindObjectsByType<SnackLifecycle>(FindObjectsSortMode.None);
//     for (int i = 0; i < snacks.Length; i++)
//       if (snacks[i]) Destroy(snacks[i].gameObject);

//     // Safety: also clear any objects on Snack/SnackPreLand layers if they lack components
//     int snack = LayerMask.NameToLayer("Snack");
//     int snackPre = LayerMask.NameToLayer("SnackPreLand");
//     var all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
//     for (int i = 0; i < all.Length; i++)
//     {
//       var go = all[i].gameObject;
//       if (!go || go.scene.rootCount == 0) continue;
//       int L = go.layer;
//       if (L == snack || L == snackPre) Destroy(go);
//     }
//   }

//   // What it does: Disables gameplay systems, shows/fades in the game-over UI, and optionally pauses time.
//   // What it's used for: Called when a fail condition happens (like crossing the ceiling or re-entering the game-over zone).
//   public void TriggerGameOver()
//   {
//     if (IsGameOver) return;
//     IsGameOver = true;

//     if (toDisable != null)
//       for (int i = 0; i < toDisable.Length; i++)
//         if (toDisable[i]) toDisable[i].enabled = false;

//     // Fade in the overlay
//     StartFadeInGameOverPanel(0.35f); // 350 ms feels snappy

//     if (pauseOnEnd) Time.timeScale = 0f;
//   }

//   // What it does: Prepares the game-over panel, then starts a fade-in animation via a CanvasGroup.
// // What it's used for: Smoothly introduces the game-over UI instead of an abrupt pop-in.
//   void StartFadeInGameOverPanel(float duration)
//   {
//     if (!gameOverPanel)
//     {
//       Debug.LogWarning("GameManager: gameOverPanel not assigned.");
//       return;
//     }

//     gameOverPanel.SetActive(true);

//     var cg = gameOverPanel.GetComponent<CanvasGroup>();
//     if (!cg) cg = gameOverPanel.AddComponent<CanvasGroup>();

//     cg.interactable = false;
//     cg.blocksRaycasts = false;
//     StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, duration, () =>
//     {
//       // enable interaction after fade completes
//       cg.interactable = true;
//       cg.blocksRaycasts = true;
//     }));
//   }

//   // What it does: Lerps a CanvasGroup’s alpha over time using unscaled deltaTime, then calls an optional callback.
// // What it's used for: Drives both fade-in and fade-out transitions for the game-over panel.
//   IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, System.Action onDone = null)
//   {
//     float t = 0f;
//     cg.alpha = from;
//     while (t < duration)
//     {
//       t += Time.unscaledDeltaTime;
//       float k = Mathf.Clamp01(t / duration);
//       cg.alpha = Mathf.Lerp(from, to, k);
//       yield return null;
//     }
//     cg.alpha = to;
//     onDone?.Invoke();
//   }
// }
