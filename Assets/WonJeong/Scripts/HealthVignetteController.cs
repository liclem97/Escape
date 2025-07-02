using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HealthVignetteController : MonoBehaviour
{
    private Volume volume;
    private Vignette vignette;
    private ColorAdjustments colorAdjust;

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

        vignette.intensity.value = Mathf.Lerp(0f, 0.6f, 1 - hpPercent);
        vignette.color.value = Color.red;
        colorAdjust.postExposure.value = Mathf.Lerp(0f, -1.5f, 1 - hpPercent);
    }
}
