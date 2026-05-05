using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MemoryController : MonoBehaviour
{
    public static MemoryController Instance { get; private set; }

    [Header("Grid")]
    public Transform gridParent;
    public GameObject cardPrefab;

    [Header("Sprites")]
    public Sprite[] cardSprites;

    [Header("UI — Juego")]
    public TMP_Text timerText;

    [Header("UI — Resultado")]
    public GameObject resultPanel;
    public TMP_Text titleText;
    public TMP_Text timeText;
    public TMP_Text pointsText;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    private List<CardController> _cards = new();
    private CardController _first = null;
    private CardController _second = null;
    private bool _canPick = true;
    private bool _running = false;
    private float _elapsed = 0f;
    private int _pairsFound = 0;
    private const int PAIRS = 8;

    static readonly Color[] COLORS =
    {
        new Color(0.95f, 0.30f, 0.30f),
        new Color(0.25f, 0.65f, 0.95f),
        new Color(0.25f, 0.80f, 0.35f),
        new Color(0.95f, 0.80f, 0.15f),
        new Color(0.65f, 0.25f, 0.95f),
        new Color(0.95f, 0.50f, 0.10f),
        new Color(0.10f, 0.80f, 0.75f),
        new Color(0.95f, 0.35f, 0.70f),
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        resultPanel.SetActive(false);
        AudioManager.Instance.ResetMatchIndex();
        SpawnCards();
        _running = true;
    }

    void Update()
    {
        if (!_running) return;
        _elapsed += Time.deltaTime;
        timerText.text = $"Tiempo: {Mathf.FloorToInt(_elapsed)}s";
    }

    void SpawnCards()
    {
        var ids = new List<int>();
        for (int i = 0; i < PAIRS; i++) { ids.Add(i); ids.Add(i); }

        for (int i = ids.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }

        foreach (int id in ids)
        {
            var go = Instantiate(cardPrefab, gridParent);
            var card = go.GetComponent<CardController>();
            card.Setup(id, COLORS[id], cardSprites[id]);
            _cards.Add(card);
        }
    }

    public void OnCardSelected(CardController card)
    {
        if (!_canPick) return;

        if (_first == null)
        {
            _first = card;
            card.FlipToFront();
            AudioManager.Instance.PlayFlip();
            return;
        }

        if (_first == card) return;

        _second = card;
        _canPick = false;
        AudioManager.Instance.PlayFlip();
        card.FlipToFront(() => StartCoroutine(Evaluate()));
    }

    IEnumerator Evaluate()
    {
        yield return new WaitForSeconds(0.8f);

        if (_first.cardId == _second.cardId)
        {
            _first.isMatched = true;
            _second.isMatched = true;
            _pairsFound++;
            AudioManager.Instance.PlayMatch();

            if (_pairsFound >= PAIRS) { GameWon(); yield break; }
        }
        else
        {
            AudioManager.Instance.PlayWrong();
            _first.FlipToBack();
            _second.FlipToBack();
        }

        _first = null;
        _second = null;
        _canPick = true;
    }

    void GameWon()
    {
        _running = false;
        AudioManager.Instance.PlayVictory();

        int pts;
        int stars;
        string titulo;

        if (_elapsed < 30f)
        {
            pts = 100;
            stars = 3;
            titulo = "¡Increíble!";
        }
        else if (_elapsed < 60f)
        {
            pts = 50;
            stars = 2;
            titulo = "¡Muy bien!";
        }
        else
        {
            pts = 25;
            stars = 1;
            titulo = "¡Completado!";
        }

        titleText.text = titulo;
        timeText.text = $"Tiempo: {Mathf.FloorToInt(_elapsed)}s";
        pointsText.text = $"+{pts} puntos";

        star1.SetActive(stars >= 1);
        star2.SetActive(stars >= 2);
        star3.SetActive(stars >= 3);

        StartCoroutine(ShowResultPanel());

        StartCoroutine(GameManager.Instance.SendReward("memory", "Points", pts, 0));
    }

    IEnumerator ShowResultPanel()
    {
        resultPanel.SetActive(true);

        Transform panelBg = resultPanel.transform.Find("PanelBg");
        if (panelBg == null) yield break;

        panelBg.localScale = Vector3.zero;

        float t = 0f;
        float duration = 0.35f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float ratio = t / duration;
            float scale = 1f - Mathf.Pow(1f - ratio, 3f);
            panelBg.localScale = Vector3.one * scale;
            yield return null;
        }

        panelBg.localScale = Vector3.one;
    }

    public void OnBackToMenu() => SceneManager.LoadScene("MainMenu");
}