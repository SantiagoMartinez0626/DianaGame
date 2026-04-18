using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] CircleCollider2D bullseyeCollider;

    bool _done;

    public void SetBullseyeCollider(CircleCollider2D col) => bullseyeCollider = col;

    static bool IsInsideBullseye(CircleCollider2D bull, Vector2 worldPos)
    {
        if (bull == null)
            return false;
        if (bull.OverlapPoint(worldPos))
            return true;
        float scale = Mathf.Max(Mathf.Abs(bull.transform.lossyScale.x), Mathf.Abs(bull.transform.lossyScale.y));
        float r = bull.radius * scale + 0.04f;
        return Vector2.Distance(worldPos, bull.transform.position) <= r;
    }

    Vector2 GetTipWorld()
    {
        var bc = GetComponent<BoxCollider2D>();
        var rb = GetComponent<Rigidbody2D>();
        float half = bc != null
            ? Mathf.Max(
                bc.size.x * Mathf.Abs(transform.lossyScale.x),
                bc.size.y * Mathf.Abs(transform.lossyScale.y)) * 0.5f
            : 0.2f;
        if (rb != null && rb.linearVelocity.sqrMagnitude > 1e-6f)
            return (Vector2)transform.position + rb.linearVelocity.normalized * half;
        return (Vector2)transform.position + (Vector2)transform.up * half;
    }

    void TryRegisterBullseyeHit()
    {
        if (_done || bullseyeCollider == null)
            return;
        if (IsInsideBullseye(bullseyeCollider, GetTipWorld()))
            RegisterHit();
    }

    void FreezeAndHide()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;
    }

    void MissAndDestroy()
    {
        if (_done)
            return;
        _done = true;
        FreezeAndHide();
        if (GameFeedback.Instance != null)
            GameFeedback.Instance.ShowMiss();
        else
            Debug.Log("¡Fallaste!");
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_done)
            return;

        if (other.CompareTag("Bullseye"))
        {
            TryRegisterBullseyeHit();
            return;
        }

        if (other.CompareTag("OuterRing"))
        {
            TryRegisterBullseyeHit();
            return;
        }

        if (other.CompareTag("KillZone"))
        {
            _done = true;
            Destroy(gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (_done)
            return;
        if (other.CompareTag("Bullseye") || other.CompareTag("OuterRing"))
            TryRegisterBullseyeHit();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (_done)
            return;
        if (!other.CompareTag("OuterRing"))
            return;
        MissAndDestroy();
    }

    void RegisterHit()
    {
        if (_done)
            return;
        _done = true;
        FreezeAndHide();
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(10);
        if (GameFeedback.Instance != null)
            GameFeedback.Instance.ShowGreat();
        Destroy(gameObject);
    }
}
