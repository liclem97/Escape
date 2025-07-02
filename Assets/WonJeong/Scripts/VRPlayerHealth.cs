using Photon.Pun;
using UnityEngine;

public class VRPlayerHealth : MonoBehaviourPun, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    [SerializeField] bool isInvincible = false; // �÷��̾� ���� �Ǻ� ����

    public float HealthPercent => currentHealth / maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible) return; // �÷��̾ ����, �� �÷��̾�2�� return

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
    }

    [PunRPC]
    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
}
