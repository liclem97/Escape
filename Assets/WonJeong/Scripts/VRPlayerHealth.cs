using Photon.Pun;
using UnityEngine;

/* 플레이어의 체력을 관리하는 스크립트 */
public class VRPlayerHealth : MonoBehaviourPun, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    [SerializeField] bool isInvincible = false; // 플레이어 무적 판별 여부

    public bool IsInvincible => isInvincible;

    public float HealthPercent => currentHealth / maxHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // IDamageable 인터페이스 상속 함수
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

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: RestoreHealth
    * 기능: 체력을 회복시키는 함수
    * 입력:
    *   - amount: 회복시킬 체력의 양
    ***********************************************************************************/
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
