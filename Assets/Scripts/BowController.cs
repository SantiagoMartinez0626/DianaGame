using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Carga potencia con Espacio y dispara al soltar; actualiza la barra de potencia.
/// </summary>
public class BowController : MonoBehaviour
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] Transform shootPoint;
    [SerializeField] float maxPower = 14f;
    [SerializeField] float powerPerSecond = 12f;
    [SerializeField] float shotAngleDeg = 38f;
    [SerializeField] Image powerBarFill;
    [SerializeField] CircleCollider2D bullseyeCollider;
    [SerializeField] Sprite arrowSprite;
    [SerializeField] Vector3 arrowVisualScale = new Vector3(0.45f, 0.45f, 1f);
    [SerializeField] float minShotPower = 4f;
    [SerializeField] int arrowSortingOrder = 40;

    float _power;
    float _nextShotTime;

    /// <summary>
    /// Asignado por GameBootstrap al montar la escena.
    /// </summary>
    public void Wire(GameObject arrow, Transform shoot, Image powerFill, CircleCollider2D bullseye)
    {
        arrowPrefab = arrow;
        shootPoint = shoot;
        powerBarFill = powerFill;
        bullseyeCollider = bullseye;
    }

    /// <summary>Valores por defecto pensados para cámara ortográfica ~6 y diana a la derecha.</summary>
    public void SetShotTuning(float maxP, float chargeRate, float angleDegrees)
    {
        maxPower = maxP;
        powerPerSecond = chargeRate;
        shotAngleDeg = angleDegrees;
    }

    public void SetArrowVisual(Sprite sprite, Vector3 scale)
    {
        arrowSprite = sprite;
        arrowVisualScale = scale;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null)
            return;

        if (kb.spaceKey.isPressed)
        {
            _power += powerPerSecond * Time.deltaTime;
            _power = Mathf.Clamp(_power, 0f, maxPower);
        }

        if (powerBarFill != null)
            powerBarFill.fillAmount = maxPower > 0 ? _power / maxPower : 0f;

        if (kb.spaceKey.wasReleasedThisFrame)
        {
            if (Time.time >= _nextShotTime)
            {
                Shoot();
                _nextShotTime = Time.time + 0.12f;
            }
            _power = 0f;
            if (powerBarFill != null)
                powerBarFill.fillAmount = 0f;
        }
    }

    void Shoot()
    {
        if (arrowPrefab == null || shootPoint == null)
            return;

        var arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        var sr = arrow.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (arrowSprite != null)
                sr.sprite = arrowSprite;
            sr.sortingOrder = arrowSortingOrder;
            sr.enabled = true;
            sr.color = Color.white;
        }

        arrow.transform.localScale = arrowVisualScale;

        var rb = arrow.GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        float shotPower = Mathf.Max(_power, minShotPower);
        float rad = shotAngleDeg * Mathf.Deg2Rad;
        var vel = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * shotPower;
        rb.linearVelocity = vel;
        float angleDeg = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        var arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null && bullseyeCollider != null)
            arrowScript.SetBullseyeCollider(bullseyeCollider);
    }
}
