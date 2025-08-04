using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

[TestFixture]
public class BallSpawnerTests
{
  private BallSpawner spawner;

  [SetUp]
  public void Setup()
  {
    // Create BallPool GameObject and attach component
    GameObject poolObject = new GameObject("BallPool");
    BallPool ballPool = poolObject.AddComponent<BallPool>();
    ballPool.ballPrefab = CreateFakeBallPrefab();
    ballPool.poolSize = 10;

    // Initialize to initialize pool
    ballPool.Initialize();

    // Create BallSpawner GameObject and attach component
    GameObject spawnerObject = new GameObject("BallSpawner");
    spawner = spawnerObject.AddComponent<BallSpawner>();

    // Assign BallPool reference
    spawner.ballPool = ballPool;

    // Provide at least one variant
    BallVariant testVariant = new BallVariant
    {
      size = 1f,
      value = 1,
      sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,1,1), Vector2.zero)
    };
    spawner.ballVariants = new List<BallVariant> { testVariant };

    // Call any custom setup if needed
    spawner.Initialize();
  }

  [TearDown]
  public void TearDown()
  {
    // Clean up created GameObjects after each test
    Object.DestroyImmediate(spawner.gameObject);
    Object.DestroyImmediate(spawner.ballPool.gameObject);
  }

  [Test]
  public void SpawnBallWithSpecificValue_CreatesBall()
  {
    // Arrange
    Vector2 spawnPos = new Vector2(0, 0);
    int valueToSpawn = 1;

    // Act
    GameObject spawnedBall = spawner.SpawnBallWithValue(spawnPos, valueToSpawn);

    // Assert
    Assert.IsNotNull(spawnedBall, "No ball was spawned");
    Assert.AreEqual(spawnPos, (Vector2)spawnedBall.transform.position, "Spawn position mismatch");

    BallProperties props = spawnedBall.GetComponent<BallProperties>();
    Assert.IsNotNull(props, "BallProperties not found on spawned ball");
    Assert.AreEqual(valueToSpawn, props.value, "Ball value mismatch");

    LogAssert.NoUnexpectedReceived(); // Ensure no errors were logged
  }

  /// <summary>
  /// Helper method to create a mock ball prefab for testing.
  /// </summary>
  private GameObject CreateFakeBallPrefab()
  {
    GameObject fakeBall = new GameObject("FakeBall");
    fakeBall.AddComponent<SpriteRenderer>();
    BallProperties props = fakeBall.AddComponent<BallProperties>();

    // Add TextMeshPro and assign
    GameObject textGO = new GameObject("ValueText");
    textGO.transform.SetParent(fakeBall.transform);
    var tmp = textGO.AddComponent<TextMeshProUGUI>();
    props.valueText = tmp;

    return fakeBall;
  }
}
