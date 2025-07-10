using UnityEngine;
using Photon.Pun;

public class DestructibleObject : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount, int instigatorID)
    {
        // PhotonView�� ������ �ڵ����� �߰�
        if (GetComponent<PhotonView>() == null)
        {
            gameObject.AddComponent<PhotonView>();
        }

        // �ı� ��û
        PhotonNetwork.Destroy(gameObject);
    }
}
