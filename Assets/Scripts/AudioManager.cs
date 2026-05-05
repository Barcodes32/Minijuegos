using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Voltear carta (4 variantes)")]
    public AudioClip[] flipSounds;

    [Header("Par encontrado (4 variantes, en orden)")]
    public AudioClip[] matchSounds;

    [Header("Par incorrecto (4 variantes)")]
    public AudioClip[] wrongSounds;

    [Header("Victoria (4 variantes)")]
    public AudioClip[] victorySounds;

    private AudioSource _source;
    private int _matchIndex = 0;  // progresa en orden

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _source = GetComponent<AudioSource>();
    }

    public void PlayFlip()
        => PlayRandom(flipSounds);

    public void PlayMatch()
    {
        if (matchSounds.Length == 0) return;
        _source.PlayOneShot(matchSounds[_matchIndex % matchSounds.Length]);
        _matchIndex++;
    }

    public void PlayWrong()
        => PlayRandom(wrongSounds);

    public void PlayVictory()
        => PlayRandom(victorySounds);

    public void ResetMatchIndex()
        => _matchIndex = 0;

    void PlayRandom(AudioClip[] clips)
    {
        if (clips.Length == 0) return;
        _source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}