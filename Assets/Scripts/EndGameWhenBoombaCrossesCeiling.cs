using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class EndGameWhenBoombaCrossesCeiling : MonoBehaviour
{
  [Header("Source of the flag")]
  [SerializeField] BoombaSpawner spawner;

  public static event Action<BoombaProperties> OnBoombaCrossedCeiling;

  bool listening = false;

  void Reset()
  {
    var col = GetComponent<Collider2D>();
    if (col) col.isTrigger = true; // ceiling should act as a trigger line
  }

  void Update()
  {
    if (!listening && spawner != null && spawner.InitialSpawnCompleted)
      listening = true; // start listening AFTER initial spawn completes
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    if (!listening ) return;

    // Treat anything with BoombaProperties in its hierarchy as a Boomba
    var props = other.GetComponentInParent<BoombaProperties>();
    if (props == null) return;

    OnBoombaCrossedCeiling?.Invoke(props);
  }
}
