using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager.cs (versión ampliada para Raspa y Gana)
/// 
/// Si ya tienes tu GameManager.cs de la Ruleta, solo AGREGA el método SendReward
/// con los parámetros adicionales (discountPct). El resto es igual.
/// 
/// Este archivo es la versión standalone por si partes de cero.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // SINGLETON
    // ─────────────────────────────────────────────
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // ─────────────────────────────────────────────
    // CONFIG
    // ─────────────────────────────────────────────
    [Header("=== BACKEND CONFIG ===")]
    public string backendURL = "http://localhost:3001";

    [Header("=== USER ===")]
    public string userId = ""; // Se obtiene desde React via jslib

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        //DontDestroyOnLoad(gameObject);

        // Obtener userId desde JavaScript (React)
        GetUserIdFromReact();
    }

    // ─────────────────────────────────────────────
    // OBTENER userId DESDE REACT
    // ─────────────────────────────────────────────
    void GetUserIdFromReact()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Llama a función JavaScript definida en GameBridge.jslib
        userId = GetUserIdJS();
#else
        // En editor, usar valor de prueba
        userId = "test-user-123";
        Debug.Log($"[GameManager] Editor mode - userId: {userId}");
#endif
    }

    // Declaración del método externo de JavaScript
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern string GetUserIdJS();

    // ─────────────────────────────────────────────
    // SEND REWARD (compatible Ruleta + Raspa y Gana)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Envía el premio al backend.
    /// Compatible con Ruleta (discountPct = 0) y Raspa y Gana.
    /// </summary>
    public IEnumerator SendReward(
        string gameType,
        string rewardType,
        int points = 0,
        int discountPct = 0)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[GameManager] userId vacío. No se puede enviar premio.");
            yield break;
        }

        // Construir payload
        RewardPayload payload = new RewardPayload
        {
            userId = userId,
            gameType = gameType,
            rewardType = rewardType,
            points = points,
            discountPct = discountPct,
            timestamp = System.DateTime.UtcNow.ToString("o")
        };

        string json = JsonUtility.ToJson(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        string endpoint = $"{backendURL}/api/games/{gameType}-reward";
        Debug.Log($"[GameManager] POST {endpoint} → {json}");

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[GameManager] ✅ Respuesta: {request.downloadHandler.text}");

                // Parsear respuesta
                RewardResponse response = JsonUtility.FromJson<RewardResponse>(request.downloadHandler.text);

                // Notificar a React
                NotifyReact(payload, response);
            }
            else
            {
                Debug.LogError($"[GameManager] ❌ Error: {request.error}");
                // Notificar error a React
                NotifyReactError(request.error);
            }
        }
    }

    // ─────────────────────────────────────────────
    // NOTIFY REACT VIA JAVASCRIPT
    // ─────────────────────────────────────────────
    void NotifyReact(RewardPayload payload, RewardResponse response)
    {
        // Construir objeto de evento para React
        GameCompleteEvent eventData = new GameCompleteEvent
        {
            gameType = payload.gameType,
            rewardType = payload.rewardType,
            points = payload.points,
            discountPct = payload.discountPct,
            totalPoints = response != null ? response.totalPoints : 0,
            success = true
        };

        string eventJson = JsonUtility.ToJson(eventData);

#if UNITY_WEBGL && !UNITY_EDITOR
        // Disparar evento JavaScript que React escucha
        DispatchGameEventJS(eventJson);
#else
        Debug.Log($"[GameManager] Evento para React: {eventJson}");
#endif
    }

    void NotifyReactError(string error)
    {
        string eventJson = $"{{\"success\":false,\"error\":\"{error}\"}}";

#if UNITY_WEBGL && !UNITY_EDITOR
        DispatchGameEventJS(eventJson);
#else
        Debug.LogError($"[GameManager] Error para React: {eventJson}");
#endif
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void DispatchGameEventJS(string eventJson);


    public void OnBackToMenu() => SceneManager.LoadScene("MainMenu");
}

// ─────────────────────────────────────────────
// DATA MODELS
// ─────────────────────────────────────────────
[System.Serializable]
public class RewardPayload
{
    public string userId;
    public string gameType;
    public string rewardType;
    public int points;
    public int discountPct;
    public string timestamp;
}

[System.Serializable]
public class RewardResponse
{
    public bool success;
    public int totalPoints;
    public string message;
    public string couponCode; // Para descuentos
}

[System.Serializable]
public class GameCompleteEvent
{
    public string gameType;
    public string rewardType;
    public int points;
    public int discountPct;
    public int totalPoints;
    public bool success;
}

