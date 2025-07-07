using Photon.Pun;
using UnityEngine;

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
            float healAmount = GameManager.player1Health.MaxHealth - GameManager.player1Health.currentHealth;
            GameManager.player1Health.RestoreHealth(healAmount);

            Debug.Log($"Use HealPack Item, healAmount: {healAmount}");
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
