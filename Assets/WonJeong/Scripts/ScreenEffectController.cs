using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ScreenDarkenEffect : MonoBehaviour
{
    [SerializeField] private PostProcessVolume volume;

    private ColorGrading colorGrading;

    private int currentStep = 1; // 1~5단계
    private readonly int minStep = 1;
    private readonly int maxStep = 5;

    private float targetExposure;
    private float targetSaturation;
    private Color targetColorFilter;

    private void Start()
    {
        if (volume.profile.TryGetSettings(out colorGrading) == false)
        {
            Debug.LogError("ColorGrading not found in Post Process Profile.");
            enabled = false;
            return;
        }

        ApplyStep(currentStep);
    }

    private void Update()
    {
        // 2번 키: 단계 증가
        if (Input.GetKeyDown(KeyCode.Alpha2) && currentStep < maxStep)
        {
            currentStep++;
            ApplyStep(currentStep);
        }

        // 3번 키: 단계 감소
        if (Input.GetKeyDown(KeyCode.Alpha3) && currentStep > minStep)
        {
            currentStep--;
            ApplyStep(currentStep);
        }

        // 부드럽게 적용
        colorGrading.postExposure.value = Mathf.Lerp(colorGrading.postExposure.value, targetExposure, Time.deltaTime * 2f);
        colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, targetSaturation, Time.deltaTime * 2f);
        colorGrading.colorFilter.value = Color.Lerp(colorGrading.colorFilter.value, targetColorFilter, Time.deltaTime * 2f);
    }

    private void ApplyStep(int step)
    {
        // 단계에 따른 설정값 지정
        switch (step)
        {
            case 1:
                targetExposure = 0f;
                targetSaturation = 0f;
                targetColorFilter = Color.white;
                break;
            case 2:
                targetExposure = -0.5f;
                targetSaturation = -20f;
                targetColorFilter = new Color(1f, 0.7f, 0.7f);
                break;
            case 3:
                targetExposure = -1f;
                targetSaturation = -40f;
                targetColorFilter = new Color(1f, 0.5f, 0.5f);
                break;
            case 4:
                targetExposure = -1.5f;
                targetSaturation = -60f;
                targetColorFilter = new Color(1f, 0.3f, 0.3f);
                break;
            case 5:
                targetExposure = -2f;
                targetSaturation = -80f;
                targetColorFilter = new Color(1f, 0.1f, 0.1f);
                break;
        }

        Debug.Log($"[ScreenEffect] 현재 단계: {step}");
    }
}