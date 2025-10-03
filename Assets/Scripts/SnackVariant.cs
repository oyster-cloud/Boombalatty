using UnityEngine;

[System.Serializable]
public class SnackVariant
{
  public Sprite sprite;
  public float size = 1f;        // world size for collider
  public int value = 1;          // match your Boomba values
  public float imageScale = 1f;  // visual scale for child "Art"
}
