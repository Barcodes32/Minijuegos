using UnityEngine;

public class FlappyAudioManager : MonoBehaviour
{
    public static FlappyAudioManager Instance { get; private set; }

    [Header("Sonidos")]
    public AudioClip jumpSound;
    public AudioClip scoreSound;
    public AudioClip dieSound;

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

    public void PlayJump()
    {
        if (jumpSound != null)
            _audioSource.PlayOneShot(jumpSound);
    }

    public void PlayScore()
    {
        if (scoreSound != null)
            _audioSource.PlayOneShot(scoreSound);
    }

    public void PlayDie()
    {
        if (dieSound != null)
            _audioSource.PlayOneShot(dieSound);
    }
}