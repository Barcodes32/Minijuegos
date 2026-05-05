using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// ScratchController.cs — versión final completa
///
/// HIERARCHY requerida:
/// Canvas
/// ├── Background (Image)
/// ├── CardContainer (Panel)
/// │   ├── PrizeRevealImage (Image)   ← imagen del premio debajo
/// │   ├── ScratchHint (GameObject)   ← texto "raspa aquí"
/// │   └── ScratchLayer (Raw Image)   ← este script va acá
/// ├── RevealAllButton (Button)       ← FUERA del ResultPanel
/// ├── ResultPanel (Panel)            ← inactivo al inicio
/// │   ├── TitleText (TMP o Text)
/// │   └── DescText (TMP o Text)
/// └── DailyLimitText (TMP o Text)    ← inactivo al inicio
/// </summary>
public class ScratchController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // ─────────────────────────────────────────────
    // INSPECTOR
    // ─────────────────────────────────────────────

    [Header("=== SCRATCH CARD ===")]
    public RawImage scratchLayer;
    public Image prizeRevealImage;
    public Sprite[] prizeSprites;         // Size=4: [0]Puntos [1]Descuento [2]SinPremio [3]Producto

    [Range(20f, 150f)]
    public float brushSize = 60f;

    [Header("=== PREMIOS ===")]
    public PrizeConfig[] prizes = new PrizeConfig[]
    {
        new PrizeConfig { type = PrizeType.Points,      label = "10 Puntos",        points = 10,  discountPct = 0,  weight = 35 },
        new PrizeConfig { type = PrizeType.Points,      label = "25 Puntos",        points = 25,  discountPct = 0,  weight = 25 },
        new PrizeConfig { type = PrizeType.Points,      label = "50 Puntos",        points = 50,  discountPct = 0,  weight = 15 },
        new PrizeConfig { type = PrizeType.Points,      label = "100 Puntos",       points = 100, discountPct = 0,  weight = 8  },
        new PrizeConfig { type = PrizeType.Discount,    label = "5% Descuento",     points = 0,   discountPct = 5,  weight = 8  },
        new PrizeConfig { type = PrizeType.Discount,    label = "10% Descuento",    points = 0,   discountPct = 10, weight = 5  },
        new PrizeConfig { type = PrizeType.NoWin,       label = "No ganaste nada.", points = 0,   discountPct = 0,  weight = 3  },
        new PrizeConfig { type = PrizeType.FreeProduct, label = "Producto Gratis!", points = 0,   discountPct = 0,  weight = 1  },
    };

    [Header("=== THRESHOLD ===")]
    [Range(0.3f, 0.9f)]
    public float revealThreshold = 0.6f;  // 60% rascado = revelar

    [Range(5, 30)]
    public int checkInterval = 10;

    [Header("=== UI ===")]
    public GameObject resultPanel;
    public GameObject resultTitleTextObj;  // Arrastrá el TitleText aquí
    public GameObject resultDescTextObj;   // Arrastrá el DescText aquí
    public Button revealAllButton;     // El botón FUERA del ResultPanel
    public GameObject dailyLimitTextObj;   // Arrastrá el DailyLimitText aquí
    public GameObject scratchHintUI;       // El hint "raspa aquí"

    [Header("=== AUDIO ===")]
    public AudioSource audioSource;
    public AudioClip scratchSound;
    public AudioClip revealSound;
    public AudioClip noWinSound;

    [Header("=== CONFIG ===")]
    public int textureResolution = 256;

    // ─────────────────────────────────────────────
    // ESTADO INTERNO
    // ─────────────────────────────────────────────
    private Texture2D _scratchTex;
    private Color[] _pixels;
    private Color _transparent = new Color(0, 0, 0, 0);

    private PrizeConfig _selectedPrize;
    private bool _isScratching = false;
    private bool _prizeRevealed = false;
    private bool _hasPlayedToday = false;
    private int _frameCount = 0;
    private int _totalSampledPixels;

    private RectTransform _scratchRect;
    private Camera _uiCamera = null; // null = Screen Space Overlay

    private const string PREFS_DATE = "ScratchLastPlayDate";

    // ─────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────
    void Awake()
    {
        _scratchRect = scratchLayer.GetComponent<RectTransform>();
    }

    void Start()
    {
        // Ocultar UI al inicio
        if (resultPanel != null)
        {
            resultPanel.transform.localScale = Vector3.one;
            resultPanel.SetActive(false);
        }
        if (dailyLimitTextObj != null) dailyLimitTextObj.SetActive(false);

        // Asignar listener al botón
        if (revealAllButton != null)
            revealAllButton.onClick.AddListener(OnRevealAllClicked);

        //CheckDailyLimit();

        if (!_hasPlayedToday)
        {
            SelectPrize();
            SetupScratchTexture();
            SetupPrizeReveal();
        }
    }

    // ─────────────────────────────────────────────
    // DAILY LIMIT
    // ─────────────────────────────────────────────
    void CheckDailyLimit()
    {
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        _hasPlayedToday = (PlayerPrefs.GetString(PREFS_DATE, "") == today);

        if (_hasPlayedToday)
        {
            if (scratchLayer != null) scratchLayer.raycastTarget = false;
            if (revealAllButton != null) revealAllButton.gameObject.SetActive(false);
            if (dailyLimitTextObj != null) dailyLimitTextObj.SetActive(true);
        }
    }

    void SaveDailyLimit()
    {
        PlayerPrefs.SetString(PREFS_DATE, System.DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
    }

    // ─────────────────────────────────────────────
    // SELECCIÓN DE PREMIO
    // ─────────────────────────────────────────────
    void SelectPrize()
    {
        int total = 0;
        foreach (var p in prizes) total += p.weight;

        int roll = Random.Range(0, total);
        int cumulative = 0;
        foreach (var p in prizes)
        {
            cumulative += p.weight;
            if (roll < cumulative) { _selectedPrize = p; return; }
        }
        _selectedPrize = prizes[0];
    }

    // ─────────────────────────────────────────────
    // SETUP TEXTURA (sin shader)
    // ─────────────────────────────────────────────
    void SetupScratchTexture()
    {
        _scratchTex = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, false);
        _scratchTex.filterMode = FilterMode.Bilinear;

        int total = textureResolution * textureResolution;
        _pixels = new Color[total];

        Color silver = new Color(0.78f, 0.78f, 0.78f, 1f);
        for (int i = 0; i < total; i++)
            _pixels[i] = silver;

        _scratchTex.SetPixels(_pixels);
        _scratchTex.Apply();

        _totalSampledPixels = total / 4;
        scratchLayer.texture = _scratchTex;
        scratchLayer.color = Color.white;
        scratchLayer.raycastTarget = true;
    }

    void SetupPrizeReveal()
    {
        if (prizeRevealImage == null) return;

        int idx = 0;
        switch (_selectedPrize.type)
        {
            case PrizeType.Points: idx = 0; break;
            case PrizeType.Discount: idx = 1; break;
            case PrizeType.NoWin: idx = 2; break;
            case PrizeType.FreeProduct: idx = 3; break;
        }

        if (prizeSprites != null && idx < prizeSprites.Length && prizeSprites[idx] != null)
            prizeRevealImage.sprite = prizeSprites[idx];
    }

    // ─────────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────────
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_hasPlayedToday || _prizeRevealed) return;
        _isScratching = true;

        // Ocultar botón en cuanto empieza a raspar
        if (revealAllButton != null)
            revealAllButton.gameObject.SetActive(false);

        if (scratchHintUI != null) scratchHintUI.SetActive(false);

        if (audioSource != null && scratchSound != null)
        {
            audioSource.clip = scratchSound;
            audioSource.loop = true;
            if (!audioSource.isPlaying) audioSource.Play();
        }

        DoScratch(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isScratching || _prizeRevealed) return;
        DoScratch(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isScratching = false;
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    // ─────────────────────────────────────────────
    // LÓGICA DE RASCADO
    // ─────────────────────────────────────────────
    void DoScratch(Vector2 screenPos)
    {
        Vector2 localPoint;
        bool inside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _scratchRect, screenPos, _uiCamera, out localPoint);

        if (!inside) return;

        float u = (localPoint.x + _scratchRect.rect.width * 0.5f) / _scratchRect.rect.width;
        float v = (localPoint.y + _scratchRect.rect.height * 0.5f) / _scratchRect.rect.height;

        int cx = Mathf.Clamp(Mathf.RoundToInt(u * textureResolution), 0, textureResolution - 1);
        int cy = Mathf.Clamp(Mathf.RoundToInt(v * textureResolution), 0, textureResolution - 1);

        float ratio = (float)textureResolution / _scratchRect.rect.width;
        int radius = Mathf.Max(4, Mathf.RoundToInt(brushSize * ratio * 0.5f));

        EraseCircle(cx, cy, radius);

        _scratchTex.SetPixels(_pixels);
        _scratchTex.Apply();

        _frameCount++;
        if (_frameCount % checkInterval == 0)
            CheckPercentage();
    }

    void EraseCircle(int cx, int cy, int radius)
    {
        int r2 = radius * radius;
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y > r2) continue;
                int px = cx + x;
                int py = cy + y;
                if (px < 0 || px >= textureResolution) continue;
                if (py < 0 || py >= textureResolution) continue;
                _pixels[py * textureResolution + px] = _transparent;
            }
        }
    }

    // ─────────────────────────────────────────────
    // VERIFICAR % RASCADO
    // ─────────────────────────────────────────────
    void CheckPercentage()
    {
        if (_totalSampledPixels == 0) return;

        int transparentCount = 0;
        for (int i = 0; i < _pixels.Length; i += 4)
            if (_pixels[i].a < 0.1f) transparentCount++;

        float pct = (float)transparentCount / _totalSampledPixels;
        Debug.Log($"[Scratch] {pct * 100f:F1}% rascado");

        if (pct >= revealThreshold)
            RevealPrize();
    }

    // ─────────────────────────────────────────────
    // BOTÓN "REVELAR TODO"
    // ─────────────────────────────────────────────
    void OnRevealAllClicked()
    {
        if (_prizeRevealed) return;

        // Borrar toda la textura
        for (int i = 0; i < _pixels.Length; i++)
            _pixels[i] = _transparent;

        _scratchTex.SetPixels(_pixels);
        _scratchTex.Apply();

        // Ocultar el botón
        if (revealAllButton != null)
            revealAllButton.gameObject.SetActive(false);

        // Revelar
        RevealPrize();
    }

    // ─────────────────────────────────────────────
    // REVELAR PREMIO
    // ─────────────────────────────────────────────
    void RevealPrize()
    {
        if (_prizeRevealed) return;
        _prizeRevealed = true;

        // Deshabilitar input y botón
        scratchLayer.raycastTarget = false;
        if (revealAllButton != null)
            revealAllButton.gameObject.SetActive(false);

        SaveDailyLimit();

        // Audio
        if (audioSource != null)
        {
            audioSource.loop = false;
            if (audioSource.isPlaying) audioSource.Stop();
            AudioClip clip = (_selectedPrize.type == PrizeType.NoWin) ? noWinSound : revealSound;
            if (clip != null) audioSource.PlayOneShot(clip);
        }

        // Mostrar resultado
        ShowResultPanel();

        // Enviar al backend
        if (GameManager.Instance != null)
        {
            StartCoroutine(GameManager.Instance.SendReward(
                gameType: "scratch",
                rewardType: _selectedPrize.type.ToString(),
                points: _selectedPrize.points,
                discountPct: _selectedPrize.discountPct
            ));
        }
    }

    // ─────────────────────────────────────────────
    // MOSTRAR PANEL RESULTADO
    // ─────────────────────────────────────────────
    void ShowResultPanel()
    {
        if (resultPanel == null)
        {
            Debug.LogError("[Scratch] resultPanel es null! Asignar en el Inspector.");
            return;
        }

        resultPanel.transform.localScale = Vector3.one;
        resultPanel.SetActive(true);

        string titulo = (_selectedPrize.type == PrizeType.NoWin)
            ? "Sin suerte esta vez"
            : "Felicidades!";

        SetText(resultTitleTextObj, titulo);
        SetText(resultDescTextObj, _selectedPrize.label);

        Debug.Log($"[Scratch] Panel mostrado. Premio: {_selectedPrize.label}");
    }

    // ─────────────────────────────────────────────
    // HELPER: escribe texto en TMP o Text legacy
    // ─────────────────────────────────────────────
    void SetText(GameObject obj, string text)
    {
        if (obj == null) return;

        var tmp = obj.GetComponent<TMPro.TMP_Text>();
        if (tmp != null) { tmp.text = text; return; }

        var legacy = obj.GetComponent<Text>();
        if (legacy != null) { legacy.text = text; return; }

        Debug.LogWarning($"[Scratch] {obj.name} no tiene TMP_Text ni Text.");
    }

    // ─────────────────────────────────────────────
    // CLEANUP
    // ─────────────────────────────────────────────
    void OnDestroy()
    {
        if (_scratchTex != null) Destroy(_scratchTex);
    }
}

// ─────────────────────────────────────────────
// DATA
// ─────────────────────────────────────────────
public enum PrizeType { Points, Discount, NoWin, FreeProduct }

[System.Serializable]
public class PrizeConfig
{
    public PrizeType type;
    public string label;
    public int points;
    public int discountPct;
    [Range(0, 100)]
    public int weight;
}