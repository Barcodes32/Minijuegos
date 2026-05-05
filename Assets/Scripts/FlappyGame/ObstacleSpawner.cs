using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject obstaclePrefab;
    public float spawnInterval = 2f;
    public float gapSize = 3f;
    public float minY = -3f;
    public float maxY = 3f;

    [Header("Dificultad")]
    public float baseSpeed = 5f;

    private bool _isSpawning = true;
    private float _currentSpeed;
    private float _currentSpawnInterval;

    void Start()
    {
        _currentSpeed = baseSpeed;
        _currentSpawnInterval = spawnInterval;
        StartCoroutine(SpawnObstacles());
    }

    IEnumerator SpawnObstacles()
    {
        yield return new WaitForSeconds(1f); // Esperar un poco al inicio

        while (_isSpawning)
        {
            SpawnObstacle();
            yield return new WaitForSeconds(_currentSpawnInterval);
        }
    }

    void SpawnObstacle()
    {
        // Posición aleatoria en Y
        float randomY = Random.Range(minY, maxY);

        // Instanciar obstáculo
        GameObject obstacle = Instantiate(obstaclePrefab, transform.position, Quaternion.identity);
        obstacle.transform.position = new Vector3(transform.position.x, randomY, 0f);

        // Asignar velocidad actual al obstáculo
        ObstacleController obstacleCtrl = obstacle.GetComponent<ObstacleController>();
        if (obstacleCtrl != null)
        {
            obstacleCtrl.moveSpeed = _currentSpeed;
        }
    }

    public void IncreaseSpeed(float amount)
    {
        _currentSpeed += amount;
    }

    public void DecreaseSpawnInterval(float amount, float minInterval)
    {
        _currentSpawnInterval -= amount;
        if (_currentSpawnInterval < minInterval)
        {
            _currentSpawnInterval = minInterval;
        }
    }

    public float GetCurrentSpeed()
    {
        return _currentSpeed;
    }

    public float GetCurrentSpawnInterval()
    {
        return _currentSpawnInterval;
    }

    public void StopSpawning()
    {
        _isSpawning = false;
    }
}