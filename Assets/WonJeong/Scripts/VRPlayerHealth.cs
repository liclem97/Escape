using Photon.Pun;
using UnityEngine;

public class VRPlayerHealth : MonoBehaviourPun, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    [SerializeField] bool isInvincible = false; // �÷��̾� ���� �Ǻ� ����

    public bool IsInvincible => isInvincible;

    public float HealthPercent => currentHealth / maxHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, int instigatorID)
    {
        if (isInvincible) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        if (photonView.IsMine)
        {
            float percent = HealthPercent;
            PhotonView gameManagerView = GameManager.Instance.GetComponent<PhotonView>();
            gameManagerView.RPC("RPC_SetSharedHealthPercent", RpcTarget.AllBuffered, percent);
        }
    }

    public void RestoreHealth(float amount)
    {
        if (isInvincible || GameManager.Instance.IsGameOver) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);

        if (photonView.IsMine)
        {
            float percent = HealthPercent;
            PhotonView gameManagerView = GameManager.Instance.GetComponent<PhotonView>();
            gameManagerView.RPC("RPC_SetSharedHealthPercent", RpcTarget.AllBuffered, percent);
        }
    }

    [PunRPC]
    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
}
