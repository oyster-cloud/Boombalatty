using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

[TestFixture]
public class AnimalSpawnerTests
{
  private AnimalSpawner spawner;

  [SetUp]
  public void Setup()
  {
    // Create AnimalPool GameObject and attach component
    GameObject poolObject = new GameObject("AnimalPool");
    AnimalPool ballPool = poolObject.AddComponent<AnimalPool>();
    ballPool.ballPrefab = CreateFakeAnimalPrefab();
    ballPool.poolSize = 10;

    // Initialize to initialize pool
    ballPool.Initialize();

    // Create AnimalSpawner GameObject and attach component
    GameObject spawnerObject = new GameObject("AnimalSpawner");
    spawner = spawnerObject.AddComponent<AnimalSpawner>();

    // Assign AnimalPool reference
    spawner.ballPool = ballPool;

    // Provide at least one variant
    AnimalVariant testVariant = new AnimalVariant
    {
      size = 1f,
      value = 1,
      sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,1,1), Vector2.zero)
    };
    spawner.ballVariants = new List<AnimalVariant> { testVariant };

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
  public void SpawnAnimalWithSpecificValue_CreatesAnimal()
  {
    // Arrange
    Vector2 spawnPos = new Vector2(0, 0);
    int valueToSpawn = 1;

    // Act
    GameObject spawnedAnimal = spawner.SpawnAnimalWithValue(spawnPos, valueToSpawn);

    // Assert
    Assert.IsNotNull(spawnedAnimal, "No ball was spawned");
    Assert.AreEqual(spawnPos, (Vector2)spawnedAnimal.transform.position, "Spawn position mismatch");

    AnimalProperties props = spawnedAnimal.GetComponent<AnimalProperties>();
    Assert.IsNotNull(props, "AnimalProperties not found on spawned ball");
    Assert.AreEqual(valueToSpawn, props.Value, "Animal value mismatch");

    LogAssert.NoUnexpectedReceived(); // Ensure no errors were logged
  }

  /// <summary>
  /// Helper method to create a mock ball prefab for testing.
  /// </summary>
  private GameObject CreateFakeAnimalPrefab()
  {
    GameObject fakeAnimal = new GameObject("FakeAnimal");
    fakeAnimal.AddComponent<SpriteRenderer>();
    AnimalProperties props = fakeAnimal.AddComponent<AnimalProperties>();

    // Add TextMeshPro and assign
    GameObject textGO = new GameObject("ValueText");
    textGO.transform.SetParent(fakeAnimal.transform);
    var tmp = textGO.AddComponent<TextMeshProUGUI>();
    props.valueText = tmp;

    return fakeAnimal;
  }

  private class MockAnimalSpawner : AnimalSpawner
  {
      public bool WasCalled { get; private set; } = false;
      public Vector2 LastSpawnPosition { get; private set; }

      public override GameObject SpawnAnimalWithValue(Vector2 position, int value)
      {
          WasCalled = true;
          LastSpawnPosition = position;
          return new GameObject("MockAnimal");
      }
  }
}
