using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Clips")]
    [SerializeField] private AudioClip hitNormal;
    [SerializeField] private AudioClip hitRage;
    [SerializeField] private AudioClip failPunish;
    [SerializeField] private AudioClip rageActivate;
    [SerializeField] private AudioClip chainSuccess;
    [SerializeField] private AudioClip telegraphStep;

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Play(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, masterVolume * volumeScale);
    }

    public void PlayHitNormal() => Play(hitNormal);
    public void PlayHitRage() => Play(hitRage, 1.2f);
    public void PlayFailPunish() => Play(failPunish);
    public void PlayRageActivate() => Play(rageActivate);
    public void PlayChainSuccess() => Play(chainSuccess);
    public void PlayTelegraphStep() => Play(telegraphStep);
}