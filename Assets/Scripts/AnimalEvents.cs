// AnimalEvents.cs
using System;

public static class AnimalEvents
{
  // a + b merged into result (result can be null if you destroy instead of spawn)
  public static event Action<AnimalProperties, AnimalProperties, AnimalProperties> OnMerged;

  public static void RaiseMerged(AnimalProperties a, AnimalProperties b, AnimalProperties result) {
    OnMerged?.Invoke(a, b, result);
  }
}
