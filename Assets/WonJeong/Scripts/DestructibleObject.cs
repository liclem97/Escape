using UnityEngine;
using Photon.Pun;

public class DestructibleObject : MonoBehaviourPun, IDamageable
{
    public void TakeDamage(float amount, int instigatorID)
    {
        if (gameObject != null)
        {
            //PhotonNetwork.Destroy(gameObject);
            //gameObject.SetActive(false);
            photonView.RPC(nameof(RPC_DeactiveObject), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_DeactiveObject()
    {
        gameObject.SetActive(false);
    }
}
