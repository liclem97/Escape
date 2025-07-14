using Photon.Pun;

/* 폭탄 등으로 제거할 수 있는 장애물 스크립트 */
public class DestructibleObject : MonoBehaviourPun, IDamageable
{
    // IDamageable 인터페이스 함수
    public void TakeDamage(float amount, int instigatorID)
    {
        if (gameObject != null)
        {
            photonView.RPC(nameof(RPC_DeactiveObject), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_DeactiveObject()
    {
        gameObject.SetActive(false);
    }
}
