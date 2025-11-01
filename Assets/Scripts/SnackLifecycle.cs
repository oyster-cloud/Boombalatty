using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    preLandLayer = LayerMask.NameToLayer(preLandLayerName);
    snackLayer   = LayerMask.NameToLayer(snackLayerName);
    if (preLandLayer != -1)
      gameObject.layer = preLandLayer;
  }

  void OnEnable()
  {
    startY = transform.position.y;
    snackLayerEnabled = false;
  }

  void FixedUpdate()
  {
    if (snackLayerEnabled) return;
    if (!rb || rb.bodyType != RigidbodyType2D.Dynamic) return;

    // Once it falls below the drop distance, enable Snack layer
    if ((startY - transform.position.y) >= dropDistanceToEnable)
      EnableSnackLayer();
  }

  void EnableSnackLayer()
  {
    if (snackLayer == -1) return;
    gameObject.layer = snackLayer;
    snackLayerEnabled = true;
    // Debug.Log($"Snack {name} is now on Snack layer at y={transform.position.y}");
  }
}
