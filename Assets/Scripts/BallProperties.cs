using UnityEngine;
using TMPro;

public class BallProperties : MonoBehaviour
{
  public int value;                      // Ball's current logical value
  public TextMeshProUGUI valueText;     // Reference to the TextMeshPro label in the ball

  // Sets the value and updates the on-ball UI
  public void SetValue(int newValue)
  {
    value = newValue;

    if (valueText != null)
    {
      valueText.text = value.ToString();
    }
    else
    {
      Debug.LogWarning("valueText is not assigned on: " + gameObject.name);
    }
  }
}
