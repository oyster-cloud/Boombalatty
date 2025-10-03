using UnityEngine;
using System.Collections.Generic;

public class SnackTouchSpawner : MonoBehaviour
{
  [Header("Prefab")]
  [SerializeField] GameObject snackPrefab;

  [Header("Variants")]
  [SerializeField] List<SnackVariant> snackVariants = new();  // set in Inspector

  [Header("Spawn gating (optional)")]
  [SerializeField] bool requireInitialSpawnCompleted = false;
  [SerializeField] BoombaSpawner initialSpawnSource;          // read-only gate

  [Header("Placement")]
  [SerializeField] float spawnY = 4f;
  [SerializeField] Vector2 spawnAreaMin = new(-2.5f, 0);
  [SerializeField] Vector2 spawnAreaMax = new( 2.5f, 0);

  Camera cam;

  void Awake()
  {
    cam = Camera.main;
    if (!snackPrefab) Debug.LogError("SnackTouchSpawner: assign snackPrefab.");
  }

  void Update()
  {
    if (requireInitialSpawnCompleted && initialSpawnSource && !initialSpawnSource.InitialSpawnCompleted)
      return;

    if (Input.GetMouseButtonDown(0))
      SpawnAtScreen(Input.mousePosition);

    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
      SpawnAtScreen(Input.GetTouch(0).position);
  }

  void SpawnAtScreen(Vector2 screenPos)
  {
    if (!cam || !snackPrefab) return;

    Vector2 world = cam.ScreenToWorldPoint(screenPos);
    float x = Mathf.Clamp(world.x, spawnAreaMin.x, spawnAreaMax.x);
    Vector2 pos = new Vector2(x, spawnY);

    // Pick a variant (random). If you want “by value”, add a SpawnSnackWithValue like Boomba.
    SnackVariant variant = (snackVariants.Count > 0)
      ? snackVariants[Random.Range(0, snackVariants.Count)]
      : null;

    var go = Instantiate(snackPrefab, pos, Quaternion.identity);
    if (go && !go.activeSelf) go.SetActive(true);

    ApplyVariant(go, variant);
  }

  void ApplyVariant(GameObject go, SnackVariant v)
  {
    if (go == null || v == null) return;

    // Sprite on child "Art" (same structure as your Boomba prefab)
    var sr = go.GetComponentInChildren<SpriteRenderer>(true);
    if (sr) {
      sr.sprite = v.sprite;
      float s = Mathf.Max(0.05f, v.imageScale);
      sr.transform.localScale = new Vector3(s, s, 1f);
    }

    // Physics size
    var circle = go.GetComponent<CircleCollider2D>();
    if (circle) circle.radius = Mathf.Max(0.01f, v.size * 0.5f);

    var box = go.GetComponent<BoxCollider2D>();
    if (box) box.size = new Vector2(Mathf.Max(0.01f, v.size), Mathf.Max(0.01f, v.size));
  }
}
