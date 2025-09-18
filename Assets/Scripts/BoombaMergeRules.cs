// This static class defines the merge rules logic — separated from Unity so it's unit testable
public static class BoombaMergeRules
{
  // Determines if two boomba values qualify for merging
  public static bool ShouldMerge(int valueA, int valueB)
  {
    return valueA == valueB;
  }

  // Defines the new value that results from merging two matching boombas
  public static int GetNextValue(int value)
  {
    return value + 1;
  }
}
