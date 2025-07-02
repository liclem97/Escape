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
    private void Start()
    {

    }

    public void TakeDamage(float amount)
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
        if (isInvincible) return;

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
