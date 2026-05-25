using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clip")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip gameLoseSound, gameWinSound;
    [SerializeField] private AudioClip planeHitSound;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayCollectSound()
    {
        audioSource.PlayOneShot(collectSound);
    }

    public void PlayGameOverSound(int winOrLose)
    {
        if (winOrLose == 0)
            audioSource.PlayOneShot(gameLoseSound);
        else
            audioSource.PlayOneShot(gameWinSound);
    }

    public void PlayPlaneHitSound()
    {
        audioSource.PlayOneShot(planeHitSound);
    }
}