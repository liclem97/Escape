using UnityEngine;
using Photon.Pun;

public class DestructibleObject : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount, int instigatorID)
    {
        // PhotonView가 없으면 자동으로 추가
        if (GetComponent<PhotonView>() == null)
        {
            gameObject.AddComponent<PhotonView>();
        }

        // 파괴 요청
        PhotonNetwork.Destroy(gameObject);
    }
}
