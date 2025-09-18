using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for BoombaAutoDespawn functionality (despawning on invisible).
/// </summary>
public class BoombaAutoDespawnTests
{
  [Test]
  public void BoombaAutoDespawn_Component_CanBeAdded()
  {
    var go = new GameObject("Boomba");
    go.AddComponent<SpriteRenderer>();

    // Ensure the component can be added without errors
    var despawner = go.AddComponent<BoombaAutoDespawn>();
    Assert.IsNotNull(despawner);

    Object.DestroyImmediate(go);
  }

  [Test]
  public void BoombaAutoDespawn_GameObject_CanBeDeactivated()
  {
    var go = new GameObject("Boomba");
    go.AddComponent<BoombaAutoDespawn>();

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
    go.AddComponent<BoombaAutoDespawn>();

    go.SendMessage("OnBecameInvisible"); // triggers despawn

    Assert.IsFalse(go.activeSelf); // should be inactive
    Object.DestroyImmediate(go);
  }
}
