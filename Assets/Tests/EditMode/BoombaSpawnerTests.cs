using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

[TestFixture]
public class BoombaSpawnerTests
{
  private BoombaSpawner spawner;

  [SetUp]
  public void Setup()
  {
    // Create BoombaPool GameObject and attach component
    GameObject poolObject = new GameObject("BoombaPool");
    BoombaPool boombaPool = poolObject.AddComponent<BoombaPool>();
    boombaPool.boombaPrefab = CreateFakeBoombaPrefab();
    boombaPool.poolSize = 10;

    // Initialize to initialize pool
    boombaPool.Initialize();

    // Create BoombaSpawner GameObject and attach component
    GameObject spawnerObject = new GameObject("BoombaSpawner");
    spawner = spawnerObject.AddComponent<BoombaSpawner>();

    // Assign BoombaPool reference
    spawner.boombaPool = boombaPool;

    // Provide at least one variant
    BoombaVariant testVariant = new BoombaVariant
    {
      size = 1f,
      value = 1,
      sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,1,1), Vector2.zero)
    };
    spawner.boombaVariants = new List<BoombaVariant> { testVariant };

    // Call any custom setup if needed
    spawner.Initialize();
  }

  [TearDown]
  public void TearDown()
  {
    // Clean up created GameObjects after each test
    Object.DestroyImmediate(spawner.gameObject);
    Object.DestroyImmediate(spawner.boombaPool.gameObject);
  }

  [Test]
  public void SpawnBoombaWithSpecificValue_CreatesBoomba()
  {
    // Arrange
    Vector2 spawnPos = new Vector2(0, 0);
    int valueToSpawn = 1;

    // Act
    GameObject spawnedBoomba = spawner.SpawnBoombaWithValue(spawnPos, valueToSpawn);

    // Assert
    Assert.IsNotNull(spawnedBoomba, "No boomba was spawned");
    Assert.AreEqual(spawnPos, (Vector2)spawnedBoomba.transform.position, "Spawn position mismatch");

    BoombaProperties props = spawnedBoomba.GetComponent<BoombaProperties>();
    Assert.IsNotNull(props, "BoombaProperties not found on spawned boomba");
    Assert.AreEqual(valueToSpawn, props.Value, "Boomba value mismatch");

    LogAssert.NoUnexpectedReceived(); // Ensure no errors were logged
  }

  /// <summary>
  /// Helper method to create a mock boomba prefab for testing.
  /// </summary>
  private GameObject CreateFakeBoombaPrefab()
  {
    GameObject fakeBoomba = new GameObject("FakeBoomba");
    fakeBoomba.AddComponent<SpriteRenderer>();
    BoombaProperties props = fakeBoomba.AddComponent<BoombaProperties>();

    // Add TextMeshPro and assign
    GameObject textGO = new GameObject("ValueText");
    textGO.transform.SetParent(fakeBoomba.transform);
    var tmp = textGO.AddComponent<TextMeshProUGUI>();
    props.valueText = tmp;

    return fakeBoomba;
  }

  private class MockBoombaSpawner : BoombaSpawner
  {
      public bool WasCalled { get; private set; } = false;
      public Vector2 LastSpawnPosition { get; private set; }

      public override GameObject SpawnBoombaWithValue(Vector2 position, int value)
      {
          WasCalled = true;
          LastSpawnPosition = position;
          return new GameObject("MockBoomba");
      }
  }
}
