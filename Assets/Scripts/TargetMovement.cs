using UnityEngine;

// Movimiento horizontal de la diana: va de -limit a +limit invirtiendo dirección (requisito del parcial).
public class TargetMovement : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    [SerializeField] float limit = 4f;

    int _direction = 1;

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
