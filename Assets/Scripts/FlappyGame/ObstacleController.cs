using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;

    private bool _hasScored = false;

    void Update()
    {
        // Mover de derecha a izquierda
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Destruir cuando sale de pantalla
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    public bool HasScored()
    {
        return _hasScored;
    }

    public void SetScored()
    {
        _hasScored = true;
    }
}