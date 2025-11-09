// GameOverZone.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GameOverZone : MonoBehaviour
{
  [SerializeField] string snackLiveLayer = "Snack";
  [SerializeField] string boombaLiveLayer = "Boomba";

  int snackLayer, boombaLayer;

  void Reset()
  {
    var col = GetComponent<Collider2D>();
    if (col) col.isTrigger = true;
    gameObject.tag = "GameOverZone"; // optional but convenient
  }

  void Awake()
  {
    snackLayer = LayerMask.NameToLayer(snackLiveLayer);
    boombaLayer = LayerMask.NameToLayer(boombaLiveLayer);
  }

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
