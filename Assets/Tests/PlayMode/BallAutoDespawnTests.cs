using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for BallAutoDespawn functionality (despawning on invisible).
/// </summary>
public class BallAutoDespawnTests
{
  [Test]
  public void BallAutoDespawn_Component_CanBeAdded()
  {
    var go = new GameObject("Ball");
    go.AddComponent<SpriteRenderer>();

    // Ensure the component can be added without errors
    var despawner = go.AddComponent<BallAutoDespawn>();
    Assert.IsNotNull(despawner);

    Object.DestroyImmediate(go);
  }

  [Test]
  public void BallAutoDespawn_GameObject_CanBeDeactivated()
  {
    var go = new GameObject("Ball");
    go.AddComponent<BallAutoDespawn>();

    // Simulate manual deactivation
    go.SetActive(false);

    Assert.IsFalse(go.activeSelf);

    Object.DestroyImmediate(go);
  }

  [Test]
  public void DisablesOnInvisibility()
  {
    // Simulate OnBecameInvisible behavior
    var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    go.AddComponent<BallAutoDespawn>();

    go.SendMessage("OnBecameInvisible"); // triggers despawn

    Assert.IsFalse(go.activeSelf); // should be inactive
    Object.DestroyImmediate(go);
  }
}
