using Photon.Pun;
using UnityEngine;

/* 플레이어 1의 체력을 회복시켜주는 힐팩 아이템 */
public class HealPack : Item
{
    [PunRPC]
    protected override void RPC_AttachToLeftHand(int playerViewID)
    {
        base.RPC_AttachToLeftHand(playerViewID);
    }

    [PunRPC]
    protected override void RPC_DetachFromHand()
    {
        base.RPC_DetachFromHand();
    }

    [PunRPC]
    protected override void RPC_FlyTo(Vector3 pos, Quaternion rot, float duration)
    {
        base.RPC_FlyTo(pos, rot, duration);
    }

    [PunRPC]
    protected override void UseItem()
    {   
        if (GameManager.player1Health != null)
        {   
            // 힐량은 플레이어가 닳은 피 만큼, 항상 최대 체력으로 채움
            float healAmount = GameManager.player1Health.MaxHealth - GameManager.player1Health.currentHealth;
            GameManager.player1Health.RestoreHealth(healAmount);
        }
        base.UseItem(); // Destroy
    }

    protected void OnTriggerEnter(Collider other)
    {
        VRPlayer player = other.GetComponent<VRPlayer>();
        if (player != null)
        {
            PhotonView playerView = player.GetComponent<PhotonView>();
            if (playerView != null && playerView.ViewID != holdingPlayerViewID && shouldUseItem)
            {
                photonView.RPC(nameof(UseItem), RpcTarget.AllBuffered);
            }
        }
    }
}
