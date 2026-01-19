// This static class defines the merge rules logic — separated from Unity so it's unit testable
public static class BoombaMergeRules
{
  // What it does: Returns true if two boomba values qualify to merge under current rules.
  // What it's used for: Called by merge handlers to decide whether a collision should trigger a merge.
  public static bool ShouldMerge(int valueA, int valueB)
  {
    return valueA == valueB;
  }

  // What it does: Computes the new value produced by merging two matching boombas.
  // What it's used for: Provides the resulting level/value for the spawned merged boomba.
  public static int GetNextValue(int value)
  {
    return value + 1;
  }
}
