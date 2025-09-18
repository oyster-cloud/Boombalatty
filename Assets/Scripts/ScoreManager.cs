using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
  [SerializeField] TMP_Text scoreText;
  int score;

  void Awake()        => ResetScore();
  void OnEnable()     => BoombaAutoDespawn.OnBoombaOffscreen += HandleOffscreen;
  void OnDisable()    => BoombaAutoDespawn.OnBoombaOffscreen -= HandleOffscreen;

  void HandleOffscreen(BoombaProperties props)
  {
    if (props != null && props.IsLastVariant)
    {
      score++;
      UpdateUI();
    }
  }

  void UpdateUI()
  {
    if (scoreText) scoreText.text = $"Score: {score}";
  }

  public void ResetScore() {
    score = 0;
    UpdateUI();
  }
}
