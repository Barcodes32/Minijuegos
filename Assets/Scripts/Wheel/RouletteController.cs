using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RouletteController : MonoBehaviour
{
    [Header("Ruleta")]
    public Image wheelImage;
    public Button spinButton;

    [Header("Secciones — en orden del sprite")]
    public WheelSection[] sections = new WheelSection[8];

    [Header("UI — Resultado")]
    public GameObject resultPanel;
    public TMP_Text resultTitle;
    public TMP_Text resultDescription;

    [Header("Calibración")]
    public float firstSectionAngle = 0f;
    private const float SECTION_ANGLE = 45f;
    private const float MIN_SPINS = 5f;
    private const float MAX_SPINS = 8f;

    private bool _spinning = false;
    private float _currentAngle = 0f;

    void Start()
    {
        resultPanel.SetActive(false);
        spinButton.onClick.AddListener(OnSpinClicked);
    }

    void OnSpinClicked()
    {
        if (_spinning) return;
        StartCoroutine(SpinWheel());
    }

    IEnumerator SpinWheel()
    {
        _spinning = true;
        spinButton.interactable = false;

        // Reproducir sonido de girar
        RouletteAudioManager.Instance.PlaySpin();

        float extraSpins = Random.Range(MIN_SPINS, MAX_SPINS) * 360f;
        float randomExtra = Random.Range(0f, 360f);
        float finalAngle = _currentAngle + extraSpins + randomExtra;

        float duration = Random.Range(3f, 5f);
        float elapsed = 0f;
        float startAngle = _currentAngle;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            _currentAngle = Mathf.Lerp(startAngle, finalAngle, eased);
            wheelImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, -_currentAngle);
            yield return null;
        }

        _currentAngle = finalAngle;
        wheelImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, -_currentAngle);

        // Detener sonido de girar
        RouletteAudioManager.Instance.StopSpin();

        _spinning = false;

        int winIndex = GetSectionAtPointer(_currentAngle);
        ShowResult(sections[winIndex]);
    }

    int GetSectionAtPointer(float angle)
    {
        float normalized = ((angle % 360f) + 360f) % 360f;
        float adjusted = ((normalized - firstSectionAngle) % 360f + 360f) % 360f;
        int index = Mathf.FloorToInt(adjusted / SECTION_ANGLE) % sections.Length;

        Debug.Log($"Ángulo: {normalized:F1}° → Ajustado: {adjusted:F1}° → Sección: {index} ({sections[index].name})");

        return index;
    }

    void ShowResult(WheelSection section)
    {
        resultPanel.SetActive(true);

        switch (section.type)
        {
            case SectionType.Points:
                RouletteAudioManager.Instance.PlayReward();
                resultTitle.text = "¡Ganaste puntos!";
                resultDescription.text = $"+{section.points} puntos";
                break;
            case SectionType.BonusPoints:
                RouletteAudioManager.Instance.PlayBigReward();
                resultTitle.text = "¡Bonus!";
                resultDescription.text = "+20% de puntos en tu próxima compra";
                break;
            case SectionType.Discount:
                if (section.discountPct >= 20)
                    RouletteAudioManager.Instance.PlayBigReward();
                else
                    RouletteAudioManager.Instance.PlayReward();
                resultTitle.text = "¡Descuento!";
                resultDescription.text = $"{section.discountPct}% de descuento";
                break;
            case SectionType.SpecialPrize:
                RouletteAudioManager.Instance.PlayBigReward();
                resultTitle.text = "¡Premio Especial!";
                resultDescription.text = "Reclamá tu premio en tienda";
                break;
            case SectionType.SpinAgain:
                RouletteAudioManager.Instance.PlaySpinAgain();
                resultTitle.text = "¡Tirá de nuevo!";
                resultDescription.text = "¡Tenés otra oportunidad!";
                StartCoroutine(SpinAgainDelay());
                return;
            case SectionType.Nothing:
                resultTitle.text = "Sin premio";
                resultDescription.text = "¡Mejor suerte la próxima!";
                break;
        }

        StartCoroutine(GameManager.Instance.SendReward(
            "roulette",
            section.type.ToString(),
            section.points,
            section.discountPct
        ));
    }

    IEnumerator SpinAgainDelay()
    {
        yield return new WaitForSeconds(1.5f);
        resultPanel.SetActive(false);
        spinButton.interactable = true;
        StartCoroutine(SpinWheel());
    }

    public void OnBackToMenu() => SceneManager.LoadScene("MainMenu");
}