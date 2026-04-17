using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mantiene el puntaje y actualiza el texto de UI.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] Text scoreText;

    int _score;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (scoreText != null)
            scoreText.text = "Puntos: " + _score;
    }

    public void SetScoreText(Text text)
    {
        scoreText = text;
        RefreshDisplay();
    }

    public void AddScore(int value)
    {
        _score += value;
        RefreshDisplay();
    }
}
