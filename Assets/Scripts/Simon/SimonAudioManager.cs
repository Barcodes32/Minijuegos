using UnityEngine;

public class SimonAudioManager : MonoBehaviour
{
    public static SimonAudioManager Instance { get; private set; }

    [Header("Sonidos de Botones")]
    public AudioClip buttonSound0; // Rojo
    public AudioClip buttonSound1; // Azul
    public AudioClip buttonSound2; // Verde
    public AudioClip buttonSound3; // Amarillo

    [Header("Sonidos de Eventos")]
    public AudioClip errorSound;
    public AudioClip levelCompleteSound;

    private AudioSource _audioSource;

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

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayButtonSound(int index)
    {
        AudioClip clip = null;
        switch (index)
        {
            case 0: clip = buttonSound0; break;
            case 1: clip = buttonSound1; break;
            case 2: clip = buttonSound2; break;
            case 3: clip = buttonSound3; break;
        }

        if (clip != null)
            _audioSource.PlayOneShot(clip);
    }

    public void PlayError()
    {
        _audioSource.PlayOneShot(errorSound);
    }

    public void PlayLevelComplete()
    {
        _audioSource.PlayOneShot(levelCompleteSound);
    }
}
