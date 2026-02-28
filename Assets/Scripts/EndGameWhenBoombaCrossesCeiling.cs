using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
// What it does: Watches a trigger "ceiling" line and fires game-over when a live boomba crosses it after initial spawn.
// What it's used for: Implements the primary fail condition for the game—stacking pieces too high.
public class EndGameWhenBoombaCrossesCeiling : MonoBehaviour
{
  [Header("Source of the flag")]
  [SerializeField] BoombaSpawner spawner;

  public static event Action<BoombaProperties> OnBoombaCrossedCeiling;

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

  // What it does: Ensures the attached Collider2D is configured as a trigger volume.
  // What it's used for: Sets up the ceiling so it detects overlaps instead of causing physical collisions.
  void Reset()
  {
    var col = GetComponent<Collider2D>();
    if (col) col.isTrigger = true;
  }

  // What it does: Detects when a live boomba crosses the ceiling trigger and triggers game over.
  // What it's used for: Ends the game when the stack reaches the top, while ignoring snacks.
  void OnTriggerEnter2D(Collider2D other)
  {
    // Don't trigger during initial spawn
    if (!spawner || !spawner.InitialSpawnCompleted) return;

    // Skip anything on the SnackPreZone layer (snacks before they exit GameOverZone)
    int snackPreZoneLayer = LayerMask.NameToLayer("SnackPreZone");
    if (other.gameObject.layer == snackPreZoneLayer) return;

    // Skip live snacks - they should be able to cross the ceiling
    int snackLayer = LayerMask.NameToLayer("Snack");
    if (other.gameObject.layer == snackLayer) return;

    // Only Boombas trigger game over
    var gm = GameManager.Instance;
    if (gm && !gm.IsGameOver)
    {
      gm.TriggerGameOver();
    }

    // Fire event for any listeners
    var props = other.GetComponentInParent<BoombaProperties>();
    if (props != null)
      OnBoombaCrossedCeiling?.Invoke(props);
  }
}