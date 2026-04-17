using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra mensajes breves de feedback (por ejemplo, fallo en anillo exterior).
/// </summary>
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
        if (feedbackText == null)
            return;
        if (_running != null)
            StopCoroutine(_running);
        _running = StartCoroutine(ShowRoutine("¡Fallaste!"));
    }

    IEnumerator ShowRoutine(string msg)
    {
        feedbackText.text = msg;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        feedbackText.gameObject.SetActive(false);
        _running = null;
    }
}
