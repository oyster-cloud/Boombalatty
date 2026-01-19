// GameOverZone.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
// What it does: Detects when a live Snack or Boomba re-enters a certain trigger zone (usually the bottom) and ends the game.
// What it's used for: Acts as a safety net so pieces falling back into the game-over area trigger a loss condition.
public class GameOverZone : MonoBehaviour
{
  [SerializeField] string snackLiveLayer = "Snack";
  [SerializeField] string boombaLiveLayer = "Boomba";

  int snackLayer, boombaLayer;

  // What it does: Sets the attached collider to be a trigger and tags this object for easy identification.
// What it's used for: Prepares the zone to detect overlaps (not physical collisions) and optionally marks it for debugging or logic.
  void Reset()
  {
    var col = GetComponent<Collider2D>();
    if (col) col.isTrigger = true;
    gameObject.tag = "GameOverZone"; // optional but convenient
  }

  // What it does: Resolves and caches the integer layer indices for snack and boomba live layers.
// What it's used for: Speeds up later layer comparisons inside the trigger handler.
  void Awake()
  {
    snackLayer = LayerMask.NameToLayer(snackLiveLayer);
    boombaLayer = LayerMask.NameToLayer(boombaLiveLayer);
  }

  // What it does: Checks if a colliding object is on the live snack or boomba layer, and if so, triggers game over.
// What it's used for: Ends the game when a live piece falls back into the game-over region (e.g., below the playfield).
  void OnTriggerEnter2D(Collider2D other)
  {
    // Only live layers trigger game over on re-entry
    int L = other.gameObject.layer;
    if (L == snackLayer || L == boombaLayer)
    {
      var gm = GameManager.Instance;
      if (gm && !gm.IsGameOver) gm.TriggerGameOver();
    }
  }
}
