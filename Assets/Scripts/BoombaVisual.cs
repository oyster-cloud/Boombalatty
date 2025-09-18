using UnityEngine;

public class BoombaVisual : MonoBehaviour
{
  [SerializeField] private Transform artTransform;        // drag Boomba/Art here
  [SerializeField] private SpriteRenderer artRenderer;     // drag Boomba/Art SpriteRenderer here

  public void SetSprite(Sprite sprite) => artRenderer.sprite = sprite;

  public void SetImageScale(float s)
  {
    if (artTransform != null)
      artTransform.localScale = new Vector3(s, s, 1f);
  }
}
