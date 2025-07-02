using Photon.Pun;
using UnityEngine;

public class VRPlayerHealth : MonoBehaviourPun, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    [SerializeField] bool isInvincible = false; // 플레이어 무적 판별 여부

    public float HealthPercent => currentHealth / maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible) return; // 플레이어가 무적, 즉 플레이어2면 return

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
    }

    [PunRPC]
    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
}
