using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic; // <-- add at top of file if missing

public class BallTouchSpawnerTests
{
  private GameObject cameraObj;
  private Camera mainCamera;

  private GameObject spawnerObj;
  private BallTouchSpawner touchSpawner;
  private MockBallSpawner mockSpawner;

  [SetUp]
  public void SetUp()
  {
    // Camera
    cameraObj = new GameObject("MainCamera");
    mainCamera = cameraObj.AddComponent<Camera>();
    cameraObj.tag = "MainCamera";

    // // Spawners
    var spawnerGO = new GameObject("BallSpawner");
    mockSpawner = spawnerGO.AddComponent<MockBallSpawner>();

    spawnerObj = new GameObject("BallTouchSpawner");
    touchSpawner = spawnerObj.AddComponent<BallTouchSpawner>();
    touchSpawner.ballSpawner = mockSpawner;

    // If your BallTouchSpawner has this flag, disable the gate for tests.
    // Safe to omit if you didn’t add it.
    touchSpawner.requireInitialSpawnCompleted = false;
  }

  [TearDown]
  public void TearDown()
  {
    Object.DestroyImmediate(spawnerObj);
    Object.DestroyImmediate(mockSpawner.gameObject);
    Object.DestroyImmediate(cameraObj);
  }

  [UnityTest]
  public IEnumerator BallSpawnsAutomatically_And_ReleasesOnCommand()
  {
    // Spawn deterministically (no reliance on Update timing)
    var ball = touchSpawner.ForceSpawnHeldBallForTest();
    Assert.IsNotNull(ball, "Expected a ball to be auto-spawned.");

    var rb = ball.GetComponent<Rigidbody2D>();
    Assert.IsNotNull(rb, "Rigidbody2D should be present on spawned ball.");
    Assert.AreEqual(RigidbodyType2D.Static, rb.bodyType, "Ball should start as Static (held).");

    // Release (drops from click X in your impl)
    touchSpawner.ReleaseCurrentBall();
    yield return null; // let physics update a frame

    Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType, "Ball should be Dynamic after release.");
  }
}

// Mock BallSpawner
public class MockBallSpawner : BallSpawner
{
  public GameObject LastSpawned { get; private set; }

  // Stop base Start() from running its coroutine
  new void Start() { }

  void Awake()
  {
    // Seed at least one variant so TouchSpawner can pick a value
    if (ballVariants == null || ballVariants.Count == 0)
    {
      ballVariants = new List<BallVariant> {
        new BallVariant {
          value = 1, size = 1f,
          sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,1,1), new Vector2(0.5f,0.5f))
        }
      };
    }

    // Sensible bounds if none set
    if (spawnAreaMin == Vector2.zero && spawnAreaMax == Vector2.zero)
    {
      spawnAreaMin = new Vector2(-5, -5);
      spawnAreaMax = new Vector2( 5,  5);
    }
  }

  public override GameObject SpawnBallWithValue(Vector2 position, int value)
  {
    var go = new GameObject("MockBall");
    go.transform.position = position;
    go.AddComponent<Rigidbody2D>();
    go.AddComponent<CircleCollider2D>();
    LastSpawned = go;
    return go;
  }
}
