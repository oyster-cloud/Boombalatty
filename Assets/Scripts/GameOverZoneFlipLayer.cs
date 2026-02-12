using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
// What it does: Manages layer flipping for snacks as they leave the GameOverZone trigger.
// What it's used for: Keeps snacks on a "safe" pre-zone layer until they fully exit the game-over region, then switches them to the live play layer.
public class GameOverZoneFlipLayer : MonoBehaviour
{
  [Header("Layers")]
  [SerializeField] string preZoneLayerName = "SnackPreZone"; // set per prefab
  [SerializeField] string liveLayerName    = "Snack";        // set per prefab

  [Header("Zone identification")]
  [SerializeField] string gameOverZoneTag = "GameOverZone";

  int preLayer = -1, liveLayer = -1;
  bool flipped;

  // What it does: Resolves the integer layer indices for the pre-zone and live layers.
// What it's used for: Prepares the component to quickly switch layers at runtime without repeated lookups.
  void Awake()
  {
    preLayer = LayerMask.NameToLayer(preZoneLayerName);
    liveLayer = LayerMask.NameToLayer(liveLayerName);
  }

  // What it does: Resets the flipped flag and assigns the pre-zone layer when the object is enabled.
// What it's used for: Ensures newly spawned or pooled snacks start in a safe, non-live layer inside the GameOverZone.
  void OnEnable()
  {
    flipped = false;
    if (preLayer != -1) gameObject.layer = preLayer; // start safe
  }

  // What it does: When this object exits the GameOverZone trigger for the first time, switches it and all children to the live layer.
// What it's used for: Activates snacks as "in play" only after they fully leave the game-over area, preventing accidental game-overs.
  void OnTriggerExit2D(Collider2D other)
  {
    if (flipped) return;
    if (!other || other.tag != gameOverZoneTag) return;

    if (liveLayer != -1)
    {
      foreach (var t in GetComponentsInChildren<Transform>(true))
        t.gameObject.layer = liveLayer;
      flipped = true;
    }
  }

  // What it does: Forces this object and all children to the live layer immediately, marking it as already flipped.
// What it's used for: Used by other systems (e.g., merge logic) to ensure a boomba/snack is fully in the live layer after special events.
  public void ForceLiveLayer()
  {
    if (liveLayer == -1) return;
    var trs = GetComponentsInChildren<Transform>(true);
    for (int i = 0; i < trs.Length; i++)
      trs[i].gameObject.layer = liveLayer;
    flipped = true; // mark as already handled
  }
}
