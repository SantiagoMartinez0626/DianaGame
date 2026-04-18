using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Mensajes temporales centrados: fallo (¡Fallaste!) y acierto (¡Genial!).
public class GameFeedback : MonoBehaviour
{
    public static GameFeedback Instance { get; private set; }

    [SerializeField] Text feedbackText;
    [SerializeField] float messageDuration = 1.2f;

    Coroutine _running;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    public void SetFeedbackText(Text text)
    {
        feedbackText = text;
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    public void ShowMiss()
    {
        ShowFeedback("¡Fallaste!", new Color(1f, 0.4f, 0.35f));
    }

    public void ShowGreat()
    {
        ShowFeedback("¡Genial!", new Color(0.35f, 0.95f, 0.5f));
    }

    void ShowFeedback(string msg, Color color)
    {
        if (feedbackText == null)
            return;
        if (_running != null)
            StopCoroutine(_running);
        _running = StartCoroutine(ShowRoutine(msg, color));
    }

    IEnumerator ShowRoutine(string msg, Color color)
    {
        feedbackText.text = msg;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        feedbackText.gameObject.SetActive(false);
        _running = null;
    }
}
