// BoombaEvents.cs
using System;

public static class BoombaEvents
{
  // a + b merged into result (result can be null if you destroy instead of spawn)
  public static event Action<BoombaProperties, BoombaProperties, BoombaProperties> OnMerged;

  public static void RaiseMerged(BoombaProperties a, BoombaProperties b, BoombaProperties result) {
    OnMerged?.Invoke(a, b, result);
  }
}
