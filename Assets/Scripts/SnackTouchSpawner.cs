using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// What it does: Manages the "held and released" spawning of Snacks via mouse/touch input.
// What it's used for: Lets the player position and drop Snacks into the playfield, similar to how Boombas are spawned.
public class SnackTouchSpawner : MonoBehaviour
{
  public BoombaPool boombaPool;  // assign in Inspector

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
  private int _lastPickedValue = -1;

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

    ApplyVariant(currentSnack, variant);
    FreezeSnack(currentSnack);
  }

  // What it does: Drops the currently held snack at the given screen X position and arms a landing callback.
  // What it's used for: Converts click/touch coordinates into world space, moves the snack there, and lets physics take over.
  void ReleaseAtScreen(Vector2 screenPos)
  {
    if (!cam || currentSnack == null) return;

    var gm = GameManager.Instance;
    if (gm != null && (gm.IsGameOver || gm.IsInteractionLocked))
        return;

    Vector2 world = cam.ScreenToWorldPoint(screenPos);
    float x = Mathf.Clamp(world.x, spawnAreaMin.x, spawnAreaMax.x);

    // Move the hanging snack to the chosen X (keep spawnY)
    currentSnack.transform.position = new Vector2(x, spawnY);

    ReleaseSnack(currentSnack);
    AttachFirstCollisionReporter(currentSnack);

    waitingForLanding = true;
    currentSnack = null; // No longer hanging; we'll spawn another after the callback
  }

  // What it does: Sets a snack to Kinematic mode so it hangs in place without physics.
  // What it's used for: Freezes the held snack above the playfield until the player releases it.
  private void FreezeSnack(GameObject snack)
  {
    var rb = snack.GetComponent<Rigidbody2D>();
    if (rb)
    {
      rb.linearVelocity = Vector2.zero;
      rb.angularVelocity = 0f;
      rb.bodyType = RigidbodyType2D.Kinematic;
    }
  }

  // What it does: Sets a snack to Dynamic mode so physics takes over.
  // What it's used for: Releases the snack to fall and interact with the game world.
  private void ReleaseSnack(GameObject snack)
  {
    var rb = snack.GetComponent<Rigidbody2D>();
    if (rb)
    {
      rb.bodyType = RigidbodyType2D.Dynamic;
      rb.linearVelocity = Vector2.zero;
      rb.angularVelocity = 0f;
    }
  }

  // What it does: Adds or reuses a one-shot collision reporter to call back when the snack first lands/merges.
  // What it's used for: Signals when it's safe to spawn another held snack after the current one interacts with the board.
  // In SnackTouchSpawner.cs - update AttachFirstCollisionReporter
  void AttachFirstCollisionReporter(GameObject go)
  {
    var reporter = go.GetComponent<BoombaLandingReporter>();
    if (reporter == null) reporter = go.AddComponent<BoombaLandingReporter>();

    reporter.Init(() =>
    {
      // Wait a frame to let merges complete before spawning next snack
      StartCoroutine(DelayedUnlockSpawning());
    });
  }

  // Add this new method
  IEnumerator DelayedUnlockSpawning()
  {
    yield return new WaitForEndOfFrame();
    waitingForLanding = false; // Allow the next hanging spawn
  }

  // What it does: Applies visual and physical properties from a SnackVariant onto an instantiated snack GameObject.
  // What it's used for: Configures sprite, scale, collider radius, and value so different snack types behave and look distinct.
  void ApplyVariant(GameObject go, SnackVariant v)
  {
    if (go == null || v == null) return;

    SetupSnackVisual(go, v);
    SetupSnackProperties(go, v);
  }

  // What it does: Configures the visual rendering for a snack.
  // What it's used for: Sets up the sprite/animated visual and scales it appropriately.
  private void SetupSnackVisual(GameObject go, SnackVariant v)
  {
    Transform visualRoot = go.transform.Find("Art");
    if (visualRoot == null)
    {
      Debug.LogWarning("SnackTouchSpawner: no 'Art' child found on snackPrefab; using root instead.");
      visualRoot = go.transform;
    }

    ClearPreviousVisuals(visualRoot);
    InstantiateVisualPrefab(visualRoot, v);
  }

  // What it does: Removes any existing visual children from the visual root.
  // What it's used for: Clears old visuals before applying new ones (important for pooling or reuse).
  private void ClearPreviousVisuals(Transform visualRoot)
  {
    for (int i = visualRoot.childCount - 1; i >= 0; i--)
      Destroy(visualRoot.GetChild(i).gameObject);
  }

  // What it does: Instantiates and scales the visual prefab for a snack variant.
  // What it's used for: Creates the actual visible representation of the snack.
  private void InstantiateVisualPrefab(Transform visualRoot, SnackVariant v)
  {
    if (v.visualPrefab == null)
    {
      Debug.LogWarning($"SnackTouchSpawner: SnackVariant {v.value} has no visualPrefab assigned.");
      return;
    }

    GameObject visual = Instantiate(v.visualPrefab, visualRoot);
    visual.transform.localPosition = Vector3.zero;
    visual.transform.localRotation = Quaternion.identity;
    
    float s = Mathf.Max(0.05f, v.size);
    visual.transform.localScale = new Vector3(s, s, 1f);
  }

  // What it does: Sets the value property on a snack's BoombaProperties component.
  // What it's used for: Initializes the data that other systems read from the snack.
  private void SetupSnackProperties(GameObject go, SnackVariant v)
  {
    var props = go.GetComponent<BoombaProperties>();
    if (props) props.SetValue(v.value);
  }

  // What it does: Clears any currently held snack and resets state so a new one can be armed.
  // What it's used for: Called on game restart to ensure there are no leftover hanging snacks from the previous run.
  public void ResetHeldAndArm()
  {
    if (currentSnack)
    {
      Destroy(currentSnack);
      currentSnack = null;
    }

    waitingForLanding = false;
  }

  // What it does: Picks a SnackVariant whose value matches one of the currently active Boombas.
  // What it's used for: Ensures spawned snacks can merge with existing boombas on the board.
  SnackVariant PickVariantByActiveBoombas()
{
    if (snackVariants == null || snackVariants.Count == 0)
        return null;

    var activeValues = boombaPool?.GetActiveBoombaValues();
    var candidates = GetCandidateVariants(activeValues);

    // If more than one candidate, exclude the last picked to prevent streaks
    if (candidates.Count > 1)
        candidates = candidates.FindAll(v => v.value != _lastPickedValue);

    int index = Random.Range(0, candidates.Count);
    var picked = candidates[index];
    _lastPickedValue = picked.value;
    return picked;
}

  // What it does: Returns a list of snack variants that match active boomba values, or all variants if no matches.
  // What it's used for: Filters the available snack pool based on what's currently on the board.
  private List<SnackVariant> GetCandidateVariants(HashSet<int> activeValues)
  {
    // No active boombas? Allow all variants
    if (activeValues == null || activeValues.Count == 0)
      return snackVariants;

    // Filter to matching values
    var matches = snackVariants.FindAll(v => activeValues.Contains(v.value));
    
    // Fallback to all if no matches (safety)
    return matches.Count > 0 ? matches : snackVariants;
  }
}
