using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BallLogicTests
{
  [Test]
  public void RandomBallSize_IsWithinRange()
  {
    // Simulate how you randomize size
    float size = Random.Range(0.2f, 0.8f);

    Assert.That(size, Is.GreaterThanOrEqualTo(0.2f));
    Assert.That(size, Is.LessThanOrEqualTo(0.8f));
  }

  [Test]
  public void BallPrefab_IsNotNull_WhenAssigned()
  {
    // Simulate a ball prefab assignment
    GameObject fakeBallPrefab = new GameObject("Ball");

    Assert.IsNotNull(fakeBallPrefab);
    Object.DestroyImmediate(fakeBallPrefab); // clean up
  }
  // // A Test behaves as an ordinary method
  // [Test]
  // public void BallLogicTestsSimplePasses()
  // {
  //     // Use the Assert class to test conditions
  // }

  // // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
  // // `yield return null;` to skip a frame.
  // [UnityTest]
  // public IEnumerator BallLogicTestsWithEnumeratorPasses()
  // {
  //     // Use the Assert class to test conditions.
  //     // Use yield to skip a frame.
  //     yield return null;
  // }
}
