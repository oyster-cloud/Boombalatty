using UnityEngine;

/// <summary>
/// Spawns a held (static) ball and doesn’t spawn the next one
/// until the last dropped ball collides with something.
/// </summary>
public class BallTouchSpawner : MonoBehaviour
{
  [Header("Dependencies")]
  public BallSpawner ballSpawner;

  [Header("Testing")]
  public bool requireInitialSpawnCompleted = false; // test-only hook

  [Header("Spawn Settings")]
  public float spawnY = 5f;

  private GameObject currentBall;     // held (static) ball waiting to drop
  private bool waitingForLanding = false; // true after releasing a ball, until it collides
  private bool canSpawnHeld = false;

  void OnEnable()
  {
    if (ballSpawner != null)
      ballSpawner.OnInitialSpawnComplete += HandleInitialSpawnComplete;
  }

  void OnDisable()
  {
    if (ballSpawner != null)
      ballSpawner.OnInitialSpawnComplete -= HandleInitialSpawnComplete;
  }

  private void HandleInitialSpawnComplete()
  {
    canSpawnHeld = true;
  }

  void Update()
  {
    // If event hasn’t fired yet, do nothing
    if (!canSpawnHeld) return;

    // If we’re allowed to spawn and there isn't a held ball yet, make one
    if (!waitingForLanding && currentBall == null)
    {
      currentBall = SpawnHeldBall();
      return; // don’t process click on same frame
    }

    // Release on click
    if (currentBall != null && Input.GetMouseButtonDown(0))
    {
      ReleaseCurrentBall();
    }
  }

  private GameObject SpawnHeldBall()
  {
    // Safety checks
    if (ballSpawner == null || ballSpawner.ballVariants == null || ballSpawner.ballVariants.Count == 0)
    {
      Debug.LogError("BallTouchSpawner: BallSpawner or variants not set.");
      return null;
    }

    // Pick one of the first 3 variants (or fewer if not available)
    int idx = Random.Range(0, Mathf.Min(3, ballSpawner.ballVariants.Count));
    int value = ballSpawner.ballVariants[idx].value;

    // Hold position: fixed Y, centered X (clamped to bounds just in case)
    float startX = 0f;
    float clampedX = Mathf.Clamp(startX, ballSpawner.spawnAreaMin.x, ballSpawner.spawnAreaMax.x);
    Vector2 pos = new Vector2(clampedX, spawnY);

    // Spawn via BallSpawner
    GameObject ball = ballSpawner.SpawnBallWithValue(pos, value);
    if (ball == null) return null;

    // Freeze so it doesn't fall yet
    var rb = ball.GetComponent<Rigidbody2D>();
    if (rb != null) rb.bodyType = RigidbodyType2D.Static;

    return ball;
  }

  public void ReleaseCurrentBall()
  {
    if (currentBall == null) return;

    // Move X to click/touch once (no tracking)
    if (Camera.main != null)
    {
      Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      float clampedX = Mathf.Clamp(world.x, ballSpawner.spawnAreaMin.x, ballSpawner.spawnAreaMax.x);
      currentBall.transform.position = new Vector2(clampedX, spawnY);
    }

    var rb = currentBall.GetComponent<Rigidbody2D>();
    if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

    // Add a one-shot landing reporter so we know when to spawn the next ball
    var reporter = currentBall.AddComponent<BallLandingReporter>();
    reporter.Init(OnBallLanded);

    // We’re now waiting for that collision before making a new ball
    waitingForLanding = true;
    currentBall = null;
  }

  // Called by BallLandingReporter on first collision
  private void OnBallLanded()
  {
    waitingForLanding = false; // next frame Update() will spawn a new held ball
  }

  // Spawns the held ball immediately (for tests). Returns the spawned GameObject.
  public GameObject ForceSpawnHeldBallForTest()
  {
    if (currentBall == null)
    {
      currentBall = SpawnHeldBall();
    }
    return currentBall;
  }
}
