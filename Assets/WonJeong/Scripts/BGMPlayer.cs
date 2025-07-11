using UnityEngine;
using System.Collections;

public class BGMPlayer : MonoBehaviour
{
    public static BGMPlayer Instance { get; private set; }
    private AudioSource bgmSource;
    private Coroutine fadeCoroutine;

    [SerializeField] private float fadeDuration = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        bgmSource = GetComponent<AudioSource>();
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || (bgmSource.clip == clip && bgmSource.isPlaying)) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeToBGM(clip));
    }

    private IEnumerator FadeToBGM(AudioClip newClip)
    {
        // ���� ���� ����
        float startVolume = bgmSource.volume;

        // ���� ��� ���� ���� ������ ���̱�
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();

        // �ٽ� ������ ���� ��������
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        bgmSource.volume = startVolume;
    }

    public void StopBGM()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        bgmSource.Stop();
    }
}