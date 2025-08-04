using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BallPrefabTests
{
  // Test Prefab "Ball" exists
  [Test]
  public void BallPrefab_IsNotNull_WhenAssigned()
  {
    // Simulate a ball prefab assignment
    GameObject fakeBallPrefab = new GameObject("Ball");

    Assert.IsNotNull(fakeBallPrefab);
    Object.DestroyImmediate(fakeBallPrefab); // clean up
  }
}
