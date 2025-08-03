using NUnit.Framework;
using UnityEngine;

public class BallAutoDespawnTests
{
  [Test]
  public void BallAutoDespawn_Component_CanBeAdded()
  {
    var go = new GameObject("Ball");
    go.AddComponent<SpriteRenderer>();

    var despawner = go.AddComponent<BallAutoDespawn>();

    Assert.IsNotNull(despawner);

    Object.DestroyImmediate(go);
  }

  [Test]
  public void BallAutoDespawn_GameObject_CanBeDeactivated()
  {
    var go = new GameObject("Ball");
    go.AddComponent<BallAutoDespawn>();

    go.SetActive(false); // simulate despawn

    Assert.IsFalse(go.activeSelf);

    Object.DestroyImmediate(go);
  }
}
