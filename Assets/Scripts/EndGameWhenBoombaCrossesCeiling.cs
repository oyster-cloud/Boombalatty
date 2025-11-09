using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class EndGameWhenBoombaCrossesCeiling : MonoBehaviour
{
  [Header("Source of the flag")]
  [SerializeField] BoombaSpawner spawner;

  [Header("Optional gate")]
  [SerializeField] BoombaSpawner initialSpawnSource;

  public static event Action<BoombaProperties> OnBoombaCrossedCeiling;

  // I'm defining this in multiple places. I want to use this in one universal place
  bool IsInitialSpawnCompleted = false;

  void OnEnable()
  {
    Services.Ceiling = this;
  }

  void OnDisable()
  {
    if (Services.Ceiling == this) Services.Ceiling = null;
  }

  // maybe get rid of this
  public void ResetInitialSpawnCompleted()
  {
    IsInitialSpawnCompleted = false;  // your existing field
  }


  void Reset()
  {
    var col = GetComponent<Collider2D>();
    if (col) col.isTrigger = true; // ceiling should act as a trigger line
  }

  void Update()
  {
    // Start listening only after the initial spawn completes
    if (!IsInitialSpawnCompleted && spawner != null && spawner.InitialSpawnCompleted)
      IsInitialSpawnCompleted = true;
  }

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

  public void ResetListening()
  {
    IsInitialSpawnCompleted = false;                    // ✅ force disarm on restart
  }
}
