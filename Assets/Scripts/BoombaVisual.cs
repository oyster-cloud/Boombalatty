// using UnityEngine;

// // What it does: Wraps sprite assignment and scaling for a Boomba’s visual art transform.
// // What it's used for: Provides a simple interface to swap sprites and adjust visual scale without touching gameplay logic.
// public class BoombaVisual : MonoBehaviour
// {
//   [SerializeField] private Transform artTransform;        // drag Boomba/Art here
//   [SerializeField] private SpriteRenderer artRenderer;    // drag Boomba/Art SpriteRenderer here

//   // What it does: Assigns a new sprite to the configured SpriteRenderer.
//   // What it's used for: Changes the Boomba’s appearance when selecting a different variant or skin.
//   public void SetSprite(Sprite sprite) => artRenderer.sprite = sprite;

//   // What it does: Sets a uniform local scale on the art transform using the given scalar.
//   // What it's used for: Adjusts how large the sprite appears on screen without affecting colliders or other hierarchy transforms.
//   public void SetImageScale(float s)
//   {
//     if (artTransform != null)
//       artTransform.localScale = new Vector3(s, s, 1f);
//   }
// }
