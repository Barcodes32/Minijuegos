using UnityEngine;

public class FlappyController : MonoBehaviour
{
    [Header("Movimiento")]
    public float jumpForce = 8f;

    [Header("Rotaci�n")]
    public float rotationSpeed = 5f;

    private Rigidbody2D _rb;
    private bool _isAlive = true;
    private FlappyGameManager _gameManager;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _gameManager = FindFirstObjectByType<FlappyGameManager>();
    }

    void Update()
    {
        if (!_isAlive) return;

        // Detectar input (mouse/touch/teclado)
        bool inputDetected = Input.GetMouseButtonDown(0) ||
                            Input.GetKeyDown(KeyCode.Space) ||
                            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (inputDetected)
        {
            Jump();
        }

        // Rotar seg�n velocidad
        float rotation = Mathf.Clamp(_rb.linearVelocity.y * rotationSpeed, -90f, 30f);
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);
    }

    void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        FlappyAudioManager.Instance.PlayJump();
    }

    public void Die()
    {
        if (!_isAlive) return;

        _isAlive = false;
        FlappyAudioManager.Instance.PlayDie();
        _gameManager.GameOver();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Choca con obst�culo o suelo
        Die();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Score"))
        {
            _gameManager.AddScore();
        }
    }
}