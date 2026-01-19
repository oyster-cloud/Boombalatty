// BoombaEvents.cs
using System;

// What it does: Provides a static event hub for Boomba-related gameplay events.
// What it's used for: Lets other systems subscribe to merge events without needing direct references.
public static class BoombaEvents
{
  // What it does: Event that fires whenever two boombas merge into a result boomba (which may be null).
  // What it's used for: Used by listeners (score, effects, analytics, etc.) to react when a merge happens.
  public static event Action<BoombaProperties, BoombaProperties, BoombaProperties> OnMerged;

  // What it does: Safely raises the OnMerged event with the two source boombas and the resulting merged boomba.
  // What it's used for: Called by merge logic so any subscribers can respond to merge outcomes.
  public static void RaiseMerged(BoombaProperties a, BoombaProperties b, BoombaProperties result) {
    OnMerged?.Invoke(a, b, result);
  }
}
