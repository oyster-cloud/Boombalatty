using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
// What it does: Watches a trigger “ceiling” line and fires game-over when a live boomba/snack crosses it after initial spawn.
// What it's used for: Implements the primary fail condition for the game—stacking pieces too high.
public class EndGameWhenBoombaCrossesCeiling : MonoBehaviour
{
  [Header("Source of the flag")]
  [SerializeField] BoombaSpawner spawner;

  [Header("Optional gate")]
  [SerializeField] BoombaSpawner initialSpawnSource;

  public static event Action<BoombaProperties> OnBoombaCrossedCeiling;

  // I'm defining this in multiple places. I want to use this in one universal place
  bool IsInitialSpawnCompleted = false;

  // What it does: Registers this ceiling trigger in the global Services registry when enabled.
  // What it's used for: Lets other systems (like GameManager) find and reset the ceiling logic easily.
  void OnEnable()
  {
    Services.Ceiling = this;
  }

  // What it does: Clears the Services registry reference if this instance is disabled.
  // What it's used for: Prevents stale references to a ceiling trigger that is no longer active.
  void OnDisable()
  {
    if (Services.Ceiling == this) Services.Ceiling = null;
  }

  // What it does: Resets the internal flag tracking whether the initial spawn has completed.
  // What it's used for: Allows external callers (like GameManager) to re-arm the ceiling after a restart.
  public void ResetInitialSpawnCompleted()
  {
    IsInitialSpawnCompleted = false;  // your existing field
  }

  // What it does: Ensures the attached Collider2D is configured as a trigger volume.
  // What it's used for: Sets up the ceiling so it detects overlaps instead of causing physical collisions.
  void Reset()
  {
    var col = GetComponent<Collider2D>();
    if (col) col.isTrigger = true; // ceiling should act as a trigger line
  }

  // What it does: Polls the spawner to see when initial spawning has finished, then arms the ceiling detection.
  // What it's used for: Delays ceiling-based game over until after the starting board setup is complete.
  void Update()
  {
    // Start listening only after the initial spawn completes
    if (!IsInitialSpawnCompleted && spawner != null && spawner.InitialSpawnCompleted)
      IsInitialSpawnCompleted = true;
  }

  // What it does: Detects when a live object crosses the ceiling trigger and triggers game over.
// What it's used for: Ends the game when the stack reaches the top, while ignoring pre-land snacks on a special layer.
  void OnTriggerEnter2D(Collider2D other)
  {
    if (!IsInitialSpawnCompleted) return;

    // 🧠 Skip anything still on the SnackPreLand layer (it’s not in play yet)
    int snackPreLandLayer = LayerMask.NameToLayer("SnackPreLand");
    if (other.gameObject.layer == snackPreLandLayer)
      return;

    // 🎯 Any other object (Boomba, Snack, etc.) triggers game over
    var gm = GameManager.Instance;
    if (gm && !gm.IsGameOver)
    {
      gm.TriggerGameOver();
    }

    // Optional: keep this if you still want BoombaCrossedCeiling event hooks
    var props = other.GetComponentInParent<BoombaProperties>();
    if (props != null)
      OnBoombaCrossedCeiling?.Invoke(props);
  }

  // What it does: Disarms the ceiling listener until initial spawn completes again.
// What it's used for: Called on restart to make sure the ceiling doesn’t trigger during board setup.
  public void ResetListening()
  {
    IsInitialSpawnCompleted = false;  // ✅ force disarm on restart
  }
}
