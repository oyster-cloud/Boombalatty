// GameOverZoneFlipLayer.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
public class GameOverZoneFlipLayer : MonoBehaviour
{
  [Header("Layers")]
  [SerializeField] string preZoneLayerName = "SnackPreZone"; // set per prefab
  [SerializeField] string liveLayerName    = "Snack";        // set per prefab

  [Header("Zone identification")]
  [SerializeField] string gameOverZoneTag = "GameOverZone";

  int preLayer = -1, liveLayer = -1;
  bool flipped;

  void Awake()
  {
    preLayer = LayerMask.NameToLayer(preZoneLayerName);
    liveLayer = LayerMask.NameToLayer(liveLayerName);
  }

  void OnEnable()
  {
    flipped = false;
    if (preLayer != -1) gameObject.layer = preLayer; // start safe
  }

  void OnTriggerExit2D(Collider2D other)
  {
    Debug.Log($"OnTriggerExit2D, other: '{other}'");
    Debug.Log($"OnTriggerExit2D, flipped: '{flipped}'");
    Debug.Log($"OnTriggerExit2D, tag: '{other.tag}'");
    if (flipped) return;
    if (!other || other.tag != gameOverZoneTag) return;
    Debug.Log($"OnTriggerExit2D THRU");
    if (liveLayer != -1)
    {
      foreach (var t in GetComponentsInChildren<Transform>(true))
        t.gameObject.layer = liveLayer;
      flipped = true;
      Debug.Log($"{name} flipped to live layer '{liveLayerName}' after exiting GameOverZone");
    }
  }

  // In GameOverZoneFlipLayer.cs (inside the class)
  public void ForceLiveLayer()
  {
    if (liveLayer == -1) return;
    var trs = GetComponentsInChildren<Transform>(true);
    for (int i = 0; i < trs.Length; i++)
      trs[i].gameObject.layer = liveLayer;
    flipped = true; // mark as already handled
  }
}
