using UnityEngine;

public static class Services
{
  public static BoombaSpawner BoombaSpawner { get; internal set; }
  public static SnackTouchSpawner SnackTouchSpawner { get; internal set; }
  public static EndGameWhenBoombaCrossesCeiling Ceiling { get; internal set; } 
}
