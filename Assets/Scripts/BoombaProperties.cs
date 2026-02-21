using UnityEngine;
using TMPro;

public class BoombaProperties : MonoBehaviour
{
  public bool IsLastVariant { get; set; }

  [Header("Value")]
  public int Value;

  public void SetValue(int newValue)
  {
    Value = newValue;
  }
}
