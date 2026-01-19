using UnityEngine;
using TMPro;

public class BoombaProperties : MonoBehaviour
{
  public bool IsLastVariant { get; set; }

  public int Value;                      // Boomba's current logical value
  public TextMeshProUGUI valueText;     // Reference to the TextMeshPro label in the boomba

  // What it does: Updates the boomba’s logical value and refreshes the on-boomba UI label.
  // What it's used for: Keeps the visual number on the boomba in sync with its underlying gameplay value.
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
