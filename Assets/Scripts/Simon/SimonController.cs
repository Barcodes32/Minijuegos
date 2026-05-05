using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SimonController : MonoBehaviour
{
    [Header("Botones")]
    public SimonButton[] buttons = new SimonButton[4]; // 0:Rojo, 1:Azul, 2:Verde, 3:Amarillo

    [Header("UI")]
    public TMP_Text levelText;
    public TMP_Text instructionText;
    public GameObject resultPanel;
    public TMP_Text resultTitle;
    public TMP_Text pointsText;
    public Button startButton;

    [Header("Configuración")]
    public float showDelay = 0.5f;
    public float buttonHighlightDuration = 0.5f;

    private List<int> _sequence = new List<int>();
    private List<int> _playerInput = new List<int>();
    private int _currentLevel = 0;
    private bool _isShowingSequence = false;
    private bool _isPlayerTurn = false;
    private bool _gameStarted = false;

    void Start()
    {
        resultPanel.SetActive(false);
        instructionText.text = "Presioná START para comenzar";
        levelText.text = "Nivel: 0";
        startButton.onClick.AddListener(StartGame);

        // Deshabilitar botones al inicio
        foreach (var btn in buttons)
        {
            btn.button.interactable = false;
        }
    }

    void StartGame()
    {
        _gameStarted = true;
        startButton.gameObject.SetActive(false);
        _sequence.Clear();
        _currentLevel = 0;
        NextLevel();
    }

    void NextLevel()
    {
        _currentLevel++;
        levelText.text = $"Nivel: {_currentLevel}";
        instructionText.text = "Observá la secuencia...";

        // Agregar nuevo paso a la secuencia
        _sequence.Add(Random.Range(0, 4));

        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
        _isShowingSequence = true;
        _playerInput.Clear();

        // Deshabilitar botones
        foreach (var btn in buttons)
        {
            btn.button.interactable = false;
        }

        yield return new WaitForSeconds(showDelay);

        // Mostrar secuencia
        foreach (int index in _sequence)
        {
            yield return StartCoroutine(HighlightButton(index));
            yield return new WaitForSeconds(showDelay);
        }

        _isShowingSequence = false;
        _isPlayerTurn = true;
        instructionText.text = "¡Tu turno! Repetí la secuencia";

        // Habilitar botones
        foreach (var btn in buttons)
        {
            btn.button.interactable = true;
        }
    }

    IEnumerator HighlightButton(int index)
    {
        buttons[index].Highlight();
        SimonAudioManager.Instance.PlayButtonSound(index);
        yield return new WaitForSeconds(buttonHighlightDuration);
        buttons[index].ResetColor();
    }

    public void OnButtonPressed(int index)
    {
        if (!_isPlayerTurn) return;

        StartCoroutine(HighlightButton(index));
        _playerInput.Add(index);

        // Verificar si la entrada es correcta
        if (_playerInput[_playerInput.Count - 1] != _sequence[_playerInput.Count - 1])
        {
            // Error
            GameOver();
            return;
        }

        // Verificar si completó la secuencia
        if (_playerInput.Count == _sequence.Count)
        {
            _isPlayerTurn = false;
            SimonAudioManager.Instance.PlayLevelComplete();
            StartCoroutine(NextLevelDelay());
        }
    }

    IEnumerator NextLevelDelay()
    {
        instructionText.text = "¡Correcto!";
        yield return new WaitForSeconds(1f);
        NextLevel();
    }

    void GameOver()
    {
        _isPlayerTurn = false;
        SimonAudioManager.Instance.PlayError();

        // Deshabilitar botones
        foreach (var btn in buttons)
        {
            btn.button.interactable = false;
        }

        int points = (_currentLevel - 1) * 10; // 10 puntos por nivel completado
        ShowResult(points);
    }

    void ShowResult(int points)
    {
        resultPanel.SetActive(true);

        if (_currentLevel <= 3)
        {
            resultTitle.text = "Seguí practicando";
        }
        else if (_currentLevel <= 6)
        {
            resultTitle.text = "¡Bien hecho!";
        }
        else
        {
            resultTitle.text = "¡Increíble!";
        }

        pointsText.text = $"+{points} puntos\nNivel alcanzado: {_currentLevel - 1}";

        StartCoroutine(GameManager.Instance.SendReward(
            "simon",    
            "Points",
            points,
            0
        ));
    }

    public void OnBackToMenu() => SceneManager.LoadScene("MainMenu");
}
