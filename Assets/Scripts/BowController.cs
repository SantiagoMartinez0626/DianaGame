using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Mecánica de disparo: mantener Espacio carga potencia; al soltar instancia la flecha con velocidad y orientación del sprite.
public class BowController : MonoBehaviour
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] Transform shootPoint;
    [SerializeField] float maxPower = 28f;
    [SerializeField] float powerPerSecond = 22f;
    [SerializeField] float shotAngleDeg = 37f;
    [SerializeField] Image powerBarFill;
    [SerializeField] CircleCollider2D bullseyeCollider;
    [SerializeField] Sprite arrowSprite;
    [SerializeField] Vector3 arrowVisualScale = new Vector3(0.45f, 0.45f, 1f);
    [SerializeField] float minShotPower = 8f;
    [SerializeField] int arrowSortingOrder = 40;
    [SerializeField] float arrowSpriteFacingOffsetDeg = 0f;
    [SerializeField] [Range(32f, 42f)] float arrowTipAngleFromHorizontalDeg = 37f;

    float _power;
    float _nextShotTime;

    public void Wire(GameObject arrow, Transform shoot, Image powerFill, CircleCollider2D bullseye)
    {
        arrowPrefab = arrow;
        shootPoint = shoot;
        powerBarFill = powerFill;
        bullseyeCollider = bullseye;
    }

    public void SetShotTuning(float maxP, float chargeRate, float angleDegrees, float minShotPowerOverride = 0f)
    {
        maxPower = maxP;
        powerPerSecond = chargeRate;
        shotAngleDeg = angleDegrees;
        if (minShotPowerOverride > 0f)
            minShotPower = minShotPowerOverride;
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
        // Sprite arrow.png: astil en +Y local; se compensa -90° para alinear la punta con la trayectoria.
        float alpha = Mathf.Clamp(arrowTipAngleFromHorizontalDeg, 32f, 42f);
        float tipDeg = alpha - 90f + arrowSpriteFacingOffsetDeg;
        arrow.transform.rotation = Quaternion.Euler(0f, 0f, tipDeg);

        var arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null && bullseyeCollider != null)
            arrowScript.SetBullseyeCollider(bullseyeCollider);
    }
}
