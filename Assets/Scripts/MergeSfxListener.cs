// MergeSfxListener.cs
using UnityEngine;

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

  void Reset()
  {
    sfx = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    sfx.playOnAwake = false;
    sfx.spatialBlend = 0f; // 2D
  }

  void OnEnable()  => AnimalEvents.OnMerged += HandleMerged;
  void OnDisable() => AnimalEvents.OnMerged -= HandleMerged;

  void HandleMerged(AnimalProperties a, AnimalProperties b, AnimalProperties result)
  {
    Debug.Log("HandleMerged");
    var clip = PickClip(result);
    if (clip) {
      // tiny pitch variance keeps repeats fresh
      sfx.pitch = Random.Range(0.97f, 1.03f);
      sfx.PlayOneShot(clip);
      sfx.pitch = 1f;
    }
  }

  AudioClip PickClip(AnimalProperties result)
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
