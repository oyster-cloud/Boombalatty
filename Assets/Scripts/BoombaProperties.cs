using UnityEngine;
using TMPro;

public class BoombaProperties : MonoBehaviour
{
  public bool IsLastVariant { get; set; }

  public int Value;                      // Boomba's current logical value
  public TextMeshProUGUI valueText;     // Reference to the TextMeshPro label in the boomba

  // Sets the value and updates the on-boomba UI
  public void SetValue(int newValue)
  {
    Value = newValue;

    if (valueText != null)
    {
      valueText.text = Value.ToString();
    }
    else
    {
      Debug.LogWarning("valueText is not assigned on: " + gameObject.name);
    }
  }
}
