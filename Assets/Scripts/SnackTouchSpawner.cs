using UnityEngine;
using System.Collections.Generic;

public class SnackTouchSpawner : MonoBehaviour
{
  [Header("Prefab")]
  [SerializeField] GameObject snackPrefab;

  [Header("Variants")]
  [SerializeField] List<SnackVariant> snackVariants = new();  // set in Inspector

  [Header("Spawn gating (optional)")]
  [SerializeField] BoombaSpawner initialSpawnSource;          // read-only gate

  [Header("Placement")]
  [SerializeField] float spawnY = 4f;
  [SerializeField] Vector2 spawnAreaMin = new(-2.5f, 0);
  [SerializeField] Vector2 spawnAreaMax = new( 2.5f, 0);

  Camera cam;

  // hold/release state
  GameObject currentSnack;
  bool waitingForLanding;

  void Awake()
  {
    cam = Camera.main;
    if (!snackPrefab) Debug.LogError("SnackTouchSpawner: assign snackPrefab.");
  }

  void OnEnable()
  {
    Services.SnackTouchSpawner = this;
  }

  void OnDisable()
  {
    if (Services.SnackTouchSpawner == this)
      Services.SnackTouchSpawner = null;
  }

  void Update()
  {
    if (initialSpawnSource && !initialSpawnSource.InitialSpawnCompleted)
      return;

    // Ensure there is exactly one hanging snack when not waiting for landing
    if (!waitingForLanding && currentSnack == null)
      SpawnHeldSnack();

    // Only allow release if we have a hanging snack
    if (currentSnack == null) return;

    if (Input.GetMouseButtonDown(0))
      ReleaseAtScreen(Input.mousePosition);

    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
      ReleaseAtScreen(Input.GetTouch(0).position);
  }

  void SpawnHeldSnack()
  {
    if (!snackPrefab) return;

    // center X between bounds
    float centerX = 0.5f * (spawnAreaMin.x + spawnAreaMax.x);
    Vector2 pos = new Vector2(centerX, spawnY);

    // pick a variant up front so the player sees the snack while it hangs
    SnackVariant variant = (snackVariants.Count > 0)
      ? snackVariants[Random.Range(0, snackVariants.Count)]
      : null;

    currentSnack = Instantiate(snackPrefab, pos, Quaternion.identity);
    if (!currentSnack.activeSelf) currentSnack.SetActive(true);

    // apply art/size like your original spawner
    ApplyVariant(currentSnack, variant);

    // make it hang in place
    var rb = currentSnack.GetComponent<Rigidbody2D>();
    if (rb)
    {
      rb.linearVelocity = Vector2.zero;
      rb.angularVelocity = 0f;
      rb.bodyType = RigidbodyType2D.Kinematic; // ignore gravity until release
    }
  }

  void ReleaseAtScreen(Vector2 screenPos)
  {
    if (!cam || currentSnack == null) return;

    Vector2 world = cam.ScreenToWorldPoint(screenPos);
    float x = Mathf.Clamp(world.x, spawnAreaMin.x, spawnAreaMax.x);

    // move the hanging snack to the chosen X (keep spawnY)
    currentSnack.transform.position = new Vector2(x, spawnY);

    // switch to dynamic so physics takes over
    var rb = currentSnack.GetComponent<Rigidbody2D>();
    if (rb)
    {
      rb.bodyType = RigidbodyType2D.Dynamic;
      rb.linearVelocity = Vector2.zero;      // clean start
      rb.angularVelocity = 0f;
    }

    // listen for the FIRST collision/merge, then queue the next held snack
    AttachFirstCollisionReporter(currentSnack);

    waitingForLanding = true;
    currentSnack = null; // no longer hanging; we’ll spawn another after the callback
  }

  void AttachFirstCollisionReporter(GameObject go)
  {
    // Reuse your reporter if you already have BoombaLandingReporter in the project.
    // If not, add a tiny one-shot component with OnCollisionEnter2D that calls the lambda.
    var reporter = go.GetComponent<BoombaLandingReporter>();
    if (reporter == null) reporter = go.AddComponent<BoombaLandingReporter>();

    // reporter.Init(Action onFirstCollision)
    reporter.Init(() =>
    {
      waitingForLanding = false; // allow the next hanging spawn
      // (Optionally: SFX/UI for "snack landed/merged")
    });
  }

  // void SpawnAtScreen(Vector2 screenPos)
  // {
  //   if (!cam || !snackPrefab) return;

  //   Vector2 world = cam.ScreenToWorldPoint(screenPos);
  //   float x = Mathf.Clamp(world.x, spawnAreaMin.x, spawnAreaMax.x);
  //   Vector2 pos = new Vector2(x, spawnY);

  //   // Pick a variant (random). If you want “by value”, add a SpawnSnackWithValue like Boomba.
  //   SnackVariant variant = (snackVariants.Count > 0)
  //     ? snackVariants[Random.Range(0, snackVariants.Count)]
  //     : null;

  //   var go = Instantiate(snackPrefab, pos, Quaternion.identity);
  //   if (go && !go.activeSelf) go.SetActive(true);

  //   ApplyVariant(go, variant);
  // }

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

    var props = go.GetComponent<BoombaProperties>();
    if (props) props.SetValue(v.value);
  }

  public void ResetHeldAndArm()
  {
    // kill any held instance
    if (currentSnack)
    {
      Destroy(currentSnack);
      currentSnack = null;
    }

    // reset internal flags so Start/Update will spawn the held snack again
    waitingForLanding = false;
    // Optionally force immediate held-spawn here:
    // SpawnHeldSnack();  // if your flow expects it right away
  }
}
