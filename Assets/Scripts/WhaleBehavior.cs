using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class WhaleBehavior : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private BoombaProperties props;

  [Header("Whale Rules")]
  [SerializeField] private int whaleValue = 7;
  [SerializeField] private float whaleUprightZDegrees = 0f;

  [Header("Sorting Layers")]
  [SerializeField] private string dynamicSortingLayer = "Dynamic";
  [SerializeField] private string whaleSortingLayer = "Whale";

  private Rigidbody2D _rb;
  private RigidbodyConstraints2D _baseConstraints;
  private SortingGroup[] _sortingGroups;

  private bool _whaleUprightApplied;
  private bool _wasWhaleLastFrame;

  void Awake()
  {
    if (props == null)
      props = GetComponent<BoombaProperties>();

    _rb = GetComponent<Rigidbody2D>();
    if (_rb != null)
      _baseConstraints = _rb.constraints;

    _sortingGroups = GetComponentsInChildren<SortingGroup>(true);
  }

  void OnEnable()
  {
    // Pool safety: apply correct state immediately
    _whaleUprightApplied = false;
    _wasWhaleLastFrame = false;
    ApplyWhaleStateIfChanged(force: true);
  }

  void FixedUpdate()
  {
    ApplyWhaleStateIfChanged(force: false);

    // Apply upright once (physics-tick safe)
    if (IsWhale() && !_whaleUprightApplied)
    {
      ForceWhaleUpright();
      _whaleUprightApplied = true;
    }
  }

  private bool IsWhale()
  {
    return props != null && props.Value == whaleValue;
  }

  private void ApplyWhaleStateIfChanged(bool force)
  {
    bool isWhale = IsWhale();

    if (!force && isWhale == _wasWhaleLastFrame)
      return;

    _wasWhaleLastFrame = isWhale;

    if (isWhale)
      TransitionToWhale();
    else
      TransitionFromWhale();
  }

  // What it does: Applies whale-specific state when a boomba becomes a whale.
  // What it's used for: Changes sorting layer to make the whale visually prominent.
  private void TransitionToWhale()
  {
    ApplySortingLayer(whaleSortingLayer);
  }

  // What it does: Restores normal boomba state when no longer a whale.
  // What it's used for: Resets sorting layer and physics constraints (pool safe).
  private void TransitionFromWhale()
  {
    ApplySortingLayer(dynamicSortingLayer);
    _whaleUprightApplied = false;
    
    if (_rb != null)
      _rb.constraints = _baseConstraints;
  }

  private void ApplySortingLayer(string layerName)
  {
    if (_sortingGroups == null || _sortingGroups.Length == 0)
      return;

    foreach (var g in _sortingGroups)
      g.sortingLayerName = layerName;
  }

  private void ForceWhaleUpright()
  {
    if (_rb == null)
      return;

    _rb.angularVelocity = 0f;
    _rb.rotation = whaleUprightZDegrees;
    _rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
  }
}
