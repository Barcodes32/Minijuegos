using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UserData
{
    public string name;
    public int points;
}

[System.Serializable]
public class PlaysData
{
    public int playsToday;
    public int maxPlays;
    public bool canPlay;
}

public class MenuManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text userNameText;
    public TMP_Text pointsText;
    public TMP_Text playsText;
    public Button playButton;
    public GameObject loadingPanel;

    [Header("Configuración")]
    public string gameSceneName = "SimonGame";
    public string gameName = "simon"; // "simon", "flappy", "roulette", "memory", "scratch"

    private string userId;
    private const string USER_API = "https://ecommerce-backend-dy79.onrender.com/api/v1/games/user/";
    private const string PLAYS_API = "https://ecommerce-backend-dy79.onrender.com/api/v1/games/user/";

    void Start()
    {
        // Obtener userId desde React/JavaScript
#if UNITY_WEBGL && !UNITY_EDITOR
            // En WebGL, window.unityUserId se setea desde JavaScript antes de cargar Unity
            userId = "guest"; // Default, se reemplaza por JS
#else
        userId = "test123"; // Para testing en Editor
#endif

        StartCoroutine(LoadData());
    }

    IEnumerator LoadData()
    {
        loadingPanel.SetActive(true);
        playButton.interactable = false;

        // Cargar datos del usuario
        yield return StartCoroutine(LoadUserData());

        // Cargar partidas restantes
        yield return StartCoroutine(LoadPlaysRemaining());

        loadingPanel.SetActive(false);
    }

    IEnumerator LoadUserData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(USER_API + userId))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                UserData userData = JsonUtility.FromJson<UserData>(json);

                userNameText.text = $"{userData.name}";
                pointsText.text = $"{userData.points:N0} puntos";
            }
            else
            {
                Debug.LogError($"Error cargando usuario: {request.error}");
                userNameText.text = "Usuario";
                pointsText.text = "0 puntos";
            }
        }
    }

    IEnumerator LoadPlaysRemaining()
    {
        string url = $"{PLAYS_API}{userId}/plays/{gameName}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                PlaysData playsData = JsonUtility.FromJson<PlaysData>(json);

                playsText.text = $"Partidas hoy: {playsData.playsToday}/{playsData.maxPlays}";

                if (playsData.canPlay)
                {
                    playButton.interactable = true;
                }
                else
                {
                    playButton.interactable = false;
                    playsText.text += "\nLímite diario alcanzado";
                }
            }
            else
            {
                Debug.LogError($"Error cargando partidas: {request.error}");
                playsText.text = "Partidas hoy: ?/?";
                playButton.interactable = true; // Por si falla el request, permitir jugar
            }
        }
    }

    public void OnPlayClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}