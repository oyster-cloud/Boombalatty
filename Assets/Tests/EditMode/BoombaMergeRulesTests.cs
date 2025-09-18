using NUnit.Framework;

// Tests for the BoombaMergeRules class, which handles merge conditions and value calculation
public class BoombaMergeRulesTests
{
  // ShouldMerge should return true if both values are equal
  [Test]
  public void ShouldMerge_ReturnsTrue_WhenValuesAreEqual()
  {
    Assert.IsTrue(BoombaMergeRules.ShouldMerge(4, 4));
  }

  // ShouldMerge should return false if values differ
  [Test]
  public void ShouldMerge_ReturnsFalse_WhenValuesDiffer()
  {
    Assert.IsFalse(BoombaMergeRules.ShouldMerge(3, 5));
  }

  // GetNextValue should return value + 1
  [Test]
  public void GetNextValue_IncrementsCorrectly()
  {
    Assert.AreEqual(6, BoombaMergeRules.GetNextValue(5));
  }
}
