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

    float _power;

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
            Shoot();
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
        var rb = arrow.GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        float rad = shotAngleDeg * Mathf.Deg2Rad;
        var vel = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _power;
        rb.linearVelocity = vel;

        var arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null && bullseyeCollider != null)
            arrowScript.SetBullseyeCollider(bullseyeCollider);
    }
}
