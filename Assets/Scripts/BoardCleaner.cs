using UnityEngine;

// What it does: Clears all active game objects (boombas and snacks) from the scene.
// What it's used for: Resets the board when restarting the game to provide a clean playfield.
public class BoardCleaner
{
  // What it does: Deactivates all active boomba and snack objects in the scene.
  // What it's used for: Resets the board so a new game can start with a clean playfield.
  public void ClearBoard()
  {
    DeactivateBoombas();
    ClearSnacks();
    ClearSnackLayers();
  }

  // What it does: Deactivates all GameObjects with BoombaProperties components (returns them to pool).
  // What it's used for: Clears all boombas from the scene without destroying them.
  private void DeactivateBoombas()
  {
    var boombas = Object.FindObjectsByType<BoombaProperties>(FindObjectsSortMode.None);
    foreach (var boomba in boombas)
    {
      if (boomba && boomba.gameObject.activeSelf)
      {
        boomba.gameObject.SetActive(false);
      }
    }
  }

  // What it does: Destroys all GameObjects with SnackLifecycle components.
  // What it's used for: Removes all snacks from the scene (snacks aren't pooled).
  private void ClearSnacks()
  {
    var snacks = Object.FindObjectsByType<SnackLifecycle>(FindObjectsSortMode.None);
    foreach (var snack in snacks)
    {
      if (snack) Object.Destroy(snack.gameObject);
    }
  }

  // What it does: Destroys any remaining objects on Snack/SnackPreLand layers that lack components.
  // What it's used for: Safety cleanup for edge cases where snacks might exist without proper components.
  private void ClearSnackLayers()
  {
    int snackLayer = LayerMask.NameToLayer("Snack");
    int snackPreLandLayer = LayerMask.NameToLayer("SnackPreLand");
    
    var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
    foreach (var transform in allTransforms)
    {
      var go = transform.gameObject;
      if (!go || go.scene.rootCount == 0) continue;
      
      int layer = go.layer;
      if (layer == snackLayer || layer == snackPreLandLayer)
      {
        Object.Destroy(go);
      }
    }
  }
}