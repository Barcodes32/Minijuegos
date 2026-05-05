using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FlappyGameManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;

    [Header("Referencias")]
    public ObstacleSpawner spawner;

    [Header("Dificultad Progresiva")]
    public float difficultyIncreaseInterval = 10f;
    public float minSpawnInterval = 1f;

    private int _pointsPerObstacle = 10; // Valor por defecto
    private float _speedIncrease = 0.5f; // Valor por defecto
    private int _score = 0;
    private bool _gameOver = false;
    private float _gameTime = 0f;
    private bool _gameStarted = false;

    void Start()
    {
        gameOverPanel.SetActive(false);
        UpdateScoreText();
        StartCoroutine(LoadConfigAndStart());
    }

    IEnumerator LoadConfigAndStart()
    {
        // Esperar a que la configuración se cargue
        yield return StartCoroutine(ConfigManager.Instance.LoadConfig());

        // Aplicar configuración
        if (ConfigManager.Instance.configLoaded && ConfigManager.Instance.configs.flappy != null)
        {
            _pointsPerObstacle = ConfigManager.Instance.configs.flappy.pointsPerObstacle;
            _speedIncrease = ConfigManager.Instance.configs.flappy.speedIncrease;

            // Configurar spawner con velocidad inicial
            spawner.baseSpeed = ConfigManager.Instance.configs.flappy.initialSpeed;

            Debug.Log($"Config cargada - Puntos: {_pointsPerObstacle}, Velocidad: {spawner.baseSpeed}, Aceleración: {_speedIncrease}");
        }

        _gameStarted = true;
        StartCoroutine(IncreaseDifficulty());
    }

    void Update()
    {
        if (!_gameOver && _gameStarted)
        {
            _gameTime += Time.deltaTime;
        }
    }

    IEnumerator IncreaseDifficulty()
    {
        while (!_gameOver)
        {
            yield return new WaitForSeconds(difficultyIncreaseInterval);

            if (!_gameOver)
            {
                // Usar el speedIncrease de la config
                spawner.IncreaseSpeed(_speedIncrease);

                // Reducir intervalo de spawn (esto puede quedarse fijo)
                spawner.DecreaseSpawnInterval(0.1f, minSpawnInterval);

                Debug.Log($"Dificultad aumentada! Velocidad: {spawner.GetCurrentSpeed()}, Intervalo: {spawner.GetCurrentSpawnInterval()}");
            }
        }
    }

    public void AddScore()
    {
        if (_gameOver) return;

        // Usar puntos de la config
        _score += _pointsPerObstacle;
        UpdateScoreText();
        FlappyAudioManager.Instance.PlayScore();
    }

    void UpdateScoreText()
    {
        scoreText.text = _score.ToString();
    }

    public void GameOver()
    {
        if (_gameOver) return;

        _gameOver = true;
        spawner.StopSpawning();

        gameOverPanel.SetActive(true);
        finalScoreText.text = $"Puntos: {_score}\nTiempo: {_gameTime:F1}s";

        StartCoroutine(GameManager.Instance.SendReward(
            "flappy",
            "Points",
            _score,
            0
        ));
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}