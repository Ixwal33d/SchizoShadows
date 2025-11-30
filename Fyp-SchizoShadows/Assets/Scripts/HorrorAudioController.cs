using UnityEngine;

public class HorrorAudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] ambienceClips;

    public float minDelay = 5f;
    public float maxDelay = 12f;

    private float timer;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            PlayRandomAmbience();
            ResetTimer();
        }
    }

    void PlayRandomAmbience()
    {
        if (ambienceClips.Length == 0) return;

        AudioClip clip = ambienceClips[Random.Range(0, ambienceClips.Length)];
        audioSource.PlayOneShot(clip);
    }

    void ResetTimer()
    {
        timer = Random.Range(minDelay, maxDelay);
    }
}
