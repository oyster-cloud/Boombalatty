using UnityEngine;
using TMPro;

public class BoombaProperties : MonoBehaviour
{
  public bool IsLastVariant { get; set; }

  [Header("Value")]
  public int Value;

  [Header("UI")]
  [SerializeField] private TextMeshProUGUI valueText;

  void OnEnable()
  {
    RefreshValueText();
  }

  public void SetValue(int newValue)
  {
    Value = newValue;
    RefreshValueText();
  }

  private void RefreshValueText()
  {
    if (valueText != null)
    {
      valueText.text = Value.ToString();
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    else
    {
      Debug.LogWarning($"BoombaProperties: valueText is not assigned on {gameObject.name}");
    }
#endif
  }
}
