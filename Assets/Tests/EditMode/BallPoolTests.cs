using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

/// <summary>
/// Unit tests for BallPool behavior: preloading, reusing, and expanding.
/// </summary>
public class BallPoolTests
{
  private BallPool pool;
  private GameObject prefab;

  [SetUp]
  public void Setup()
  {
    // Create BallPool instance
    GameObject obj = new GameObject("BallPool");
    pool = obj.AddComponent<BallPool>();

    // Create dummy ball prefab
    prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    prefab.name = "BallPrefab";

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
  public void PreloadsBallPool()
  {
    // Ensure pool is preloaded with the correct number of balls
    Assert.AreEqual(3, pool.GetAllBalls().Count);
  }

  [Test]
  public void ReusesInactiveBall()
  {
    // Simulate despawning a ball
    var ball1 = pool.GetBall(Vector2.zero);
    ball1.SetActive(false);

    // Should reuse the inactive ball
    var reused = pool.GetBall(Vector2.one);
    Assert.AreSame(ball1, reused);
  }

  [Test]
  public void ExpandsPoolWhenAllActive()
  {
    // Use up all preloaded balls
    pool.GetBall(Vector2.zero);
    pool.GetBall(Vector2.zero);
    pool.GetBall(Vector2.zero);

    // Pool should expand to create a new ball
    var newBall = pool.GetBall(Vector2.zero);
    Assert.AreEqual(4, pool.GetAllBalls().Count);
    Assert.IsNotNull(newBall);
  }
}
