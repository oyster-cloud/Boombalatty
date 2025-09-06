using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic; // <-- add at top of file if missing

public class AnimalTouchSpawnerTests
{
  private GameObject cameraObj;
  private Camera mainCamera;

  private GameObject spawnerObj;
  private AnimalTouchSpawner touchSpawner;
  private MockAnimalSpawner mockSpawner;

  [SetUp]
  public void SetUp()
  {
    // Camera
    cameraObj = new GameObject("MainCamera");
    mainCamera = cameraObj.AddComponent<Camera>();
    cameraObj.tag = "MainCamera";

    // // Spawners
    var spawnerGO = new GameObject("AnimalSpawner");
    mockSpawner = spawnerGO.AddComponent<MockAnimalSpawner>();

    spawnerObj = new GameObject("AnimalTouchSpawner");
    touchSpawner = spawnerObj.AddComponent<AnimalTouchSpawner>();
    touchSpawner.ballSpawner = mockSpawner;

    // If your AnimalTouchSpawner has this flag, disable the gate for tests.
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
  public IEnumerator AnimalSpawnsAutomatically_And_ReleasesOnCommand()
  {
    // Spawn deterministically (no reliance on Update timing)
    var ball = touchSpawner.ForceSpawnHeldAnimalForTest();
    Assert.IsNotNull(ball, "Expected a ball to be auto-spawned.");

    var rb = ball.GetComponent<Rigidbody2D>();
    Assert.IsNotNull(rb, "Rigidbody2D should be present on spawned ball.");
    Assert.AreEqual(RigidbodyType2D.Static, rb.bodyType, "Animal should start as Static (held).");

    // Release (drops from click X in your impl)
    touchSpawner.ReleaseCurrentAnimal();
    yield return null; // let physics update a frame

    Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType, "Animal should be Dynamic after release.");
  }
}

// Mock AnimalSpawner
public class MockAnimalSpawner : AnimalSpawner
{
  public GameObject LastSpawned { get; private set; }

  // Stop base Start() from running its coroutine
  void Start() { }

  void Awake()
  {
    // Seed at least one variant so TouchSpawner can pick a value
    if (ballVariants == null || ballVariants.Count == 0)
    {
      ballVariants = new List<AnimalVariant> {
        new AnimalVariant {
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

  public override GameObject SpawnAnimalWithValue(Vector2 position, int value)
  {
    var go = new GameObject("MockAnimal");
    go.transform.position = position;
    go.AddComponent<Rigidbody2D>();
    go.AddComponent<CircleCollider2D>();
    LastSpawned = go;
    return go;
  }
}
