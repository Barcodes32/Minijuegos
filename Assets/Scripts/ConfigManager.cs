using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class FlappyConfig
{
    public int pointsPerObstacle;
    public float initialSpeed;
    public float speedIncrease;
}

[Serializable]
public class GameConfigs
{
    public FlappyConfig flappy;
    // Después agregamos simon, roulette, memory, scratch
}

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }

    private const string CONFIG_URL = "https://ecommerce-backend-dy79.onrender.com/api/v1/games/config";

    public GameConfigs configs;
    public bool configLoaded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator LoadConfig()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(CONFIG_URL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                configs = JsonUtility.FromJson<GameConfigs>(json);
                configLoaded = true;
                Debug.Log("Configuración cargada correctamente");
            }
            else
            {
                Debug.LogError($"Error cargando config: {request.error}");
                // Usar valores por defecto
                SetDefaultValues();
            }
        }
    }

    void SetDefaultValues()
    {
        configs = new GameConfigs();
        configs.flappy = new FlappyConfig
        {
            pointsPerObstacle = 10,
            initialSpeed = 5f,
            speedIncrease = 0.5f
        };
        configLoaded = true;
    }
}