using UnityEngine;
using System.Collections;

/*  씬에서 메인 BGM을 플레이하는 스크립트  */
public class BGMPlayer : MonoBehaviour
{
    public static BGMPlayer Instance { get; private set; }

    private AudioSource bgmSource;
    private Coroutine fadeCoroutine;    // 보스 스테이지 진행 시 BGM전환 코루틴 저장 변수

    [SerializeField] private float fadeDuration = 1.5f; // BGM 변환 페이드 시간

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

    /***********************************************************************************
     * 작성자: 박원정
     * 함수: PlayBGM
     * 기능: 씬에서 BGM을 재생함
     * 입력: 
     *  - clip: 플레이 할 BGM
     ***********************************************************************************/
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || (bgmSource.clip == clip && bgmSource.isPlaying)) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeToBGM(clip));
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: FadeToBGM
    * 기능: BGM을 Fade 하여 자연스럽게 변환시킴
    * 입력: 
    *  - newClip: 변환시킬 새로운 BGM
    ***********************************************************************************/
    private IEnumerator FadeToBGM(AudioClip newClip)
    {
        // 현재 볼륨 저장
        float startVolume = bgmSource.volume;

        // 현재 재생 중인 음악 서서히 줄이기
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;   // BGM소스 변환 및 플레이
        bgmSource.Play();

        // 다시 서서히 원래 볼륨으로
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        bgmSource.volume = startVolume;
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: StopBGM
    * 기능: 현재 실행 중인 BGM을 멈춤    
    ***********************************************************************************/
    public void StopBGM()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        bgmSource.Stop();
    }
}