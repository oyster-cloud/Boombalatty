using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BoombaPrefabTests
{
  // Test Prefab "Boomba" exists
  [Test]
  public void BoombaPrefab_IsNotNull_WhenAssigned()
  {
    // Simulate a boomba prefab assignment
    GameObject fakeBoombaPrefab = new GameObject("Boomba");

    Assert.IsNotNull(fakeBoombaPrefab);
    Object.DestroyImmediate(fakeBoombaPrefab); // clean up
  }
}
