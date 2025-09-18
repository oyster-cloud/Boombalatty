using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

/// <summary>
/// Unit tests for BoombaPool behavior: preloading, reusing, and expanding.
/// </summary>
public class BoombaPoolTests
{
  private BoombaPool pool;
  private GameObject prefab;

  [SetUp]
  public void Setup()
  {
    // Create BoombaPool instance
    GameObject obj = new GameObject("BoombaPool");
    pool = obj.AddComponent<BoombaPool>();

    // Create dummy boomba prefab
    prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    prefab.name = "BoombaPrefab";

    // Set pool settings
    pool.boombaPrefab = prefab;
    pool.poolSize = 3;

    pool.Initialize(); // Preload boombas
  }

  [TearDown]
  public void Teardown()
  {
    // Cleanup GameObjects after each test
    Object.DestroyImmediate(pool.gameObject);
    Object.DestroyImmediate(prefab);
  }

  [Test]
  public void PreloadsBoombaPool()
  {
    // Ensure pool is preloaded with the correct number of boombas
    Assert.AreEqual(3, pool.GetAllBoombas().Count);
  }

  [Test]
  public void ReusesInactiveBoomba()
  {
    // Simulate despawning a boomba
    var boomba1 = pool.GetBoomba(Vector2.zero);
    boomba1.SetActive(false);

    // Should reuse the inactive boomba
    var reused = pool.GetBoomba(Vector2.one);
    Assert.AreSame(boomba1, reused);
  }

  [Test]
  public void ExpandsPoolWhenAllActive()
  {
    // Use up all preloaded boombas
    pool.GetBoomba(Vector2.zero);
    pool.GetBoomba(Vector2.zero);
    pool.GetBoomba(Vector2.zero);

    // Pool should expand to create a new boomba
    var newBoomba = pool.GetBoomba(Vector2.zero);
    Assert.AreEqual(4, pool.GetAllBoombas().Count);
    Assert.IsNotNull(newBoomba);
  }
}
