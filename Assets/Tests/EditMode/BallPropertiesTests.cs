using NUnit.Framework;
using UnityEngine;
using TMPro;

public class BallPropertiesTests
{
  [Test]
  public void SetValue_UpdatesValueAndText()
  {
    // Set up dummy GameObject with BallProperties
    var go = new GameObject();
    var props = go.AddComponent<BallProperties>();

    // Add dummy TextMeshProUGUI (requires TMPro assembly reference)
    var canvasGO = new GameObject();
    canvasGO.AddComponent<Canvas>();
    var text = canvasGO.AddComponent<TextMeshProUGUI>();

    props.valueText = text;

    // Act
    props.SetValue(42);

    // Assert
    Assert.AreEqual(42, props.value);
    Assert.AreEqual("42", text.text);

    Object.DestroyImmediate(go);
    Object.DestroyImmediate(canvasGO);
  }
}
