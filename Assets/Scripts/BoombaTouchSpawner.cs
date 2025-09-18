using UnityEngine;

/// <summary>
/// Spawns a held (static) boomba and doesn’t spawn the next one
/// until the last dropped boomba collides with something.
/// </summary>
public class BoombaTouchSpawner : MonoBehaviour
{
  [Header("Dependencies")]
  public BoombaSpawner boombaSpawner;

  [Header("Testing")]
  public bool requireInitialSpawnCompleted = false; // test-only hook

  [Header("Spawn Settings")]
  public float spawnY = 5f;

  private GameObject currentBoomba;     // held (static) boomba waiting to drop
  private bool waitingForLanding = false; // true after releasing a boomba, until it collides
  private bool canSpawnHeld = false;

  void OnEnable()
  {
    if (boombaSpawner != null)
      boombaSpawner.OnInitialSpawnComplete += HandleInitialSpawnComplete;
  }

  void OnDisable()
  {
    if (boombaSpawner != null)
      boombaSpawner.OnInitialSpawnComplete -= HandleInitialSpawnComplete;
  }

  private void HandleInitialSpawnComplete()
  {
    canSpawnHeld = true;
  }

  void Update()
  {
    // If event hasn’t fired yet, do nothing
    if (!canSpawnHeld) return;

    // If we’re allowed to spawn and there isn't a held boomba yet, make one
    if (!waitingForLanding && currentBoomba == null)
    {
      currentBoomba = SpawnHeldBoomba();
      return; // don’t process click on same frame
    }

    // Release on click
    if (currentBoomba != null && Input.GetMouseButtonDown(0))
    {
      ReleaseCurrentBoomba();
    }
  }

  private GameObject SpawnHeldBoomba()
  {
    // Safety checks
    if (boombaSpawner == null || boombaSpawner.boombaVariants == null || boombaSpawner.boombaVariants.Count == 0)
    {
      Debug.LogError("BoombaTouchSpawner: BoombaSpawner or variants not set.");
      return null;
    }

    // Pick one of the first 3 variants (or fewer if not available)
    int idx = Random.Range(0, Mathf.Min(3, boombaSpawner.boombaVariants.Count));
    int value = boombaSpawner.boombaVariants[idx].value;

    // Hold position: fixed Y, centered X (clamped to bounds just in case)
    float startX = 0f;
    float clampedX = Mathf.Clamp(startX, boombaSpawner.spawnAreaMin.x, boombaSpawner.spawnAreaMax.x);
    Vector2 pos = new Vector2(clampedX, spawnY);

    // Spawn via BoombaSpawner
    GameObject boomba = boombaSpawner.SpawnBoombaWithValue(pos, value);
    if (boomba == null) return null;

    // Freeze so it doesn't fall yet
    var rb = boomba.GetComponent<Rigidbody2D>();
    if (rb != null) rb.bodyType = RigidbodyType2D.Static;

    return boomba;
  }

  public void ReleaseCurrentBoomba()
  {
    if (currentBoomba == null) return;

    // Move X to click/touch once (no tracking)
    if (Camera.main != null)
    {
      Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      float clampedX = Mathf.Clamp(world.x, boombaSpawner.spawnAreaMin.x, boombaSpawner.spawnAreaMax.x);
      currentBoomba.transform.position = new Vector2(clampedX, spawnY);
    }

    var rb = currentBoomba.GetComponent<Rigidbody2D>();
    if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

    // Add a one-shot landing reporter so we know when to spawn the next boomba
    var reporter = currentBoomba.AddComponent<BoombaLandingReporter>();
    reporter.Init(OnBoombaLanded);

    // We’re now waiting for that collision before making a new boomba
    waitingForLanding = true;
    currentBoomba = null;
  }

  // Called by BoombaLandingReporter on first collision
  private void OnBoombaLanded()
  {
    waitingForLanding = false; // next frame Update() will spawn a new held boomba
  }

  // Spawns the held boomba immediately (for tests). Returns the spawned GameObject.
  public GameObject ForceSpawnHeldBoombaForTest()
  {
    if (currentBoomba == null)
    {
      currentBoomba = SpawnHeldBoomba();
    }
    return currentBoomba;
  }
}
