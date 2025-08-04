using NUnit.Framework;

// Tests for the BallMergeRules class, which handles merge conditions and value calculation
public class BallMergeRulesTests
{
  // ShouldMerge should return true if both values are equal
  [Test]
  public void ShouldMerge_ReturnsTrue_WhenValuesAreEqual()
  {
    Assert.IsTrue(BallMergeRules.ShouldMerge(4, 4));
  }

  // ShouldMerge should return false if values differ
  [Test]
  public void ShouldMerge_ReturnsFalse_WhenValuesDiffer()
  {
    Assert.IsFalse(BallMergeRules.ShouldMerge(3, 5));
  }

  // GetNextValue should return value + 1
  [Test]
  public void GetNextValue_IncrementsCorrectly()
  {
    Assert.AreEqual(6, BallMergeRules.GetNextValue(5));
  }
}
