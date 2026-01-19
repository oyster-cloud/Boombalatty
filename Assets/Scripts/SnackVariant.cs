using UnityEngine;

[System.Serializable]
// What it does: Defines the data for a single type of Snack, including its sprite, size, value, and visual scale.
// What it's used for: Configured in the inspector and consumed by SnackTouchSpawner to spawn different snack types.
public class SnackVariant
{
  public Sprite sprite;        // What it does: The artwork used to render this snack.
                               // What it's used for: Assigned to the Snack's SpriteRenderer when spawned.

  public float size = 1f;      // What it does: A world-space size used to size the collider.
                               // What it's used for: Controls how large the snack's hitbox is relative to other snacks.

  public int value = 1;        // What it does: The logical value of this snack, matching Boomba values.
                               // What it's used for: Used in scoring/merge logic to identify the snack's level.

  public float imageScale = 1f; // What it does: A scalar applied to the visual "Art" transform for this snack.
                                // What it's used for: Fine-tunes how big the sprite appears without changing physics size.
}
