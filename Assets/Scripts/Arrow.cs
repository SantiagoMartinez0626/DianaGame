using UnityEngine;

/// <summary>
/// Flecha con trigger: solo el centro (tag Bullseye o solapamiento con su collider) suma puntos.
/// </summary>
public class Arrow : MonoBehaviour
{
    [SerializeField] CircleCollider2D bullseyeCollider;

    bool _done;

    /// <summary>
    /// Asignado al instanciar la flecha para comprobar acierto si el primer trigger es el anillo exterior.
    /// </summary>
    public void SetBullseyeCollider(CircleCollider2D col) => bullseyeCollider = col;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_done)
            return;

        // Acierto: collider del centro (requisito del parcial)
        if (other.CompareTag("Bullseye"))
        {
            RegisterHit();
            return;
        }

        // Anillo exterior: puede activarse antes que el bullseye; comprobar si el impacto cae en el centro
        if (other.CompareTag("OuterRing"))
        {
            if (bullseyeCollider != null && bullseyeCollider.OverlapPoint(transform.position))
            {
                RegisterHit();
                return;
            }

            if (GameFeedback.Instance != null)
                GameFeedback.Instance.ShowMiss();
            else
                Debug.Log("¡Fallaste!");

            _done = true;
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("KillZone"))
        {
            _done = true;
            Destroy(gameObject);
        }
    }

    void RegisterHit()
    {
        if (_done)
            return;
        _done = true;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(10);
        Destroy(gameObject);
    }
}
