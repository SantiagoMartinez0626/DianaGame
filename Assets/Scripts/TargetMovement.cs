using UnityEngine;

/// <summary>
/// Mueve la diana horizontalmente entre -limit y +limit, invirtiendo dirección en los bordes.
/// </summary>
public class TargetMovement : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    [SerializeField] float limit = 4f;

    int _direction = 1;

    /// <summary>Ajuste opcional desde el bootstrap (velocidad y mitad del recorrido en X).</summary>
    public void Configure(float moveSpeed, float horizontalLimit)
    {
        speed = moveSpeed;
        limit = horizontalLimit;
    }

    void Update()
    {
        transform.Translate(Vector3.right * (speed * _direction * Time.deltaTime));

        if (transform.position.x > limit)
            _direction = -1;
        else if (transform.position.x < -limit)
            _direction = 1;
    }
}
