using Photon.Pun;

/* ��ź ������ ������ �� �ִ� ��ֹ� ��ũ��Ʈ */
public class DestructibleObject : MonoBehaviourPun, IDamageable
{
    // IDamageable �������̽� �Լ�
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
