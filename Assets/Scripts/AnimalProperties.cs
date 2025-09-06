using UnityEngine;
using TMPro;

public class AnimalProperties : MonoBehaviour
{
  public bool IsLastVariant { get; set; }

  public int Value;                      // Animal's current logical value
  public TextMeshProUGUI valueText;     // Reference to the TextMeshPro label in the boomby

  // Sets the value and updates the on-boomby UI
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
