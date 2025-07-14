using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/* 플레이어의 체력에 따른 카메라의 Vignette를 조정하는 스크립트 */
/* 이 외에 플레이어 카메라의 Fade도 조정함 */
public class HealthVignetteController : MonoBehaviour
{
    private Volume volume;
    private Vignette vignette;
    private ColorAdjustments colorAdjust;

    private float fadeOutTimer = 0f;
    private float fadeOutDuration = 1.5f; // 페이드 아웃 시간

    private float fadeInTimer = 0f;
    private float fadeInDuration = 2f;

    private void Start()
    {
        volume = GetComponent<Volume>();

        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out colorAdjust);
        }
    }

    private void Update()
    {
        if (GameManager.player1Health == null || vignette == null || colorAdjust == null)
            return;

        float hpPercent = GameManager.sharedHealthPercent;
        bool isFadingOut = GameManager.isFadingOut;
        bool isFadingIn = GameManager.isFadingIn;

        if (isFadingIn)
        {
            fadeInTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeInTimer / fadeInDuration);

            vignette.intensity.value = Mathf.Lerp(1.0f, 0.0f, t);
            colorAdjust.postExposure.value = Mathf.Lerp(-10f, 0f, t);

            return;
        }

        if (!isFadingOut)
        {
            vignette.intensity.value = Mathf.Lerp(0f, 0.6f, 1 - hpPercent);
            vignette.color.value = Color.red;
            colorAdjust.postExposure.value = Mathf.Lerp(0f, -1.5f, 1 - hpPercent);
        }
        else
        {
            fadeOutTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeOutTimer / fadeOutDuration);

            vignette.intensity.value = Mathf.Lerp(0.6f, 1.0f, t);
            colorAdjust.postExposure.value = Mathf.Lerp(-2.5f, -10f, t);
        }
    }
}
