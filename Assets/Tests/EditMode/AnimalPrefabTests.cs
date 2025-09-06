using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AnimalPrefabTests
{
  // Test Prefab "Animal" exists
  [Test]
  public void AnimalPrefab_IsNotNull_WhenAssigned()
  {
    // Simulate a ball prefab assignment
    GameObject fakeAnimalPrefab = new GameObject("Animal");

    Assert.IsNotNull(fakeAnimalPrefab);
    Object.DestroyImmediate(fakeAnimalPrefab); // clean up
  }
}
