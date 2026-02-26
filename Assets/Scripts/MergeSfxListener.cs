// MergeSfxListener.cs
using UnityEngine;

// What it does: Listens for boomba merge events and plays appropriate merge sound effects.
// What it's used for: Centralizes all merge audio feedback so it can be tuned in one place via the inspector.
public class MergeSfxListener : MonoBehaviour
{
  [SerializeField] AudioSource sfx;

  [Header("Default")]
  [SerializeField] AudioClip defaultMerge;

  [Header("Special case: last variant (e.g., whale)")]
  [SerializeField] AudioClip lastVariantMerge;

  [Header("Optional per-value overrides")]
  [SerializeField] ClipByValue[] clipsByValue;

  [System.Serializable]
  public struct ClipByValue { public int value; public AudioClip clip; }

  // What it does: Ensures there is an AudioSource on this GameObject and configures it for 2D SFX.
  // What it's used for: Sets up a reliable SFX channel without requiring manual AudioSource configuration.
  void Reset()
  {
    sfx = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    sfx.playOnAwake = false;
    sfx.spatialBlend = 0f; // 2D
  }

  // What it does: Subscribes to the global boomba merge event when enabled.
  // What it's used for: Starts listening for merges so SFX can be played.
  void OnEnable()  => BoombaEvents.OnMerged += HandleMerged;

  // What it does: Unsubscribes from the global boomba merge event when disabled.
  // What it's used for: Prevents duplicate subscriptions and leaks if this listener is turned off or destroyed.
  void OnDisable() => BoombaEvents.OnMerged -= HandleMerged;

  // What it does: Picks a merge clip based on the result boomba and plays it with slight pitch variation.
  // What it's used for: Provides responsive audio feedback whenever a merge occurs, with variety to avoid repetitive sound.
  void HandleMerged(BoombaProperties a, BoombaProperties b, BoombaProperties result)
  {
    var clip = PickClip(result);
    if (clip) {
      // tiny pitch variance keeps repeats fresh
      sfx.pitch = Random.Range(0.97f, 1.03f);
      sfx.PlayOneShot(clip);
      sfx.pitch = 1f;
    }
  }

  // What it does: Chooses the appropriate AudioClip based on whether this is the last variant or a specific value.
  // What it's used for: Allows special SFX for the final variant and optional per-level sound variations.
  AudioClip PickClip(BoombaProperties result)
  {
    // If you set this earlier when spawning (as we did for scoring)
    if (result != null && result.IsLastVariant && lastVariantMerge) return lastVariantMerge;

    // If you key SFX by value (optional)
    if (result != null)
    {
      foreach (var kv in clipsByValue)
          if (kv.value == result.Value && kv.clip) return kv.clip;
    }

    return defaultMerge;
  }
}
