using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
// What it does: Manages when a Snack transitions from a pre-land layer into the live Snack layer based on how far it has fallen.
// What it's used for: Prevents Snacks from interacting with normal game collisions/zones until they've dropped a minimum distance.
public class SnackLifecycle : MonoBehaviour
{
  [Header("Layers")]
  [SerializeField] string preLandLayerName = "SnackPreLand";
  [SerializeField] string snackLayerName   = "Snack";

  [Header("Enable Snack layer after falling this distance")]
  [SerializeField] float dropDistanceToEnable = 1.0f;

  Rigidbody2D rb;
  float startY;
  int preLandLayer;
  int snackLayer;
  bool snackLayerEnabled;

  // What it does: Caches the Rigidbody2D and layer indices, and sets the Snack to its pre-land layer initially.
  // What it's used for: Ensures Snacks start on a non-live layer so they don't immediately trigger ceiling/goal logic.
  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    preLandLayer = LayerMask.NameToLayer(preLandLayerName);
    snackLayer   = LayerMask.NameToLayer(snackLayerName);
    if (preLandLayer != -1)
      gameObject.layer = preLandLayer;
  }

  // What it does: Records the starting Y position and resets the internal state when the Snack is enabled or reused from a pool.
  // What it's used for: Makes the fall-distance logic work correctly each time a Snack spawns or respawns.
  void OnEnable()
  {
    startY = transform.position.y;
    snackLayerEnabled = false;
  }

  // What it does: Each physics tick, checks whether the Snack has fallen far enough to switch to the live Snack layer.
  // What it's used for: Delays enabling full interactions until the Snack is actually in play and falling under gravity.
  void FixedUpdate()
  {
    if (snackLayerEnabled) return;
    if (!rb || rb.bodyType != RigidbodyType2D.Dynamic) return;

    // Once it falls below the drop distance, enable Snack layer
    if ((startY - transform.position.y) >= dropDistanceToEnable)
      EnableSnackLayer();
  }

  // What it does: Switches the GameObject’s layer to the live Snack layer and locks that state so it only happens once.
  // What it's used for: Activates normal collision and game-over behavior for Snacks once they're in the active play area.
  void EnableSnackLayer()
  {
    if (snackLayer == -1) return;
    gameObject.layer = snackLayer;
    snackLayerEnabled = true;
    // Debug.Log($"Snack {name} is now on Snack layer at y={transform.position.y}");
  }
}
