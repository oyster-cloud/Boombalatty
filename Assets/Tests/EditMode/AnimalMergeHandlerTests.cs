using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AnimalMergeHandlerTests
{
  [Test]
  public void OnEnable_ResetsIsMerging()
  {
    // Arrange: create a GameObject and add AnimalMergeHandler
    GameObject go = new GameObject("TestAnimal");
    AnimalMergeHandler mergeHandler = go.AddComponent<AnimalMergeHandler>();

    // Simulate a merge in progress
    mergeHandler.isMerging = true;

    // Act: simulate Unity reactivating the object
    mergeHandler.OnEnable();

    // Assert
    Assert.IsFalse(mergeHandler.isMerging, "OnEnable should reset isMerging to false");
  }

  [TearDown]
  public void Cleanup()
  {
    foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
    {
      Object.DestroyImmediate(obj);
    }
  }
}
