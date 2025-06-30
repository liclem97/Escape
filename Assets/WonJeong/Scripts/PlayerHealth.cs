using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Post Process Volume")]
    [SerializeField] private PostProcessVolume volume;

    [Header("Player Health")]
    [SerializeField] private float maxHealth;
    private float health;

    private ColorGrading colorGrading;

    private float targetExposure;
    private float targetSaturation;
    private Color targetColorFilter;

    public float Health
    {
        get => health;
        set
        {
            health = value;
            UpdateScreenEffect();
        }
    }

    private void Start()
    {
        Health = maxHealth;

        if (!volume.profile.TryGetSettings(out colorGrading))
        {
            Debug.LogError("ColorGrading not found in PostProcessProfile.");
            enabled = false;
            return;
        }

        UpdateScreenEffect(); // 초기화
    }

    private void Update()
    {
        // 부드러운 보간 처리
        colorGrading.postExposure.value = Mathf.Lerp(colorGrading.postExposure.value, targetExposure, Time.deltaTime * 2f);
        colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, targetSaturation, Time.deltaTime * 2f);
        colorGrading.colorFilter.value = Color.Lerp(colorGrading.colorFilter.value, targetColorFilter, Time.deltaTime * 2f);
    }

    public void OnDamaged(float damageAmount)
    {
        Health = Mathf.Max(Health - damageAmount, 0f);
    }

    public void RestoreHealth(float healAmount)
    {
        Health = Mathf.Min(Health + healAmount, maxHealth);
    }

    private void UpdateScreenEffect()
    {
        float healthRatio = Mathf.Clamp01(Health / maxHealth); // 0.0 ~ 1.0

        // 건강할수록 원래대로, 체력이 낮을수록 붉고 어두움
        targetExposure = Mathf.Lerp(-2f, 0f, healthRatio);
        targetSaturation = Mathf.Lerp(-80f, 0f, healthRatio);
        targetColorFilter = Color.Lerp(new Color(1f, 0.1f, 0.1f), Color.white, healthRatio);
    }
}
