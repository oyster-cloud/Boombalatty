using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

/// <summary>
/// Unit tests for AnimalPool behavior: preloading, reusing, and expanding.
/// </summary>
public class AnimalPoolTests
{
  private AnimalPool pool;
  private GameObject prefab;

  [SetUp]
  public void Setup()
  {
    // Create AnimalPool instance
    GameObject obj = new GameObject("AnimalPool");
    pool = obj.AddComponent<AnimalPool>();

    // Create dummy ball prefab
    prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    prefab.name = "AnimalPrefab";

    // Set pool settings
    pool.ballPrefab = prefab;
    pool.poolSize = 3;

    pool.Initialize(); // Preload balls
  }

  [TearDown]
  public void Teardown()
  {
    // Cleanup GameObjects after each test
    Object.DestroyImmediate(pool.gameObject);
    Object.DestroyImmediate(prefab);
  }

  [Test]
  public void PreloadsAnimalPool()
  {
    // Ensure pool is preloaded with the correct number of balls
    Assert.AreEqual(3, pool.GetAllAnimals().Count);
  }

  [Test]
  public void ReusesInactiveAnimal()
  {
    // Simulate despawning a ball
    var ball1 = pool.GetAnimal(Vector2.zero);
    ball1.SetActive(false);

    // Should reuse the inactive ball
    var reused = pool.GetAnimal(Vector2.one);
    Assert.AreSame(ball1, reused);
  }

  [Test]
  public void ExpandsPoolWhenAllActive()
  {
    // Use up all preloaded balls
    pool.GetAnimal(Vector2.zero);
    pool.GetAnimal(Vector2.zero);
    pool.GetAnimal(Vector2.zero);

    // Pool should expand to create a new ball
    var newAnimal = pool.GetAnimal(Vector2.zero);
    Assert.AreEqual(4, pool.GetAllAnimals().Count);
    Assert.IsNotNull(newAnimal);
  }
}
