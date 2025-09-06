using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
  [SerializeField] TMP_Text scoreText;
  int score;

  void Awake()        => ResetScore();
  void OnEnable()     => AnimalAutoDespawn.OnAnimalOffscreen += HandleOffscreen;
  void OnDisable()    => AnimalAutoDespawn.OnAnimalOffscreen -= HandleOffscreen;

  void HandleOffscreen(AnimalProperties props)
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
