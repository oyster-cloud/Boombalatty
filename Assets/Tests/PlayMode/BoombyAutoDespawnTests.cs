using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for AnimalAutoDespawn functionality (despawning on invisible).
/// </summary>
public class AnimalAutoDespawnTests
{
  [Test]
  public void AnimalAutoDespawn_Component_CanBeAdded()
  {
    var go = new GameObject("Animal");
    go.AddComponent<SpriteRenderer>();

    // Ensure the component can be added without errors
    var despawner = go.AddComponent<AnimalAutoDespawn>();
    Assert.IsNotNull(despawner);

    Object.DestroyImmediate(go);
  }

  [Test]
  public void AnimalAutoDespawn_GameObject_CanBeDeactivated()
  {
    var go = new GameObject("Animal");
    go.AddComponent<AnimalAutoDespawn>();

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
    go.AddComponent<AnimalAutoDespawn>();

    go.SendMessage("OnBecameInvisible"); // triggers despawn

    Assert.IsFalse(go.activeSelf); // should be inactive
    Object.DestroyImmediate(go);
  }
}
