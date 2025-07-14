using Photon.Pun;
using UnityEngine;

/* 리볼버 탄창 아이템 */
public class RevolverAmmo : Item
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
        // 아이템을 놓치면 항상 최초 위치로 되돌림
        base.RPC_FlyTo(Spawner.SpawnPoint.position, Spawner.SpawnPoint.rotation, 0.5f);
    }

    [PunRPC]
    protected override void UseItem()
    {
        DetachFromHand();
        base.UseItem(); // Destroy
    }

    // 리볼버와 부딪힐 시 리볼버의 Reload 함수를 호출
    protected void OnTriggerEnter(Collider other)
    {
        Revolver revolver = other.GetComponent<Revolver>();
        if (revolver)
        {
            revolver.Reload();
            photonView.RPC(nameof(UseItem), RpcTarget.AllBuffered);
        }
    }
}
