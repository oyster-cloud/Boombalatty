using NUnit.Framework;
using UnityEngine;
using TMPro;

public class BoombaPropertiesTests
{
  [Test]
  public void SetValue_UpdatesValueAndText()
  {
    // Set up dummy GameObject with BoombaProperties
    var go = new GameObject();
    var props = go.AddComponent<BoombaProperties>();

    // Add dummy TextMeshProUGUI (requires TMPro assembly reference)
    var canvasGO = new GameObject();
    canvasGO.AddComponent<Canvas>();
    var text = canvasGO.AddComponent<TextMeshProUGUI>();

    // props.valueText = text;

    // Act
    props.SetValue(42);

    // Assert
    Assert.AreEqual(42, props.Value);
    Assert.AreEqual("42", text.text);

    Object.DestroyImmediate(go);
    Object.DestroyImmediate(canvasGO);
  }
}
