using UnityEngine;

public class BallVisual : MonoBehaviour
{
  [SerializeField] private Transform artTransform;        // drag Ball/Art here
  [SerializeField] private SpriteRenderer artRenderer;     // drag Ball/Art SpriteRenderer here

  public void SetSprite(Sprite sprite) => artRenderer.sprite = sprite;

  public void SetImageScale(float s)
  {
    if (artTransform != null)
      artTransform.localScale = new Vector3(s, s, 1f);
  }
}
