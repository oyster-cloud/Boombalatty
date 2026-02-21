using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// What it does: Manages the "held and released" spawning of Snacks via mouse/touch input.
// What it's used for: Lets the player position and drop Snacks into the playfield, similar to how Boombas are spawned.
public class SnackTouchSpawner : MonoBehaviour
{
  public BoombaPool boombaPool;  // assign in Inspector
  // private readonly HashSet<int> _activeBoombaValues = new HashSet<int>();

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

  // What it does: Caches the main camera reference and validates that a snack prefab has been assigned.
  // What it's used for: Ensures the spawner can convert input screen positions to world space and knows what to instantiate.
  void Awake()
  {
    cam = Camera.main;
    if (!snackPrefab) Debug.LogError("SnackTouchSpawner: assign snackPrefab.");
  }

  // What it does: Registers this spawner in the Services locator when enabled.
  // What it's used for: Allows other systems (like GameManager) to find and control snack spawning.
  void OnEnable()
  {
    Services.SnackTouchSpawner = this;
  }

  // What it does: Clears the Services locator entry when disabled if it still points to this instance.
  // What it's used for: Prevents stale references when scenes or objects are unloaded.
  void OnDisable()
  {
    if (Services.SnackTouchSpawner == this)
      Services.SnackTouchSpawner = null;
  }

  // What it does: Drives the main snack spawning loop—spawns a hanging snack, then drops it on click/touch when allowed.
  // What it's used for: Implements the core player input behavior for creating and releasing Snacks into the world.
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

  // What it does: Spawns a snack at a fixed Y position, centers it between bounds, and freezes it in place.
// What it's used for: Creates the visible "held" snack above the playfield that the player will later drop.
  void SpawnHeldSnack()
  {
    if (!snackPrefab) return;

    float centerX = 0.5f * (spawnAreaMin.x + spawnAreaMax.x);
    Vector2 pos = new Vector2(centerX, spawnY);

    SnackVariant variant = PickVariantByActiveBoombas();

    currentSnack = Instantiate(snackPrefab, pos, Quaternion.identity);
    if (!currentSnack.activeSelf) currentSnack.SetActive(true);

    // apply art/size like your original spawner
    ApplyVariant(currentSnack, variant);

    // make it hang in place...
    var rb = currentSnack.GetComponent<Rigidbody2D>();
    if (rb)
    {
      rb.linearVelocity = Vector2.zero;
      rb.angularVelocity = 0f;
      rb.bodyType = RigidbodyType2D.Kinematic; // ignore gravity until release
    }
  }

  // What it does: Drops the currently held snack at the given screen X position and arms a landing callback.
  // What it's used for: Converts click/touch coordinates into world space, moves the snack there, and lets physics take over.
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

  // What it does: Adds or reuses a one-shot collision reporter to call back when the snack first lands/merges.
  // What it's used for: Signals when it's safe to spawn another held snack after the current one interacts with the board.
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

  // What it does: Applies visual and physical properties from a SnackVariant onto an instantiated snack GameObject.
  // What it's used for: Configures sprite, scale, collider radius, and value so different snack types behave and look distinct.
  void ApplyVariant(GameObject go, SnackVariant v)
  {
    if (go == null || v == null) return;

    // 1. Find the visual root (Art) under this snack
    Transform visualRoot = go.transform.Find("Art");
    if (visualRoot == null)
    {
        Debug.LogWarning("SnackTouchSpawner: no 'Art' child found on snackPrefab; using root instead.");
        visualRoot = go.transform;
    }

    // 2. Clear any previous visuals
    for (int i = visualRoot.childCount - 1; i >= 0; i--)
    {
        Destroy(visualRoot.GetChild(i).gameObject);
    }

    // 3. Instantiate the visual prefab and scale it
    if (v.visualPrefab != null)
    {
        GameObject visual = Instantiate(v.visualPrefab, visualRoot);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        // single slider that scales both sprite *and* colliders
        float s = Mathf.Max(0.05f, v.size);   // use SnackVariant.size as the master scale
        visual.transform.localScale = new Vector3(s, s, 1f);
    }
    else
    {
        Debug.LogWarning($"SnackTouchSpawner: SnackVariant {v.value} has no visualPrefab assigned.");
    }

    // 4. Set the snack's logical value
    var props = go.GetComponent<BoombaProperties>();
    if (props) props.SetValue(v.value);
  }

  // What it does: Clears any currently held snack and resets state so a new one can be armed.
  // What it's used for: Called on game restart to ensure there are no leftover hanging snacks from the previous run.
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

  // Pick a SnackVariant whose value matches one of the currently active Boombas.
  // If no Boombas are active or no variants match, we fall back to the full list.
  SnackVariant PickVariantByActiveBoombas()
  {
    if (snackVariants == null || snackVariants.Count == 0)
      return null;

    HashSet<int> activeValues = null;

    if (boombaPool != null)
    {
      // reuse the private buffer to avoid allocations:
      activeValues = boombaPool.GetActiveBoombaValues();
    }

    List<SnackVariant> candidates;

    if (activeValues != null && activeValues.Count > 0)
    {
      // only variants whose value matches an active Boomba value
      candidates = snackVariants.FindAll(v => activeValues.Contains(v.value));

      // Safety: if for some reason there's no matching variant, fall back to all
      if (candidates.Count == 0) {
        string valuesLog = (activeValues == null)
            ? "NULL"
            : string.Join(",", activeValues);

        candidates = snackVariants;
      }
    }
    else
    {
      // No active Boombas → allow all snack values
      candidates = snackVariants;
    }

    int index = Random.Range(0, candidates.Count);
    return candidates[index];
  }

}
