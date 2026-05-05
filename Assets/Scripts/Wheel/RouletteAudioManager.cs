using UnityEngine;

public class RouletteAudioManager : MonoBehaviour
{
    public static RouletteAudioManager Instance { get; private set; }

    [Header("Sonidos Ruleta")]
    public AudioClip spinSound;
    public AudioClip spinAgainSound;
    public AudioClip rewardSound;
    public AudioClip bigRewardSound;

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

    public void PlaySpin()
    {
        _audioSource.clip = spinSound;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    public void StopSpin()
    {
        _audioSource.loop = false;
        _audioSource.Stop();
    }

    public void PlaySpinAgain()
    {
        _audioSource.PlayOneShot(spinAgainSound);
    }

    public void PlayReward()
    {
        _audioSource.PlayOneShot(rewardSound);
    }

    public void PlayBigReward()
    {
        _audioSource.PlayOneShot(bigRewardSound);
    }
}
