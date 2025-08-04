using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

public class BallPoolTests
{
  private BallPool pool;
  private GameObject prefab;

  [SetUp]
  public void Setup()
  {
    GameObject obj = new GameObject("BallPool");
    pool = obj.AddComponent<BallPool>();

    prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    prefab.name = "BallPrefab";
    pool.ballPrefab = prefab;
    pool.poolSize = 3;

    pool.Initialize();
  }

  [TearDown]
  public void Teardown()
  {
    Object.DestroyImmediate(pool.gameObject);
    Object.DestroyImmediate(prefab);
  }

  [Test]
  public void PreloadsBallPool()
  {
    Assert.AreEqual(3, pool.GetAllBalls().Count);
  }

  [Test]
  public void ReusesInactiveBall()
  {
    var ball1 = pool.GetBall(Vector2.zero);
    ball1.SetActive(false); // simulate returning to pool
    var reused = pool.GetBall(Vector2.one);
    Assert.AreSame(ball1, reused);
  }

  [Test]
  public void ExpandsPoolWhenAllActive()
  {
    pool.GetBall(Vector2.zero);
    pool.GetBall(Vector2.zero);
    pool.GetBall(Vector2.zero);
    var newBall = pool.GetBall(Vector2.zero);
    Assert.AreEqual(4, pool.GetAllBalls().Count);
    Assert.IsNotNull(newBall);
  }
}
